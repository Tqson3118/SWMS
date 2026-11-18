using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;

namespace Swms.Api.Infrastructure.Security;

public sealed record GoogleTokenPayload(
    string Subject,
    string Email,
    bool EmailVerified,
    string FullName,
    string? AvatarUrl);

public interface IGoogleIdTokenValidator
{
    bool IsConfigured { get; }
    string? PrimaryClientId { get; }
    Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken ct = default);
}

/// <summary>
/// Validates Google Identity Services ID tokens (RS256 JWTs) against Google's official JWKS endpoint.
/// </summary>
public sealed class GoogleJwksIdTokenValidator : IGoogleIdTokenValidator
{
    private const string DefaultJwksUri = "https://www.googleapis.com/oauth2/v3/certs";
    private static readonly string[] ValidIssuers = ["accounts.google.com", "https://accounts.google.com"];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleJwksIdTokenValidator> _logger;
    private readonly string[] _clientIds;
    private readonly string _jwksUri;
    private readonly SemaphoreSlim _jwksLock = new(1, 1);

    private JsonWebKeySet? _cachedJwks;
    private DateTimeOffset _jwksFetchedAt = DateTimeOffset.MinValue;
    private DateTimeOffset _jwksExpiresAt = DateTimeOffset.MinValue;

    public GoogleJwksIdTokenValidator(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleJwksIdTokenValidator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _clientIds = ResolveGoogleClientIds(configuration);
        var configuredJwks = configuration["Authentication:Google:JwksUri"];
        _jwksUri = !string.IsNullOrWhiteSpace(configuredJwks) ? configuredJwks.Trim() : DefaultJwksUri;
    }

    public bool IsConfigured => _clientIds.Length > 0;
    public string? PrimaryClientId => _clientIds.Length > 0 ? _clientIds[0] : null;

    public static string[] ResolveGoogleClientIds(IConfiguration configuration)
    {
        var rawCandidates = new[]
        {
            Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"),
            Environment.GetEnvironmentVariable("Authentication__Google__ClientId"),
            configuration["Authentication:Google:ClientId"],
            configuration["Google:ClientId"],
            Environment.GetEnvironmentVariable("NEXT_PUBLIC_GOOGLE_CLIENT_ID")
        };

        var results = new List<string>();
        foreach (var candidate in rawCandidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            foreach (var part in candidate.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (IsValidClientIdValue(part) && !results.Contains(part, StringComparer.Ordinal))
                {
                    results.Add(part);
                }
            }
        }

        return results.ToArray();
    }

    private static bool IsValidClientIdValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase)) return false;
        if (value.StartsWith("<", StringComparison.Ordinal)) return false;
        return value.Length >= 10;
    }

    public async Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        if (!handler.CanReadToken(idToken))
        {
            return null;
        }

        try
        {
            var jwks = await GetJwksAsync(forceRefresh: false, ct);
            if (jwks == null)
            {
                return null;
            }

            return ValidateWithJwks(handler, idToken, jwks);
        }
        catch (Exception ex) when (ex is SecurityTokenSignatureKeyNotFoundException or SecurityTokenInvalidSignatureException)
        {
            // Google rotates signing keys periodically; refresh JWKS once if cache isn't brand new
            try
            {
                var refreshedJwks = await GetJwksAsync(forceRefresh: true, ct);
                if (refreshedJwks != null)
                {
                    return ValidateWithJwks(handler, idToken, refreshedJwks);
                }
            }
            catch (Exception retryEx)
            {
                _logger.LogWarning(retryEx, "Google ID token signature validation failed after JWKS refresh.");
            }
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Google ID token validation rejected token.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error while validating Google ID token.");
            return null;
        }
    }

    private GoogleTokenPayload? ValidateWithJwks(JwtSecurityTokenHandler handler, string idToken, JsonWebKeySet jwks)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = ValidIssuers,
            ValidateAudience = true,
            ValidAudiences = _clientIds,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = jwks.GetSigningKeys()
        };

        var principal = handler.ValidateToken(idToken, parameters, out var validatedToken);
        if (validatedToken is not JwtSecurityToken jwt ||
            !string.Equals(jwt.Header.Alg, SecurityAlgorithms.RsaSha256, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var sub = principal.FindFirst("sub")?.Value?.Trim();
        var rawEmail = principal.FindFirst("email")?.Value?.Trim();
        var emailVerifiedRaw = principal.FindFirst("email_verified")?.Value;
        var emailVerified = string.Equals(emailVerifiedRaw, "true", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(sub) ||
            string.IsNullOrWhiteSpace(rawEmail) ||
            !rawEmail.Contains('@') ||
            !emailVerified)
        {
            return null;
        }

        var email = rawEmail.ToLowerInvariant();
        var fullName = principal.FindFirst("name")?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = email.Split('@')[0];
        }
        if (fullName.Length > 150)
        {
            fullName = fullName[..150];
        }

        string? avatarUrl = null;
        var rawPicture = principal.FindFirst("picture")?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(rawPicture) &&
            rawPicture.Length <= 500 &&
            Uri.TryCreate(rawPicture, UriKind.Absolute, out var picUri) &&
            picUri.Scheme == Uri.UriSchemeHttps)
        {
            avatarUrl = picUri.ToString();
        }

        return new GoogleTokenPayload(sub, email, emailVerified, fullName, avatarUrl);
    }

    private async Task<JsonWebKeySet?> GetJwksAsync(bool forceRefresh, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        if (!forceRefresh && _cachedJwks != null && now < _jwksExpiresAt)
        {
            return _cachedJwks;
        }

        await _jwksLock.WaitAsync(ct);
        try
        {
            now = DateTimeOffset.UtcNow;
            if (!forceRefresh && _cachedJwks != null && now < _jwksExpiresAt)
            {
                return _cachedJwks;
            }
            if (forceRefresh && _cachedJwks != null && (now - _jwksFetchedAt) < TimeSpan.FromSeconds(30))
            {
                return _cachedJwks;
            }

            var client = _httpClientFactory.CreateClient("GoogleAuth");
            using var req = new HttpRequestMessage(HttpMethod.Get, _jwksUri);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var resp = await client.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch Google JWKS endpoint ({StatusCode}).", (int)resp.StatusCode);
                return _cachedJwks;
            }

            var json = await resp.Content.ReadAsStringAsync(ct);
            var jwks = new JsonWebKeySet(json);
            if (jwks.Keys == null || jwks.Keys.Count == 0)
            {
                return _cachedJwks;
            }

            var maxAgeSeconds = resp.Headers.CacheControl?.MaxAge?.TotalSeconds ?? 3600;
            var ttlSeconds = Math.Clamp(maxAgeSeconds, 60, 86400);

            _cachedJwks = jwks;
            _jwksFetchedAt = now;
            _jwksExpiresAt = now.AddSeconds(ttlSeconds);
            return _cachedJwks;
        }
        finally
        {
            _jwksLock.Release();
        }
    }
}
