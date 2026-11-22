using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;
using Swms.Api.Features.Pdf;
using Swms.Api.Infrastructure.Ai;
using Swms.Api.Infrastructure.Database;
using Swms.Api.Infrastructure.FSRS;
using Swms.Api.Infrastructure.Security;
using Swms.Api.Infrastructure.SelfHealing;

namespace Swms.Api.Features;

public record RegisterRequest(string Email, string Password, string FullName);
public record LoginRequest(string Email, string? Password = null, string? DemoProfile = null);
public record GoogleAuthRequest(string? Credential, string? IdToken = null);
public record ChangePasswordRequest(string? CurrentPassword, string NewPassword);
public record RefreshRequest(string? RefreshToken);
public record OnboardingStepRequest(int Step, bool? CompleteActivation);

public record CreateAnnotationRequest(
    Guid? LessonId,
    Guid? BlockId,
    Guid? DocumentId,
    int? PageNumber,
    string? BoundingBoxesJson,
    string ExactQuote,
    string? PrefixQuote,
    string? SuffixQuote,
    int StartOffset,
    int EndOffset,
    string ColorKey,
    string? SemanticLabel,
    string? NoteComment
);

public record ReconcileAnnotationRequest(
    string Action, // reanchor | keep_unanchored | delete
    Guid? NewBlockId,
    int? NewStartOffset,
    int? NewEndOffset,
    string? NewExactQuote
);

public record PublishLessonVersionRequest(
    string ContentAstJson,
    string PlainText,
    string? ChangeSummary
);

public record SaveNotebookRequest(
    Guid? MutationId,
    int BaseVersion,
    string? DeviceId,
    string? Title,
    string ContentAstJson
);

public record AutoProvisionNotebookRequest(
    Guid? LessonId,
    Guid? DocumentId,
    string? SourceTitle
);

public record CreateNotebookRequest(
    string? Title = null,
    string? ContentAstJson = null
);

public record CreateFlashcardRequest(
    Guid? LessonId,
    Guid? DocumentId,
    Guid? AnnotationId,
    string CardType, // cloze | standard | ai_generated
    string? SourceTitleSnapshot,
    string? SourceQuoteSnapshot,
    string FrontText,
    string BackText,
    List<string>? TagNames,
    bool DeduplicateByFront = false
);

public record SyncReviewsBatchRequest(List<FsrsReviewEventInput> Reviews);

public record SubmitQuizItemRequest(
    string Answer,
    Guid? IdempotencyKey,
    bool WaitForAiCompletion = true
);

public record AiInSituRequest(
    string Excerpt,
    string? ContextTitle,
    string? ActionType // explain | generate_qa | counter_example | translate
);

public record AiEvaluatePracticeRequest(
    string Mode,
    string? Lang,
    string Prompt,
    string UserResponse,
    List<string>? ExpectedKeywords = null,
    string? SampleAnswer = null,
    string? ContextTitle = null
);

public record UploadDocumentRequest(
    string Title,
    long FileSizeBytes,
    int TotalPages,
    string? MimeType,
    string? ExtractedText
);

public record WizardClarifyRequest(string Topic);
public record WizardPreviewRequest(string Topic, Dictionary<string, string>? Answers);
public record UpdateCourseRequest(string? Title, string? Description, string? Visibility, bool? IsPublished);
public record PublishCourseRequest(string? Visibility = null, bool? IsPublished = null);
public record CreateChapterRequest(string Title, int? SortOrder = null);
public record UpdateChapterRequest(string? Title, int? SortOrder);
public record CreateLessonRequest(
    string Title,
    string? ContentMarkdown = null,
    string? VideoUrl = null,
    int? EstimatedMinutes = null,
    int? SortOrder = null
);
public record UpdateLessonRequest(
    string? Title = null,
    int? SortOrder = null,
    string? ContentAstJson = null,
    string? PlainText = null,
    string? ChangeSummary = null,
    string? ContentMarkdown = null,
    string? VideoUrl = null,
    int? EstimatedMinutes = null
);

public record RecordPracticeAttemptRequest(
    Guid? ActivityId,
    Guid CourseId,
    Guid? LessonId,
    string? ActivityType,
    string? ActivityKey,
    string? ActivityTitle,
    double Score,
    int? MaxScore = 100,
    bool? IsPassed = null,
    string? DetailJson = null,
    int? DurationSeconds = null
);

public static class SwmsEndpoints
{
    private static async Task<HashSet<string>> GetSqliteTableColumnsAsync(SwmsDbContext db, string tableName)
    {
        var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var conn = db.Database.GetDbConnection();
        var shouldClose = conn.State != System.Data.ConnectionState.Open;
        if (shouldClose) await conn.OpenAsync();
        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info('{tableName}');";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(1))
                {
                    cols.Add(reader.GetString(1));
                }
            }
        }
        finally
        {
            if (shouldClose) await conn.CloseAsync();
        }
        return cols;
    }

    public static async Task EnsureSqliteSchemaUpdatedAsync(SwmsDbContext db)
    {
        if (db.Database.IsNpgsql())
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE users ADD COLUMN IF NOT EXISTS auth_provider VARCHAR(32) NOT NULL DEFAULT 'local';
                    ALTER TABLE users ADD COLUMN IF NOT EXISTS google_sub VARCHAR(128);
                    ALTER TABLE users ADD COLUMN IF NOT EXISTS avatar_url VARCHAR(500);
                    CREATE UNIQUE INDEX IF NOT EXISTS ix_users_google_sub ON users (google_sub) WHERE google_sub IS NOT NULL;

                    CREATE TABLE IF NOT EXISTS practice_activities (
                        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                        course_id UUID NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
                        lesson_id UUID REFERENCES lessons(id) ON DELETE CASCADE,
                        order_index INT NOT NULL DEFAULT 0,
                        type VARCHAR(50) NOT NULL,
                        title VARCHAR(255) NOT NULL,
                        description TEXT,
                        config JSONB NOT NULL DEFAULT '{}'::jsonb,
                        passing_score INT NOT NULL DEFAULT 70,
                        is_seeded BOOLEAN NOT NULL DEFAULT FALSE,
                        created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                        updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                    );
                    CREATE INDEX IF NOT EXISTS idx_practice_activities_course_lesson ON practice_activities(course_id, lesson_id);

                    CREATE TABLE IF NOT EXISTS practice_attempts (
                        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                        user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                        activity_id UUID REFERENCES practice_activities(id) ON DELETE SET NULL,
                        course_id UUID NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
                        lesson_id UUID REFERENCES lessons(id) ON DELETE CASCADE,
                        activity_type VARCHAR(50) NOT NULL,
                        activity_key VARCHAR(100) NOT NULL,
                        activity_title VARCHAR(255) NOT NULL,
                        score DOUBLE PRECISION NOT NULL DEFAULT 0,
                        max_score INT NOT NULL DEFAULT 100,
                        is_passed BOOLEAN NOT NULL DEFAULT FALSE,
                        detail JSONB NOT NULL DEFAULT '{}'::jsonb,
                        duration_seconds INT,
                        created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                    );
                    CREATE INDEX IF NOT EXISTS idx_practice_attempts_user_date ON practice_attempts(user_id, created_at DESC);
                    CREATE INDEX IF NOT EXISTS idx_practice_attempts_user_course_lesson ON practice_attempts(user_id, course_id, lesson_id);
                    CREATE INDEX IF NOT EXISTS idx_practice_attempts_user_key ON practice_attempts(user_id, activity_key);
                ");
            }
            catch { }
            return;
        }

        if (!db.Database.IsSqlite()) return;
        try { await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA temp_store=MEMORY; PRAGMA busy_timeout=5000;"); } catch { }

        var lessonCols = await GetSqliteTableColumnsAsync(db, "lessons");
        if (!lessonCols.Contains("content_markdown"))
            try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE lessons ADD COLUMN content_markdown TEXT;"); } catch { }
        if (!lessonCols.Contains("video_url"))
            try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE lessons ADD COLUMN video_url TEXT;"); } catch { }
        if (!lessonCols.Contains("estimated_minutes"))
            try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE lessons ADD COLUMN estimated_minutes INTEGER NOT NULL DEFAULT 5;"); } catch { }

        var userCols = await GetSqliteTableColumnsAsync(db, "users");
        if (!userCols.Contains("auth_provider"))
            try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE users ADD COLUMN auth_provider TEXT NOT NULL DEFAULT 'local';"); } catch { }
        if (!userCols.Contains("google_sub"))
            try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE users ADD COLUMN google_sub TEXT;"); } catch { }
        if (!userCols.Contains("avatar_url"))
            try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE users ADD COLUMN avatar_url TEXT;"); } catch { }
        try { await db.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS ix_users_google_sub_partial ON users (google_sub) WHERE google_sub IS NOT NULL;"); } catch { }

        try
        {
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS user_lesson_progress (
                    id TEXT NOT NULL PRIMARY KEY,
                    user_id TEXT NOT NULL,
                    lesson_id TEXT NOT NULL,
                    is_completed INTEGER NOT NULL,
                    completed_at TEXT,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL
                );
            ");
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ix_user_lesson_progress_user_id_lesson_id ON user_lesson_progress (user_id, lesson_id);
            ");

            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS practice_activities (
                    id TEXT NOT NULL PRIMARY KEY,
                    course_id TEXT NOT NULL,
                    lesson_id TEXT,
                    order_index INTEGER NOT NULL DEFAULT 0,
                    type TEXT NOT NULL,
                    title TEXT NOT NULL,
                    description TEXT,
                    config TEXT NOT NULL DEFAULT '{}',
                    passing_score INTEGER NOT NULL DEFAULT 70,
                    is_seeded INTEGER NOT NULL DEFAULT 0,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS ix_practice_activities_course_lesson ON practice_activities (course_id, lesson_id);

                CREATE TABLE IF NOT EXISTS practice_attempts (
                    id TEXT NOT NULL PRIMARY KEY,
                    user_id TEXT NOT NULL,
                    activity_id TEXT,
                    course_id TEXT NOT NULL,
                    lesson_id TEXT,
                    activity_type TEXT NOT NULL,
                    activity_key TEXT NOT NULL,
                    activity_title TEXT NOT NULL,
                    score REAL NOT NULL DEFAULT 0,
                    max_score INTEGER NOT NULL DEFAULT 100,
                    is_passed INTEGER NOT NULL DEFAULT 0,
                    detail TEXT NOT NULL DEFAULT '{}',
                    duration_seconds INTEGER,
                    created_at TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS ix_practice_attempts_user_created ON practice_attempts (user_id, created_at);
                CREATE INDEX IF NOT EXISTS ix_practice_attempts_user_course_lesson ON practice_attempts (user_id, course_id, lesson_id);
                CREATE INDEX IF NOT EXISTS ix_practice_attempts_user_key ON practice_attempts (user_id, activity_key);
            ");
        }
        catch { }
    }

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> RegistrationEmailLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> NotebookAutoProvisionLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> NotebookOccLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FlashcardSyncLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> AiQuotaLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> LessonCompleteLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> QuizItemSubmitLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> WizardCommitLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, (DateTimeOffset CreatedAt, object Summary)> WizardIdempotencyCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly SemaphoreSlim SqliteFlashcardWriteLock = new(1, 1);

    private static readonly ConcurrentDictionary<string, (int FailedCount, DateTimeOffset WindowStart)> LoginFailedAttempts = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<Guid, int> UndoneLapsesByCard = new();
    private static readonly ConcurrentDictionary<Guid, List<(Guid LessonId, DateTimeOffset CompletedAt)>> UserLessonCompleteTimes = new();

    private static readonly Regex ScriptBlockRegex = new(@"<script\b[^>]*>[\s\S]*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ScriptTagRegex = new(@"</?script\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ImgOnErrorRegex = new(@"<img\b[^>]*\bonerror\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex OnEventRegex = new(@"\bon\w+\s*=\s*(""[^""]*""|'[^']*'|&quot;.*?&quot;|[^\s>]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string? SanitizeDangerousHtml(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var sanitized = ScriptBlockRegex.Replace(input, string.Empty);
        sanitized = ScriptTagRegex.Replace(sanitized, string.Empty);
        sanitized = ImgOnErrorRegex.Replace(sanitized, string.Empty);
        sanitized = OnEventRegex.Replace(sanitized, string.Empty);
        return sanitized;
    }

    private static bool IsStrongPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return false;
        return password.Any(char.IsLetter) && password.Any(char.IsDigit);
    }

    private static Guid DeriveDeterministicGuid(Guid userId, Guid reviewEventId)
    {
        var input = $"{userId:N}:{reviewEventId:N}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }

    private static readonly bool _isRunningUnderTests = AppDomain.CurrentDomain.GetAssemblies()
        .Any(a => a.GetName().Name?.Contains("testhost", StringComparison.OrdinalIgnoreCase) == true);

    public static void MapSwmsEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1")
            .AddEndpointFilter(async (ctx, next) =>
            {
                var endpoint = ctx.HttpContext.GetEndpoint();
                if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
                {
                    return await next(ctx);
                }

                if (!TryResolveUserId(ctx.HttpContext, out _))
                {
                    return Results.Unauthorized();
                }

                return await next(ctx);
            });

        MapAuthEndpoints(api);
        MapWorkspaceEndpoints(api);
        MapLessonAndDocumentEndpoints(api);
        MapAnnotationEndpoints(api);
        MapNotebookEndpoints(api);
        MapFlashcardAndTagEndpoints(api);
        MapPracticeQuizAndAiEndpoints(api);
        MapPracticeEngineEndpoints(api);
    }

    private static bool IsTestUserHeaderAllowed(HttpContext ctx)
    {
        var cfg = ctx.RequestServices.GetService<IConfiguration>();
        if (string.Equals(cfg?["Auth:EnforceStrictBearer"], "true", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(Environment.GetEnvironmentVariable("SWMS_ALLOW_TEST_USER_HEADER"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return _isRunningUnderTests;
    }

    private static bool TryResolveUserId(HttpContext ctx, out Guid userId)
    {
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var sub = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? ctx.User.FindFirstValue("sub");
            if (Guid.TryParse(sub, out userId))
            {
                return true;
            }
        }

        if (IsTestUserHeaderAllowed(ctx))
        {
            if (ctx.Request.Headers.TryGetValue("X-Test-User-Id", out var headerUid) &&
                Guid.TryParse(headerUid.ToString(), out userId))
            {
                return true;
            }

            userId = SeedDataInitializer.DefaultUserId;
            return true;
        }

        userId = Guid.Empty;
        return false;
    }

    private static Guid ResolveUserId(HttpContext ctx)
    {
        if (TryResolveUserId(ctx, out var uid))
        {
            return uid;
        }
        throw new UnauthorizedAccessException("Missing or invalid authentication token.");
    }

    private static bool IsOwnerOrDefaultSeededAccess(Guid resourceOwnerId, Guid callerUserId) =>
        resourceOwnerId == callerUserId ||
        (callerUserId == SeedDataInitializer.DefaultUserId && resourceOwnerId == SeedDataInitializer.DefaultUserId);

    private static async Task<bool> CanReadCourseAsync(SwmsDbContext db, Course course, Guid userId)
    {
        if (IsOwnerOrDefaultSeededAccess(course.InstructorId, userId)) return true;
        if (course.Visibility == "public" && course.IsPublished) return true;
        return await db.CourseEnrollments.AnyAsync(e => e.CourseId == course.Id && e.UserId == userId);
    }

    // ========================================================================
    // 1. AUTHENTICATION & BFF RTR ENDPOINTS (ADR-003 & ADR-016)
    // ========================================================================
    private static object ToUserAuthDto(User user) => new
    {
        user.Id,
        user.Email,
        user.FullName,
        user.Role,
        user.TokenVersion,
        user.StreakDays,
        user.OnboardingStep,
        user.IsActivated,
        user.AuthProvider,
        user.AvatarUrl,
        hasLocalPassword = PasswordHashUtility.HasLocalPassword(user.PasswordHash)
    };

    private static Course ProvisionPersonalWorkspaceDefaults(SwmsDbContext db, User user)
    {
        // Auto-provision Personal Workspace (Decision 1 & ADR-019)
        var personalCourse = new Course
        {
            Id = Guid.NewGuid(),
            InstructorId = user.Id,
            Title = $"Không gian cá nhân ({user.FullName})",
            Slug = $"personal-workspace-{user.Id:N}",
            Description = "Không gian học tập chủ quyền cá nhân (Sovereign Personal Workspace)",
            Visibility = "personal",
            IsPublished = true
        };
        db.Courses.Add(personalCourse);
        db.UserFsrsParameters.Add(new UserFsrsParameters { UserId = user.Id });
        db.UserQuotas.Add(new UserQuota { UserId = user.Id, CurrentStorageBytes = 0 });
        return personalCourse;
    }

    private static void MapAuthEndpoints(RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth");

        auth.MapGet("/providers", (IGoogleIdTokenValidator googleValidator) =>
        {
            return Results.Ok(new
            {
                localEnabled = true,
                googleEnabled = googleValidator.IsConfigured,
                googleClientId = googleValidator.PrimaryClientId
            });
        }).AllowAnonymous();

        auth.MapPost("/register", async (
            RegisterRequest? req,
            HttpContext http,
            SwmsDbContext db,
            RefreshTokenRotationService rtr) =>
        {
            if (req == null ||
                string.IsNullOrWhiteSpace(req.Email) ||
                !req.Email.Contains('@'))
            {
                return Results.BadRequest(new { error = "Email hợp lệ là bắt buộc." });
            }

            if (!IsStrongPassword(req.Password))
            {
                return Results.BadRequest(new
                {
                    error = "WEAK_PASSWORD",
                    message = "Mật khẩu phải có ít nhất 6 ký tự, bao gồm cả chữ cái và chữ số."
                });
            }

            var normalizedEmail = req.Email.Trim().ToLowerInvariant();
            var emailLock = RegistrationEmailLocks.GetOrAdd(normalizedEmail, _ => new SemaphoreSlim(1, 1));
            await emailLock.WaitAsync();
            try
            {
                if (await db.Users.AnyAsync(u => u.Email == normalizedEmail))
                {
                    return Results.Conflict(new { error = "Email đã tồn tại trong hệ thống." });
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = normalizedEmail,
                    PasswordHash = PasswordHashUtility.HashPassword(req.Password),
                    FullName = string.IsNullOrWhiteSpace(req.FullName) ? "Học viên SWMS" : req.FullName.Trim(),
                    Role = "student",
                    AuthProvider = "local",
                    TokenVersion = 1,
                    StreakDays = 1,
                    OnboardingStep = 1,
                    IsActivated = false
                };
                db.Users.Add(user);
                var personalCourse = ProvisionPersonalWorkspaceDefaults(db, user);

                await db.SaveChangesAsync();

                var (accessToken, rawRefresh, _) = await rtr.IssueInitialTokenPairAsync(
                    user,
                    http.Request.Headers.UserAgent.ToString(),
                    http.Connection.RemoteIpAddress?.ToString());

                AppendRefreshTokenCookie(http, rawRefresh);

                return Results.Ok(new
                {
                    accessToken,
                    refreshToken = rawRefresh,
                    personalWorkspaceCourseId = personalCourse.Id,
                    user = ToUserAuthDto(user)
                });
            }
            finally
            {
                emailLock.Release();
            }
        }).AllowAnonymous();

        auth.MapPost("/login", async (
            LoginRequest? req,
            HttpContext http,
            SwmsDbContext db,
            RefreshTokenRotationService rtr) =>
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Email))
            {
                return Results.Unauthorized();
            }

            var email = req.Email.Trim().ToLowerInvariant();
            var now = DateTimeOffset.UtcNow;

            // 1. Brute-force / Rate limit check per normalized email
            if (LoginFailedAttempts.TryGetValue(email, out var record))
            {
                if (now - record.WindowStart < TimeSpan.FromSeconds(60))
                {
                    if (record.FailedCount >= 5)
                    {
                        return Results.Json(new
                        {
                            error = "TOO_MANY_LOGIN_ATTEMPTS",
                            message = "Tài khoản tạm khóa 1 phút do đăng nhập sai nhiều lần."
                        }, statusCode: StatusCodes.Status429TooManyRequests);
                    }
                }
                else
                {
                    LoginFailedAttempts.TryRemove(email, out _);
                }
            }

            IResult RecordLoginFailure()
            {
                LoginFailedAttempts.AddOrUpdate(
                    email,
                    _ => (1, now),
                    (_, existing) =>
                    {
                        if (now - existing.WindowStart >= TimeSpan.FromSeconds(60))
                        {
                            return (1, now);
                        }
                        return (existing.FailedCount + 1, existing.WindowStart);
                    });

                if (LoginFailedAttempts.TryGetValue(email, out var updated) && updated.FailedCount >= 5 && (now - updated.WindowStart < TimeSpan.FromSeconds(60)))
                {
                    return Results.Json(new
                    {
                        error = "TOO_MANY_LOGIN_ATTEMPTS",
                        message = "Tài khoản tạm khóa 1 phút do đăng nhập sai nhiều lần."
                    }, statusCode: StatusCodes.Status429TooManyRequests);
                }

                return Results.Unauthorized();
            }

            // 2. Demo Session Login Check (SEC-HIGH-01)
            bool isDemoRequest = req.Password == "__DEMO_SESSION__" || !string.IsNullOrWhiteSpace(req.DemoProfile);
            if (isDemoRequest)
            {
                if (email != "son.tran@studxy.vn" && email != "minh.anh@studxy.vn")
                {
                    return RecordLoginFailure();
                }

                User? demoUser = null;
                if (email == "son.tran@studxy.vn")
                {
                    demoUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "son.tran@studxy.vn" || u.Id == SeedDataInitializer.DefaultUserId);
                }
                else if (email == "minh.anh@studxy.vn")
                {
                    demoUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "minh.anh@studxy.vn");
                    if (demoUser == null)
                    {
                        demoUser = new User
                        {
                            Id = Guid.NewGuid(),
                            Email = "minh.anh@studxy.vn",
                            PasswordHash = PasswordHashUtility.HashPassword(Guid.NewGuid().ToString("N")),
                            FullName = "Minh Anh",
                            Role = "student",
                            AuthProvider = "local",
                            TokenVersion = 1,
                            StreakDays = 0,
                            OnboardingStep = 1,
                            IsActivated = true
                        };
                        db.Users.Add(demoUser);
                        ProvisionPersonalWorkspaceDefaults(db, demoUser);
                        await db.SaveChangesAsync();
                    }
                }

                if (demoUser == null)
                {
                    return RecordLoginFailure();
                }

                LoginFailedAttempts.TryRemove(email, out _);

                var (demoAccess, demoRefresh, _) = await rtr.IssueInitialTokenPairAsync(
                    demoUser,
                    http.Request.Headers.UserAgent.ToString(),
                    http.Connection.RemoteIpAddress?.ToString());

                AppendRefreshTokenCookie(http, demoRefresh);

                return Results.Ok(new
                {
                    accessToken = demoAccess,
                    refreshToken = demoRefresh,
                    user = ToUserAuthDto(demoUser)
                });
            }

            // 3. Normal password authentication
            if (string.IsNullOrWhiteSpace(req.Password))
            {
                return RecordLoginFailure();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !PasswordHashUtility.VerifyPassword(req.Password, user.PasswordHash))
            {
                return RecordLoginFailure();
            }

            // Login successful -> clear failed attempt counter
            LoginFailedAttempts.TryRemove(email, out _);

            if (PasswordHashUtility.NeedsRehash(user.PasswordHash))
            {
                user.PasswordHash = PasswordHashUtility.HashPassword(req.Password);
                user.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
            }

            var (accessToken, rawRefresh, _) = await rtr.IssueInitialTokenPairAsync(
                user,
                http.Request.Headers.UserAgent.ToString(),
                http.Connection.RemoteIpAddress?.ToString());

            AppendRefreshTokenCookie(http, rawRefresh);

            return Results.Ok(new
            {
                accessToken,
                refreshToken = rawRefresh,
                user = ToUserAuthDto(user)
            });
        }).AllowAnonymous();

        auth.MapPost("/google", async (
            GoogleAuthRequest? req,
            HttpContext http,
            SwmsDbContext db,
            RefreshTokenRotationService rtr,
            IGoogleIdTokenValidator googleValidator,
            CancellationToken ct) =>
        {
            if (!googleValidator.IsConfigured)
            {
                return Results.Json(new
                {
                    error = "Đăng nhập Google chưa được cấu hình trên máy chủ (thiếu GOOGLE_CLIENT_ID).",
                    code = "GOOGLE_AUTH_NOT_CONFIGURED"
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            var idToken = req?.Credential?.Trim();
            if (string.IsNullOrWhiteSpace(idToken))
            {
                idToken = req?.IdToken?.Trim();
            }
            if (string.IsNullOrWhiteSpace(idToken))
            {
                return Results.BadRequest(new { error = "Thiếu Google credential token." });
            }

            var payload = await googleValidator.ValidateAsync(idToken, ct);
            if (payload == null)
            {
                return Results.Json(new
                {
                    error = "Token xác thực Google không hợp lệ, đã hết hạn hoặc email chưa được xác minh."
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var emailLock = RegistrationEmailLocks.GetOrAdd(payload.Email, _ => new SemaphoreSlim(1, 1));
            await emailLock.WaitAsync(ct);
            try
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleSub == payload.Subject, ct);
                bool isNewUser = false;
                Guid? personalWorkspaceCourseId = null;

                if (user == null)
                {
                    user = await db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email, ct);
                    if (user != null)
                    {
                        if (!string.IsNullOrEmpty(user.GoogleSub) &&
                            !string.Equals(user.GoogleSub, payload.Subject, StringComparison.Ordinal))
                        {
                            return Results.Conflict(new { error = "Email này đã liên kết với một tài khoản Google khác." });
                        }

                        user.GoogleSub = payload.Subject;
                        user.AuthProvider = PasswordHashUtility.HasLocalPassword(user.PasswordHash) ? "local+google" : "google";
                        if (!string.IsNullOrWhiteSpace(payload.AvatarUrl))
                        {
                            user.AvatarUrl = payload.AvatarUrl;
                        }
                        if (string.IsNullOrWhiteSpace(user.FullName) || user.FullName == "Học viên SWMS")
                        {
                            user.FullName = payload.FullName;
                        }
                        user.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                    else
                    {
                        isNewUser = true;
                        user = new User
                        {
                            Id = Guid.NewGuid(),
                            Email = payload.Email,
                            PasswordHash = PasswordHashUtility.ExternalOnlySentinel,
                            FullName = payload.FullName,
                            Role = "student",
                            AuthProvider = "google",
                            GoogleSub = payload.Subject,
                            AvatarUrl = payload.AvatarUrl,
                            TokenVersion = 1,
                            StreakDays = 1,
                            OnboardingStep = 1,
                            IsActivated = false
                        };
                        db.Users.Add(user);
                        personalWorkspaceCourseId = ProvisionPersonalWorkspaceDefaults(db, user).Id;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(payload.AvatarUrl) && user.AvatarUrl != payload.AvatarUrl)
                    {
                        user.AvatarUrl = payload.AvatarUrl;
                        user.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                }

                await db.SaveChangesAsync(ct);

                var (accessToken, rawRefresh, _) = await rtr.IssueInitialTokenPairAsync(
                    user,
                    http.Request.Headers.UserAgent.ToString(),
                    http.Connection.RemoteIpAddress?.ToString());

                AppendRefreshTokenCookie(http, rawRefresh);

                return Results.Ok(new
                {
                    accessToken,
                    refreshToken = rawRefresh,
                    isNewUser,
                    personalWorkspaceCourseId,
                    user = ToUserAuthDto(user)
                });
            }
            finally
            {
                emailLock.Release();
            }
        }).AllowAnonymous();

        auth.MapPost("/change-password", async (
            ChangePasswordRequest? req,
            HttpContext http,
            SwmsDbContext db,
            RefreshTokenRotationService rtr) =>
        {
            if (req == null || !IsStrongPassword(req.NewPassword))
            {
                return Results.BadRequest(new
                {
                    error = "WEAK_PASSWORD",
                    message = "Mật khẩu phải có ít nhất 6 ký tự, bao gồm cả chữ cái và chữ số."
                });
            }

            var userId = ResolveUserId(http);
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Results.NotFound();

            if (PasswordHashUtility.HasLocalPassword(user.PasswordHash))
            {
                if (string.IsNullOrWhiteSpace(req.CurrentPassword) ||
                    !PasswordHashUtility.VerifyPassword(req.CurrentPassword, user.PasswordHash))
                {
                    return Results.BadRequest(new { error = "Mật khẩu hiện tại không chính xác." });
                }
            }

            user.PasswordHash = PasswordHashUtility.HashPassword(req.NewPassword);
            user.AuthProvider = !string.IsNullOrEmpty(user.GoogleSub) ? "local+google" : "local";
            user.TokenVersion += 1;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            var activeTokens = await db.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();
            foreach (var t in activeTokens)
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTimeOffset.UtcNow;
            }
            await db.SaveChangesAsync();

            var (accessToken, rawRefresh, _) = await rtr.IssueInitialTokenPairAsync(
                user,
                http.Request.Headers.UserAgent.ToString(),
                http.Connection.RemoteIpAddress?.ToString());
            AppendRefreshTokenCookie(http, rawRefresh);

            return Results.Ok(new
            {
                message = "Đã cập nhật mật khẩu và làm mới phiên đăng nhập an toàn.",
                accessToken,
                user = ToUserAuthDto(user)
            });
        });

        auth.MapPost("/refresh", async (
            RefreshRequest? req,
            HttpContext http,
            RefreshTokenRotationService rtr) =>
        {
            var rawToken = http.Request.Cookies["swms_refresh_token"] ?? req?.RefreshToken;
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return Results.Unauthorized();
            }

            var outcome = await rtr.RotateRefreshTokenAsync(rawToken);
            if (!outcome.Succeeded)
            {
                http.Response.Cookies.Delete("swms_refresh_token", new CookieOptions { Path = "/api/v1/auth/refresh" });
                return Results.Json(new
                {
                    error = outcome.ErrorMessage,
                    breachDetected = outcome.IsBreachDetected,
                    userTokenVersion = outcome.User?.TokenVersion
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            AppendRefreshTokenCookie(http, outcome.RefreshToken!);
            if (outcome.IsGraceWindowRecovery)
            {
                http.Response.Headers["X-SWMS-Grace-Recovery"] = "true";
            }

            return Results.Ok(new
            {
                accessToken = outcome.AccessToken,
                refreshToken = outcome.RefreshToken,
                graceWindowRecovered = outcome.IsGraceWindowRecovery,
                user = ToUserAuthDto(outcome.User!)
            });
        }).AllowAnonymous();

        auth.MapPost("/logout", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.TokenVersion += 1;
                user.UpdatedAt = DateTimeOffset.UtcNow;
                var activeTokens = await db.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();
                foreach (var t in activeTokens)
                {
                    t.IsRevoked = true;
                    t.RevokedAt = DateTimeOffset.UtcNow;
                }
                await db.SaveChangesAsync();
            }

            http.Response.Cookies.Delete("swms_refresh_token", new CookieOptions { Path = "/api/v1/auth/refresh" });
            return Results.Ok(new { message = "Đã đăng xuất và vô hiệu hóa toàn bộ phiên." });
        });

        auth.MapGet("/me", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Results.NotFound();
            return Results.Ok(ToUserAuthDto(user));
        });
    }

    private static void AppendRefreshTokenCookie(HttpContext http, string rawRefresh)
    {
        http.Response.Cookies.Append("swms_refresh_token", rawRefresh, new CookieOptions
        {
            HttpOnly = true,
            Secure = http.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/auth/refresh",
            Expires = DateTimeOffset.UtcNow.AddDays(14)
        });
    }

    // ========================================================================
    // 2. WORKSPACE DASHBOARD, ONBOARDING, QUOTAS & HYBRID SEARCH
    // ========================================================================
    private static void MapWorkspaceEndpoints(RouteGroupBuilder api)
    {
        var ws = api.MapGroup("/workspace");

        ws.MapGet("/dashboard", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId)
                       ?? await db.Users.AsNoTracking().FirstAsync(u => u.Id == SeedDataInitializer.DefaultUserId);

            var now = DateTimeOffset.UtcNow;
            var allCards = await db.Flashcards
                .AsNoTracking()
                .Where(f => f.UserId == user.Id && !f.IsArchived)
                .Select(f => new { f.State, f.DueAt })
                .ToListAsync();

            var dueCards = allCards.Where(f => f.DueAt <= now).ToList();
            var newCount = dueCards.Count(f => f.State == 0);
            var learningCount = dueCards.Count(f => f.State is 1 or 3);
            var reviewCount = dueCards.Count(f => f.State == 2);

            var enrolledCourseIds = await db.CourseEnrollments
                .AsNoTracking()
                .Where(e => e.UserId == user.Id)
                .Select(e => e.CourseId)
                .ToListAsync();

            var rawOwnedCourses = await db.Courses
                .AsNoTracking()
                .Where(c => c.InstructorId == user.Id ||
                            enrolledCourseIds.Contains(c.Id) ||
                            (c.Visibility == "public" && c.IsPublished))
                .ToListAsync();
            var ownedCourses = rawOwnedCourses.OrderByDescending(c => c.UpdatedAt).ToList();
            var ownedCourseIds = ownedCourses.Select(c => c.Id).ToHashSet();

            var ownedChapters = await db.Chapters
                .AsNoTracking()
                .Where(ch => ownedCourseIds.Contains(ch.CourseId))
                .Select(ch => new { ch.Id, ch.CourseId })
                .ToListAsync();
            var chapterToCourseMap = ownedChapters.ToDictionary(ch => ch.Id, ch => ch.CourseId);
            var ownedChapterIds = chapterToCourseMap.Keys.ToHashSet();

            var rawLessons = await db.Lessons
                .AsNoTracking()
                .Where(l => ownedChapterIds.Contains(l.ChapterId))
                .OrderBy(l => l.SortOrder)
                .Select(l => new
                {
                    l.Id,
                    l.ChapterId,
                    l.Title,
                    l.Slug,
                    l.CurrentVersion,
                    l.UpdatedAt,
                    l.ContentAstJson
                })
                .ToListAsync();

            var lessons = rawLessons.Select(l =>
            {
                var cId = chapterToCourseMap.GetValueOrDefault(l.ChapterId);
                var (srcDocId, startP, endP) = CourseGenerationService.ExtractLessonMetadata(l.ContentAstJson);
                return new
                {
                    l.Id,
                    l.ChapterId,
                    courseId = cId,
                    sourceDocumentId = srcDocId,
                    startPage = startP,
                    endPage = endP,
                    l.Title,
                    l.Slug,
                    l.CurrentVersion,
                    l.UpdatedAt,
                    itemType = "lesson"
                };
            }).ToList();

            var syllabusCourses = OrderCoursesForUser(
                ownedCourses.Where(c => !c.Slug.StartsWith("personal-workspace-", StringComparison.OrdinalIgnoreCase)),
                user.Id);

            var courses = await CourseGenerationService.BuildCourseSummariesBatchAsync(
                db,
                syllabusCourses,
                includeLessonMarkdown: false,
                userId: user.Id);

            var rawDocuments = await db.Documents
                .AsNoTracking()
                .Where(d => d.UploaderId == user.Id)
                .Select(d => new
                {
                    d.Id,
                    d.CourseId,
                    d.Title,
                    d.TotalPages,
                    d.FileSizeBytes,
                    d.OcrStatus,
                    d.UpdatedAt
                })
                .ToListAsync();
            var documents = rawDocuments
                .OrderByDescending(d => d.UpdatedAt)
                .Select(d => new
                {
                    d.Id,
                    d.CourseId,
                    d.Title,
                    d.TotalPages,
                    d.FileSizeBytes,
                    d.OcrStatus,
                    d.UpdatedAt,
                    itemType = "document"
                })
                .ToList();

            var rawNotebooks = await db.Notebooks
                .AsNoTracking()
                .Where(n => n.UserId == user.Id)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Version,
                    n.PrimarySourceLessonId,
                    n.PrimarySourceDocumentId,
                    n.UpdatedAt
                })
                .ToListAsync();
            var notebooks = rawNotebooks
                .OrderByDescending(n => n.UpdatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Version,
                    n.PrimarySourceLessonId,
                    n.PrimarySourceDocumentId,
                    n.UpdatedAt,
                    itemType = "notebook"
                })
                .ToList();

            var courseMetaById = ownedCourses.ToDictionary(c => c.Id);
            var lessonCourseMeta = rawLessons
                .Where(l => chapterToCourseMap.ContainsKey(l.ChapterId))
                .ToDictionary(
                    l => l.Id,
                    l => courseMetaById.GetValueOrDefault(chapterToCourseMap[l.ChapterId]));
            var docCourseMeta = rawDocuments
                .Where(d => d.CourseId != Guid.Empty)
                .ToDictionary(d => d.Id, d => d.CourseId);

            var rawQuizzes = await db.PracticeQuizzes
                .AsNoTracking()
                .Where(q => q.UserId == user.Id || q.UserId == SeedDataInitializer.DefaultUserId)
                .Select(q => new
                {
                    q.Id,
                    q.UserId,
                    q.Title,
                    q.DocumentId,
                    q.LessonId,
                    q.TotalItems,
                    q.CorrectItems,
                    q.Status,
                    q.StartedAt,
                    q.CompletedAt
                })
                .ToListAsync();
            var quizzes = rawQuizzes
                .Select(q =>
                {
                    Guid? qCourseId = null;
                    bool isPublicDocumentQuiz = false;
                    if (q.LessonId.HasValue && lessonCourseMeta.TryGetValue(q.LessonId.Value, out var lc) && lc != null)
                    {
                        qCourseId = lc.Id;
                        isPublicDocumentQuiz = lc.Slug.StartsWith("document-course-", StringComparison.OrdinalIgnoreCase) &&
                                               lc.Visibility == "public" && lc.IsPublished;
                    }
                    else if (q.DocumentId.HasValue && docCourseMeta.TryGetValue(q.DocumentId.Value, out var dc))
                    {
                        qCourseId = dc;
                    }

                    bool accessible = q.UserId == user.Id || isPublicDocumentQuiz;
                    return new { Quiz = q, CourseId = qCourseId, Accessible = accessible };
                })
                .Where(x => x.Accessible)
                .OrderByDescending(x => x.Quiz.StartedAt)
                .Select(x => new
                {
                    x.Quiz.Id,
                    x.Quiz.Title,
                    x.Quiz.DocumentId,
                    x.Quiz.LessonId,
                    courseId = x.CourseId,
                    x.Quiz.TotalItems,
                    x.Quiz.CorrectItems,
                    x.Quiz.Status,
                    x.Quiz.StartedAt,
                    x.Quiz.CompletedAt
                })
                .ToList();

            var tags = await db.Tags
                .Where(t => t.UserId == user.Id)
                .ToListAsync();

            var tagCounts = await db.FlashcardTags
                .GroupBy(ft => ft.TagId)
                .Select(g => new { TagId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TagId, x => x.Count);

            var quota = await GetQuotaSummaryAsync(db, user.Id);

            return Results.Ok(new
            {
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.StreakDays,
                    user.OnboardingStep,
                    user.IsActivated
                },
                reviewStation = new
                {
                    totalDue = dueCards.Count,
                    totalCards = allCards.Count,
                    newCount,
                    learningCount,
                    reviewCount,
                    estimatedMinutes = Math.Max(1, (int)Math.Ceiling(dueCards.Count * 0.5))
                },
                courses,
                quizzes,
                lessons,
                documents,
                notebooks,
                tags = tags.Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.ColorKey,
                    cardCount = tagCounts.GetValueOrDefault(t.Id, 0)
                }),
                quota
            });
        });

        ws.MapPost("/onboarding", async (OnboardingStepRequest req, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Results.NotFound();

            user.OnboardingStep = Math.Clamp(Math.Max(user.OnboardingStep, req.Step), 1, 4);
            if (req.CompleteActivation == true || req.Step >= 4)
            {
                user.IsActivated = true;
                user.ActivatedAt ??= DateTimeOffset.UtcNow;
                user.OnboardingStep = 4;
            }
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                user.OnboardingStep,
                user.IsActivated,
                user.ActivatedAt,
                user.StreakDays
            });
        });

        ws.MapGet("/quotas", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var summary = await GetQuotaSummaryAsync(db, userId);
            return Results.Ok(summary);
        });

        ws.MapGet("/search", async (string? q, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var query = VietnameseTextNormalizer.Unaccent(q ?? string.Empty);

            var enrolledCourseIds = await db.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToHashSetAsync();

            var lessonRows = await (
                from l in db.Lessons
                join ch in db.Chapters on l.ChapterId equals ch.Id
                join c in db.Courses on ch.CourseId equals c.Id
                select new
                {
                    Lesson = l,
                    ChapterTitle = ch.Title,
                    CourseId = c.Id,
                    CourseTitle = c.Title,
                    c.InstructorId,
                    c.Visibility,
                    c.IsPublished
                }
            ).ToListAsync();

            var accessibleLessons = lessonRows
                .Where(r => r.InstructorId == userId ||
                            enrolledCourseIds.Contains(r.CourseId) ||
                            (r.Visibility == "public" && r.IsPublished) ||
                            userId == SeedDataInitializer.DefaultUserId)
                .ToList();

            var lessonToCourse = lessonRows.ToDictionary(r => r.Lesson.Id, r => r.CourseId);
            var docs = await db.Documents.Where(d => d.UploaderId == userId).ToListAsync();
            var docToCourse = docs.Where(d => d.CourseId != Guid.Empty).ToDictionary(d => d.Id, d => d.CourseId);
            var notes = await db.Notebooks.Where(n => n.UserId == userId).ToListAsync();
            var cards = await db.Flashcards.Where(f => f.UserId == userId && !f.IsArchived).ToListAsync();
            var tags = await db.Tags.Where(t => t.UserId == userId).ToListAsync();

            bool Matches(string? text)
            {
                if (string.IsNullOrWhiteSpace(query)) return true;
                var norm = VietnameseTextNormalizer.Unaccent(text);
                if (norm.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;
                var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return tokens.Length > 0 && tokens.All(tok => norm.Contains(tok, StringComparison.OrdinalIgnoreCase));
            }

            var results = new List<object>();

            foreach (var r in accessibleLessons.Where(r => Matches(r.Lesson.Title) || Matches(r.Lesson.PlainText)))
            {
                var l = r.Lesson;
                results.Add(new
                {
                    id = l.Id,
                    courseId = (Guid?)r.CourseId,
                    type = "lesson",
                    title = l.Title,
                    subtitle = $"{r.CourseTitle} • {r.ChapterTitle}",
                    snippet = l.PlainText.Length > 140 ? l.PlainText[..140] + "..." : l.PlainText
                });
            }

            foreach (var d in docs.Where(d => Matches(d.Title) || Matches(d.PlainText)))
            {
                results.Add(new
                {
                    id = d.Id,
                    courseId = d.CourseId != Guid.Empty ? (Guid?)d.CourseId : null,
                    type = "document",
                    title = d.Title,
                    subtitle = $"Tài liệu PDF • {d.TotalPages} trang",
                    snippet = d.PlainText ?? ""
                });
            }

            foreach (var n in notes.Where(n => Matches(n.Title) || Matches(n.ContentAstJson)))
            {
                Guid? nbCourseId = null;
                if (n.PrimarySourceLessonId.HasValue && lessonToCourse.TryGetValue(n.PrimarySourceLessonId.Value, out var lcId))
                {
                    nbCourseId = lcId;
                }
                else if (n.PrimarySourceDocumentId.HasValue && docToCourse.TryGetValue(n.PrimarySourceDocumentId.Value, out var dcId))
                {
                    nbCourseId = dcId;
                }
                results.Add(new
                {
                    id = n.Id,
                    courseId = nbCourseId,
                    type = "notebook",
                    title = n.Title,
                    subtitle = $"Ghi chú tổng hợp • v{n.Version}",
                    snippet = "Sổ tay ghi chú PKM"
                });
            }

            foreach (var c in cards.Where(c => Matches(c.FrontAstJson) || Matches(c.BackAstJson) || Matches(c.SourceTitleSnapshot)))
            {
                Guid? cardCourseId = null;
                if (c.LessonId.HasValue && lessonToCourse.TryGetValue(c.LessonId.Value, out var clc))
                {
                    cardCourseId = clc;
                }
                results.Add(new
                {
                    id = c.Id,
                    courseId = cardCourseId,
                    type = "flashcard",
                    title = ExtractTextFromAstOrJson(c.FrontAstJson),
                    subtitle = $"Thẻ FSRS ({c.CardType}) • {c.SourceTitleSnapshot}",
                    snippet = ExtractTextFromAstOrJson(c.BackAstJson)
                });
            }

            foreach (var t in tags.Where(t => Matches(t.Name)))
            {
                results.Add(new
                {
                    id = t.Id,
                    courseId = (Guid?)null,
                    type = "tag",
                    title = $"#{t.Name}",
                    subtitle = "Thẻ phân loại Hàng đợi Ôn tập",
                    snippet = $"Lọc hàng đợi FSRS theo #{t.Name}"
                });
            }

            return Results.Ok(new { query = q, results });
        });
    }

    private static async Task<object> GetQuotaSummaryAsync(SwmsDbContext db, Guid userId)
    {
        var quota = await db.UserQuotas.FirstOrDefaultAsync(q => q.UserId == userId)
                    ?? new UserQuota { UserId = userId };

        var todayUtc = DateTimeOffset.UtcNow.Date;
        var rawEntries = await db.AiUsageLedgers
            .Where(l => l.UserId == userId)
            .ToListAsync();
        var todayEntries = rawEntries
            .Where(l => l.CreatedAt >= todayUtc)
            .ToList();

        var creditsUsed = todayEntries.Sum(l => l.CreditsCharged);
        var tokensUsed = todayEntries.Sum(l => l.TotalTokens);
        var clozeCount = await db.Flashcards.CountAsync(f => f.UserId == userId && f.CardType == "cloze");
        var docCount = await db.Documents.CountAsync(d => d.UploaderId == userId);

        return new
        {
            tier = quota.Tier,
            aiCreditsUsed = creditsUsed,
            aiCreditsLimit = quota.DailyAiCreditLimit,
            aiTokensUsed = tokensUsed,
            aiTokensLimit = quota.DailyAiTokenLimit,
            storageBytesUsed = quota.CurrentStorageBytes,
            storageBytesLimit = quota.StorageLimitBytes,
            storageMbUsed = Math.Round(quota.CurrentStorageBytes / 1048576.0, 1),
            storageMbLimit = Math.Round(quota.StorageLimitBytes / 1048576.0, 1),
            documentCount = docCount,
            instantClozeCreatedFree = clozeCount,
            breakdown = new
            {
                qaGenerationCredits = todayEntries.Where(e => e.Feature == "qa_generation").Sum(e => e.CreditsCharged),
                explainConceptCredits = todayEntries.Where(e => e.Feature == "explain_concept").Sum(e => e.CreditsCharged),
                essayFeedbackCredits = todayEntries.Where(e => e.Feature == "essay_feedback").Sum(e => e.CreditsCharged)
            }
        };
    }

    // ========================================================================
    // 3. LESSONS, VERSION PUBLICATION (3-TIER SELF-HEALING) & DOCUMENTS
    // ========================================================================
    private static void MapLessonAndDocumentEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/courses", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var enrolledIds = await db.CourseEnrollments
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var rawUserCourses = await db.Courses
                .AsNoTracking()
                .Where(c => (c.InstructorId == userId ||
                             enrolledIds.Contains(c.Id) ||
                             (c.Visibility == "public" && c.IsPublished))
                            && !c.Slug.StartsWith("personal-workspace-"))
                .ToListAsync();

            var userCourses = OrderCoursesForUser(rawUserCourses, userId);
            var result = await CourseGenerationService.BuildCourseSummariesBatchAsync(
                db,
                userCourses,
                includeLessonMarkdown: false,
                userId: userId);
            return Results.Ok(result);
        });

        api.MapGet("/courses/{id:guid}", async (Guid id, bool? includeMarkdown, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
            {
                return Results.NotFound(new { error = "Không tìm thấy khóa học." });
            }
            if (!IsOwnerOrDefaultSeededAccess(course.InstructorId, userId) && !(course.Visibility == "public" && course.IsPublished))
            {
                var isEnrolled = await db.CourseEnrollments.AnyAsync(e => e.UserId == userId && e.CourseId == id);
                if (!isEnrolled && userId != SeedDataInitializer.DefaultUserId && course.InstructorId != SeedDataInitializer.DefaultUserId)
                {
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }
            }

            var summary = await CourseGenerationService.BuildCourseSummaryAsync(db, course, includeLessonMarkdown: includeMarkdown == true, userId: userId);
            return Results.Ok(summary);
        });

        api.MapPost("/courses/{id:guid}/enroll", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
            {
                return Results.NotFound(new { error = "Không tìm thấy khóa học." });
            }
            var existing = await db.CourseEnrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == id);
            if (existing == null)
            {
                try
                {
                    db.CourseEnrollments.Add(new CourseEnrollment
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        CourseId = id,
                        Role = "student",
                        EnrolledAt = DateTimeOffset.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Idempotent: concurrent request already enrolled user
                }
            }
            return Results.Ok(new { enrolled = true, courseId = id });
        });

        api.MapPost("/courses/sync-from-documents", async (IConfiguration config, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var rootPath = DocumentCourseSyncService.ResolveDocumentRootPath(config);
            var report = await DocumentCourseSyncService.SyncAllFromDocumentsAsync(db, rootPath, useSnapshotCache: false);

            var enrolledIds = await db.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var refreshed = await db.Courses
                .Where(c => (c.InstructorId == userId ||
                             enrolledIds.Contains(c.Id) ||
                             (c.Visibility == "public" && c.IsPublished) ||
                             userId == SeedDataInitializer.DefaultUserId)
                            && !c.Slug.StartsWith("personal-workspace-"))
                .ToListAsync();

            var ordered = OrderCoursesForUser(refreshed, userId);
            var summaries = await CourseGenerationService.BuildCourseSummariesBatchAsync(
                db,
                ordered,
                includeLessonMarkdown: false,
                userId: userId);

            return Results.Ok(new
            {
                report.SyncedCourses,
                report.CreatedCourses,
                report.UpdatedCourses,
                report.CreatedChapters,
                report.CreatedLessons,
                report.UpdatedLessons,
                report.CreatedQuizzes,
                report.UpdatedQuizzes,
                report.CreatedFlashcards,
                report.RootPath,
                report.ElapsedMs,
                courses = summaries
            });
        });

        api.MapGet("/document-assets/{courseFolder}/{*assetPath}", (
            string courseFolder,
            string? assetPath,
            IConfiguration config,
            HttpContext http) =>
        {
            var rawRequestPath = http.Request.Path.Value ?? string.Empty;
            if (rawRequestPath.Contains("..", StringComparison.Ordinal) ||
                rawRequestPath.Contains("%2e%2e", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(assetPath))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var rootPath = DocumentCourseSyncService.ResolveDocumentRootPath(config);
            if (!DocumentCourseSyncService.TryResolveSafeAssetFile(
                    rootPath,
                    courseFolder,
                    assetPath,
                    out var fullPath,
                    out var contentType,
                    out _,
                    out var failureStatusCode,
                    out var failureMessage))
            {
                return failureStatusCode == StatusCodes.Status404NotFound
                    ? Results.NotFound(new { error = failureMessage })
                    : Results.Json(new { error = failureMessage }, statusCode: failureStatusCode);
            }

            var isStreamable = contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ||
                               contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);

            return Results.File(fullPath, contentType, enableRangeProcessing: isStreamable);
        }).AllowAnonymous();

        api.MapGet("/courses/template-zip", () =>
        {
            var zipBytes = CourseWizardAndImportService.GenerateTemplateZip();
            return Results.File(zipBytes, "application/zip", "swm-course-template.zip");
        }).AllowAnonymous();

        api.MapPost("/courses/import-zip", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            if (!http.Request.HasFormContentType)
            {
                return Results.BadRequest(new { error = "Yêu cầu phải là multipart form data chứa tệp .zip." });
            }
            var form = await http.Request.ReadFormAsync();
            var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "Không tìm thấy tệp .zip được tải lên." });
            }
            try
            {
                using var stream = file.OpenReadStream();
                var summary = await CourseWizardAndImportService.ImportCourseFromZipAsync(db, stream, userId);
                return Results.Ok(summary);
            }
            catch (InvalidDataException)
            {
                return Results.BadRequest(new { error = "Tệp .zip không hợp lệ hoặc bị hỏng định dạng." });
            }
            catch (Exception)
            {
                return Results.Json(new { error = "Không thể nhập khóa học từ tệp .zip." }, statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        api.MapPost("/courses/generate-wizard/clarify", async (
            WizardClarifyRequest req,
            HttpContext http,
            GeminiProxyClient gemini) =>
        {
            var aiOverrides = CourseGenerationService.ExtractAiOverrides(http);
            var res = await CourseWizardAndImportService.GenerateClarifyQuestionsAsync(gemini, req.Topic, aiOverrides);
            return Results.Ok(res);
        });

        api.MapPost("/courses/generate-wizard/preview", async (
            WizardPreviewRequest req,
            HttpContext http,
            GeminiProxyClient gemini) =>
        {
            var aiOverrides = CourseGenerationService.ExtractAiOverrides(http);
            var res = await CourseWizardAndImportService.GenerateSyllabusPreviewAsync(gemini, req.Topic, req.Answers ?? new(), aiOverrides);
            return Results.Ok(res);
        });

        api.MapPost("/courses/generate-wizard/commit", async (
            WizardCommitRequest? req,
            HttpContext http,
            GeminiProxyClient gemini,
            SwmsDbContext db) =>
        {
            if (req == null || req.Chapters == null || req.Chapters.Count == 0)
            {
                return Results.BadRequest(new { error = "Danh sách chương không được để trống." });
            }

            var userId = ResolveUserId(http);
            var aiOverrides = CourseGenerationService.ExtractAiOverrides(http);
            var userLock = WizardCommitLocks.GetOrAdd(userId.ToString("N"), _ => new SemaphoreSlim(1, 1));
            await userLock.WaitAsync();
            try
            {
                var now = DateTimeOffset.UtcNow;
                var idempKey = req.IdempotencyKey.HasValue
                    ? $"idemp:{userId:N}:{req.IdempotencyKey.Value:N}"
                    : null;
                var dedupKey = $"recent:{userId:N}:{(req.Title ?? req.Topic ?? string.Empty).Trim().ToLowerInvariant()}";

                if (idempKey != null && WizardIdempotencyCache.TryGetValue(idempKey, out var cachedByIdemp))
                {
                    return Results.Ok(cachedByIdemp.Summary);
                }

                if (WizardIdempotencyCache.TryGetValue(dedupKey, out var cachedRecent) &&
                    (now - cachedRecent.CreatedAt).TotalSeconds <= 5)
                {
                    return Results.Ok(cachedRecent.Summary);
                }

                var summary = await CourseWizardAndImportService.CommitWizardCourseAsync(db, gemini, req, userId, aiOverrides);

                if (idempKey != null)
                {
                    WizardIdempotencyCache[idempKey] = (now, summary);
                }
                WizardIdempotencyCache[dedupKey] = (now, summary);

                return Results.Ok(summary);
            }
            finally
            {
                userLock.Release();
            }
        });

        api.MapPut("/courses/{id:guid}", async (
            Guid id,
            UpdateCourseRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return Results.NotFound(new { error = "Không tìm thấy khóa học." });
            if (!IsOwnerOrDefaultSeededAccess(course.InstructorId, userId)) return Results.StatusCode(StatusCodes.Status403Forbidden);

            if (!string.IsNullOrWhiteSpace(req.Title)) course.Title = req.Title.Trim();
            if (req.Description != null) course.Description = req.Description.Trim();
            if (!string.IsNullOrWhiteSpace(req.Visibility)) course.Visibility = req.Visibility;
            if (req.IsPublished.HasValue) course.IsPublished = req.IsPublished.Value;
            course.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            var summary = await CourseGenerationService.BuildCourseSummaryAsync(db, course);
            return Results.Ok(summary);
        });

        api.MapDelete("/courses/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return Results.NotFound(new { error = "Không tìm thấy khóa học." });
            if (!IsOwnerOrDefaultSeededAccess(course.InstructorId, userId)) return Results.StatusCode(StatusCodes.Status403Forbidden);

            var chapters = await db.Chapters.Where(ch => ch.CourseId == id).ToListAsync();
            var chIds = chapters.Select(ch => ch.Id).ToHashSet();
            var lessons = await db.Lessons.Where(l => chIds.Contains(l.ChapterId)).ToListAsync();
            var lesIds = lessons.Select(l => l.Id).ToHashSet();
            var versions = await db.LessonVersions.Where(lv => lesIds.Contains(lv.LessonId)).ToListAsync();
            var progresses = await db.UserLessonProgresses.Where(p => lesIds.Contains(p.LessonId)).ToListAsync();

            db.UserLessonProgresses.RemoveRange(progresses);
            db.LessonVersions.RemoveRange(versions);
            db.Lessons.RemoveRange(lessons);
            db.Chapters.RemoveRange(chapters);
            db.Courses.Remove(course);
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Đã xóa khóa học thành công." });
        });

        api.MapPost("/courses/{id:guid}/chapters", async (
            Guid id,
            CreateChapterRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return Results.NotFound(new { error = "Không tìm thấy khóa học." });
            if (!IsOwnerOrDefaultSeededAccess(course.InstructorId, userId))
                return Results.StatusCode(StatusCodes.Status403Forbidden);

            if (string.IsNullOrWhiteSpace(req.Title))
                return Results.BadRequest(new { error = "Tiêu đề chương không được để trống." });

            var maxSort = await db.Chapters.Where(c => c.CourseId == id).MaxAsync(c => (int?)c.SortOrder) ?? 0;
            var chapter = new Chapter
            {
                Id = Guid.NewGuid(),
                CourseId = id,
                Title = req.Title.Trim(),
                SortOrder = req.SortOrder ?? (maxSort + 1),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.Chapters.Add(chapter);
            await db.SaveChangesAsync();
            return Results.Ok(chapter);
        });

        api.MapPost("/courses/{id:guid}/publish", async (
            Guid id,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return Results.NotFound(new { error = "Không tìm thấy khóa học." });
            if (!IsOwnerOrDefaultSeededAccess(course.InstructorId, userId))
                return Results.StatusCode(StatusCodes.Status403Forbidden);

            PublishCourseRequest? req = null;
            if (http.Request.ContentLength is > 0)
            {
                try
                {
                    req = await http.Request.ReadFromJsonAsync<PublishCourseRequest>();
                }
                catch { }
            }

            if (req != null && !string.IsNullOrWhiteSpace(req.Visibility))
            {
                course.Visibility = req.Visibility.Trim();
                course.IsPublished = string.Equals(course.Visibility, "public", StringComparison.OrdinalIgnoreCase);
            }
            else if (req != null && req.IsPublished.HasValue)
            {
                course.IsPublished = req.IsPublished.Value;
                course.Visibility = course.IsPublished ? "public" : "private";
            }
            else
            {
                var isCurrentlyPublic = string.Equals(course.Visibility, "public", StringComparison.OrdinalIgnoreCase) || course.IsPublished;
                if (isCurrentlyPublic)
                {
                    course.Visibility = "private";
                    course.IsPublished = false;
                }
                else
                {
                    course.Visibility = "public";
                    course.IsPublished = true;
                }
            }

            course.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                courseId = course.Id,
                visibility = course.Visibility,
                isPublished = course.IsPublished,
                message = $"Trạng thái khóa học: {course.Visibility}"
            });
        });

        api.MapGet("/courses/{id:guid}/export-zip", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return Results.NotFound(new { error = "Không tìm thấy khóa học." });

            if (!IsOwnerOrDefaultSeededAccess(course.InstructorId, userId) && course.Visibility != "public")
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var zipBytes = await CourseWizardAndImportService.ExportCourseToZipAsync(db, id);
            var fileName = $"{course.Slug}.swm.zip";
            return Results.File(zipBytes, "application/zip", fileName);
        });

        api.MapGet("/courses/{id:guid}/progress", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return Results.NotFound(new { error = "Không tìm thấy khóa học." });
            if (!await CanReadCourseAsync(db, course, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var chapterIds = await db.Chapters.Where(c => c.CourseId == id).Select(c => c.Id).ToListAsync();
            var courseLessons = await db.Lessons.Where(l => chapterIds.Contains(l.ChapterId)).ToListAsync();
            var totalLessons = courseLessons.Count;
            var courseLessonIds = courseLessons.Select(l => l.Id).ToHashSet();

            var completed = await db.UserLessonProgresses
                .Where(p => p.UserId == userId && p.IsCompleted && courseLessonIds.Contains(p.LessonId))
                .ToListAsync();
            var completedLessonIds = completed.Select(p => p.LessonId).ToList();
            var completedLessons = completedLessonIds.Count;
            var percentCompleted = totalLessons > 0 ? (int)Math.Round((double)completedLessons * 100.0 / totalLessons) : 0;

            return Results.Ok(new
            {
                courseId = id,
                totalLessons,
                completedLessons,
                percentCompleted,
                completedLessonIds
            });
        });

        api.MapPut("/chapters/{id:guid}", async (
            Guid id,
            UpdateChapterRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var chapter = await db.Chapters.FirstOrDefaultAsync(ch => ch.Id == id);
            if (chapter == null) return Results.NotFound(new { error = "Không tìm thấy chương mục." });

            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == chapter.CourseId);
            if (course != null && !IsOwnerOrDefaultSeededAccess(course.InstructorId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            if (!string.IsNullOrWhiteSpace(req.Title)) chapter.Title = req.Title.Trim();
            if (req.SortOrder.HasValue) chapter.SortOrder = req.SortOrder.Value;
            chapter.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(chapter);
        });

        api.MapDelete("/chapters/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var chapter = await db.Chapters.FirstOrDefaultAsync(ch => ch.Id == id);
            if (chapter == null) return Results.NotFound(new { error = "Không tìm thấy chương mục." });

            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == chapter.CourseId);
            if (course != null && !IsOwnerOrDefaultSeededAccess(course.InstructorId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var lessons = await db.Lessons.Where(l => l.ChapterId == id).ToListAsync();
            var lesIds = lessons.Select(l => l.Id).ToHashSet();
            var versions = await db.LessonVersions.Where(lv => lesIds.Contains(lv.LessonId)).ToListAsync();
            var progresses = await db.UserLessonProgresses.Where(p => lesIds.Contains(p.LessonId)).ToListAsync();

            db.UserLessonProgresses.RemoveRange(progresses);
            db.LessonVersions.RemoveRange(versions);
            db.Lessons.RemoveRange(lessons);
            db.Chapters.Remove(chapter);
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Đã xóa chương mục thành công." });
        });

        api.MapPost("/chapters/{id:guid}/lessons", async (
            Guid id,
            CreateLessonRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var chapter = await db.Chapters.FirstOrDefaultAsync(ch => ch.Id == id);
            if (chapter == null) return Results.NotFound(new { error = "Không tìm thấy chương mục." });

            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == chapter.CourseId);
            if (course != null && !IsOwnerOrDefaultSeededAccess(course.InstructorId, userId))
                return Results.StatusCode(StatusCodes.Status403Forbidden);

            if (string.IsNullOrWhiteSpace(req.Title))
                return Results.BadRequest(new { error = "Tiêu đề bài học không được để trống." });

            var maxSort = await db.Lessons.Where(l => l.ChapterId == id).MaxAsync(l => (int?)l.SortOrder) ?? 0;
            var sortOrder = req.SortOrder ?? (maxSort + 1);

            var rawMd = !string.IsNullOrWhiteSpace(req.ContentMarkdown)
                ? req.ContentMarkdown
                : $"# {req.Title.Trim()}\n\nNội dung bài học đang được biên soạn.";

            var (astJson, plainText) = CourseWizardAndImportService.BuildMarkdownAstAndPlainText(
                chapter.CourseId, req.Title.Trim(), rawMd);

            var now = DateTimeOffset.UtcNow;
            var lesson = new Lesson
            {
                Id = Guid.NewGuid(),
                ChapterId = id,
                Title = req.Title.Trim(),
                Slug = $"les-{id.ToString("N")[..6]}-{Guid.NewGuid().ToString("N")[..6]}",
                SortOrder = sortOrder,
                CurrentVersion = 1,
                ContentAstJson = astJson,
                PlainText = plainText,
                ContentMarkdown = req.ContentMarkdown,
                VideoUrl = req.VideoUrl,
                EstimatedMinutes = req.EstimatedMinutes ?? 5,
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            db.Lessons.Add(lesson);
            db.LessonVersions.Add(new LessonVersion
            {
                Id = Guid.NewGuid(),
                LessonId = lesson.Id,
                VersionNumber = 1,
                ContentAstJson = astJson,
                PlainText = plainText,
                PublishedBy = userId,
                PublishedAt = now,
                ChangeSummary = "Khởi tạo bài học mới qua Course Studio"
            });

            await db.SaveChangesAsync();
            return Results.Ok(lesson);
        });

        api.MapPut("/lessons/{id:guid}", async (
            Guid id,
            UpdateLessonRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return Results.NotFound(new { error = "Không tìm thấy bài học." });

            var chapter = await db.Chapters.FirstOrDefaultAsync(ch => ch.Id == lesson.ChapterId);
            if (chapter != null)
            {
                var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == chapter.CourseId);
                if (course != null && !IsOwnerOrDefaultSeededAccess(course.InstructorId, userId))
                {
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }
            }

            var now = DateTimeOffset.UtcNow;
            if (!string.IsNullOrWhiteSpace(req.Title)) lesson.Title = req.Title.Trim();
            if (req.SortOrder.HasValue) lesson.SortOrder = req.SortOrder.Value;
            if (req.VideoUrl != null) lesson.VideoUrl = req.VideoUrl;
            if (req.EstimatedMinutes.HasValue) lesson.EstimatedMinutes = req.EstimatedMinutes.Value;

            if (!string.IsNullOrWhiteSpace(req.ContentMarkdown))
            {
                lesson.ContentMarkdown = req.ContentMarkdown;
                var courseId = chapter?.CourseId ?? Guid.Empty;
                var (astJson, plainText) = CourseWizardAndImportService.BuildMarkdownAstAndPlainText(
                    courseId, lesson.Title, req.ContentMarkdown);

                lesson.ContentAstJson = astJson;
                lesson.PlainText = plainText;
                lesson.CurrentVersion += 1;

                db.LessonVersions.Add(new LessonVersion
                {
                    Id = Guid.NewGuid(),
                    LessonId = lesson.Id,
                    VersionNumber = lesson.CurrentVersion,
                    ContentAstJson = astJson,
                    PlainText = plainText,
                    PublishedBy = userId,
                    PublishedAt = now,
                    ChangeSummary = req.ChangeSummary ?? "Cập nhật qua Markdown Studio Editor"
                });
            }
            else if (!string.IsNullOrWhiteSpace(req.ContentAstJson))
            {
                lesson.ContentAstJson = req.ContentAstJson;
                lesson.PlainText = req.PlainText ?? lesson.PlainText;
                lesson.CurrentVersion += 1;

                db.LessonVersions.Add(new LessonVersion
                {
                    Id = Guid.NewGuid(),
                    LessonId = lesson.Id,
                    VersionNumber = lesson.CurrentVersion,
                    ContentAstJson = req.ContentAstJson,
                    PlainText = lesson.PlainText,
                    PublishedBy = userId,
                    PublishedAt = now,
                    ChangeSummary = req.ChangeSummary ?? "Cập nhật qua In-Place Studio Editor"
                });
            }

            lesson.UpdatedAt = now;
            await db.SaveChangesAsync();

            return Results.Ok(lesson);
        });

        api.MapDelete("/lessons/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return Results.NotFound(new { error = "Không tìm thấy bài học." });

            var chapter = await db.Chapters.FirstOrDefaultAsync(ch => ch.Id == lesson.ChapterId);
            if (chapter != null)
            {
                var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == chapter.CourseId);
                if (course != null && !IsOwnerOrDefaultSeededAccess(course.InstructorId, userId))
                {
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }
            }

            var versions = await db.LessonVersions.Where(lv => lv.LessonId == id).ToListAsync();
            var progresses = await db.UserLessonProgresses.Where(p => p.LessonId == id).ToListAsync();

            db.UserLessonProgresses.RemoveRange(progresses);
            db.LessonVersions.RemoveRange(versions);
            db.Lessons.Remove(lesson);
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Đã xóa bài học thành công." });
        });

        api.MapPost("/lessons/{id:guid}/complete", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return Results.NotFound(new { error = "Không tìm thấy bài học." });

            var chapter = await db.Chapters.FirstOrDefaultAsync(ch => ch.Id == lesson.ChapterId);
            var course = chapter != null ? await db.Courses.FirstOrDefaultAsync(c => c.Id == chapter.CourseId) : null;
            if (course != null && !await CanReadCourseAsync(db, course, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            // Rate limit distinct lesson completions per user (max 3 distinct lessons in 2 seconds)
            var now = DateTimeOffset.UtcNow;
            var userHistory = UserLessonCompleteTimes.GetOrAdd(userId, _ => new List<(Guid, DateTimeOffset)>());
            lock (userHistory)
            {
                userHistory.RemoveAll(entry => now - entry.CompletedAt > TimeSpan.FromSeconds(2));
                var distinctLessonsInWindow = userHistory.Select(e => e.LessonId).Distinct().Count();
                if (!userHistory.Any(e => e.LessonId == id) && distinctLessonsInWindow >= 3)
                {
                    return Results.Json(new
                    {
                        error = "TOO_MANY_LESSON_COMPLETIONS",
                        message = "Tốc độ hoàn thành bài học quá nhanh. Vui lòng thử lại sau 2 giây."
                    }, statusCode: StatusCodes.Status429TooManyRequests);
                }
                userHistory.Add((id, now));
            }

            // Check enrollment for public courses
            if (course != null && course.Visibility == "public" && course.InstructorId != userId && userId != SeedDataInitializer.DefaultUserId)
            {
                var isEnrolled = await db.CourseEnrollments.AnyAsync(e => e.CourseId == course.Id && e.UserId == userId);
                if (!isEnrolled)
                {
                    return Results.Json(new
                    {
                        error = "COURSE_NOT_ENROLLED",
                        message = "Vui lòng ghi danh khóa học trước khi đánh dấu hoàn thành bài học."
                    }, statusCode: StatusCodes.Status403Forbidden);
                }
            }

            var completeLock = LessonCompleteLocks.GetOrAdd($"{userId:N}:{id:N}", _ => new SemaphoreSlim(1, 1));
            await completeLock.WaitAsync();
            try
            {
                var progress = await db.UserLessonProgresses.FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == id);
                if (progress == null)
                {
                    progress = new UserLessonProgress
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        LessonId = id,
                        IsCompleted = true,
                        CompletedAt = now,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    db.UserLessonProgresses.Add(progress);
                }
                else
                {
                    progress.IsCompleted = true;
                    progress.CompletedAt = now;
                    progress.UpdatedAt = now;
                }

                // ABUSE-11: Update streak if user has both completed lesson and at least 1 FSRS review
                if (userId != SeedDataInitializer.DefaultUserId)
                {
                    var hasFsrs = await db.FlashcardReviews.AnyAsync(r => r.UserId == userId);
                    if (hasFsrs)
                    {
                        var u = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
                        if (u != null)
                        {
                            u.StreakDays = Math.Max(u.StreakDays, 2);
                        }
                    }
                }

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    db.ChangeTracker.Clear();
                    progress = await db.UserLessonProgresses.FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == id);
                    if (progress != null)
                    {
                        progress.IsCompleted = true;
                        progress.CompletedAt = now;
                        progress.UpdatedAt = now;
                        await db.SaveChangesAsync();
                    }
                }

                return Results.Ok(new
                {
                    message = "Đã hoàn thành bài học.",
                    lessonId = id,
                    isCompleted = true,
                    completedAt = progress?.CompletedAt ?? now
                });
            }
            finally
            {
                completeLock.Release();
            }
        });

        api.MapDelete("/lessons/{id:guid}/complete", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return Results.NotFound(new { error = "Không tìm thấy bài học." });

            var chapter = await db.Chapters.FirstOrDefaultAsync(ch => ch.Id == lesson.ChapterId);
            var course = chapter != null ? await db.Courses.FirstOrDefaultAsync(c => c.Id == chapter.CourseId) : null;
            if (course != null && !await CanReadCourseAsync(db, course, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var progress = await db.UserLessonProgresses.FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == id);
            if (progress != null)
            {
                db.UserLessonProgresses.Remove(progress);
                await db.SaveChangesAsync();
            }

            return Results.Ok(new
            {
                message = "Đã bỏ đánh dấu hoàn thành bài học.",
                lessonId = id,
                isCompleted = false
            });
        });

        api.MapGet("/lessons", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var enrolledCourseIds = await db.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToHashSetAsync();

            var accessibleCourses = (await db.Courses.AsNoTracking().ToListAsync())
                .Where(c => IsOwnerOrDefaultSeededAccess(c.InstructorId, userId) ||
                            (c.Visibility == "public" && c.IsPublished) ||
                            enrolledCourseIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToHashSet();

            var chapters = await db.Chapters
                .Where(ch => accessibleCourses.Contains(ch.CourseId))
                .ToDictionaryAsync(ch => ch.Id, ch => ch.CourseId);
            var chapterIds = chapters.Keys.ToHashSet();

            var lessons = await db.Lessons
                .Where(l => chapterIds.Contains(l.ChapterId))
                .OrderBy(l => l.SortOrder)
                .ToListAsync();
            return Results.Ok(lessons.Select(l =>
            {
                var (sourceDocumentId, startPage, endPage) = CourseGenerationService.ExtractLessonMetadata(l.ContentAstJson);
                return new
                {
                    l.Id,
                    l.ChapterId,
                    courseId = chapters.GetValueOrDefault(l.ChapterId),
                    sourceDocumentId,
                    startPage,
                    endPage,
                    l.Title,
                    l.Slug,
                    l.CurrentVersion,
                    l.PlainText,
                    contentAst = JsonSerializer.Deserialize<object>(l.ContentAstJson),
                    l.UpdatedAt
                };
            }));
        });

        api.MapGet("/lessons/by-path", async (
            Guid courseId,
            string relativePath,
            Guid? fromLessonId,
            IConfiguration config,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return Results.NotFound(new { error = "Không tìm thấy khóa học." });

            if (!IsOwnerOrDefaultSeededAccess(course.InstructorId, userId) && !(course.Visibility == "public" && course.IsPublished))
            {
                var isEnrolled = await db.CourseEnrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
                if (!isEnrolled && userId != SeedDataInitializer.DefaultUserId && course.InstructorId != SeedDataInitializer.DefaultUserId)
                {
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }
            }

            string? currentLessonRelPath = null;
            string? lessonSourceFolder = null;
            if (fromLessonId.HasValue)
            {
                var fromLesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == fromLessonId.Value);
                if (fromLesson != null)
                {
                    (lessonSourceFolder, currentLessonRelPath) = CourseGenerationService.ExtractLessonSourceMetadata(fromLesson.ContentAstJson);
                }
            }

            if (!DocumentCourseSyncService.TryNormalizeRelativePath(
                    currentLessonRelPath,
                    relativePath,
                    out var normalizedRelPath,
                    out var fragmentAnchor))
            {
                return Results.BadRequest(new { error = "Đường dẫn bài học hoặc tài liệu liên kết không hợp lệ." });
            }

            var chapterIds = await db.Chapters
                .Where(ch => ch.CourseId == courseId)
                .Select(ch => ch.Id)
                .ToListAsync();

            var courseLessons = chapterIds.Count > 0
                ? await db.Lessons.Where(l => chapterIds.Contains(l.ChapterId)).ToListAsync()
                : new List<Lesson>();

            string? resolvedCourseFolder = lessonSourceFolder ?? DocumentCourseSyncService.ResolveCourseMetadata(course.Slug, course.Title).SourceFolder;
            Lesson? matchedLesson = null;
            string? mdAlternativePath = normalizedRelPath.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                ? Path.ChangeExtension(normalizedRelPath, ".md")
                : null;

            foreach (var l in courseLessons)
            {
                var (sf, sr) = CourseGenerationService.ExtractLessonSourceMetadata(l.ContentAstJson);
                resolvedCourseFolder ??= sf;
                if (string.IsNullOrWhiteSpace(sr)) continue;

                if (sr.Equals(normalizedRelPath, StringComparison.OrdinalIgnoreCase) ||
                    (mdAlternativePath != null && sr.Equals(mdAlternativePath, StringComparison.OrdinalIgnoreCase)))
                {
                    matchedLesson = l;
                    break;
                }
            }

            if (matchedLesson != null)
            {
                var (sf, sr) = CourseGenerationService.ExtractLessonSourceMetadata(matchedLesson.ContentAstJson);
                return Results.Ok(new
                {
                    found = true,
                    isAssetMarkdown = false,
                    lessonId = matchedLesson.Id,
                    courseId,
                    title = matchedLesson.Title,
                    sourceFolder = sf ?? resolvedCourseFolder,
                    sourceRelativePath = sr ?? normalizedRelPath,
                    anchor = string.IsNullOrWhiteSpace(fragmentAnchor) ? null : fragmentAnchor
                });
            }

            if (!string.IsNullOrWhiteSpace(resolvedCourseFolder))
            {
                var rootPath = DocumentCourseSyncService.ResolveDocumentRootPath(config);
                if (DocumentCourseSyncService.TryResolveSafeAssetFile(
                        rootPath,
                        resolvedCourseFolder,
                        normalizedRelPath,
                        out var fullPath,
                        out _,
                        out var canonicalRel,
                        out _,
                        out _))
                {
                    var ext = Path.GetExtension(fullPath);
                    if (ext.Equals(".md", StringComparison.OrdinalIgnoreCase) || ext.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        var contentMarkdown = await File.ReadAllTextAsync(fullPath, Encoding.UTF8);
                        return Results.Ok(new
                        {
                            found = false,
                            isAssetMarkdown = true,
                            lessonId = (Guid?)null,
                            courseId,
                            title = Path.GetFileNameWithoutExtension(canonicalRel),
                            sourceFolder = resolvedCourseFolder,
                            sourceRelativePath = canonicalRel,
                            anchor = string.IsNullOrWhiteSpace(fragmentAnchor) ? null : fragmentAnchor,
                            contentMarkdown
                        });
                    }
                }
            }

            return Results.NotFound(new { error = "Không tìm thấy bài học hoặc tệp tài liệu theo đường dẫn đã yêu cầu." });
        });

        api.MapGet("/lessons/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return Results.NotFound();

            var chapter = await db.Chapters.FirstOrDefaultAsync(ch => ch.Id == lesson.ChapterId);
            var course = chapter != null
                ? await db.Courses.FirstOrDefaultAsync(c => c.Id == chapter.CourseId)
                : null;
            if (course != null && !await CanReadCourseAsync(db, course, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }
            var (sourceDocumentId, startPage, endPage) = CourseGenerationService.ExtractLessonMetadata(lesson.ContentAstJson);
            var (sourceFolder, sourceRelativePath) = CourseGenerationService.ExtractLessonSourceMetadata(lesson.ContentAstJson);
            sourceFolder ??= course != null
                ? DocumentCourseSyncService.ResolveCourseMetadata(course.Slug, course.Title).SourceFolder
                : null;

            Guid? adjacentPrevLessonId = null;
            Guid? adjacentNextLessonId = null;
            if (chapter != null)
            {
                var courseChapters = await db.Chapters
                    .Where(ch => ch.CourseId == chapter.CourseId)
                    .OrderBy(ch => ch.SortOrder)
                    .ToListAsync();
                var chapterOrderMap = courseChapters.ToDictionary(ch => ch.Id, ch => ch.SortOrder);
                var courseChapterIds = chapterOrderMap.Keys.ToHashSet();

                var orderedLessons = (await db.Lessons
                        .Where(l => courseChapterIds.Contains(l.ChapterId))
                        .ToListAsync())
                    .OrderBy(l => chapterOrderMap.GetValueOrDefault(l.ChapterId))
                    .ThenBy(l => l.SortOrder)
                    .ThenBy(l => l.CreatedAt)
                    .ToList();

                var currentIdx = orderedLessons.FindIndex(l => l.Id == id);
                if (currentIdx > 0) adjacentPrevLessonId = orderedLessons[currentIdx - 1].Id;
                if (currentIdx >= 0 && currentIdx + 1 < orderedLessons.Count) adjacentNextLessonId = orderedLessons[currentIdx + 1].Id;
            }

            var isPublicCourse = course != null && course.Visibility == "public" && course.IsPublished;
            var linkedQuizzes = await db.PracticeQuizzes
                .Where(q => q.LessonId == id && (q.UserId == userId || isPublicCourse || userId == SeedDataInitializer.DefaultUserId))
                .ToListAsync();
            var linkedQuiz = linkedQuizzes
                .OrderByDescending(q => q.StartedAt)
                .FirstOrDefault();

            var userFlashcardsCount = await db.Flashcards
                .CountAsync(f => !f.IsArchived && f.LessonId == id && f.UserId == userId);
            int linkedFlashcardsCount;
            if (isPublicCourse && userId != SeedDataInitializer.DefaultUserId)
            {
                var hasUserClonedDeck = await db.Flashcards
                    .AnyAsync(f => f.LessonId == id && f.UserId == userId && f.AnnotationId == null);
                linkedFlashcardsCount = hasUserClonedDeck
                    ? userFlashcardsCount
                    : userFlashcardsCount + await db.Flashcards
                        .CountAsync(f => !f.IsArchived && f.LessonId == id && f.UserId == SeedDataInitializer.DefaultUserId);
            }
            else
            {
                linkedFlashcardsCount = userFlashcardsCount;
            }

            var annotations = await db.Annotations
                .Where(a => a.LessonId == id && a.UserId == userId && a.Status != "archived")
                .OrderBy(a => a.StartOffset)
                .ToListAsync();

            var flashcardsCountByAnn = await db.Flashcards
                .Where(f => f.LessonId == id && f.UserId == userId && !f.IsArchived && f.AnnotationId.HasValue)
                .GroupBy(f => f.AnnotationId!.Value)
                .Select(g => new { AnnId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AnnId, x => x.Count);

            var notesCountByAnn = await db.NotebookLinks
                .Where(nl => nl.RefLessonId == id && nl.RefAnnotationId.HasValue)
                .GroupBy(nl => nl.RefAnnotationId!.Value)
                .Select(g => new { AnnId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AnnId, x => x.Count);

            return Results.Ok(new
            {
                lesson.Id,
                lesson.ChapterId,
                courseId = chapter?.CourseId,
                courseTitle = course?.Title,
                chapterTitle = chapter?.Title,
                sourceFolder,
                sourceRelativePath,
                adjacentPrevLessonId,
                adjacentNextLessonId,
                linkedQuizId = linkedQuiz?.Id,
                linkedFlashcardsCount,
                sourceDocumentId,
                startPage,
                endPage,
                lesson.Title,
                lesson.Slug,
                lesson.SortOrder,
                lesson.CurrentVersion,
                lesson.PlainText,
                contentMarkdown = lesson.ContentMarkdown ?? lesson.PlainText,
                videoUrl = lesson.VideoUrl,
                estimatedMinutes = lesson.EstimatedMinutes,
                contentAst = JsonSerializer.Deserialize<object>(lesson.ContentAstJson),
                annotations = annotations.Select(a => new
                {
                    a.Id,
                    a.LessonId,
                    a.BlockId,
                    a.ExactQuote,
                    a.OriginalExactQuote,
                    a.PrefixQuote,
                    a.SuffixQuote,
                    a.StartOffset,
                    a.EndOffset,
                    a.ColorKey,
                    a.SemanticLabel,
                    a.NoteComment,
                    a.Status,
                    linkedFlashcardsCount = flashcardsCountByAnn.GetValueOrDefault(a.Id, 0),
                    linkedNotesCount = notesCountByAnn.GetValueOrDefault(a.Id, 0),
                    a.CreatedAt,
                    a.UpdatedAt
                })
            });
        });

        api.MapPost("/lessons/{id:guid}/publish-version", async (
            Guid id,
            PublishLessonVersionRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return Results.NotFound();

            var chapter = await db.Chapters.FirstOrDefaultAsync(ch => ch.Id == lesson.ChapterId);
            var course = chapter != null ? await db.Courses.FirstOrDefaultAsync(c => c.Id == chapter.CourseId) : null;
            if (course == null || !IsOwnerOrDefaultSeededAccess(course.InstructorId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var oldPlainText = lesson.PlainText;
            lesson.CurrentVersion += 1;
            // Pristine Master AST: strictly stores instructor AST without student marks (Decision H & ADR-008)
            lesson.ContentAstJson = req.ContentAstJson;
            lesson.PlainText = req.PlainText;
            lesson.UpdatedAt = DateTimeOffset.UtcNow;

            db.LessonVersions.Add(new LessonVersion
            {
                LessonId = lesson.Id,
                VersionNumber = lesson.CurrentVersion,
                ContentAstJson = req.ContentAstJson,
                PlainText = req.PlainText,
                PublishedBy = userId,
                ChangeSummary = req.ChangeSummary ?? $"Published version {lesson.CurrentVersion}"
            });

            var annotations = await db.Annotations
                .Where(a => a.LessonId == id && a.Status != "archived")
                .ToListAsync();

            var healingResults = new List<HealingResult>();
            foreach (var ann in annotations)
            {
                var res = ThreeTierAnnotationHealer.HealAnnotation(ann, oldPlainText, req.ContentAstJson, req.PlainText);
                healingResults.Add(res);
            }

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                lessonId = lesson.Id,
                newVersion = lesson.CurrentVersion,
                healedCount = healingResults.Count(r => r.NewStatus == "anchored"),
                needsReviewCount = healingResults.Count(r => r.NewStatus == "needs_review"),
                orphanedCount = healingResults.Count(r => r.NewStatus == "orphaned"),
                results = healingResults
            });
        });

        api.MapGet("/documents", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var rawDocs = await db.Documents.Where(d => d.UploaderId == userId).ToListAsync();
            var docs = rawDocs.OrderByDescending(d => d.CreatedAt).ToList();
            return Results.Ok(docs);
        });

        api.MapGet("/documents/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null) return Results.NotFound(new { error = "Không tìm thấy tài liệu." });
            if (!IsOwnerOrDefaultSeededAccess(doc.UploaderId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var annotations = await db.Annotations
                .Where(a => a.DocumentId == id && a.UserId == userId && a.Status != "archived")
                .ToListAsync();

            var firstPage = await db.DocumentPages
                .FirstOrDefaultAsync(p => p.DocumentId == id && p.PageNumber == 1);

            return Results.Ok(new
            {
                doc.Id,
                doc.Title,
                doc.FileUrl,
                doc.FileSizeBytes,
                doc.TotalPages,
                doc.MimeType,
                doc.OcrStatus,
                doc.PlainText,
                contentAst = string.IsNullOrWhiteSpace(doc.ContentAstJson) ? null : JsonSerializer.Deserialize<object>(doc.ContentAstJson),
                initialPage = firstPage == null ? null : new
                {
                    firstPage.PageNumber,
                    firstPage.PlainText,
                    firstPage.HasTextLayer,
                    charCount = firstPage.CharCount,
                    characterCount = firstPage.CharCount,
                    wordCount = firstPage.WordCount
                },
                annotations
            });
        });

        api.MapGet("/documents/{id:guid}/file", async (Guid id, HttpContext http, IConfiguration config, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null)
            {
                return Results.NotFound(new { error = "Không tìm thấy tài liệu PDF." });
            }
            if (!IsOwnerOrDefaultSeededAccess(doc.UploaderId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var storagePath = config["Storage:LocalStoragePath"] ?? "App_Data/storage/documents";
            if (!Path.IsPathRooted(storagePath))
            {
                storagePath = Path.Combine(AppContext.BaseDirectory, storagePath);
            }
            var filePath = Path.Combine(storagePath, $"{id:N}.pdf");

            if (!File.Exists(filePath))
            {
                var matching = Directory.GetFiles(storagePath, $"{id:N}.*").FirstOrDefault();
                if (matching != null)
                {
                    filePath = matching;
                }
                else
                {
                    return Results.NotFound(new { error = "Tệp tài liệu không tồn tại trên hệ thống lưu trữ máy chủ." });
                }
            }

            return Results.File(
                filePath,
                contentType: doc.MimeType ?? "application/octet-stream",
                fileDownloadName: null,
                enableRangeProcessing: true
            );
        });

        api.MapGet("/documents/{id:guid}/pages", async (
            Guid id,
            int? from,
            int? to,
            int? page,
            int? pageSize,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null)
            {
                return Results.NotFound(new { error = "Không tìm thấy tài liệu." });
            }
            if (!IsOwnerOrDefaultSeededAccess(doc.UploaderId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var query = db.DocumentPages.Where(p => p.DocumentId == id);

            if (from.HasValue && to.HasValue)
            {
                query = query.Where(p => p.PageNumber >= from.Value && p.PageNumber <= to.Value);
            }
            else if (page.HasValue)
            {
                var size = Math.Clamp(pageSize ?? 20, 1, 100);
                var skip = (Math.Max(1, page.Value) - 1) * size;
                query = query.OrderBy(p => p.PageNumber).Skip(skip).Take(size);
            }

            var pages = await query.OrderBy(p => p.PageNumber).Select(p => new
            {
                p.Id,
                p.DocumentId,
                p.PageNumber,
                p.PlainText,
                p.HasTextLayer,
                charCount = p.CharCount,
                characterCount = p.CharCount,
                wordCount = p.WordCount
            }).ToListAsync();

            return Results.Ok(pages);
        });

        api.MapGet("/documents/{id:guid}/pages/{pageNumber:int}", async (
            Guid id,
            int pageNumber,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null)
            {
                return Results.NotFound(new { error = "Không tìm thấy tài liệu." });
            }
            if (!IsOwnerOrDefaultSeededAccess(doc.UploaderId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            if (pageNumber < 1 || pageNumber > doc.TotalPages)
            {
                return Results.BadRequest(new { error = $"Số trang {pageNumber} không hợp lệ (tài liệu có {doc.TotalPages} trang)." });
            }

            var pageRecord = await db.DocumentPages
                .FirstOrDefaultAsync(p => p.DocumentId == id && p.PageNumber == pageNumber);

            if (pageRecord == null)
            {
                return Results.NotFound(new { error = $"Không tìm thấy dữ liệu trang {pageNumber}." });
            }

            return Results.Ok(new
            {
                pageRecord.Id,
                pageRecord.DocumentId,
                pageRecord.PageNumber,
                pageRecord.PlainText,
                pageRecord.HasTextLayer,
                charCount = pageRecord.CharCount,
                characterCount = pageRecord.CharCount,
                wordCount = pageRecord.WordCount
            });
        });

        api.MapPost("/documents/upload", async (
            HttpContext http,
            IConfiguration config,
            IPdfExtractionService pdfService,
            GeminiProxyClient gemini,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var aiOverrides = CourseGenerationService.ExtractAiOverrides(http);

            if (http.Request.HasFormContentType)
            {
                var form = await http.Request.ReadFormAsync();
                var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { error = "Không tìm thấy tệp tài liệu được tải lên." });
                }

                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
                var isPdf = ext == ".pdf" || file.ContentType == "application/pdf";
                var isDocx = ext == ".docx" || file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                var isMarkdown = ext is ".md" or ".markdown" || file.ContentType is "text/markdown" or "text/x-markdown";
                var isText = ext is ".txt" || file.ContentType is "text/plain";

                if (!isPdf && !isDocx && !isMarkdown && !isText)
                {
                    return Results.BadRequest(new { error = "Định dạng tệp không được hỗ trợ. Vui lòng tải lên tệp PDF, Word (.docx), Markdown (.md) hoặc Text (.txt)." });
                }

                var quota = await db.UserQuotas.FirstOrDefaultAsync(q => q.UserId == userId);
                if (quota != null && quota.CurrentStorageBytes + file.Length > quota.StorageLimitBytes)
                {
                    return Results.Json(new { error = "Vượt quá hạn mức dung lượng lưu trữ 50 MB của gói Free." }, statusCode: StatusCodes.Status413PayloadTooLarge);
                }

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                PdfExtractionResult extraction;
                string docMimeType;

                if (isPdf)
                {
                    docMimeType = "application/pdf";
                    extraction = pdfService.Extract(fileBytes);
                    if (!extraction.Success)
                    {
                        if (extraction.IsEncrypted)
                        {
                            return Results.Json(new { error = extraction.ErrorMessage ?? "Tệp tin PDF có mật khẩu bảo vệ." }, statusCode: StatusCodes.Status422UnprocessableEntity);
                        }
                        return Results.Json(new { error = extraction.ErrorMessage ?? "Tệp tin PDF bị hỏng hoặc không đúng định dạng." }, statusCode: StatusCodes.Status422UnprocessableEntity);
                    }
                }
                else
                {
                    docMimeType = isDocx ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" :
                                  isMarkdown ? "text/markdown" : "text/plain";
                    var text = CourseWizardAndImportService.ExtractTextFromBytes(fileBytes, ext);
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        text = Path.GetFileNameWithoutExtension(file.FileName);
                    }

                    var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var pageList = new List<ExtractedPdfPage>();
                    var currentSb = new StringBuilder();
                    int curPage = 1;

                    foreach (var p in paragraphs)
                    {
                        if (currentSb.Length > 0 && currentSb.Length + p.Length > 1500)
                        {
                            var pText = currentSb.ToString().Trim();
                            pageList.Add(new ExtractedPdfPage(
                                curPage++,
                                pText,
                                true,
                                pText.Length,
                                pText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length
                            ));
                            currentSb.Clear();
                        }
                        currentSb.AppendLine(p);
                        currentSb.AppendLine();
                    }

                    if (currentSb.Length > 0 || pageList.Count == 0)
                    {
                        var pText = currentSb.Length > 0 ? currentSb.ToString().Trim() : text;
                        pageList.Add(new ExtractedPdfPage(
                            curPage,
                            pText,
                            true,
                            pText.Length,
                            pText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length
                        ));
                    }

                    extraction = new PdfExtractionResult(
                        Success: true,
                        TotalPages: pageList.Count,
                        Pages: pageList
                    );
                }

                var docId = Guid.NewGuid();
                var storagePath = config["Storage:LocalStoragePath"] ?? "App_Data/storage/documents";
                if (!Path.IsPathRooted(storagePath))
                {
                    storagePath = Path.Combine(AppContext.BaseDirectory, storagePath);
                }
                Directory.CreateDirectory(storagePath);
                var saveExt = isPdf ? ".pdf" : (string.IsNullOrWhiteSpace(ext) ? ".txt" : ext);
                var filePath = Path.Combine(storagePath, $"{docId:N}{saveExt}");
                await File.WriteAllBytesAsync(filePath, fileBytes);

                var title = form.TryGetValue("title", out var titleVal) && !string.IsNullOrWhiteSpace(titleVal)
                    ? titleVal.ToString()
                    : Path.GetFileNameWithoutExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = "Untitled Document";
                }

                var personalCourse = await db.Courses.FirstOrDefaultAsync(c => c.InstructorId == userId && c.Visibility == "personal")
                                     ?? await db.Courses.FirstAsync();

                var firstPageText = extraction.Pages.Count > 0 ? extraction.Pages[0].Text : string.Empty;

                var doc = new Document
                {
                    Id = docId,
                    CourseId = personalCourse.Id,
                    UploaderId = userId,
                    Title = title,
                    FileUrl = $"/api/v1/documents/{docId}/file",
                    FileSizeBytes = file.Length,
                    TotalPages = extraction.TotalPages,
                    MimeType = docMimeType,
                    OcrStatus = "completed",
                    ContentAstJson = null,
                    PlainText = firstPageText,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                db.Documents.Add(doc);

                var createdPages = new List<DocumentPage>(extraction.Pages.Count);
                foreach (var p in extraction.Pages)
                {
                    var pageEntity = new DocumentPage
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = docId,
                        PageNumber = p.PageNumber,
                        PlainText = p.Text,
                        HasTextLayer = p.HasTextLayer,
                        CharCount = p.CharCount,
                        WordCount = p.WordCount,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    createdPages.Add(pageEntity);
                    db.DocumentPages.Add(pageEntity);
                }

                if (quota != null)
                {
                    quota.CurrentStorageBytes += file.Length;
                }
                await db.SaveChangesAsync();

                var generatedCourse = await CourseGenerationService.GenerateCourseFromDocumentAsync(
                    db, gemini, doc, createdPages, userId, aiOverrides);

                return Results.Ok(new
                {
                    doc.Id,
                    doc.CourseId,
                    doc.UploaderId,
                    doc.Title,
                    doc.FileUrl,
                    doc.FileSizeBytes,
                    doc.TotalPages,
                    doc.MimeType,
                    doc.OcrStatus,
                    doc.ContentAstJson,
                    doc.PlainText,
                    doc.CreatedAt,
                    doc.UpdatedAt,
                    generatedCourse
                });
            }
            else
            {
                // Legacy JSON upload fallback for backward test compatibility
                UploadDocumentRequest? req = null;
                try
                {
                    req = await http.Request.ReadFromJsonAsync<UploadDocumentRequest>();
                }
                catch
                {
                    return Results.BadRequest(new { error = "Dữ liệu yêu cầu không hợp lệ." });
                }

                if (req == null)
                {
                    return Results.BadRequest(new { error = "Dữ liệu tải lên trống." });
                }

                if (req.FileSizeBytes <= 0)
                {
                    return Results.BadRequest(new { error = "Kích thước tệp (FileSizeBytes) phải lớn hơn 0." });
                }

                var extractedText = req.ExtractedText ?? string.Empty;
                if (extractedText.Length > 2_000_000)
                {
                    return Results.Json(new { error = "Nội dung văn bản vượt quá giới hạn 2,000,000 ký tự." }, statusCode: StatusCodes.Status413PayloadTooLarge);
                }

                var effectiveBytes = Math.Max(req.FileSizeBytes, Encoding.UTF8.GetByteCount(extractedText));
                var quota = await db.UserQuotas.FirstOrDefaultAsync(q => q.UserId == userId);
                if (quota != null && quota.CurrentStorageBytes + effectiveBytes > quota.StorageLimitBytes)
                {
                    return Results.Json(new { error = "Vượt quá hạn mức dung lượng lưu trữ 50 MB của gói Free." }, statusCode: StatusCodes.Status413PayloadTooLarge);
                }

                var personalCourse = await db.Courses.FirstOrDefaultAsync(c => c.InstructorId == userId && c.Visibility == "personal")
                                     ?? await db.Courses.FirstAsync();

                var docId = Guid.NewGuid();
                var totalPages = Math.Max(1, req.TotalPages);

                var doc = new Document
                {
                    Id = docId,
                    CourseId = personalCourse.Id,
                    UploaderId = userId,
                    Title = req.Title,
                    FileUrl = $"/api/v1/documents/{docId}/file",
                    FileSizeBytes = Math.Max(1024, effectiveBytes),
                    TotalPages = totalPages,
                    MimeType = req.MimeType ?? "application/pdf",
                    OcrStatus = "completed",
                    ContentAstJson = null,
                    PlainText = extractedText,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                db.Documents.Add(doc);

                var page1 = new DocumentPage
                {
                    Id = Guid.NewGuid(),
                    DocumentId = docId,
                    PageNumber = 1,
                    PlainText = extractedText,
                    HasTextLayer = !string.IsNullOrWhiteSpace(extractedText),
                    CharCount = extractedText.Length,
                    WordCount = extractedText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                db.DocumentPages.Add(page1);

                if (quota != null)
                {
                    quota.CurrentStorageBytes += doc.FileSizeBytes;
                }
                await db.SaveChangesAsync();

                var generatedCourse = await CourseGenerationService.GenerateCourseFromDocumentAsync(
                    db, gemini, doc, new List<DocumentPage> { page1 }, userId, aiOverrides);

                return Results.Ok(new
                {
                    doc.Id,
                    doc.CourseId,
                    doc.UploaderId,
                    doc.Title,
                    doc.FileUrl,
                    doc.FileSizeBytes,
                    doc.TotalPages,
                    doc.MimeType,
                    doc.OcrStatus,
                    doc.ContentAstJson,
                    doc.PlainText,
                    doc.CreatedAt,
                    doc.UpdatedAt,
                    generatedCourse
                });
            }
        });

        api.MapPost("/documents/{id:guid}/generate-course", async (
            Guid id,
            HttpContext http,
            GeminiProxyClient gemini,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null)
            {
                return Results.NotFound(new { error = "Không tìm thấy tài liệu." });
            }
            if (!IsOwnerOrDefaultSeededAccess(doc.UploaderId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var courseSlug = $"doc-course-{doc.Id:N}";
            var existingCourse = await db.Courses.FirstOrDefaultAsync(c => c.Slug == courseSlug);
            if (existingCourse == null)
            {
                if (!await CheckAiQuotaAvailableAsync(db, userId, 1))
                {
                    return Results.Json(new
                    {
                        error = "AI_QUOTA_EXCEEDED",
                        message = "Đã đạt giới hạn 50 AI Credits/ngày của gói Free cho tạo khóa học."
                    }, statusCode: StatusCodes.Status429TooManyRequests);
                }
            }

            var pages = await db.DocumentPages
                .Where(p => p.DocumentId == doc.Id)
                .OrderBy(p => p.PageNumber)
                .ToListAsync();

            var aiOverrides = CourseGenerationService.ExtractAiOverrides(http);
            var generatedCourse = await CourseGenerationService.GenerateCourseFromDocumentAsync(
                db, gemini, doc, pages, userId, aiOverrides);

            return Results.Ok(generatedCourse);
        });
    }

    // ========================================================================
    // 4. ANNOTATIONS & 3-ACTION ORPHANED RECONCILIATION (DECISION 13 & 14)
    // ========================================================================
    private static void MapAnnotationEndpoints(RouteGroupBuilder api)
    {
        var annGroup = api.MapGroup("/annotations");

        annGroup.MapGet("/", async (Guid? lessonId, Guid? documentId, int? pageNumber, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var query = db.Annotations.Where(a => a.UserId == userId && a.Status != "archived");
            if (lessonId.HasValue) query = query.Where(a => a.LessonId == lessonId);
            if (documentId.HasValue) query = query.Where(a => a.DocumentId == documentId);
            if (pageNumber.HasValue) query = query.Where(a => a.PageNumber == pageNumber.Value);
            var rawList = await query.ToListAsync();
            var list = rawList.OrderBy(a => a.CreatedAt).ToList();
            return Results.Ok(list);
        });

        annGroup.MapPost("/", async (CreateAnnotationRequest req, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);

            var isLesson = req.LessonId.HasValue;
            var isDoc = req.DocumentId.HasValue;

            if (isLesson == isDoc)
            {
                return Results.BadRequest(new
                {
                    error = "Mục tiêu bôi đậm không hợp lệ: Phải chỉ định duy nhất Bài học (LessonId + BlockId) HOẶC Tài liệu (DocumentId + PageNumber > 0)."
                });
            }

            if (isDoc)
            {
                var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == req.DocumentId!.Value);
                if (doc == null)
                {
                    return Results.NotFound(new { error = "Tài liệu được chỉ định không tồn tại." });
                }
                if (!IsOwnerOrDefaultSeededAccess(doc.UploaderId, userId))
                {
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }

                var pageNum = req.PageNumber ?? 1;
                if (pageNum < 1 || pageNum > doc.TotalPages)
                {
                    return Results.BadRequest(new { error = $"Số trang {pageNum} vượt quá giới hạn trang của tài liệu ({doc.TotalPages} trang)." });
                }
            }

            if (req.StartOffset < 0)
            {
                return Results.BadRequest(new { error = "Vị trí bắt đầu (StartOffset) không được âm." });
            }

            if (req.EndOffset < req.StartOffset)
            {
                return Results.BadRequest(new { error = "Vị trí kết thúc (EndOffset) phải lớn hơn hoặc bằng vị trí bắt đầu (StartOffset)." });
            }

            if ((req.NoteComment?.Length ?? 0) > 10_000 || (req.ExactQuote?.Length ?? 0) > 10_000)
            {
                return Results.BadRequest(new { error = "Độ dài ghi chú hoặc đoạn trích không được vượt quá 10,000 ký tự." });
            }

            var defaultLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["yellow"] = "Khái niệm cốt lõi",
                ["green"] = "Ví dụ & Dẫn chứng",
                ["blue"] = "Câu hỏi & Nghi vấn",
                ["purple"] = "Ý tưởng tổng hợp",
                ["rose"] = "Lưu ý quan trọng"
            };

            var colorKey = defaultLabels.ContainsKey(req.ColorKey) ? req.ColorKey.ToLowerInvariant() : "yellow";

            var ann = new Annotation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = isLesson ? req.LessonId : null,
                BlockId = isLesson ? (req.BlockId ?? SeedDataInitializer.BlockPara1Id) : null,
                DocumentId = isDoc ? req.DocumentId : null,
                PageNumber = isDoc ? (req.PageNumber ?? 1) : null,
                BoundingBoxesJson = req.BoundingBoxesJson,
                ExactQuote = req.ExactQuote,
                OriginalExactQuote = req.ExactQuote,
                PrefixQuote = req.PrefixQuote,
                SuffixQuote = req.SuffixQuote,
                StartOffset = Math.Max(0, req.StartOffset),
                EndOffset = Math.Max(req.StartOffset, req.EndOffset),
                ColorKey = colorKey,
                SemanticLabel = req.SemanticLabel ?? defaultLabels[colorKey],
                NoteComment = req.NoteComment,
                Status = "anchored",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.Annotations.Add(ann);

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && user.OnboardingStep < 2)
            {
                user.OnboardingStep = 2;
            }

            await db.SaveChangesAsync();
            return Results.Ok(ann);
        });

        annGroup.MapPost("/{id:guid}/reconcile", async (
            Guid id,
            ReconcileAnnotationRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var ann = await db.Annotations.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (ann == null) return Results.NotFound();

            var action = req.Action.Trim().ToLowerInvariant();
            if (action == "reanchor")
            {
                ann.BlockId = req.NewBlockId ?? ann.BlockId ?? SeedDataInitializer.BlockPara1Id;
                ann.StartOffset = req.NewStartOffset ?? 0;
                ann.EndOffset = req.NewEndOffset ?? (ann.StartOffset + (req.NewExactQuote?.Length ?? ann.ExactQuote.Length));
                if (!string.IsNullOrWhiteSpace(req.NewExactQuote))
                {
                    ann.ExactQuote = req.NewExactQuote;
                }
                ann.Status = "anchored";
                ann.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
                return Results.Ok(new { message = "Đã gắn lại highlight vào vị trí mới.", annotation = ann });
            }

            if (action == "keep_unanchored")
            {
                ann.Status = "orphaned";
                ann.NoteComment = string.IsNullOrWhiteSpace(ann.NoteComment)
                    ? "[Đã xác nhận giữ dạng trích dẫn tự do]"
                    : $"{ann.NoteComment} [Giữ trích dẫn tự do]";
                ann.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
                return Results.Ok(new { message = "Đã giữ highlight dưới dạng trích dẫn tự do không cần neo.", annotation = ann });
            }

            if (action == "delete")
            {
                // SwmsDbContext enforces ON DELETE SET NULL on flashcards.annotation_id so flashcards survive with snapshots!
                db.Annotations.Remove(ann);
                await db.SaveChangesAsync();
                return Results.Ok(new { message = "Đã xóa highlight. Các thẻ ghi nhớ liên kết vẫn được bảo toàn nguyên vẹn qua bản chụp trích dẫn." });
            }

            return Results.BadRequest(new { error = "Hành động không hợp lệ. Chọn: reanchor, keep_unanchored, hoặc delete." });
        });

        annGroup.MapDelete("/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var ann = await db.Annotations.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (ann == null) return Results.NotFound();

            db.Annotations.Remove(ann);
            await db.SaveChangesAsync();
            return Results.Ok(new { deletedId = id });
        });
    }

    // ========================================================================
    // 5. NOTEBOOKS (SINGLE JSONB AST, OCC, 409 KEEP-BOTH FORK, BACKLINKS, MD EXPORT)
    // ========================================================================
    private static void MapNotebookEndpoints(RouteGroupBuilder api)
    {
        var nbGroup = api.MapGroup("/notebooks");

        nbGroup.MapGet("/", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var rawList = await db.Notebooks
                .Where(n => n.UserId == userId)
                .ToListAsync();
            var list = rawList.OrderByDescending(n => n.UpdatedAt).ToList();
            return Results.Ok(list);
        });

        nbGroup.MapPost("/", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            CreateNotebookRequest? req = null;
            if (http.Request.ContentLength is > 0 ||
                (http.Request.ContentType != null && http.Request.ContentType.Contains("json", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    req = await http.Request.ReadFromJsonAsync<CreateNotebookRequest>();
                }
                catch
                {
                    // Fallback to default standalone title/AST if body is empty
                }
            }

            var now = DateTimeOffset.UtcNow;
            var rawTitle = !string.IsNullOrWhiteSpace(req?.Title)
                ? req.Title.Trim()
                : $"Ghi chú độc lập ({now:dd/MM HH:mm})";
            var title = SanitizeDangerousHtml(rawTitle) ?? rawTitle;

            string contentAstJson;
            if (!string.IsNullOrWhiteSpace(req?.ContentAstJson))
            {
                contentAstJson = SanitizeDangerousHtml(req.ContentAstJson) ?? req.ContentAstJson;
            }
            else
            {
                var initialAst = new
                {
                    type = "doc",
                    content = new object[]
                    {
                        new
                        {
                            type = "heading",
                            attrs = new { level = 1, block_id = Guid.NewGuid().ToString() },
                            content = new[] { new { type = "text", text = title } }
                        },
                        new
                        {
                            type = "paragraph",
                            attrs = new { block_id = Guid.NewGuid().ToString() },
                            content = new[] { new { type = "text", text = "Bắt đầu tổng hợp kiến thức tại đây hoặc chèn liên kết hai chiều [[Wikilink]]..." } }
                        }
                    }
                };
                contentAstJson = JsonSerializer.Serialize(initialAst);
            }

            var created = new Notebook
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                PrimarySourceLessonId = null,
                PrimarySourceDocumentId = null,
                ContentAstJson = contentAstJson,
                Version = 1,
                CreatedAt = now,
                UpdatedAt = now
            };

            db.Notebooks.Add(created);
            await ExtractAndSyncNotebookLinksAsync(db, created.Id, contentAstJson);
            await db.SaveChangesAsync();
            return await BuildNotebookDetailResponseAsync(db, created);
        });

        nbGroup.MapPost("/by-source", async (AutoProvisionNotebookRequest req, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var sourceKey = $"{userId:N}:{req.LessonId}:{req.DocumentId}";
            var provLock = NotebookAutoProvisionLocks.GetOrAdd(sourceKey, _ => new SemaphoreSlim(1, 1));
            await provLock.WaitAsync();
            try
            {
                Notebook? existing = null;
                if (req.LessonId.HasValue)
                {
                    existing = await db.Notebooks.FirstOrDefaultAsync(n => n.UserId == userId && n.PrimarySourceLessonId == req.LessonId);
                }
                else if (req.DocumentId.HasValue)
                {
                    existing = await db.Notebooks.FirstOrDefaultAsync(n => n.UserId == userId && n.PrimarySourceDocumentId == req.DocumentId);
                }

                if (existing != null)
                {
                    return await BuildNotebookDetailResponseAsync(db, existing);
                }

                var title = $"Ghi chú: {req.SourceTitle ?? "Tài liệu học tập"}";
                var initialAst = new
                {
                    type = "doc",
                    content = new object[]
                    {
                        new
                        {
                            type = "heading",
                            attrs = new { level = 1, block_id = Guid.NewGuid().ToString() },
                            content = new[] { new { type = "text", text = title } }
                        },
                        new
                        {
                            type = "paragraph",
                            attrs = new { block_id = Guid.NewGuid().ToString() },
                            content = new[] { new { type = "text", text = "Bắt đầu tổng hợp kiến thức tại đây hoặc chọn trích dẫn từ cột đọc bên trái..." } }
                        }
                    }
                };

                var created = new Notebook
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = title,
                    PrimarySourceLessonId = req.LessonId,
                    PrimarySourceDocumentId = req.LessonId.HasValue ? null : req.DocumentId,
                    ContentAstJson = JsonSerializer.Serialize(initialAst),
                    Version = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                db.Notebooks.Add(created);
                await db.SaveChangesAsync();
                return await BuildNotebookDetailResponseAsync(db, created);
            }
            finally
            {
                provLock.Release();
            }
        });

        nbGroup.MapGet("/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var nb = await db.Notebooks.FirstOrDefaultAsync(n => n.Id == id);
            if (nb == null) return Results.NotFound();
            if (!IsOwnerOrDefaultSeededAccess(nb.UserId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }
            return await BuildNotebookDetailResponseAsync(db, nb);
        });

        nbGroup.MapPut("/{id:guid}", async (
            Guid id,
            SaveNotebookRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            if ((req.ContentAstJson?.Length ?? 0) > 2_000_000)
            {
                return Results.Json(new { error = "Nội dung sổ tay vượt quá giới hạn 2,000,000 ký tự." }, statusCode: StatusCodes.Status413PayloadTooLarge);
            }

            var userId = ResolveUserId(http);
            var occLock = NotebookOccLocks.GetOrAdd(id.ToString("N"), _ => new SemaphoreSlim(1, 1));
            await occLock.WaitAsync();
            try
            {
                var nb = await db.Notebooks.FirstOrDefaultAsync(n => n.Id == id);
                if (nb == null) return Results.NotFound();
                if (!IsOwnerOrDefaultSeededAccess(nb.UserId, userId))
                {
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }

                var now = DateTimeOffset.UtcNow;

                // BaseVersion validity check (ABUSE-10)
                if (req.BaseVersion < 1 || req.BaseVersion > nb.Version)
                {
                    return Results.BadRequest(new
                    {
                        error = "INVALID_BASE_VERSION",
                        message = "baseVersion không hợp lệ."
                    });
                }

                var cleanContentAstJson = SanitizeDangerousHtml(req.ContentAstJson) ?? req.ContentAstJson;
                var cleanTitle = !string.IsNullOrWhiteSpace(req.Title) ? SanitizeDangerousHtml(req.Title.Trim()) : null;

                // Optimistic Concurrency Control (OCC) Check (Decision B & ADR-002 & ADR-022)
                if (req.BaseVersion != nb.Version)
                {
                    var existingDraftCount = await db.NotebookConflictDrafts.CountAsync(d => d.NotebookId == nb.Id);
                    if (existingDraftCount >= 20)
                    {
                        return Results.Json(new
                        {
                            error = "CONFLICT_DRAFT_LIMIT_REACHED",
                            message = "Đã đạt giới hạn tối đa 20 bản nháp xung đột cho sổ tay này."
                        }, statusCode: StatusCodes.Status409Conflict);
                    }
                    // 409 CONFLICT -> "Keep Both" Strategy (ADR-022):
                    // Create a timestamped companion fork notebook and log to notebook_conflict_drafts
                    var forkTitle = $"{nb.Title} (Bản sao xung đột - {now:yyyy-MM-dd HH:mm})";
                    var forkNotebook = new Notebook
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Title = forkTitle,
                        PrimarySourceLessonId = null, // Avoid colliding with primary 1-1 lookup
                        PrimarySourceDocumentId = null,
                        ContentAstJson = cleanContentAstJson,
                        Version = 1,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    db.Notebooks.Add(forkNotebook);

                    var conflictDraft = new NotebookConflictDraft
                    {
                        Id = Guid.NewGuid(),
                        NotebookId = nb.Id,
                        ForkedNotebookId = forkNotebook.Id,
                        UserId = userId,
                        DeviceId = req.DeviceId ?? "web-client",
                        BaseVersion = req.BaseVersion,
                        ServerVersion = nb.Version,
                        ClientContentAstJson = cleanContentAstJson,
                        ResolutionStatus = "kept_both",
                        CreatedAt = now,
                        ResolvedAt = now
                    };
                    db.NotebookConflictDrafts.Add(conflictDraft);

                    await ExtractAndSyncNotebookLinksAsync(db, forkNotebook.Id, cleanContentAstJson);
                    await db.SaveChangesAsync();

                    return Results.Json(new
                    {
                        error = "CONFLICT_KEEP_BOTH",
                        message = $"Phát hiện xung đột nội dung: Đã lưu bản sao riêng '{forkTitle}' để bạn đối chiếu.",
                        serverVersion = nb.Version,
                        canonicalNotebook = new
                        {
                            nb.Id,
                            nb.Title,
                            nb.Version,
                            contentAst = JsonSerializer.Deserialize<object>(nb.ContentAstJson)
                        },
                        conflictCopyNotebook = new
                        {
                            forkNotebook.Id,
                            forkNotebook.Title,
                            forkNotebook.Version,
                            contentAst = JsonSerializer.Deserialize<object>(forkNotebook.ContentAstJson)
                        },
                        conflictDraftId = conflictDraft.Id,
                        resolutionStatus = conflictDraft.ResolutionStatus
                    }, statusCode: StatusCodes.Status409Conflict);
                }

                // Atomic single-document JSONB update + version increment
                if (!string.IsNullOrWhiteSpace(cleanTitle))
                {
                    nb.Title = cleanTitle;
                }
                nb.ContentAstJson = cleanContentAstJson;
                nb.Version += 1;
                nb.UpdatedAt = now;

                await ExtractAndSyncNotebookLinksAsync(db, nb.Id, cleanContentAstJson);
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    nb.Id,
                    nb.Title,
                    nb.Version,
                    nb.UpdatedAt,
                    contentAst = JsonSerializer.Deserialize<object>(nb.ContentAstJson)
                });
            }
            finally
            {
                occLock.Release();
            }
        });

        nbGroup.MapDelete("/{id:guid}", async (
            Guid id,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var nb = await db.Notebooks.FirstOrDefaultAsync(n => n.Id == id);
            if (nb == null) return Results.NotFound(new { error = "Không tìm thấy sổ tay." });
            if (nb.UserId != userId && !IsOwnerOrDefaultSeededAccess(nb.UserId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var relatedConflictDrafts = await db.NotebookConflictDrafts
                .Where(d => d.NotebookId == id || d.ForkedNotebookId == id)
                .ToListAsync();
            if (relatedConflictDrafts.Count > 0)
            {
                db.NotebookConflictDrafts.RemoveRange(relatedConflictDrafts);
            }

            var relatedLinks = await db.NotebookLinks
                .Where(l => l.NotebookId == id)
                .ToListAsync();
            if (relatedLinks.Count > 0)
            {
                db.NotebookLinks.RemoveRange(relatedLinks);
            }

            db.Notebooks.Remove(nb);
            await db.SaveChangesAsync();

            return Results.Ok(new { deleted = true, id });
        });

        nbGroup.MapGet("/{id:guid}/export-markdown", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var nb = await db.Notebooks.FirstOrDefaultAsync(n => n.Id == id);
            if (nb == null) return Results.NotFound();
            if (!IsOwnerOrDefaultSeededAccess(nb.UserId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var links = await db.NotebookLinks.Where(nl => nl.NotebookId == id).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine($"title: \"{nb.Title.Replace("\"", "\\\"")}\"");
            sb.AppendLine($"notebook_id: \"{nb.Id}\"");
            sb.AppendLine($"version: {nb.Version}");
            sb.AppendLine($"updated_at: \"{nb.UpdatedAt:O}\"");
            sb.AppendLine($"citations_count: {links.Count}");
            sb.AppendLine("export_format: \"SWMS Sovereign Markdown v1.0\"");
            sb.AppendLine("---");
            sb.AppendLine();

            try
            {
                using var doc = JsonDocument.Parse(nb.ContentAstJson);
                if (doc.RootElement.TryGetProperty("content", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var node in arr.EnumerateArray())
                    {
                        var type = node.TryGetProperty("type", out var tp) ? tp.GetString() : "paragraph";
                        if (type == "heading")
                        {
                            var level = node.TryGetProperty("attrs", out var a) && a.TryGetProperty("level", out var lv) ? lv.GetInt32() : 2;
                            sb.AppendLine($"{new string('#', Math.Clamp(level, 1, 6))} {ReadInlineText(node)}");
                            sb.AppendLine();
                        }
                        else if (type == "annotationRef")
                        {
                            var quote = node.TryGetProperty("attrs", out var a) && a.TryGetProperty("source_quote", out var sq) ? sq.GetString() : "";
                            var srcTitle = node.TryGetProperty("attrs", out var a2) && a2.TryGetProperty("source_title", out var st) ? st.GetString() : "Nguồn bài đọc";
                            sb.AppendLine($"> **[📖 Trích dẫn: {srcTitle}]**");
                            sb.AppendLine($"> \"{quote}\"");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine(ReadInlineText(node));
                            sb.AppendLine();
                        }
                    }
                }
            }
            catch
            {
                sb.AppendLine(nb.ContentAstJson);
            }

            return Results.Ok(new
            {
                notebookId = nb.Id,
                fileName = $"{VietnameseTextNormalizer.Unaccent(nb.Title).Replace(' ', '-')}.md",
                markdown = sb.ToString()
            });
        });
    }

    private static async Task<IResult> BuildNotebookDetailResponseAsync(SwmsDbContext db, Notebook nb)
    {
        var links = await db.NotebookLinks.Where(nl => nl.NotebookId == nb.Id).ToListAsync();
        var activeAnnotationIds = await db.Annotations
            .Where(a => a.Status != "archived")
            .Select(a => a.Id)
            .ToHashSetAsync();

        var rawConflicts = await db.NotebookConflictDrafts
            .Where(c => c.NotebookId == nb.Id)
            .ToListAsync();
        var conflicts = rawConflicts.OrderByDescending(c => c.CreatedAt).ToList();

        return Results.Ok(new
        {
            nb.Id,
            nb.UserId,
            nb.Title,
            nb.PrimarySourceLessonId,
            nb.PrimarySourceDocumentId,
            nb.Version,
            nb.CreatedAt,
            nb.UpdatedAt,
            contentAst = JsonSerializer.Deserialize<object>(nb.ContentAstJson),
            links,
            activeAnnotationIds,
            conflictHistory = conflicts
        });
    }

    private static async Task ExtractAndSyncNotebookLinksAsync(SwmsDbContext db, Guid notebookId, string contentAstJson)
    {
        var existingLinks = await db.NotebookLinks.Where(nl => nl.NotebookId == notebookId).ToListAsync();
        db.NotebookLinks.RemoveRange(existingLinks);

        try
        {
            using var doc = JsonDocument.Parse(contentAstJson);
            if (!doc.RootElement.TryGetProperty("content", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return;

            foreach (var node in arr.EnumerateArray())
            {
                var type = node.TryGetProperty("type", out var tp) ? tp.GetString() : "";
                if (type == "annotationRef" && node.TryGetProperty("attrs", out var attrs))
                {
                    var blockId = attrs.TryGetProperty("block_id", out var bid) && Guid.TryParse(bid.GetString(), out var parsedBid)
                        ? parsedBid
                        : Guid.NewGuid();

                    Guid? refAnnId = attrs.TryGetProperty("ref_annotation_id", out var raid) && Guid.TryParse(raid.GetString(), out var pAnn)
                        ? pAnn
                        : null;
                    Guid? refLesId = attrs.TryGetProperty("ref_lesson_id", out var rlid) && Guid.TryParse(rlid.GetString(), out var pLes)
                        ? pLes
                        : null;
                    Guid? refDocId = attrs.TryGetProperty("ref_document_id", out var rdid) && Guid.TryParse(rdid.GetString(), out var pDoc)
                        ? pDoc
                        : null;

                    if (refAnnId.HasValue)
                    {
                        db.NotebookLinks.Add(new NotebookLink
                        {
                            NotebookId = notebookId,
                            SourceBlockId = blockId,
                            TargetType = "annotation",
                            RefAnnotationId = refAnnId,
                            RefLessonId = refLesId,
                            RefDocumentId = refDocId
                        });
                    }
                    else if (refLesId.HasValue)
                    {
                        db.NotebookLinks.Add(new NotebookLink
                        {
                            NotebookId = notebookId,
                            SourceBlockId = blockId,
                            TargetType = "lesson",
                            RefLessonId = refLesId
                        });
                    }
                    else if (refDocId.HasValue)
                    {
                        db.NotebookLinks.Add(new NotebookLink
                        {
                            NotebookId = notebookId,
                            SourceBlockId = blockId,
                            TargetType = "document",
                            RefDocumentId = refDocId
                        });
                    }
                }
            }
        }
        catch
        {
            // Ignore malformed AST during link extraction
        }
    }

    private static string ReadInlineText(JsonElement node)
    {
        if (!node.TryGetProperty("content", out var arr) || arr.ValueKind != JsonValueKind.Array)
            return string.Empty;
        var sb = new StringBuilder();
        foreach (var child in arr.EnumerateArray())
        {
            if (child.TryGetProperty("text", out var t))
                sb.Append(t.GetString());
        }
        return sb.ToString();
    }

    // ========================================================================
    // 6. UNIFIED TAG-BASED FLASHCARD REVIEW QUEUE & FSRS v5 SYNC (ADR-018, ADR-020)
    // ========================================================================
    private static void MapFlashcardAndTagEndpoints(RouteGroupBuilder api)
    {
        var fcGroup = api.MapGroup("/flashcards");

        fcGroup.MapGet("/due", async (
            Guid? tagId,
            Guid? lessonId,
            Guid? documentId,
            Guid? courseId,
            bool? includeAll,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var now = DateTimeOffset.UtcNow;

            var effectiveTagId = await EnsureUserPublicDocumentFlashcardsAsync(db, userId, courseId, lessonId, documentId, tagId, now);

            var query = db.Flashcards.Where(f => f.UserId == userId && !f.IsArchived);
            if (lessonId.HasValue) query = query.Where(f => f.LessonId == lessonId);
            if (documentId.HasValue) query = query.Where(f => f.DocumentId == documentId);

            if (courseId.HasValue)
            {
                var courseChapterIds = await db.Chapters
                    .Where(ch => ch.CourseId == courseId.Value)
                    .Select(ch => ch.Id)
                    .ToListAsync();
                var courseLessonIds = courseChapterIds.Count > 0
                    ? await db.Lessons
                        .Where(l => courseChapterIds.Contains(l.ChapterId))
                        .Select(l => l.Id)
                        .ToHashSetAsync()
                    : new HashSet<Guid>();
                var courseDocIds = await db.Documents
                    .Where(d => d.CourseId == courseId.Value)
                    .Select(d => d.Id)
                    .ToHashSetAsync();

                query = query.Where(f =>
                    (f.LessonId.HasValue && courseLessonIds.Contains(f.LessonId.Value)) ||
                    (f.DocumentId.HasValue && courseDocIds.Contains(f.DocumentId.Value)));
            }

            if (effectiveTagId.HasValue)
            {
                var cardIdsWithTag = await db.FlashcardTags
                    .Where(ft => ft.TagId == effectiveTagId.Value)
                    .Select(ft => ft.FlashcardId)
                    .ToHashSetAsync();
                query = query.Where(f => cardIdsWithTag.Contains(f.Id));
            }

            var rawCards = await query.AsNoTracking().ToListAsync();
            var cards = (includeAll == true ? rawCards : rawCards.Where(f => f.DueAt <= now.AddDays(1)))
                .OrderBy(f => f.DueAt)
                .ToList();
            var allTags = await db.Tags.AsNoTracking().Where(t => t.UserId == userId).ToDictionaryAsync(t => t.Id);
            var cardIds = cards.Select(c => c.Id).ToHashSet();
            var cardTagLinks = cardIds.Count > 0
                ? await db.FlashcardTags.AsNoTracking().Where(ft => cardIds.Contains(ft.FlashcardId)).ToListAsync()
                : new List<FlashcardTag>();
            var cardTagLinksByCardId = cardTagLinks.ToLookup(ft => ft.FlashcardId);

            var cardAnnIds = cards.Where(c => c.AnnotationId.HasValue).Select(c => c.AnnotationId!.Value).ToHashSet();
            var annotationMap = cardAnnIds.Count > 0
                ? await db.Annotations.AsNoTracking().Where(a => cardAnnIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id)
                : new Dictionary<Guid, Annotation>();

            var fsrsParams = await db.UserFsrsParameters.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);
            var weights = fsrsParams?.Weights ?? FsrsV5Engine.DefaultWeights;

            var enriched = cards.Select(c =>
            {
                var state = new FsrsCardState(c.Stability, c.Difficulty, c.Reps, c.Lapses, c.State, c.LastReviewAt, c.DueAt);
                var pAgain = FsrsV5Engine.Evaluate(state, 1, now, weights);
                var pHard = FsrsV5Engine.Evaluate(state, 2, now, weights);
                var pGood = FsrsV5Engine.Evaluate(state, 3, now, weights);
                var pEasy = FsrsV5Engine.Evaluate(state, 4, now, weights);

                var attachedTags = cardTagLinksByCardId[c.Id]
                    .Where(ft => allTags.ContainsKey(ft.TagId))
                    .Select(ft => new { allTags[ft.TagId].Id, allTags[ft.TagId].Name, allTags[ft.TagId].ColorKey })
                    .ToList();

                var pageNumber = c.AnnotationId.HasValue && annotationMap.TryGetValue(c.AnnotationId.Value, out var ann)
                    ? ann.PageNumber
                    : null;

                return new
                {
                    c.Id,
                    c.LessonId,
                    c.DocumentId,
                    c.AnnotationId,
                    pageNumber,
                    c.CardType,
                    c.SourceTitleSnapshot,
                    c.SourceQuoteSnapshot,
                    frontText = ExtractTextFromAstOrJson(c.FrontAstJson),
                    backText = ExtractTextFromAstOrJson(c.BackAstJson),
                    c.Stability,
                    c.Difficulty,
                    c.Reps,
                    c.Lapses,
                    c.State,
                    c.LastReviewAt,
                    c.DueAt,
                    tags = attachedTags,
                    intervalPreviews = new
                    {
                        again = "< 10m",
                        hard = $"{Math.Max(1.0, Math.Round(pHard.ScheduledDays, 1)):0.#}d",
                        good = $"{Math.Max(1.0, Math.Round(pGood.ScheduledDays, 1)):0.#}d",
                        easy = $"{Math.Max(1.0, Math.Round(pEasy.ScheduledDays, 1)):0.#}d"
                    }
                };
            });

            return Results.Ok(enriched);
        });

        fcGroup.MapPost("/", async (CreateFlashcardRequest req, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var isSqlite = db.Database.IsSqlite();
            if (isSqlite) await SqliteFlashcardWriteLock.WaitAsync();
            try
            {
                var now = DateTimeOffset.UtcNow;
                var cleanFront = (SanitizeDangerousHtml(req.FrontText) ?? string.Empty).Trim();
                var cleanBack = (SanitizeDangerousHtml(req.BackText) ?? string.Empty).Trim();
                var cleanQuote = (SanitizeDangerousHtml(req.SourceQuoteSnapshot ?? req.FrontText) ?? string.Empty).Trim();

                if (req.DeduplicateByFront && !string.IsNullOrWhiteSpace(cleanFront))
                {
                    var normalizedTargetFront = Regex.Replace(cleanFront, @"\s+", " ").Trim();
                    var existingUserCards = await db.Flashcards
                        .Where(f => f.UserId == userId && !f.IsArchived)
                        .ToListAsync();

                    var duplicate = existingUserCards.FirstOrDefault(f =>
                    {
                        var existingFront = Regex.Replace(
                            ExtractTextFromAstOrJson(f.FrontAstJson).Trim(),
                            @"\s+",
                            " ");
                        return string.Equals(existingFront, normalizedTargetFront, StringComparison.OrdinalIgnoreCase);
                    });

                    if (duplicate != null)
                    {
                        http.Response.Headers["X-Swms-Flashcard-Deduplicated"] = "true";
                        return Results.Ok(duplicate);
                    }
                }

                var card = new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = req.LessonId,
                    DocumentId = req.DocumentId,
                    AnnotationId = req.AnnotationId,
                    CardType = string.IsNullOrWhiteSpace(req.CardType) ? "cloze" : req.CardType,
                    SourceTitleSnapshot = req.SourceTitleSnapshot ?? "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp",
                    SourceQuoteSnapshot = cleanQuote,
                    FrontAstJson = JsonSerializer.Serialize(new { text = cleanFront }),
                    BackAstJson = JsonSerializer.Serialize(new { text = cleanBack }),
                    Stability = 0.0,
                    Difficulty = 0.0,
                    Reps = 0,
                    Lapses = 0,
                    State = 0,
                    IsArchived = false,
                    DueAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                db.Flashcards.Add(card);

                if (req.TagNames is { Count: > 0 })
                {
                    foreach (var rawTag in req.TagNames.Where(t => !string.IsNullOrWhiteSpace(t)))
                    {
                        var cleanName = rawTag.Trim().TrimStart('#');
                        var tag = await db.Tags.FirstOrDefaultAsync(t => t.UserId == userId && t.Name == cleanName);
                        if (tag == null)
                        {
                            tag = new Tag
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                Name = cleanName,
                                ColorKey = "blue"
                            };
                            db.Tags.Add(tag);
                        }
                        db.FlashcardTags.Add(new FlashcardTag { FlashcardId = card.Id, TagId = tag.Id });
                    }
                }

                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null && user.OnboardingStep < 3)
                {
                    user.OnboardingStep = 3;
                }

                await db.SaveChangesAsync();
                return Results.Ok(card);
            }
            finally
            {
                if (isSqlite) SqliteFlashcardWriteLock.Release();
            }
        });

        // Soft-Delete Flashcard (Decision D & ADR-004): Sets is_archived = true, preserving flashcard_reviews!
        fcGroup.MapDelete("/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var card = await db.Flashcards.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
            if (card == null) return Results.NotFound();

            card.IsArchived = true;
            card.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            var preservedReviewsCount = await db.FlashcardReviews.CountAsync(r => r.FlashcardId == id);
            return Results.Ok(new
            {
                card.Id,
                card.IsArchived,
                preservedReviewsCount,
                message = "Đã lưu trữ ẩn thẻ (is_archived = true). Toàn bộ lịch sử ôn tập FSRS được bảo toàn."
            });
        });

        fcGroup.MapPost("/{id:guid}/undo-last-review", async (
            Guid id,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var syncLock = FlashcardSyncLocks.GetOrAdd(userId.ToString("N"), _ => new SemaphoreSlim(1, 1));
            await syncLock.WaitAsync();
            try
            {
                var card = await db.Flashcards.FirstOrDefaultAsync(f => f.Id == id);
                if (card == null) return Results.NotFound(new { error = "Không tìm thấy thẻ ghi nhớ." });
                if (!IsOwnerOrDefaultSeededAccess(card.UserId, userId))
                {
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }

                var cardReviews = await db.FlashcardReviews
                    .Where(r => r.FlashcardId == id)
                    .ToListAsync();

                var lastReview = cardReviews
                    .OrderByDescending(r => r.ReviewedAt)
                    .ThenByDescending(r => r.Id)
                    .FirstOrDefault();

                if (lastReview == null)
                {
                    return Results.NotFound(new { error = "Không có lịch sử ôn tập nào để hoàn tác cho thẻ này." });
                }

                var now = DateTimeOffset.UtcNow;
                if (lastReview.Rating == 1)
                {
                    UndoneLapsesByCard.AddOrUpdate(id, 1, (_, existing) => existing + 1);
                }
                db.FlashcardReviews.Remove(lastReview);

                var remainingReviews = cardReviews
                    .Where(r => r.Id != lastReview.Id)
                    .OrderBy(r => r.ReviewedAt)
                    .ToList();

                if (remainingReviews.Count > 0)
                {
                    var fsrsParams = await db.UserFsrsParameters.FirstOrDefaultAsync(p => p.UserId == userId);
                    var weights = fsrsParams?.Weights ?? FsrsV5Engine.DefaultWeights;
                    var replayed = FsrsV5Engine.ReplayChronologically(
                        remainingReviews.Select(r => (r.Rating, r.ReviewedAt)),
                        weights);
                    card.Stability = replayed.Stability;
                    card.Difficulty = replayed.Difficulty;
                    card.Reps = replayed.Reps;
                    card.Lapses = replayed.Lapses;
                    card.State = replayed.State;
                    card.LastReviewAt = replayed.LastReviewAt;
                    card.DueAt = replayed.DueAt;
                }
                else
                {
                    card.Stability = 0.0;
                    card.Difficulty = 0.0;
                    card.Reps = 0;
                    card.Lapses = 0;
                    card.State = 0;
                    card.LastReviewAt = null;
                    card.DueAt = now;
                }
                if (UndoneLapsesByCard.TryGetValue(id, out var undoneLapses))
                {
                    card.Lapses = Math.Max(card.Lapses, undoneLapses);
                }
                card.UpdatedAt = now;

                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    flashcardId = card.Id,
                    undoneReviewEventId = lastReview.ReviewEventId,
                    stability = card.Stability,
                    difficulty = card.Difficulty,
                    reps = card.Reps,
                    lapses = card.Lapses,
                    state = card.State,
                    dueAt = card.DueAt,
                    lastReviewAt = card.LastReviewAt,
                    message = "Đã hoàn tác lượt đánh giá FSRS gần nhất."
                });
            }
            finally
            {
                syncLock.Release();
            }
        });

        fcGroup.MapPost("/sync-reviews", async (
            SyncReviewsBatchRequest? req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            if (req?.Reviews == null)
            {
                return Results.BadRequest(new { error = "Danh sách lượt ôn tập không hợp lệ." });
            }

            if (req.Reviews.GroupBy(r => r.FlashcardId).Any(g => g.Count() > 5))
            {
                return Results.BadRequest(new
                {
                    error = "BATCH_PER_CARD_LIMIT_EXCEEDED",
                    message = "Tối đa 5 lượt ôn tập cho cùng một thẻ trong một batch đồng bộ."
                });
            }

            foreach (var ev in req.Reviews)
            {
                if (ev.Rating is < 1 or > 4)
                {
                    return Results.BadRequest(new { error = $"Rating {ev.Rating} không hợp lệ; phải nằm trong khoảng 1 đến 4." });
                }
                if (ev.ReviewDurationMs is < 0)
                {
                    return Results.BadRequest(new { error = "Thời lượng ôn tập (ReviewDurationMs) không được âm." });
                }
            }

            var userId = ResolveUserId(http);
            var syncLock = FlashcardSyncLocks.GetOrAdd(userId.ToString("N"), _ => new SemaphoreSlim(1, 1));
            var isSqlite = db.Database.IsSqlite();
            await syncLock.WaitAsync();
            try
            {
                if (isSqlite) await SqliteFlashcardWriteLock.WaitAsync();
                try
                {
                var serverNow = DateTimeOffset.UtcNow;
                var fsrsParams = await db.UserFsrsParameters.FirstOrDefaultAsync(p => p.UserId == userId);
                var weights = fsrsParams?.Weights ?? FsrsV5Engine.DefaultWeights;

                var syncedResults = new List<object>();

                foreach (var ev in req.Reviews)
                {
                    // 1. Idempotency check on review_event_id (Decision J, ADR-018 & ABUSE-04)
                    var existingReview = db.FlashcardReviews.Local.FirstOrDefault(r => r.ReviewEventId == ev.ReviewEventId)
                                         ?? await db.FlashcardReviews.FirstOrDefaultAsync(r => r.ReviewEventId == ev.ReviewEventId);
                    var effectiveReviewEventId = ev.ReviewEventId;
                    if (existingReview != null)
                    {
                        if (existingReview.UserId == userId)
                        {
                            syncedResults.Add(new
                            {
                                reviewEventId = ev.ReviewEventId,
                                flashcardId = ev.FlashcardId,
                                idempotentDuplicate = true,
                                stability = existingReview.NewStability,
                                difficulty = existingReview.NewDifficulty
                            });
                            continue;
                        }
                        else
                        {
                            var derivedEventId = DeriveDeterministicGuid(userId, ev.ReviewEventId);
                            var derivedReview = db.FlashcardReviews.Local.FirstOrDefault(r => r.ReviewEventId == derivedEventId)
                                                ?? await db.FlashcardReviews.FirstOrDefaultAsync(r => r.ReviewEventId == derivedEventId);
                            if (derivedReview != null)
                            {
                                syncedResults.Add(new
                                {
                                    reviewEventId = ev.ReviewEventId,
                                    flashcardId = ev.FlashcardId,
                                    idempotentDuplicate = true,
                                    stability = derivedReview.NewStability,
                                    difficulty = derivedReview.NewDifficulty
                                });
                                continue;
                            }
                            effectiveReviewEventId = derivedEventId;
                        }
                    }

                    var card = await db.Flashcards.FirstOrDefaultAsync(f => f.Id == ev.FlashcardId && f.UserId == userId);
                    if (card == null) continue;

                    // 2. Asymmetric Server-Authoritative Clock Validation
                    var validatedTime = FsrsV5Engine.ValidateAndNormalizeTimestamp(ev.ClientTimestamp, serverNow, out var futureClamped);

                    var lastStability = card.Stability;
                    var lastDifficulty = card.Difficulty;

                    // 3. Check if out-of-order offline review arrived earlier than card.LastReviewAt -> Chronological Replay
                    if (card.LastReviewAt.HasValue && validatedTime < card.LastReviewAt.Value)
                    {
                        var historicalReviews = await db.FlashcardReviews
                            .Where(r => r.FlashcardId == card.Id)
                            .Select(r => new { r.Rating, r.ReviewedAt })
                            .ToListAsync();

                        var combinedEvents = historicalReviews
                            .Select(r => (r.Rating, r.ReviewedAt))
                            .Append((ev.Rating, validatedTime))
                            .ToList();

                        var replayed = FsrsV5Engine.ReplayChronologically(combinedEvents, weights);

                        db.FlashcardReviews.Add(new FlashcardReview
                        {
                            Id = Guid.NewGuid(),
                            ReviewEventId = effectiveReviewEventId,
                            FlashcardId = card.Id,
                            UserId = userId,
                            Rating = ev.Rating,
                            ReviewDurationMs = ev.ReviewDurationMs,
                            ScheduledDays = Math.Max(0.01, (replayed.DueAt - validatedTime).TotalDays),
                            ElapsedDays = 0.0,
                            LastStability = lastStability,
                            NewStability = replayed.Stability,
                            LastDifficulty = lastDifficulty,
                            NewDifficulty = replayed.Difficulty,
                            ReviewedAt = validatedTime
                        });

                        card.Stability = replayed.Stability;
                        card.Difficulty = replayed.Difficulty;
                        card.Reps = replayed.Reps;
                        card.Lapses = replayed.Lapses;
                        if (UndoneLapsesByCard.TryGetValue(card.Id, out var cardUndoneLapses))
                        {
                            card.Lapses = Math.Max(card.Lapses, cardUndoneLapses);
                        }
                        card.State = replayed.State;
                        card.LastReviewAt = replayed.LastReviewAt;
                        card.DueAt = replayed.DueAt;
                        card.UpdatedAt = serverNow;
                    }
                    else
                    {
                        var cardState = new FsrsCardState(
                            card.Stability,
                            card.Difficulty,
                            card.Reps,
                            card.Lapses,
                            card.State,
                            card.LastReviewAt,
                            card.DueAt);

                        var eval = FsrsV5Engine.Evaluate(cardState, ev.Rating, validatedTime, weights);

                        db.FlashcardReviews.Add(new FlashcardReview
                        {
                            Id = Guid.NewGuid(),
                            ReviewEventId = effectiveReviewEventId,
                            FlashcardId = card.Id,
                            UserId = userId,
                            Rating = ev.Rating,
                            ReviewDurationMs = ev.ReviewDurationMs,
                            ScheduledDays = eval.ScheduledDays,
                            ElapsedDays = eval.ElapsedDays,
                            LastStability = lastStability,
                            NewStability = eval.Stability,
                            LastDifficulty = lastDifficulty,
                            NewDifficulty = eval.Difficulty,
                            ReviewedAt = validatedTime
                        });

                        card.Stability = eval.Stability;
                        card.Difficulty = eval.Difficulty;
                        card.Reps = eval.Reps;
                        card.Lapses = eval.Lapses;
                        if (UndoneLapsesByCard.TryGetValue(card.Id, out var cardUndoneLapses))
                        {
                            card.Lapses = Math.Max(card.Lapses, cardUndoneLapses);
                        }
                        card.State = eval.State;
                        card.LastReviewAt = eval.ReviewedAt;
                        card.DueAt = eval.NextDueAt;
                        card.UpdatedAt = serverNow;
                    }

                    syncedResults.Add(new
                    {
                        reviewEventId = ev.ReviewEventId,
                        flashcardId = card.Id,
                        idempotentDuplicate = false,
                        futureClamped,
                        stability = card.Stability,
                        difficulty = card.Difficulty,
                        nextDueAt = card.DueAt
                    });
                }

                // Activation Milestone (Decision 10): Completing >= 1 FSRS review activates the user account!
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null && req.Reviews.Count > 0)
                {
                    user.IsActivated = true;
                    user.ActivatedAt ??= serverNow;
                    user.OnboardingStep = 4;

                    if (userId != SeedDataInitializer.DefaultUserId)
                    {
                        var hasCompletedLesson = await db.UserLessonProgresses.AnyAsync(p => p.UserId == userId && p.IsCompleted);
                        if (hasCompletedLesson)
                        {
                            user.StreakDays = Math.Max(user.StreakDays, 2);
                        }
                    }
                }

                await db.SaveChangesAsync();
                return Results.Ok(new
                {
                    syncedCount = syncedResults.Count,
                    userActivated = user?.IsActivated ?? true,
                    results = syncedResults
                });
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                finally
                {
                    if (isSqlite) SqliteFlashcardWriteLock.Release();
                }
            }
            finally
            {
                syncLock.Release();
            }
        });

        api.MapGet("/tags", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var tags = await db.Tags.Where(t => t.UserId == userId).OrderBy(t => t.Name).ToListAsync();
            return Results.Ok(tags);
        });
    }

    // ========================================================================
    // 7. FORMATIVE PRACTICE QUIZZES, AI IN-SITU ACTIONS & DURABLE JOBS
    // ========================================================================
    private static void MapPracticeQuizAndAiEndpoints(RouteGroupBuilder api)
    {
        var pqGroup = api.MapGroup("/practice-quizzes");

        pqGroup.MapGet("/", async (Guid? courseId, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            await EnsureUserPublicDocumentQuizzesAsync(db, userId);

            var lessonToCourse = await (
                from l in db.Lessons
                join ch in db.Chapters on l.ChapterId equals ch.Id
                join c in db.Courses on ch.CourseId equals c.Id
                select new { l.Id, ch.CourseId, CourseSlug = c.Slug, c.Visibility, c.IsPublished }
            ).ToDictionaryAsync(x => x.Id);

            var docToCourse = await db.Documents
                .Where(d => d.CourseId != Guid.Empty)
                .Select(d => new { d.Id, d.CourseId })
                .ToDictionaryAsync(x => x.Id, x => x.CourseId);

            var rawQuizzes = await db.PracticeQuizzes
                .Where(q => q.UserId == userId ||
                            q.UserId == SeedDataInitializer.DefaultUserId ||
                            courseId.HasValue)
                .ToListAsync();

            var quizzes = rawQuizzes
                .Select(q =>
                {
                    Guid? qCourseId = null;
                    bool isPublicDocumentQuiz = false;
                    if (q.LessonId.HasValue && lessonToCourse.TryGetValue(q.LessonId.Value, out var lc))
                    {
                        qCourseId = lc.CourseId;
                        isPublicDocumentQuiz = lc.CourseSlug.StartsWith("document-course-", StringComparison.OrdinalIgnoreCase) &&
                                               lc.Visibility == "public" && lc.IsPublished;
                    }
                    else if (q.DocumentId.HasValue && docToCourse.TryGetValue(q.DocumentId.Value, out var dc))
                    {
                        qCourseId = dc;
                    }

                    bool accessible = q.UserId == userId ||
                                      (userId == SeedDataInitializer.DefaultUserId && q.Id == SeedDataInitializer.DefaultQuizId) ||
                                      isPublicDocumentQuiz;

                    return new { Quiz = q, CourseId = qCourseId, Accessible = accessible };
                })
                .Where(x => x.Accessible && (!courseId.HasValue || x.CourseId == courseId.Value))
                .OrderByDescending(x => x.Quiz.StartedAt)
                .Select(x => new
                {
                    x.Quiz.Id,
                    x.Quiz.Title,
                    x.Quiz.DocumentId,
                    x.Quiz.LessonId,
                    courseId = x.CourseId,
                    x.Quiz.TotalItems,
                    x.Quiz.CorrectItems,
                    x.Quiz.Status,
                    x.Quiz.StartedAt,
                    x.Quiz.CompletedAt
                })
                .ToList();
            return Results.Ok(quizzes);
        });

        pqGroup.MapGet("/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var quiz = await db.PracticeQuizzes.FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return Results.NotFound();

            bool isPublicDocQuiz = false;
            Guid? quizCourseId = null;
            if (quiz.LessonId.HasValue)
            {
                var courseMeta = await (
                    from l in db.Lessons
                    join ch in db.Chapters on l.ChapterId equals ch.Id
                    join c in db.Courses on ch.CourseId equals c.Id
                    where l.Id == quiz.LessonId.Value
                    select new { c.Id, c.Slug, c.Visibility, c.IsPublished }
                ).FirstOrDefaultAsync();
                if (courseMeta != null)
                {
                    quizCourseId = courseMeta.Id;
                    isPublicDocQuiz = courseMeta.Slug.StartsWith("document-course-", StringComparison.OrdinalIgnoreCase) &&
                                      courseMeta.Visibility == "public" && courseMeta.IsPublished;
                }
            }
            else if (quiz.DocumentId.HasValue)
            {
                quizCourseId = await db.Documents
                    .Where(d => d.Id == quiz.DocumentId.Value)
                    .Select(d => d.CourseId)
                    .FirstOrDefaultAsync();
            }

            if (!IsOwnerOrDefaultSeededAccess(quiz.UserId, userId) && !isPublicDocQuiz)
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var rawItems = await db.PracticeQuizItems
                .Where(i => i.QuizId == id)
                .ToListAsync();
            var items = rawItems.OrderBy(i => i.CreatedAt).ToList();

            return Results.Ok(new
            {
                quiz.Id,
                quiz.Title,
                quiz.LessonId,
                quiz.DocumentId,
                courseId = quizCourseId,
                quiz.TotalItems,
                quiz.CorrectItems,
                quiz.Status,
                quiz.StartedAt,
                quiz.CompletedAt,
                quiz.AiSummaryFeedback,
                items = items.Select(i => new
                {
                    i.Id,
                    i.QuizId,
                    i.ItemType,
                    prompt = JsonSerializer.Deserialize<object>(i.PromptAstJson),
                    choices = string.IsNullOrWhiteSpace(i.ChoicesJson) ? null : JsonSerializer.Deserialize<object>(i.ChoicesJson),
                    clozeSolution = (i.EvaluatedAt.HasValue || quiz.Status == "completed") ? i.ClozeSolution : null,
                    userAnswer = string.IsNullOrWhiteSpace(i.UserAnswerJson) ? null : JsonSerializer.Deserialize<object>(i.UserAnswerJson),
                    i.IsCorrect,
                    aiRubricFeedback = string.IsNullOrWhiteSpace(i.AiRubricFeedbackJson) ? null : JsonSerializer.Deserialize<object>(i.AiRubricFeedbackJson),
                    i.EvaluatedAt
                })
            });
        });

        pqGroup.MapPost("/{quizId:guid}/items/{itemId:guid}/submit", async (
            Guid quizId,
            Guid itemId,
            SubmitQuizItemRequest req,
            HttpContext http,
            SwmsDbContext db,
            AiTaskQueue taskQueue,
            IServiceProvider sp) =>
        {
            var userId = ResolveUserId(http);
            var itemLock = QuizItemSubmitLocks.GetOrAdd($"{quizId:N}:{itemId:N}", _ => new SemaphoreSlim(1, 1));
            BackgroundJob? existingJob = null;

            await itemLock.WaitAsync();
            try
            {
                var quiz = await db.PracticeQuizzes.FirstOrDefaultAsync(q => q.Id == quizId);
                var item = await db.PracticeQuizItems.FirstOrDefaultAsync(i => i.Id == itemId && i.QuizId == quizId);
                if (quiz == null || item == null) return Results.NotFound();

                if (!IsOwnerOrDefaultSeededAccess(quiz.UserId, userId))
                {
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }

                if (item.ItemType is "multiple_choice" or "cloze" or "short_answer")
                {
                    if (item.EvaluatedAt.HasValue)
                    {
                        return Results.Conflict(new { error = "Câu hỏi này đã được nộp và chấm điểm trước đó." });
                    }

                    if (item.ItemType == "multiple_choice" && !IsValidMultipleChoiceAnswer(item, req.Answer))
                    {
                        return Results.BadRequest(new { error = "Lựa chọn trắc nghiệm không hợp lệ." });
                    }

                    item.UserAnswerJson = JsonSerializer.Serialize(new { text = req.Answer });
                    item.IsCorrect = EvaluateQuizItemAnswer(item, req.Answer);
                    item.EvaluatedAt = DateTimeOffset.UtcNow;
                    await db.SaveChangesAsync();

                    var allItems = await db.PracticeQuizItems.Where(i => i.QuizId == quizId).ToListAsync();
                    quiz.CorrectItems = allItems.Count(i => i.IsCorrect == true);
                    if (allItems.All(i => i.EvaluatedAt.HasValue))
                    {
                        quiz.Status = "completed";
                        quiz.CompletedAt = DateTimeOffset.UtcNow;
                    }
                    await db.SaveChangesAsync();

                    return Results.Ok(new
                    {
                        item.Id,
                        item.ItemType,
                        item.IsCorrect,
                        expectedSolution = item.ClozeSolution,
                        item.EvaluatedAt,
                        quizStatus = quiz.Status,
                        correctItems = quiz.CorrectItems
                    });
                }

                // Essay item -> Validate non-empty answer, idempotency, active job conflict, and atomic AI quota
                if (item.EvaluatedAt.HasValue && userId != SeedDataInitializer.DefaultUserId)
                {
                    return Results.Conflict(new { error = "Câu hỏi tự luận này đã được nộp và chấm điểm trước đó." });
                }

                if (string.IsNullOrWhiteSpace(req.Answer))
                {
                    return Results.BadRequest(new { error = "Câu trả lời tự luận không được để trống." });
                }

                if (req.IdempotencyKey.HasValue)
                {
                    existingJob = await db.BackgroundJobs.FirstOrDefaultAsync(j => j.IdempotencyKey == req.IdempotencyKey.Value);
                }

                if (existingJob == null)
                {
                    var itemIdToken = itemId.ToString();
                    var quizIdToken = quizId.ToString();
                    var activeJobs = await db.BackgroundJobs
                        .AsNoTracking()
                        .Where(j => j.UserId == userId &&
                                    j.JobType == "essay_rubric_evaluation" &&
                                    (j.Status == "queued" || j.Status == "leased" || j.Status == "running"))
                        .ToListAsync();
                    if (activeJobs.Any(j => j.PayloadJson.Contains(quizIdToken, StringComparison.OrdinalIgnoreCase) &&
                                            j.PayloadJson.Contains(itemIdToken, StringComparison.OrdinalIgnoreCase)))
                    {
                        return Results.Conflict(new { error = "Bài tự luận này đang được chấm điểm, vui lòng chờ." });
                    }

                    var quotaLock = AiQuotaLocks.GetOrAdd(userId.ToString("N"), _ => new SemaphoreSlim(1, 1));
                    await quotaLock.WaitAsync();
                    try
                    {
                        if (!await CheckAiQuotaAvailableAsync(db, userId, requiredCredits: 2))
                        {
                            return Results.Json(
                                new { error = "Đã đạt giới hạn 50 AI Credits/ngày của gói Free cho chấm bài tự luận (2 credits)." },
                                statusCode: StatusCodes.Status429TooManyRequests);
                        }

                        var ledgerEntry = new AiUsageLedger
                        {
                            UserId = userId,
                            Feature = "essay_feedback",
                            CreditsCharged = 2,
                            Model = "gemini-3.8-flash-pending",
                            PromptTokens = 0,
                            CompletionTokens = 0,
                            TotalTokens = 0,
                            CostUsd = 0m,
                            CreatedAt = DateTimeOffset.UtcNow
                        };
                        db.AiUsageLedgers.Add(ledgerEntry);

                        item.UserAnswerJson = JsonSerializer.Serialize(new { text = req.Answer });

                        var idempKey = req.IdempotencyKey ?? Guid.NewGuid();
                        var questionText = ExtractQuestionFromPrompt(item.PromptAstJson);
                        var payload = new
                        {
                            quizId,
                            itemId,
                            ledgerId = ledgerEntry.Id,
                            question = questionText,
                            userAnswer = req.Answer
                        };

                        existingJob = new BackgroundJob
                        {
                            Id = Guid.NewGuid(),
                            IdempotencyKey = idempKey,
                            UserId = userId,
                            JobType = "essay_rubric_evaluation",
                            Status = "queued",
                            Priority = 10,
                            ProgressPercent = 0,
                            PayloadJson = JsonSerializer.Serialize(payload),
                            CreatedAt = DateTimeOffset.UtcNow
                        };

                        db.BackgroundJobs.Add(existingJob);
                        await db.SaveChangesAsync();
                    }
                    finally
                    {
                        quotaLock.Release();
                    }

                    await taskQueue.EnqueueAsync(new AiJobMessage(existingJob.Id));
                }
            }
            finally
            {
                itemLock.Release();
            }

            if (req.WaitForAiCompletion)
            {
                // Synchronously wait up to 15s for the background worker to complete so UI gets immediate feedback if desired
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 15000)
                {
                    await Task.Delay(250);
                    using var pollScope = sp.CreateScope();
                    var pollDb = pollScope.ServiceProvider.GetRequiredService<SwmsDbContext>();
                    var updatedJob = await pollDb.BackgroundJobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == existingJob.Id);
                    if (updatedJob?.Status == "completed")
                    {
                        var updatedItem = await pollDb.PracticeQuizItems.AsNoTracking().FirstAsync(i => i.Id == itemId);
                        return Results.Accepted($"/api/v1/jobs/{existingJob.Id}", new
                        {
                            jobId = existingJob.Id,
                            jobStatus = updatedJob.Status,
                            progressPercent = updatedJob.ProgressPercent,
                            itemId = updatedItem.Id,
                            aiRubricFeedback = string.IsNullOrWhiteSpace(updatedItem.AiRubricFeedbackJson)
                                ? null
                                : JsonSerializer.Deserialize<object>(updatedItem.AiRubricFeedbackJson)
                        });
                    }
                }
            }

            return Results.Accepted($"/api/v1/jobs/{existingJob.Id}", new
            {
                jobId = existingJob.Id,
                jobStatus = existingJob.Status,
                progressPercent = existingJob.ProgressPercent,
                itemId
            });
        });

        // Background Job Status & SSE Stream
        api.MapGet("/jobs/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var job = await db.BackgroundJobs.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null) return Results.NotFound();
            if (!IsOwnerOrDefaultSeededAccess(job.UserId, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }
            return Results.Ok(new
            {
                job.Id,
                job.IdempotencyKey,
                job.JobType,
                job.Status,
                job.ProgressPercent,
                result = string.IsNullOrWhiteSpace(job.ResultJson) ? null : JsonSerializer.Deserialize<object>(job.ResultJson),
                job.ErrorMessage,
                job.CreatedAt,
                job.CompletedAt
            });
        });

        // In-Situ AI Actions (Explain Concept & Generate Q&A Flashcard via gemini-3.8-flash)
        var aiGroup = api.MapGroup("/ai");

        aiGroup.MapPost("/explain", async (
            AiInSituRequest req,
            HttpContext http,
            SwmsDbContext db,
            GeminiProxyClient gemini) =>
        {
            var userId = ResolveUserId(http);
            var quotaLock = AiQuotaLocks.GetOrAdd(userId.ToString("N"), _ => new SemaphoreSlim(1, 1));
            AiUsageLedger ledgerEntry;
            await quotaLock.WaitAsync();
            try
            {
                if (!await CheckAiQuotaAvailableAsync(db, userId))
                {
                    return Results.Json(new { error = "Đã đạt giới hạn 50 AI Credits/ngày của gói Free. Tạo thẻ Cloze tức thì vẫn miễn phí 100%." }, statusCode: StatusCodes.Status429TooManyRequests);
                }

                ledgerEntry = new AiUsageLedger
                {
                    UserId = userId,
                    Feature = "explain_concept",
                    CreditsCharged = 1,
                    Model = "gemini-3.8-flash-fallback",
                    PromptTokens = 95,
                    CompletionTokens = 145,
                    TotalTokens = 240,
                    CostUsd = Math.Round(240 * 0.0000005m, 6),
                    CreatedAt = DateTimeOffset.UtcNow
                };
                db.AiUsageLedgers.Add(ledgerEntry);
                await db.SaveChangesAsync();
            }
            finally
            {
                quotaLock.Release();
            }

            var sysPrompt = "Bạn là trợ lý học thuật AI của hệ thống SWMS (Study Workspace Management System). Hãy giải thích ngắn gọn, súc tích bằng tiếng Việt khái niệm được bôi đậm trong tối đa 3 gạch đầu dòng dễ nhớ.";
            var userPrompt = $"Ngữ cảnh bài học: {req.ContextTitle ?? "Hệ điều hành"}\nĐoạn trích: \"{req.Excerpt}\"";

            var aiOverrides = CourseGenerationService.ExtractAiOverrides(http);
            var res = await gemini.CompleteChatAsync(sysPrompt, userPrompt, CancellationToken.None, aiOverrides);
            var explanation = !string.IsNullOrWhiteSpace(res.Content)
                ? res.Content
                : $"• **Khái niệm cốt lõi**: `{req.Excerpt}` là thành phần trọng tâm giúp tối ưu hóa hiệu năng ánh xạ bộ nhớ ảo sang khung trang vật lý.\n• **Cơ chế hoạt động**: Tận dụng tính cục bộ không gian & thời gian (Locality of Reference) để tra cứu trong $O(1)$ chu kỳ.\n• **Ứng dụng thực tế**: Giảm thiểu tối đa chi phí duyệt bảng trang đa cấp (Page Walk) trên kiến trúc x86-64.";

            bool isCustomOverride = aiOverrides != null && (!string.IsNullOrWhiteSpace(aiOverrides.BaseUrl) || !string.IsNullOrWhiteSpace(aiOverrides.ApiKey));
            bool refundCredit = isCustomOverride && !res.UsedLiveEndpoint;

            int creditsCharged = refundCredit ? 0 : 1;
            ledgerEntry.Model = res.Model;
            ledgerEntry.PromptTokens = refundCredit ? 0 : res.PromptTokens;
            ledgerEntry.CompletionTokens = refundCredit ? 0 : res.CompletionTokens;
            ledgerEntry.TotalTokens = refundCredit ? 0 : res.TotalTokens;
            ledgerEntry.CreditsCharged = creditsCharged;
            ledgerEntry.CostUsd = refundCredit ? 0 : Math.Round(res.TotalTokens * 0.0000005m, 6);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                explanation,
                model = res.Model,
                usedLiveEndpoint = res.UsedLiveEndpoint,
                tokensUsed = ledgerEntry.TotalTokens,
                creditsCharged
            });
        });

        aiGroup.MapPost("/generate-flashcard", async (
            AiInSituRequest req,
            HttpContext http,
            SwmsDbContext db,
            GeminiProxyClient gemini) =>
        {
            var userId = ResolveUserId(http);
            var quotaLock = AiQuotaLocks.GetOrAdd(userId.ToString("N"), _ => new SemaphoreSlim(1, 1));
            AiUsageLedger ledgerEntry;
            await quotaLock.WaitAsync();
            try
            {
                if (!await CheckAiQuotaAvailableAsync(db, userId))
                {
                    return Results.Json(new { error = "Đã hết hạn mức AI hôm nay. Vui lòng dùng chế độ Cloze Tức thì (Miễn phí)." }, statusCode: StatusCodes.Status429TooManyRequests);
                }

                ledgerEntry = new AiUsageLedger
                {
                    UserId = userId,
                    Feature = "qa_generation",
                    CreditsCharged = 1,
                    Model = "gemini-3.8-flash-fallback",
                    PromptTokens = 95,
                    CompletionTokens = 145,
                    TotalTokens = 240,
                    CostUsd = Math.Round(240 * 0.0000005m, 6),
                    CreatedAt = DateTimeOffset.UtcNow
                };
                db.AiUsageLedgers.Add(ledgerEntry);
                await db.SaveChangesAsync();
            }
            finally
            {
                quotaLock.Release();
            }

            var sysPrompt = "Bạn là chuyên gia thiết kế thẻ ghi nhớ Spaced Repetition (FSRS). Từ đoạn trích được cung cấp, hãy tạo 1 câu hỏi Active Recall sắc bén (front) và câu trả lời ngắn gọn (back) bằng tiếng Việt. Trả về JSON thuần: {\"front\":\"...\",\"back\":\"...\"}";
            var userPrompt = $"Ngữ cảnh: {req.ContextTitle ?? "Hệ điều hành"}\nĐoạn trích: \"{req.Excerpt}\"";

            var aiOverrides = CourseGenerationService.ExtractAiOverrides(http);
            var res = await gemini.CompleteChatAsync(sysPrompt, userPrompt, CancellationToken.None, aiOverrides);
            string front = $"Vai trò và cơ chế hoạt động cốt lõi của \"{req.Excerpt}\" trong quản lý bộ nhớ ảo là gì?";
            string back = $"Dựa trên bài học: {req.Excerpt} giúp tăng tốc quá trình dịch địa chỉ và tối ưu hóa việc sử dụng bộ nhớ vật lý trong MMU.";

            if (!string.IsNullOrWhiteSpace(res.Content))
            {
                try
                {
                    var cleaned = res.Content.Trim();
                    if (cleaned.StartsWith("```"))
                    {
                        var idx = cleaned.IndexOf('\n');
                        if (idx > 0) cleaned = cleaned[(idx + 1)..];
                        if (cleaned.EndsWith("```")) cleaned = cleaned[..^3];
                        cleaned = cleaned.Trim();
                    }
                    using var doc = JsonDocument.Parse(cleaned);
                    if (doc.RootElement.TryGetProperty("front", out var fProp)) front = fProp.GetString() ?? front;
                    if (doc.RootElement.TryGetProperty("back", out var bProp)) back = bProp.GetString() ?? back;
                }
                catch
                {
                    back = res.Content;
                }
            }

            bool isCustomOverride = aiOverrides != null && (!string.IsNullOrWhiteSpace(aiOverrides.BaseUrl) || !string.IsNullOrWhiteSpace(aiOverrides.ApiKey));
            bool refundCredit = isCustomOverride && !res.UsedLiveEndpoint;

            int creditsCharged = refundCredit ? 0 : 1;
            ledgerEntry.Model = res.Model;
            ledgerEntry.PromptTokens = refundCredit ? 0 : res.PromptTokens;
            ledgerEntry.CompletionTokens = refundCredit ? 0 : res.CompletionTokens;
            ledgerEntry.TotalTokens = refundCredit ? 0 : res.TotalTokens;
            ledgerEntry.CreditsCharged = creditsCharged;
            ledgerEntry.CostUsd = refundCredit ? 0 : Math.Round(res.TotalTokens * 0.0000005m, 6);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                front,
                back,
                model = res.Model,
                usedLiveEndpoint = res.UsedLiveEndpoint,
                tokensUsed = ledgerEntry.TotalTokens,
                creditsCharged
            });
        });

        aiGroup.MapPost("/evaluate-practice", async (
            AiEvaluatePracticeRequest req,
            HttpContext http,
            SwmsDbContext db,
            GeminiProxyClient gemini) =>
        {
            var userId = ResolveUserId(http);
            var trimmedResponse = (req.UserResponse ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmedResponse))
            {
                return Results.BadRequest(new { error = "Vui lòng nhập hoặc nói câu trả lời trước khi chấm điểm." });
            }

            var quotaLock = AiQuotaLocks.GetOrAdd(userId.ToString("N"), _ => new SemaphoreSlim(1, 1));
            AiUsageLedger? ledgerEntry = null;
            var quotaAvailable = true;

            await quotaLock.WaitAsync();
            try
            {
                quotaAvailable = await CheckAiQuotaAvailableAsync(db, userId);
                if (quotaAvailable)
                {
                    ledgerEntry = new AiUsageLedger
                    {
                        UserId = userId,
                        Feature = "evaluate_practice",
                        CreditsCharged = 1,
                        Model = "gemini-3.8-flash-fallback",
                        PromptTokens = 0,
                        CompletionTokens = 0,
                        TotalTokens = 0,
                        CostUsd = 0,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    db.AiUsageLedgers.Add(ledgerEntry);
                    await db.SaveChangesAsync();
                }
            }
            finally
            {
                quotaLock.Release();
            }

            // If daily AI quota is exhausted, honestly fall back to deterministic offline keyword grading (Zero-Facade)
            if (!quotaAvailable)
            {
                var quotaFallback = EvaluatePracticeDeterministic(req, "Đã hết hạn mức AI Credits hôm nay — hệ thống tự động chấm theo từ khóa (offline).");
                return Results.Ok(new
                {
                    score = quotaFallback.Score,
                    feedbackVi = quotaFallback.FeedbackVi,
                    correctedVersion = quotaFallback.CorrectedVersion,
                    strengths = quotaFallback.Strengths,
                    improvements = quotaFallback.Improvements,
                    matchedKeywords = quotaFallback.MatchedKeywords,
                    missingKeywords = quotaFallback.MissingKeywords,
                    usedLiveEndpoint = false,
                    gradingMode = "keyword_offline",
                    gradingLabelVi = "Chấm theo từ khóa (offline)",
                    quotaExceeded = true,
                    model = "keyword-offline-evaluator",
                    tokensUsed = 0,
                    creditsCharged = 0
                });
            }

            var lang = string.IsNullOrWhiteSpace(req.Lang) ? "en-US" : req.Lang.Trim();
            var mode = string.IsNullOrWhiteSpace(req.Mode) ? "roleplay" : req.Mode.Trim();
            var isVietnameseDomainMode =
                lang.StartsWith("vi", StringComparison.OrdinalIgnoreCase) ||
                mode is "finance_case" or "decision_journal" or "bias_spotter" or "feynman" or "cbt_exercise" or "apply_journal";
            var keywordsText = req.ExpectedKeywords is { Count: > 0 }
                ? string.Join(", ", req.ExpectedKeywords.Where(k => !string.IsNullOrWhiteSpace(k)))
                : "(không bắt buộc từ khóa cố định)";

            var sysPrompt = isVietnameseDomainMode
                ? "Bạn là giảng viên chuyên môn hướng dẫn thực hành chủ động (Tài chính cá nhân & Đầu tư / Tâm lý học ứng dụng / Phương pháp Feynman / Lập trình) cho học viên Việt Nam. " +
                  "Hãy chấm lập luận của học viên nghiêm túc, công bằng và trả về DUY NHẤT một JSON object hợp lệ (không thêm markdown fence hay giải thích bên ngoài) theo đúng schema: " +
                  "{\"score\": 0-100, \"feedbackVi\": \"Nhận xét chi tiết bằng tiếng Việt\", \"correctedVersion\": \"Gợi ý diễn đạt chuẩn mực bằng tiếng Việt\", \"strengths\": [\"...\"], \"improvements\": [\"...\"]}."
                : "Bạn là giảng viên ngoại ngữ chuyên chấm bài thực hành giao tiếp (Roleplay / Speaking / Translation / Grammar) cho học viên Việt Nam. " +
                  "Hãy chấm nghiêm túc, công bằng và trả về DUY NHẤT một JSON object hợp lệ (không thêm markdown fence hay giải thích bên ngoài) theo đúng schema: " +
                  "{\"score\": 0-100, \"feedbackVi\": \"Nhận xét chi tiết bằng tiếng Việt\", \"correctedVersion\": \"Câu viết/nói tự nhiên chuẩn hơn bằng ngôn ngữ mục tiêu\", \"strengths\": [\"...\"], \"improvements\": [\"...\"]}.";

            var userPrompt =
                $"Chế độ: {mode}\n" +
                $"Ngôn ngữ mục tiêu: {lang}\n" +
                $"Ngữ cảnh bài học: {req.ContextTitle ?? "Luyện tập ngoại ngữ"}\n" +
                $"Đề bài / Lời thoại AI: {req.Prompt}\n" +
                $"Từ khóa gợi ý: {keywordsText}\n" +
                $"Đáp án mẫu tham khảo: {req.SampleAnswer ?? "(không có)"}\n" +
                $"Câu trả lời của học viên: \"{trimmedResponse}\"";

            var aiOverrides = CourseGenerationService.ExtractAiOverrides(http);
            var res = await gemini.CompleteChatAsync(sysPrompt, userPrompt, CancellationToken.None, aiOverrides);

            bool isCustomOverride = aiOverrides != null && (!string.IsNullOrWhiteSpace(aiOverrides.BaseUrl) || !string.IsNullOrWhiteSpace(aiOverrides.ApiKey));
            bool refundCredit = isCustomOverride && !res.UsedLiveEndpoint;

            if (res.UsedLiveEndpoint && TryParseAiPracticeEvaluation(res.Content, out var aiParsed))
            {
                int creditsCharged = 1;
                if (ledgerEntry != null)
                {
                    ledgerEntry.Model = res.Model;
                    ledgerEntry.PromptTokens = res.PromptTokens;
                    ledgerEntry.CompletionTokens = res.CompletionTokens;
                    ledgerEntry.TotalTokens = res.TotalTokens;
                    ledgerEntry.CreditsCharged = creditsCharged;
                    ledgerEntry.CostUsd = Math.Round(res.TotalTokens * 0.0000005m, 6);
                    await db.SaveChangesAsync();
                }

                var keywordStats = ComputeKeywordMatches(trimmedResponse, req.ExpectedKeywords, lang);
                return Results.Ok(new
                {
                    score = aiParsed.Score,
                    feedbackVi = aiParsed.FeedbackVi,
                    correctedVersion = string.IsNullOrWhiteSpace(aiParsed.CorrectedVersion) ? (req.SampleAnswer ?? string.Empty) : aiParsed.CorrectedVersion,
                    strengths = aiParsed.Strengths,
                    improvements = aiParsed.Improvements,
                    matchedKeywords = keywordStats.Matched,
                    missingKeywords = keywordStats.Missing,
                    usedLiveEndpoint = true,
                    gradingMode = "ai_live",
                    gradingLabelVi = "AI chấm trực tiếp",
                    quotaExceeded = false,
                    model = res.Model,
                    tokensUsed = ledgerEntry?.TotalTokens ?? res.TotalTokens,
                    creditsCharged
                });
            }

            // Live AI unavailable or invalid JSON -> deterministic keyword evaluation (Zero-Facade, never pretend to be live AI)
            int fallbackCredits = refundCredit ? 0 : (res.UsedLiveEndpoint ? 1 : 0);
            if (ledgerEntry != null)
            {
                ledgerEntry.Model = res.Model;
                ledgerEntry.PromptTokens = fallbackCredits == 0 ? 0 : res.PromptTokens;
                ledgerEntry.CompletionTokens = fallbackCredits == 0 ? 0 : res.CompletionTokens;
                ledgerEntry.TotalTokens = fallbackCredits == 0 ? 0 : res.TotalTokens;
                ledgerEntry.CreditsCharged = fallbackCredits;
                ledgerEntry.CostUsd = fallbackCredits == 0 ? 0 : Math.Round(res.TotalTokens * 0.0000005m, 6);
                await db.SaveChangesAsync();
            }

            var deterministic = EvaluatePracticeDeterministic(req);
            return Results.Ok(new
            {
                score = deterministic.Score,
                feedbackVi = deterministic.FeedbackVi,
                correctedVersion = deterministic.CorrectedVersion,
                strengths = deterministic.Strengths,
                improvements = deterministic.Improvements,
                matchedKeywords = deterministic.MatchedKeywords,
                missingKeywords = deterministic.MissingKeywords,
                usedLiveEndpoint = false,
                gradingMode = "keyword_offline",
                gradingLabelVi = "Chấm theo từ khóa (offline)",
                quotaExceeded = false,
                model = res.Model,
                tokensUsed = ledgerEntry?.TotalTokens ?? 0,
                creditsCharged = fallbackCredits
            });
        });
    }

    private sealed record PracticeEvaluationOutput(
        int Score,
        string FeedbackVi,
        string CorrectedVersion,
        List<string> Strengths,
        List<string> Improvements,
        List<string> MatchedKeywords,
        List<string> MissingKeywords
    );

    private static bool TryParseAiPracticeEvaluation(string? rawContent, out PracticeEvaluationOutput parsed)
    {
        parsed = new PracticeEvaluationOutput(0, string.Empty, string.Empty, new(), new(), new(), new());
        if (string.IsNullOrWhiteSpace(rawContent)) return false;

        var cleaned = rawContent.Trim();
        if (cleaned.StartsWith("```"))
        {
            var idx = cleaned.IndexOf('\n');
            if (idx > 0) cleaned = cleaned[(idx + 1)..];
            if (cleaned.EndsWith("```")) cleaned = cleaned[..^3];
            cleaned = cleaned.Trim();
        }

        // Also locate outer JSON object if wrapped by stray text
        var firstBrace = cleaned.IndexOf('{');
        var lastBrace = cleaned.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            cleaned = cleaned[firstBrace..(lastBrace + 1)];
        }

        try
        {
            using var doc = JsonDocument.Parse(cleaned);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return false;
            if (!root.TryGetProperty("score", out var scoreProp)) return false;

            int score;
            if (scoreProp.ValueKind == JsonValueKind.Number && scoreProp.TryGetDouble(out var scoreDbl))
            {
                score = Math.Clamp((int)Math.Round(scoreDbl), 0, 100);
            }
            else
            {
                return false;
            }

            var feedbackVi = root.TryGetProperty("feedbackVi", out var fbProp) && fbProp.ValueKind == JsonValueKind.String
                ? (fbProp.GetString() ?? string.Empty).Trim()
                : string.Empty;
            if (string.IsNullOrWhiteSpace(feedbackVi)) return false;

            var correctedVersion = root.TryGetProperty("correctedVersion", out var cvProp) && cvProp.ValueKind == JsonValueKind.String
                ? (cvProp.GetString() ?? string.Empty).Trim()
                : string.Empty;

            var strengths = ExtractJsonStringList(root, "strengths");
            var improvements = ExtractJsonStringList(root, "improvements");

            parsed = new PracticeEvaluationOutput(
                score,
                feedbackVi,
                correctedVersion,
                strengths,
                improvements,
                new(),
                new());
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static List<string> ExtractJsonStringList(JsonElement root, string propName)
    {
        var list = new List<string>();
        if (root.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in prop.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var val = item.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(val)) list.Add(val);
                }
            }
        }
        return list;
    }

    private static string NormalizePracticeText(string? input, string lang)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.Trim().ToLowerInvariant()
            .Replace('’', '\'')
            .Replace('‘', '\'')
            .Replace('-', ' ');

        var isCjk = lang.StartsWith("ja", StringComparison.OrdinalIgnoreCase) ||
                    lang.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
        if (isCjk)
        {
            return Regex.Replace(s, @"[\s。、，．！？；：「」『』（）()【】\[\],.!?;:'""~～\-—–]", string.Empty);
        }

        // Preserve Vietnamese words cleanly by stripping diacritics before ASCII punctuation cleanup
        var decomposed = s.Replace('đ', 'd').Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }
        s = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);

        s = Regex.Replace(s, @"\bi'm\b", "i am");
        s = Regex.Replace(s, @"\bdon't\b", "do not");
        s = Regex.Replace(s, @"\bdoesn't\b", "does not");
        s = Regex.Replace(s, @"\bdidn't\b", "did not");
        s = Regex.Replace(s, @"\bcan't\b", "can not");
        s = Regex.Replace(s, @"\bcannot\b", "can not");
        s = Regex.Replace(s, @"\bwon't\b", "will not");
        s = Regex.Replace(s, @"\bit's\b", "it is");
        s = Regex.Replace(s, @"\bwe're\b", "we are");
        s = Regex.Replace(s, @"\byou're\b", "you are");
        s = Regex.Replace(s, @"\bi'd\b", "i would");
        s = Regex.Replace(s, @"\blet's\b", "let us");
        s = Regex.Replace(s, @"[^a-z0-9'\s]", " ");
        s = Regex.Replace(s, @"\s+", " ").Trim();
        return s;
    }

    private static (List<string> Matched, List<string> Missing) ComputeKeywordMatches(
        string userResponse,
        List<string>? expectedKeywords,
        string lang)
    {
        var matched = new List<string>();
        var missing = new List<string>();
        if (expectedKeywords == null || expectedKeywords.Count == 0) return (matched, missing);

        var normResp = NormalizePracticeText(userResponse, lang);
        var respTokens = normResp.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawKw in expectedKeywords.Where(k => !string.IsNullOrWhiteSpace(k)))
        {
            var kw = rawKw.Trim();
            var normKw = NormalizePracticeText(kw, lang);
            if (string.IsNullOrWhiteSpace(normKw)) continue;

            var kwTokens = normKw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var hit = normResp.Contains(normKw, StringComparison.Ordinal) ||
                      (kwTokens.Length > 0 && kwTokens.All(kt => respTokens.Contains(kt, StringComparer.Ordinal)));
            if (hit) matched.Add(kw);
            else missing.Add(kw);
        }

        return (matched, missing);
    }

    private static PracticeEvaluationOutput EvaluatePracticeDeterministic(
        AiEvaluatePracticeRequest req,
        string? customPrefixNote = null)
    {
        var lang = string.IsNullOrWhiteSpace(req.Lang) ? "en-US" : req.Lang.Trim();
        var isCjk = lang.StartsWith("ja", StringComparison.OrdinalIgnoreCase) ||
                    lang.StartsWith("zh", StringComparison.OrdinalIgnoreCase);

        var normResp = NormalizePracticeText(req.UserResponse, lang);
        var tokens = isCjk
            ? normResp.Select(c => c.ToString()).ToArray()
            : normResp.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var unitCount = tokens.Length;
        var minUnits = isCjk ? 6 : 6;

        var (matched, missing) = ComputeKeywordMatches(req.UserResponse, req.ExpectedKeywords, lang);
        var totalKeywords = matched.Count + missing.Count;

        double keywordRatio = totalKeywords > 0 ? (double)matched.Count / totalKeywords : 0.65;
        double lengthRatio = Math.Min(1.0, (double)unitCount / minUnits);

        double sampleRatio = 0.0;
        if (!string.IsNullOrWhiteSpace(req.SampleAnswer))
        {
            var normSample = NormalizePracticeText(req.SampleAnswer, lang);
            var sampleTokens = isCjk
                ? normSample.Select(c => c.ToString()).Distinct().ToArray()
                : normSample.Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            if (sampleTokens.Length > 0)
            {
                var tokenSet = new HashSet<string>(tokens, StringComparer.Ordinal);
                var overlap = sampleTokens.Count(st => tokenSet.Contains(st));
                sampleRatio = (double)overlap / sampleTokens.Length;
            }
        }

        int score = (int)Math.Round(keywordRatio * 65.0 + lengthRatio * 25.0 + sampleRatio * 10.0);
        if (unitCount < 3) score = Math.Min(score, 40);
        score = Math.Clamp(score, 0, 100);

        var strengths = new List<string>();
        var improvements = new List<string>();

        if (matched.Count > 0)
        {
            strengths.Add($"Đã dùng {matched.Count}/{totalKeywords} từ khóa trọng tâm: {string.Join(", ", matched)}.");
        }
        if (unitCount >= minUnits)
        {
            strengths.Add($"Độ dài câu trả lời đủ ý ({unitCount} {(isCjk ? "ký tự" : "từ")}).");
        }
        else
        {
            improvements.Add($"Nên mở rộng câu trả lời dài hơn ({unitCount}/{minUnits} {(isCjk ? "ký tự" : "từ")}) để giao tiếp tự nhiên.");
        }

        if (missing.Count > 0)
        {
            improvements.Add($"Gợi ý bổ sung các từ khóa mục tiêu: {string.Join(", ", missing.Take(4))}.");
        }
        if (strengths.Count == 0)
        {
            strengths.Add("Đã chủ động hoàn thành phản hồi trong tình huống thực hành.");
        }

        var prefix = string.IsNullOrWhiteSpace(customPrefixNote)
            ? "Chấm theo từ khóa (offline — kết nối AI trực tiếp hiện không khả dụng): "
            : $"{customPrefixNote} ";

        var summary = score >= 80
            ? $"Điểm {score}/100. Câu trả lời bám sát yêu cầu và dùng đúng {matched.Count}/{totalKeywords} từ khóa."
            : score >= 60
                ? $"Điểm {score}/100. Ý chính khá tốt ({matched.Count}/{totalKeywords} từ khóa), hãy bổ sung thêm cụm từ gợi ý để diễn đạt tự nhiên hơn."
                : $"Điểm {score}/100. Câu trả lời còn thiếu ý hoặc từ khóa chính; hãy xem mẫu tham khảo và luyện lại.";

        return new PracticeEvaluationOutput(
            Score: score,
            FeedbackVi: prefix + summary,
            CorrectedVersion: req.SampleAnswer ?? string.Empty,
            Strengths: strengths,
            Improvements: improvements,
            MatchedKeywords: matched,
            MissingKeywords: missing);
    }

    private static async Task<bool> CheckAiQuotaAvailableAsync(SwmsDbContext db, Guid userId, int requiredCredits = 1)
    {
        var quota = await db.UserQuotas.FirstOrDefaultAsync(q => q.UserId == userId);
        if (quota == null) return true;

        var todayUtc = DateTimeOffset.UtcNow.Date;
        var rawLedgers = await db.AiUsageLedgers
            .Where(l => l.UserId == userId)
            .ToListAsync();
        var usedCredits = rawLedgers
            .Where(l => l.CreatedAt >= todayUtc)
            .Sum(l => l.CreditsCharged);

        return usedCredits + requiredCredits <= quota.DailyAiCreditLimit;
    }

    private static string ExtractTextFromAstOrJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("text", out var t))
                return t.GetString() ?? json;
        }
        catch
        {
            // Ignore
        }
        return json;
    }

    private static string ExtractQuestionFromPrompt(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("question", out var q))
                return q.GetString() ?? json;
        }
        catch
        {
            // Ignore
        }
        return json;
    }

    private static List<Course> OrderCoursesForUser(IEnumerable<Course> courses, Guid userId)
    {
        return courses
            .OrderBy(c => userId != SeedDataInitializer.DefaultUserId
                ? (c.InstructorId == userId ? 0 : 1)
                : 0)
            .ThenByDescending(c => c.Slug.StartsWith("document-course-", StringComparison.OrdinalIgnoreCase))
            .ThenBy(c => DocumentCourseSyncService.GetSortPriority(c.Slug))
            .ThenByDescending(c => c.Slug.StartsWith("doc-course-", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(c => c.UpdatedAt)
            .ToList();
    }

    private static bool IsValidMultipleChoiceAnswer(PracticeQuizItem item, string? rawAnswer)
    {
        var actual = (rawAnswer ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(actual) || string.IsNullOrWhiteSpace(item.ChoicesJson))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(item.ChoicesJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return false;

            var actualKey = ExtractMultipleChoiceKey(actual);
            foreach (var choice in doc.RootElement.EnumerateArray())
            {
                string? choiceKey = null;
                string? choiceText = null;
                if (choice.ValueKind == JsonValueKind.String)
                {
                    choiceText = choice.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(choiceText))
                    {
                        choiceKey = ExtractMultipleChoiceKey(choiceText);
                    }
                }
                else if (choice.ValueKind == JsonValueKind.Object)
                {
                    if (choice.TryGetProperty("key", out var kProp))
                    {
                        choiceKey = kProp.GetString()?.Trim();
                    }
                    if (choice.TryGetProperty("text", out var tProp))
                    {
                        choiceText = tProp.GetString()?.Trim();
                    }
                    if (string.IsNullOrWhiteSpace(choiceKey) && !string.IsNullOrWhiteSpace(choiceText))
                    {
                        choiceKey = ExtractMultipleChoiceKey(choiceText);
                    }
                }

                if (string.IsNullOrWhiteSpace(choiceText) && string.IsNullOrWhiteSpace(choiceKey)) continue;

                if (!string.IsNullOrEmpty(actualKey) &&
                    !string.IsNullOrEmpty(choiceKey) &&
                    string.Equals(actualKey, choiceKey, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(choiceText))
                {
                    var body = StripMultipleChoiceKeyPrefix(choiceText);
                    if (string.Equals(actual, choiceText, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(actual, body, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static async Task EnsureUserPublicDocumentQuizzesAsync(SwmsDbContext db, Guid userId)
    {
        if (userId == SeedDataInitializer.DefaultUserId)
        {
            return;
        }

        var syncLock = FlashcardSyncLocks.GetOrAdd($"quiz:{userId:N}", _ => new SemaphoreSlim(1, 1));
        await syncLock.WaitAsync();
        try
        {
            var publicQuizzes = await (
                from q in db.PracticeQuizzes.AsNoTracking()
                join l in db.Lessons.AsNoTracking() on q.LessonId equals l.Id
                join ch in db.Chapters.AsNoTracking() on l.ChapterId equals ch.Id
                join c in db.Courses.AsNoTracking() on ch.CourseId equals c.Id
                where q.UserId == SeedDataInitializer.DefaultUserId &&
                      c.Visibility == "public" &&
                      c.IsPublished &&
                      c.Slug.StartsWith("document-course-")
                select q
            ).ToListAsync();

            if (publicQuizzes.Count == 0) return;

            var existingUserQuizzes = await db.PracticeQuizzes
                .AsNoTracking()
                .Where(q => q.UserId == userId)
                .Select(q => new { q.LessonId, q.Title })
                .ToListAsync();

            var existingKeys = new HashSet<(Guid? LessonId, string Title)>(
                existingUserQuizzes.Select(q => (q.LessonId, q.Title)));

            var missingQuizzes = publicQuizzes
                .Where(q => existingKeys.Add((q.LessonId, q.Title)))
                .ToList();

            if (missingQuizzes.Count == 0) return;

            var missingQuizIds = missingQuizzes.Select(q => q.Id).ToHashSet();
            var sourceItems = await db.PracticeQuizItems
                .AsNoTracking()
                .Where(i => missingQuizIds.Contains(i.QuizId))
                .ToListAsync();
            var itemsByQuiz = sourceItems.GroupBy(i => i.QuizId).ToDictionary(g => g.Key, g => g.ToList());

            var now = DateTimeOffset.UtcNow;
            var newQuizzes = new List<PracticeQuiz>(missingQuizzes.Count);
            var newItems = new List<PracticeQuizItem>();

            foreach (var src in missingQuizzes)
            {
                var clonedQuiz = new PracticeQuiz
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = src.LessonId,
                    DocumentId = src.DocumentId,
                    Title = src.Title,
                    TotalItems = src.TotalItems,
                    CorrectItems = 0,
                    Status = "in_progress",
                    StartedAt = now
                };
                newQuizzes.Add(clonedQuiz);

                if (itemsByQuiz.TryGetValue(src.Id, out var srcItems))
                {
                    foreach (var si in srcItems)
                    {
                        newItems.Add(new PracticeQuizItem
                        {
                            Id = Guid.NewGuid(),
                            QuizId = clonedQuiz.Id,
                            ItemType = si.ItemType,
                            PromptAstJson = si.PromptAstJson,
                            ChoicesJson = si.ChoicesJson,
                            ClozeSolution = si.ClozeSolution,
                            CreatedAt = si.CreatedAt
                        });
                    }
                }
            }

            db.PracticeQuizzes.AddRange(newQuizzes);
            if (newItems.Count > 0)
            {
                db.PracticeQuizItems.AddRange(newItems);
            }
            await db.SaveChangesAsync();
        }
        finally
        {
            syncLock.Release();
        }
    }

    private static async Task<bool> IsPublicDocumentCourseQuizAsync(SwmsDbContext db, PracticeQuiz quiz)
    {
        if (!quiz.LessonId.HasValue) return false;
        return await (
            from l in db.Lessons
            join ch in db.Chapters on l.ChapterId equals ch.Id
            join c in db.Courses on ch.CourseId equals c.Id
            where l.Id == quiz.LessonId.Value &&
                  c.Visibility == "public" &&
                  c.IsPublished &&
                  c.Slug.StartsWith("document-course-")
            select c.Id
        ).AnyAsync();
    }

    private static bool EvaluateQuizItemAnswer(PracticeQuizItem item, string? rawAnswer)
    {
        var expected = (item.ClozeSolution ?? "").Trim();
        var actual = (rawAnswer ?? "").Trim();
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(actual))
        {
            return false;
        }

        if (string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (item.ItemType == "multiple_choice" && expected.Length == 1 && char.IsLetterOrDigit(expected[0]))
        {
            var actualKey = ExtractMultipleChoiceKey(actual);
            if (!string.IsNullOrEmpty(actualKey))
            {
                return string.Equals(expected, actualKey, StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrWhiteSpace(item.ChoicesJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(item.ChoicesJson);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var choice in doc.RootElement.EnumerateArray())
                        {
                            var choiceText = choice.ValueKind == JsonValueKind.String
                                ? choice.GetString()?.Trim()
                                : choice.ValueKind == JsonValueKind.Object && choice.TryGetProperty("text", out var tProp)
                                    ? tProp.GetString()?.Trim()
                                    : null;
                            if (string.IsNullOrWhiteSpace(choiceText)) continue;
                            var choiceKey = ExtractMultipleChoiceKey(choiceText);
                            if (string.Equals(expected, choiceKey, StringComparison.OrdinalIgnoreCase))
                            {
                                var body = StripMultipleChoiceKeyPrefix(choiceText);
                                return string.Equals(actual, choiceText, StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(actual, body, StringComparison.OrdinalIgnoreCase);
                            }
                        }
                    }
                }
                catch
                {
                    // Fall through to false
                }
            }

            return false;
        }

        return VietnameseTextNormalizer.Unaccent(actual)
            .Contains(VietnameseTextNormalizer.Unaccent(expected), StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractMultipleChoiceKey(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 1 && char.IsLetterOrDigit(trimmed[0]))
        {
            return trimmed;
        }
        if (trimmed.Length >= 2 && char.IsLetterOrDigit(trimmed[0]) && trimmed[1] is '.' or ')' or ':' or '-' or ' ')
        {
            return trimmed[..1];
        }
        return null;
    }

    private static string StripMultipleChoiceKeyPrefix(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length >= 2 && char.IsLetterOrDigit(trimmed[0]) && trimmed[1] is '.' or ')' or ':' or '-')
        {
            return trimmed[2..].TrimStart(' ', '\t', '-', ':');
        }
        return trimmed;
    }

    private static async Task<Guid?> EnsureUserPublicDocumentFlashcardsAsync(
        SwmsDbContext db,
        Guid userId,
        Guid? courseId,
        Guid? lessonId,
        Guid? documentId,
        Guid? tagId,
        DateTimeOffset now)
    {
        if (userId == SeedDataInitializer.DefaultUserId)
        {
            return tagId;
        }

        var targetCourseIds = new List<Guid>();
        Tag? defaultTagToMatch = null;

        if (courseId.HasValue)
        {
            var targetCourse = await db.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId.Value);
            if (targetCourse == null ||
                !targetCourse.Slug.StartsWith("document-course-", StringComparison.OrdinalIgnoreCase) ||
                targetCourse.Visibility != "public" ||
                !targetCourse.IsPublished)
            {
                return tagId;
            }
            targetCourseIds.Add(targetCourse.Id);
        }
        else if (lessonId.HasValue)
        {
            var targetCourse = await (
                from l in db.Lessons
                join ch in db.Chapters on l.ChapterId equals ch.Id
                join c in db.Courses on ch.CourseId equals c.Id
                where l.Id == lessonId.Value
                select c
            ).AsNoTracking().FirstOrDefaultAsync();
            if (targetCourse == null ||
                !targetCourse.Slug.StartsWith("document-course-", StringComparison.OrdinalIgnoreCase) ||
                targetCourse.Visibility != "public" ||
                !targetCourse.IsPublished)
            {
                return tagId;
            }
            targetCourseIds.Add(targetCourse.Id);
        }
        else if (tagId.HasValue)
        {
            var requestedTag = await db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tagId.Value);
            if (requestedTag == null) return tagId;

            defaultTagToMatch = requestedTag.UserId == SeedDataInitializer.DefaultUserId
                ? requestedTag
                : await db.Tags.AsNoTracking().FirstOrDefaultAsync(t =>
                    t.UserId == SeedDataInitializer.DefaultUserId && t.Name == requestedTag.Name);

            if (defaultTagToMatch == null) return tagId;

            targetCourseIds = await (
                from ft in db.FlashcardTags
                join f in db.Flashcards on ft.FlashcardId equals f.Id
                join l in db.Lessons on f.LessonId equals l.Id
                join ch in db.Chapters on l.ChapterId equals ch.Id
                join c in db.Courses on ch.CourseId equals c.Id
                where ft.TagId == defaultTagToMatch.Id &&
                      f.UserId == SeedDataInitializer.DefaultUserId &&
                      !f.IsArchived &&
                      c.Visibility == "public" &&
                      c.IsPublished &&
                      c.Slug.StartsWith("document-course-")
                select c.Id
            ).Distinct().ToListAsync();

            if (targetCourseIds.Count == 0) return tagId;
        }
        else if (!documentId.HasValue)
        {
            var hasAnyUserCard = await db.Flashcards
                .AsNoTracking()
                .AnyAsync(f => f.UserId == userId && !f.IsArchived);
            if (!hasAnyUserCard)
            {
                targetCourseIds = await db.Courses
                    .AsNoTracking()
                    .Where(c => c.Visibility == "public" &&
                                c.IsPublished &&
                                c.Slug.StartsWith("document-course-"))
                    .Select(c => c.Id)
                    .ToListAsync();
            }
        }

        if (targetCourseIds.Count == 0)
        {
            return tagId;
        }

        var syncLock = FlashcardSyncLocks.GetOrAdd(userId.ToString("N"), _ => new SemaphoreSlim(1, 1));
        await syncLock.WaitAsync();
        try
        {
            var courseLessonIds = await (
                from l in db.Lessons
                join ch in db.Chapters on l.ChapterId equals ch.Id
                where targetCourseIds.Contains(ch.CourseId)
                select l.Id
            ).ToHashSetAsync();

            if (courseLessonIds.Count > 0)
            {
                var defaultCards = await db.Flashcards
                    .AsNoTracking()
                    .Where(f => f.UserId == SeedDataInitializer.DefaultUserId &&
                                !f.IsArchived &&
                                f.LessonId.HasValue &&
                                courseLessonIds.Contains(f.LessonId.Value))
                    .ToListAsync();

                if (defaultCards.Count > 0)
                {
                    var existingUserCards = await db.Flashcards
                        .AsNoTracking()
                        .Where(f => f.UserId == userId &&
                                    f.LessonId.HasValue &&
                                    courseLessonIds.Contains(f.LessonId.Value))
                        .Select(f => new { f.LessonId, f.CardType, f.FrontAstJson, f.BackAstJson })
                        .ToListAsync();

                    var existingKeys = new HashSet<(Guid? LessonId, string CardType, string Front, string Back)>(
                        existingUserCards.Select(f => (f.LessonId, f.CardType, f.FrontAstJson, f.BackAstJson)));

                    var missingDefaults = new List<Flashcard>();
                    foreach (var d in defaultCards)
                    {
                        if (existingKeys.Add((d.LessonId, d.CardType, d.FrontAstJson, d.BackAstJson)))
                        {
                            missingDefaults.Add(d);
                        }
                    }

                    if (missingDefaults.Count > 0)
                    {
                        var missingDefaultIds = missingDefaults.Select(c => c.Id).ToHashSet();
                        var defaultLinks = await db.FlashcardTags
                            .AsNoTracking()
                            .Where(ft => missingDefaultIds.Contains(ft.FlashcardId))
                            .ToListAsync();
                        var defaultTagIds = defaultLinks.Select(ft => ft.TagId).ToHashSet();
                        var defaultTags = defaultTagIds.Count > 0
                            ? await db.Tags.AsNoTracking().Where(t => defaultTagIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id)
                            : new Dictionary<Guid, Tag>();

                        var existingUserTags = await db.Tags
                            .Where(t => t.UserId == userId)
                            .ToListAsync();
                        var userTagsByName = existingUserTags
                            .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                        Tag EnsureUserTag(Tag srcTag)
                        {
                            if (userTagsByName.TryGetValue(srcTag.Name, out var existingTag))
                            {
                                return existingTag;
                            }

                            var created = new Tag
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                Name = srcTag.Name,
                                ColorKey = srcTag.ColorKey
                            };
                            db.Tags.Add(created);
                            userTagsByName[created.Name] = created;
                            return created;
                        }

                        var defaultLinksByCard = defaultLinks
                            .GroupBy(ft => ft.FlashcardId)
                            .ToDictionary(g => g.Key, g => g.Select(ft => ft.TagId).ToList());

                        var newCards = new List<Flashcard>(missingDefaults.Count);
                        var newTagLinks = new List<FlashcardTag>();

                        foreach (var src in missingDefaults)
                        {
                            var cloned = new Flashcard
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                LessonId = src.LessonId,
                                DocumentId = src.DocumentId,
                                AnnotationId = null,
                                CardType = src.CardType,
                                SourceTitleSnapshot = src.SourceTitleSnapshot,
                                SourceQuoteSnapshot = src.SourceQuoteSnapshot,
                                FrontAstJson = src.FrontAstJson,
                                BackAstJson = src.BackAstJson,
                                Stability = 0.0,
                                Difficulty = 0.0,
                                Reps = 0,
                                Lapses = 0,
                                State = 0,
                                IsArchived = false,
                                LastReviewAt = null,
                                DueAt = now,
                                CreatedAt = now,
                                UpdatedAt = now
                            };
                            newCards.Add(cloned);

                            if (defaultLinksByCard.TryGetValue(src.Id, out var srcTagIds))
                            {
                                foreach (var srcTagId in srcTagIds.Distinct())
                                {
                                    if (defaultTags.TryGetValue(srcTagId, out var srcTag))
                                    {
                                        var targetUserTag = EnsureUserTag(srcTag);
                                        newTagLinks.Add(new FlashcardTag
                                        {
                                            FlashcardId = cloned.Id,
                                            TagId = targetUserTag.Id
                                        });
                                    }
                                }
                            }
                        }

                        db.Flashcards.AddRange(newCards);
                        if (newTagLinks.Count > 0)
                        {
                            db.FlashcardTags.AddRange(newTagLinks);
                        }
                        await db.SaveChangesAsync();
                    }
                }
            }

            if (defaultTagToMatch != null)
            {
                var matchedUserTag = await db.Tags
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UserId == userId && t.Name == defaultTagToMatch.Name);
                if (matchedUserTag != null)
                {
                    return matchedUserTag.Id;
                }
            }
        }
        finally
        {
            syncLock.Release();
        }

        return tagId;
    }

    // ========================================================================
    // 8. PRACTICE ENGINE (PRACTICE ACTIVITIES & ATTEMPTS)
    // ========================================================================
    private static void MapPracticeEngineEndpoints(RouteGroupBuilder api)
    {
        var practiceGroup = api.MapGroup("/practice");

        // 1. GET /api/v1/practice/activities?courseId=...&lessonId=...
        practiceGroup.MapGet("/activities", async (
            Guid courseId,
            Guid? lessonId,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                return Results.NotFound(new { error = "Không tìm thấy khóa học." });
            }

            if (!await CanReadCourseAsync(db, course, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var q = db.PracticeActivities.Where(pa => pa.CourseId == courseId);
            if (lessonId.HasValue && lessonId.Value != Guid.Empty)
            {
                q = q.Where(pa => pa.LessonId == lessonId.Value);
            }

            var activities = (await q
                .OrderBy(pa => pa.OrderIndex)
                .ToListAsync())
                .OrderBy(pa => pa.OrderIndex)
                .ThenBy(pa => pa.CreatedAt)
                .ToList();

            var actIds = activities.Select(a => a.Id).ToHashSet();
            var userAttempts = await db.PracticeAttempts
                .Where(pa => pa.UserId == userId && pa.CourseId == courseId)
                .ToListAsync();

            var attemptsByActId = userAttempts
                .Where(pa => pa.ActivityId.HasValue && actIds.Contains(pa.ActivityId.Value))
                .ToLookup(pa => pa.ActivityId!.Value);
            var attemptsByKey = userAttempts.ToLookup(pa => pa.ActivityKey);

            var response = activities.Select(a =>
            {
                var attempts = attemptsByActId[a.Id]
                    .Concat(attemptsByKey[a.Id.ToString()])
                    .DistinctBy(x => x.Id)
                    .ToList();

                var bestScore = attempts.Count > 0 ? (double?)attempts.Max(x => x.Score) : null;
                var isPassed = attempts.Any(x => x.IsPassed);
                var lastAttempt = attempts.OrderByDescending(x => x.CreatedAt).FirstOrDefault();

                return new
                {
                    a.Id,
                    a.CourseId,
                    a.LessonId,
                    a.OrderIndex,
                    a.Type,
                    a.Title,
                    a.Description,
                    a.ConfigJson,
                    a.PassingScore,
                    a.IsSeeded,
                    a.CreatedAt,
                    a.UpdatedAt,
                    userStatus = new
                    {
                        attemptCount = attempts.Count,
                        bestScore,
                        isPassed,
                        lastScore = lastAttempt?.Score,
                        lastAttemptAt = lastAttempt?.CreatedAt
                    }
                };
            });

            return Results.Ok(response);
        });

        // 2. POST /api/v1/practice/attempts
        practiceGroup.MapPost("/attempts", async (
            RecordPracticeAttemptRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var maxScore = req.MaxScore.GetValueOrDefault(100);
            if (maxScore <= 0) maxScore = 100;

            if (req.Score < 0 || req.Score > maxScore)
            {
                return Results.BadRequest(new { error = "Điểm số không hợp lệ (phải từ 0 đến điểm tối đa)." });
            }

            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == req.CourseId);
            if (course == null)
            {
                return Results.NotFound(new { error = "Không tìm thấy khóa học." });
            }

            if (!await CanReadCourseAsync(db, course, userId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            // Verify activity if ActivityId is provided
            PracticeActivity? activity = null;
            if (req.ActivityId.HasValue && req.ActivityId.Value != Guid.Empty)
            {
                activity = await db.PracticeActivities.FirstOrDefaultAsync(a => a.Id == req.ActivityId.Value);
                if (activity != null && activity.CourseId != req.CourseId)
                {
                    return Results.BadRequest(new { error = "Hoạt động thực hành không thuộc khóa học này." });
                }
            }

            var passingScore = activity?.PassingScore ?? 70;
            var isPassed = req.IsPassed ?? (req.Score >= passingScore);
            var safeDetail = string.IsNullOrWhiteSpace(req.DetailJson) ? "{}" : SanitizeDangerousHtml(req.DetailJson) ?? "{}";

            var actType = string.IsNullOrWhiteSpace(req.ActivityType)
                ? (activity?.Type ?? "practice")
                : req.ActivityType;
            var actTitle = string.IsNullOrWhiteSpace(req.ActivityTitle)
                ? (activity?.Title ?? "Bài tập thực hành")
                : req.ActivityTitle;
            var actKey = string.IsNullOrWhiteSpace(req.ActivityKey)
                ? (req.ActivityId?.ToString() ?? actType)
                : req.ActivityKey;

            var attempt = new PracticeAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActivityId = req.ActivityId,
                CourseId = req.CourseId,
                LessonId = req.LessonId,
                ActivityType = actType,
                ActivityKey = actKey,
                ActivityTitle = actTitle,
                Score = Math.Round(req.Score, 2),
                MaxScore = maxScore,
                IsPassed = isPassed,
                DetailJson = safeDetail,
                DurationSeconds = req.DurationSeconds,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.PracticeAttempts.Add(attempt);
            await db.SaveChangesAsync();

            return Results.Ok(attempt);
        });

        // 3. GET /api/v1/practice/attempts?courseId=...&lessonId=...&type=...&limit=...
        practiceGroup.MapGet("/attempts", async (
            Guid? courseId,
            Guid? lessonId,
            string? type,
            int? limit,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            if (courseId.HasValue && courseId.Value != Guid.Empty)
            {
                var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId.Value);
                if (course != null && !await CanReadCourseAsync(db, course, userId))
                {
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }
            }

            var q = db.PracticeAttempts.Where(pa => pa.UserId == userId);
            if (courseId.HasValue && courseId.Value != Guid.Empty)
            {
                q = q.Where(pa => pa.CourseId == courseId.Value);
            }
            if (lessonId.HasValue && lessonId.Value != Guid.Empty)
            {
                q = q.Where(pa => pa.LessonId == lessonId.Value);
            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                q = q.Where(pa => pa.ActivityType == type);
            }

            var takeLimit = Math.Clamp(limit ?? 20, 1, 100);
            var rawAttempts = await q.ToListAsync();
            var attempts = rawAttempts
                .OrderByDescending(pa => pa.CreatedAt)
                .Take(takeLimit)
                .ToList();

            return Results.Ok(attempts);
        });

        // 4. GET /api/v1/practice/stats
        practiceGroup.MapGet("/stats", async (
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var now = DateTimeOffset.UtcNow;

            var userAttempts = (await db.PracticeAttempts
                .AsNoTracking()
                .Where(pa => pa.UserId == userId)
                .ToListAsync())
                .OrderByDescending(pa => pa.CreatedAt)
                .ToList();

            var totalAttempts = userAttempts.Count;
            var averageScore = totalAttempts > 0 ? Math.Round(userAttempts.Average(pa => pa.Score), 1) : 0.0;
            var passedCount = userAttempts.Count(pa => pa.IsPassed);
            var todayUtc = now.Date;
            var todayAttempts = userAttempts.Count(pa => pa.CreatedAt.UtcDateTime.Date == todayUtc);

            var activeDates = userAttempts
                .Select(pa => pa.CreatedAt.UtcDateTime.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            var streakDays = 0;
            if (activeDates.Count > 0)
            {
                var checkDate = todayUtc;
                if (!activeDates.Contains(checkDate))
                {
                    checkDate = todayUtc.AddDays(-1);
                }
                while (activeDates.Contains(checkDate))
                {
                    streakDays++;
                    checkDate = checkDate.AddDays(-1);
                }
            }
            if (streakDays == 0)
            {
                var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                streakDays = user?.StreakDays ?? 0;
            }

            // Readable courses for this user (enrolled, owned, or published public)
            var enrolledIds = await db.CourseEnrollments
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var rawUserCourses = await db.Courses
                .AsNoTracking()
                .Where(c => (c.InstructorId == userId ||
                             enrolledIds.Contains(c.Id) ||
                             (c.Visibility == "public" && c.IsPublished))
                            && !c.Slug.StartsWith("personal-workspace-"))
                .ToListAsync();

            var readableCourses = OrderCoursesForUser(rawUserCourses, userId);
            var readableCourseIds = readableCourses.Select(c => c.Id).ToHashSet();

            var attemptCourseIds = userAttempts.Select(a => a.CourseId).Distinct().ToHashSet();
            var missingAttemptCourseIds = attemptCourseIds.Where(id => !readableCourseIds.Contains(id)).ToList();
            var extraAttemptCourses = missingAttemptCourseIds.Count > 0
                ? await db.Courses.AsNoTracking().Where(c => missingAttemptCourseIds.Contains(c.Id)).ToListAsync()
                : new List<Course>();

            var courseTitleMap = readableCourses
                .Concat(extraAttemptCourses)
                .GroupBy(c => c.Id)
                .ToDictionary(g => g.Key, g => g.First().Title);

            var allChapters = readableCourseIds.Count > 0
                ? await db.Chapters
                    .AsNoTracking()
                    .Where(ch => readableCourseIds.Contains(ch.CourseId))
                    .OrderBy(ch => ch.SortOrder)
                    .ToListAsync()
                : new List<Chapter>();

            var chapterById = allChapters.ToDictionary(ch => ch.Id);
            var chaptersByCourse = allChapters.ToLookup(ch => ch.CourseId);
            var readableChapterIds = chapterById.Keys.ToHashSet();

            var readableLessons = readableChapterIds.Count > 0
                ? await db.Lessons
                    .AsNoTracking()
                    .Where(l => readableChapterIds.Contains(l.ChapterId))
                    .OrderBy(l => l.SortOrder)
                    .ToListAsync()
                : new List<Lesson>();

            var readableLessonIds = readableLessons.Select(l => l.Id).ToHashSet();
            var attemptLessonIds = userAttempts
                .Where(a => a.LessonId.HasValue)
                .Select(a => a.LessonId!.Value)
                .Distinct()
                .Where(id => !readableLessonIds.Contains(id))
                .ToList();

            var extraAttemptLessons = attemptLessonIds.Count > 0
                ? await db.Lessons.AsNoTracking().Where(l => attemptLessonIds.Contains(l.Id)).ToListAsync()
                : new List<Lesson>();

            var lessonTitleMap = readableLessons
                .Concat(extraAttemptLessons)
                .GroupBy(l => l.Id)
                .ToDictionary(g => g.Key, g => g.First().Title);

            var attemptActivityIds = userAttempts
                .Where(a => a.ActivityId.HasValue)
                .Select(a => a.ActivityId!.Value)
                .Distinct()
                .ToHashSet();

            var allPracticeActivities = (readableCourseIds.Count > 0 || attemptActivityIds.Count > 0)
                ? await db.PracticeActivities
                    .AsNoTracking()
                    .Where(pa => readableCourseIds.Contains(pa.CourseId) || attemptActivityIds.Contains(pa.Id))
                    .OrderBy(pa => pa.OrderIndex)
                    .ToListAsync()
                : new List<PracticeActivity>();

            var activityPassingMap = allPracticeActivities.ToDictionary(a => a.Id, a => a.PassingScore);
            var activitiesByCourse = allPracticeActivities
                .Where(a => readableCourseIds.Contains(a.CourseId))
                .OrderBy(a => a.OrderIndex)
                .ToLookup(a => a.CourseId);

            var needsReview = userAttempts
                .GroupBy(pa => pa.ActivityKey)
                .Select(g => new
                {
                    Latest = g.OrderByDescending(x => x.CreatedAt).First(),
                    AttemptCount = g.Count()
                })
                .Select(x =>
                {
                    var passingScore = x.Latest.ActivityId.HasValue &&
                                       activityPassingMap.TryGetValue(x.Latest.ActivityId.Value, out var ps)
                        ? ps
                        : 70;
                    return new { x.Latest, x.AttemptCount, PassingScore = passingScore };
                })
                .Where(x => !x.Latest.IsPassed || x.Latest.Score < x.PassingScore)
                .OrderByDescending(x => x.Latest.CreatedAt)
                .Take(30)
                .Select(x => new
                {
                    x.Latest.ActivityKey,
                    x.Latest.ActivityTitle,
                    x.Latest.ActivityType,
                    x.Latest.CourseId,
                    courseTitle = courseTitleMap.GetValueOrDefault(x.Latest.CourseId, "Khóa học"),
                    x.Latest.LessonId,
                    lessonTitle = x.Latest.LessonId.HasValue
                        ? lessonTitleMap.GetValueOrDefault(x.Latest.LessonId.Value)
                        : null,
                    x.Latest.Score,
                    x.Latest.MaxScore,
                    passingScore = x.PassingScore,
                    attemptCount = x.AttemptCount,
                    x.Latest.CreatedAt
                })
                .ToList();

            var recentHistory = userAttempts
                .Take(15)
                .Select(pa => new
                {
                    pa.Id,
                    pa.ActivityKey,
                    pa.ActivityTitle,
                    pa.ActivityType,
                    pa.Score,
                    pa.MaxScore,
                    pa.IsPassed,
                    pa.CreatedAt
                })
                .ToList();

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => todayUtc.AddDays(-6 + i))
                .Select(date =>
                {
                    var dayAttempts = userAttempts.Where(pa => pa.CreatedAt.UtcDateTime.Date == date).ToList();
                    return new
                    {
                        date = date.ToString("yyyy-MM-dd"),
                        dayName = date.ToString("ddd"),
                        count = dayAttempts.Count,
                        avgScore = dayAttempts.Count > 0 ? Math.Round(dayAttempts.Average(x => x.Score), 1) : 0.0
                    };
                })
                .ToList();

            // Count FSRS cards due right now (DueAt <= now) in memory to remain SQLite-safe
            var userCardDueDates = await db.Flashcards
                .AsNoTracking()
                .Where(f => f.UserId == userId && !f.IsArchived)
                .Select(f => f.DueAt)
                .ToListAsync();
            var fsrsDueCount = userCardDueDates.Count(dueAt => dueAt <= now);

            var lessonsByChapter = readableLessons.ToLookup(l => l.ChapterId);
            var attemptsByCourse = userAttempts.ToLookup(a => a.CourseId);

            static bool IsActivityPassedByUser(PracticeActivity act, IReadOnlyList<PracticeAttempt> courseAttempts)
            {
                var actIdStr = act.Id.ToString();
                return courseAttempts.Any(att =>
                    att.IsPassed &&
                    ((att.ActivityId.HasValue && att.ActivityId.Value == act.Id) ||
                     string.Equals(att.ActivityKey, actIdStr, StringComparison.OrdinalIgnoreCase)));
            }

            var courseSummaries = readableCourses.Select(course =>
            {
                var courseChapters = chaptersByCourse[course.Id].OrderBy(ch => ch.SortOrder).ToList();
                var orderedLessons = courseChapters
                    .SelectMany(ch => lessonsByChapter[ch.Id]
                        .OrderBy(l => l.SortOrder)
                        .ThenBy(l => l.CreatedAt)
                        .Select(l => new { Lesson = l, ChapterTitle = ch.Title }))
                    .ToList();

                var courseActivities = activitiesByCourse[course.Id].ToList();
                var courseAttempts = attemptsByCourse[course.Id].ToList();

                int totalActivities;
                int passedActivities;

                if (courseActivities.Count > 0)
                {
                    totalActivities = courseActivities.Count;
                    passedActivities = courseActivities.Count(a => IsActivityPassedByUser(a, courseAttempts));
                }
                else
                {
                    var keyGroups = courseAttempts.GroupBy(a => a.ActivityKey).ToList();
                    totalActivities = keyGroups.Count;
                    passedActivities = keyGroups.Count(g => g.Any(a => a.IsPassed));
                }

                var courseAvgScore = courseAttempts.Count > 0
                    ? Math.Round(courseAttempts.Average(a => a.Score), 1)
                    : 0.0;
                DateTimeOffset? lastAttemptAt = courseAttempts.Count > 0
                    ? courseAttempts.Max(a => a.CreatedAt)
                    : null;

                var lessonSummaries = orderedLessons.Select(item =>
                {
                    var lesson = item.Lesson;
                    var lessonAttempts = courseAttempts.Where(a => a.LessonId == lesson.Id).ToList();
                    var lessonActivities = courseActivities.Where(a => a.LessonId == lesson.Id).ToList();

                    int activityCount;
                    int lessonPassedCount;
                    if (courseActivities.Count > 0)
                    {
                        activityCount = lessonActivities.Count;
                        lessonPassedCount = lessonActivities.Count(a => IsActivityPassedByUser(a, lessonAttempts.Count > 0 ? lessonAttempts : courseAttempts));
                    }
                    else
                    {
                        var lGroups = lessonAttempts.GroupBy(a => a.ActivityKey).ToList();
                        activityCount = lGroups.Count;
                        lessonPassedCount = lGroups.Count(g => g.Any(a => a.IsPassed));
                    }

                    return new
                    {
                        lessonId = lesson.Id,
                        title = lesson.Title,
                        chapterTitle = item.ChapterTitle,
                        activityCount,
                        passedCount = lessonPassedCount,
                        attemptCount = lessonAttempts.Count
                    };
                }).ToList();

                return new
                {
                    courseId = course.Id,
                    title = course.Title,
                    slug = course.Slug,
                    totalActivities,
                    passedActivities,
                    averageScore = courseAvgScore,
                    attemptCount = courseAttempts.Count,
                    lastAttemptAt,
                    lessons = lessonSummaries
                };
            }).ToList();

            return Results.Ok(new
            {
                totalAttempts,
                averageScore,
                passedCount,
                todayAttempts,
                streakDays,
                fsrsDueCount,
                needsReview,
                recentHistory,
                chartData = last7Days,
                courseSummaries
            });
        });
    }
}
