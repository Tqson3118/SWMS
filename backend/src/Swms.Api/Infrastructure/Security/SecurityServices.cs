using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Database;

namespace Swms.Api.Infrastructure.Security;

public static class PasswordHashUtility
{
    public static string HashPassword(string password)
    {
        var salt = Encoding.UTF8.GetBytes("SWMS-v1-Static-PBKDF2-Salt-2026");
        var bytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            10000,
            HashAlgorithmName.SHA256,
            32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool VerifyPassword(string password, string storedHash) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(HashPassword(password)),
            Encoding.UTF8.GetBytes(storedHash.ToLowerInvariant()));

    public static string ComputeSha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

/// <summary>
/// AES-256-GCM encryption for storing replacement refresh tokens securely in PostgreSQL
/// to support the 15-30s idempotent rotation grace window (Decision C & ADR-016).
/// </summary>
public class AesGcmTokenProtector
{
    private readonly byte[] _masterKey;

    public AesGcmTokenProtector(string? base64Key = null)
    {
        var raw = string.IsNullOrWhiteSpace(base64Key)
            ? Encoding.UTF8.GetBytes("SWMS-AES-256-GCM-MASTER-KEY-2026")
            : SHA256.HashData(Encoding.UTF8.GetBytes(base64Key));
        _masterKey = raw.Length == 32 ? raw : SHA256.HashData(raw);
    }

    public byte[] Encrypt(string plaintextToken)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintextToken);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_masterKey, 16);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Layout: [12-byte nonce][16-byte tag][ciphertext]
        var combined = new byte[12 + 16 + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, 12);
        Buffer.BlockCopy(tag, 0, combined, 12, 16);
        Buffer.BlockCopy(ciphertext, 0, combined, 28, ciphertext.Length);
        return combined;
    }

    public string Decrypt(byte[] encryptedPayload)
    {
        if (encryptedPayload.Length < 28)
            throw new CryptographicException("Invalid AES-GCM payload length.");

        var nonce = new byte[12];
        var tag = new byte[16];
        var ciphertext = new byte[encryptedPayload.Length - 28];

        Buffer.BlockCopy(encryptedPayload, 0, nonce, 0, 12);
        Buffer.BlockCopy(encryptedPayload, 12, tag, 0, 16);
        Buffer.BlockCopy(encryptedPayload, 28, ciphertext, 0, ciphertext.Length);

        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(_masterKey, 16);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }
}

public class JwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _signingKey;
    private readonly int _accessTokenMinutes;

    public JwtTokenService(IConfiguration config)
    {
        _issuer = config["Jwt:Issuer"] ?? "https://api.studxy.vn";
        _audience = config["Jwt:Audience"] ?? "https://app.studxy.vn";
        _signingKey = config["Jwt:SigningKey"] ?? "SWMS-v1-Enterprise-Jwt-HmacSha256-Secret-Key-2026-Studxy-Workspace!";
        _accessTokenMinutes = int.TryParse(config["Jwt:AccessTokenMinutes"], out var mins) ? mins : 15;
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.FullName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("ver", user.TokenVersion.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateSecureRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}

public record TokenRotationOutcome(
    bool Succeeded,
    bool IsGraceWindowRecovery,
    bool IsBreachDetected,
    string? AccessToken,
    string? RefreshToken,
    User? User,
    string? ErrorMessage
);

/// <summary>
/// Refresh Token Rotation (RTR) with Token Families, 15-30s Idempotent Grace Window,
/// AES-256-GCM Encrypted Replacement Token, and Replay Breach Alarm (ADR-003 & ADR-016).
/// </summary>
public class RefreshTokenRotationService
{
    private readonly SwmsDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly AesGcmTokenProtector _protector;
    private readonly int _graceWindowSeconds;
    private static readonly SemaphoreSlim _rotationLock = new(1, 1);

    public RefreshTokenRotationService(
        SwmsDbContext db,
        JwtTokenService jwt,
        AesGcmTokenProtector protector,
        IConfiguration config)
    {
        _db = db;
        _jwt = jwt;
        _protector = protector;
        _graceWindowSeconds = int.TryParse(config["Jwt:GraceWindowSeconds"], out var s) ? s : 30;
    }

    public async Task<(string AccessToken, string RawRefreshToken, RefreshToken Entity)> IssueInitialTokenPairAsync(
        User user,
        string? deviceInfo = null,
        string? ipAddress = null,
        DateTimeOffset? nowOverride = null)
    {
        var now = nowOverride ?? DateTimeOffset.UtcNow;
        var rawRefresh = JwtTokenService.GenerateSecureRefreshToken();
        var hash = PasswordHashUtility.ComputeSha256Hex(rawRefresh);
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = hash,
            FamilyId = Guid.NewGuid(),
            IsRevoked = false,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            CreatedAt = now,
            ExpiresAt = now.AddDays(14)
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync();

        var accessToken = _jwt.GenerateAccessToken(user);
        return (accessToken, rawRefresh, entity);
    }

    public async Task<TokenRotationOutcome> RotateRefreshTokenAsync(
        string rawRefreshToken,
        DateTimeOffset? serverNowOverride = null)
    {
        await _rotationLock.WaitAsync();
        try
        {
            await using var transaction = _db.Database.IsRelational()
                ? await _db.Database.BeginTransactionAsync()
                : null;

            var now = serverNowOverride ?? DateTimeOffset.UtcNow;
            var tokenHash = PasswordHashUtility.ComputeSha256Hex(rawRefreshToken);

            // Pessimistic Row Locking (SELECT ... FOR UPDATE) in PostgreSQL per MASTER_PROMPT.md §6.2 & ADR-003 §7
            RefreshToken? existing;
            if (_db.Database.IsRelational())
            {
                var lockedRows = await _db.RefreshTokens
                    .FromSqlInterpolated($"SELECT * FROM refresh_tokens WHERE token_hash = {tokenHash} FOR UPDATE")
                    .AsTracking()
                    .ToListAsync();
                existing = lockedRows.FirstOrDefault();
            }
            else
            {
                existing = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
            }

            if (existing == null)
            {
                return new TokenRotationOutcome(false, false, false, null, null, null, "Invalid refresh token.");
            }
            await _db.Entry(existing).ReloadAsync();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == existing.UserId);
            if (user == null)
            {
                return new TokenRotationOutcome(false, false, false, null, null, null, "User not found.");
            }
            await _db.Entry(user).ReloadAsync();

            if (existing.ExpiresAt <= now)
            {
                return new TokenRotationOutcome(false, false, false, null, null, null, "Refresh token has expired.");
            }

            if (!existing.IsRevoked)
            {
                // Standard Single-Use Rotation: Revoke old token, issue replacement in same family, encrypt replacement
                var newRawRefresh = JwtTokenService.GenerateSecureRefreshToken();
                var newHash = PasswordHashUtility.ComputeSha256Hex(newRawRefresh);
                var replacementEntity = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TokenHash = newHash,
                    FamilyId = existing.FamilyId,
                    IsRevoked = false,
                    DeviceInfo = existing.DeviceInfo,
                    IpAddress = existing.IpAddress,
                    CreatedAt = now,
                    ExpiresAt = now.AddDays(14)
                };

                existing.IsRevoked = true;
                existing.RevokedAt = now;
                existing.ReplacedByTokenId = replacementEntity.Id;
                existing.EncryptedReplacementToken = _protector.Encrypt(newRawRefresh);

                _db.RefreshTokens.Add(replacementEntity);
                await _db.SaveChangesAsync();
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                var newAccessToken = _jwt.GenerateAccessToken(user);
                return new TokenRotationOutcome(true, false, false, newAccessToken, newRawRefresh, user, null);
            }

            // Token IS already revoked -> Check 15-30s Idempotent Grace Window (ADR-003 & ADR-016)
            var elapsedSeconds = existing.RevokedAt.HasValue ? (now - existing.RevokedAt.Value).TotalSeconds : double.MaxValue;
            if (existing.RevokedAt.HasValue &&
                existing.ReplacedByTokenId.HasValue &&
                existing.EncryptedReplacementToken is { Length: > 0 } &&
                elapsedSeconds >= 0 &&
                elapsedSeconds <= _graceWindowSeconds)
            {
                var recoveredRefresh = _protector.Decrypt(existing.EncryptedReplacementToken);
                var newAccessToken = _jwt.GenerateAccessToken(user);
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }
                return new TokenRotationOutcome(true, true, false, newAccessToken, recoveredRefresh, user, null);
            }

            // 🚨 REPLAY BREACH ALARM: Revoked token reused outside grace window!
            var familyTokens = await _db.RefreshTokens
                .Where(t => t.FamilyId == existing.FamilyId)
                .ToListAsync();

            foreach (var ft in familyTokens)
            {
                ft.IsRevoked = true;
                ft.RevokedAt ??= now;
                ft.EncryptedReplacementToken = null; // Purge cached replacement tokens immediately
            }

            user.TokenVersion += 1;
            user.UpdatedAt = now;
            await _db.SaveChangesAsync();
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return new TokenRotationOutcome(
                Succeeded: false,
                IsGraceWindowRecovery: false,
                IsBreachDetected: true,
                AccessToken: null,
                RefreshToken: null,
                User: user,
                ErrorMessage: "BREACH_DETECTED: Refresh token reuse outside 30s grace window. All sessions in family revoked and token_version incremented.");
        }
        finally
        {
            _rotationLock.Release();
        }
    }
}
