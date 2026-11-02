using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Ai;
using Swms.Api.Infrastructure.Database;
using Swms.Api.Infrastructure.FSRS;
using Swms.Api.Infrastructure.Security;
using Swms.Api.Infrastructure.SelfHealing;

namespace Swms.Api.Features;

public record RegisterRequest(string Email, string Password, string FullName);
public record LoginRequest(string Email, string Password);
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

public record CreateFlashcardRequest(
    Guid? LessonId,
    Guid? DocumentId,
    Guid? AnnotationId,
    string CardType, // cloze | standard | ai_generated
    string? SourceTitleSnapshot,
    string? SourceQuoteSnapshot,
    string FrontText,
    string BackText,
    List<string>? TagNames
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

public record UploadDocumentRequest(
    string Title,
    long FileSizeBytes,
    int TotalPages,
    string? MimeType,
    string? ExtractedText
);

public static class SwmsEndpoints
{
    public static void MapSwmsEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1");

        MapAuthEndpoints(api);
        MapWorkspaceEndpoints(api);
        MapLessonAndDocumentEndpoints(api);
        MapAnnotationEndpoints(api);
        MapNotebookEndpoints(api);
        MapFlashcardAndTagEndpoints(api);
        MapPracticeQuizAndAiEndpoints(api);
    }

    private static Guid ResolveUserId(HttpContext ctx)
    {
        var sub = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? ctx.User.FindFirstValue("sub");
        if (Guid.TryParse(sub, out var uid))
            return uid;
        if (ctx.Request.Headers.TryGetValue("X-Test-User-Id", out var headerUid) &&
            Guid.TryParse(headerUid.ToString(), out var parsedHeaderUid))
            return parsedHeaderUid;
        return SeedDataInitializer.DefaultUserId;
    }

    // ========================================================================
    // 1. AUTHENTICATION & BFF RTR ENDPOINTS (ADR-003 & ADR-016)
    // ========================================================================
    private static void MapAuthEndpoints(RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth");

        auth.MapPost("/register", async (
            RegisterRequest req,
            HttpContext http,
            SwmsDbContext db,
            RefreshTokenRotationService rtr) =>
        {
            var normalizedEmail = req.Email.Trim().ToLowerInvariant();
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
                TokenVersion = 1,
                StreakDays = 1,
                OnboardingStep = 1,
                IsActivated = false
            };
            db.Users.Add(user);

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
                user = new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.Role,
                    user.TokenVersion,
                    user.StreakDays,
                    user.OnboardingStep,
                    user.IsActivated
                }
            });
        });

        auth.MapPost("/login", async (
            LoginRequest req,
            HttpContext http,
            SwmsDbContext db,
            RefreshTokenRotationService rtr) =>
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !PasswordHashUtility.VerifyPassword(req.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
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
                user = new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.Role,
                    user.TokenVersion,
                    user.StreakDays,
                    user.OnboardingStep,
                    user.IsActivated
                }
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
                user = new
                {
                    outcome.User!.Id,
                    outcome.User.Email,
                    outcome.User.FullName,
                    outcome.User.TokenVersion
                }
            });
        });

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
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Results.NotFound();
            return Results.Ok(user);
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
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                       ?? await db.Users.FirstAsync(u => u.Id == SeedDataInitializer.DefaultUserId);

            var now = DateTimeOffset.UtcNow;
            var allCards = await db.Flashcards
                .Where(f => f.UserId == user.Id && !f.IsArchived)
                .ToListAsync();

            var dueCards = allCards.Where(f => f.DueAt <= now).ToList();
            var newCount = dueCards.Count(f => f.State == 0);
            var learningCount = dueCards.Count(f => f.State is 1 or 3);
            var reviewCount = dueCards.Count(f => f.State == 2);

            var lessons = await db.Lessons
                .OrderBy(l => l.SortOrder)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.Slug,
                    l.CurrentVersion,
                    l.UpdatedAt,
                    itemType = "lesson"
                })
                .ToListAsync();

            var documents = await db.Documents
                .Where(d => d.UploaderId == user.Id)
                .OrderByDescending(d => d.UpdatedAt)
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.TotalPages,
                    d.FileSizeBytes,
                    d.OcrStatus,
                    d.UpdatedAt,
                    itemType = "document"
                })
                .ToListAsync();

            var notebooks = await db.Notebooks
                .Where(n => n.UserId == user.Id)
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
                .ToListAsync();

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

            var lessons = await db.Lessons.ToListAsync();
            var docs = await db.Documents.Where(d => d.UploaderId == userId).ToListAsync();
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

            foreach (var l in lessons.Where(l => Matches(l.Title) || Matches(l.PlainText)))
            {
                results.Add(new
                {
                    id = l.Id,
                    type = "lesson",
                    title = l.Title,
                    subtitle = "Bài học • Chương 3: Quản lý Bộ nhớ",
                    snippet = l.PlainText.Length > 140 ? l.PlainText[..140] + "..." : l.PlainText
                });
            }

            foreach (var d in docs.Where(d => Matches(d.Title) || Matches(d.PlainText)))
            {
                results.Add(new
                {
                    id = d.Id,
                    type = "document",
                    title = d.Title,
                    subtitle = $"Tài liệu PDF • {d.TotalPages} trang",
                    snippet = d.PlainText ?? ""
                });
            }

            foreach (var n in notes.Where(n => Matches(n.Title) || Matches(n.ContentAstJson)))
            {
                results.Add(new
                {
                    id = n.Id,
                    type = "notebook",
                    title = n.Title,
                    subtitle = $"Ghi chú tổng hợp • v{n.Version}",
                    snippet = "Tài liệu ghi chú ProseMirror JSON AST"
                });
            }

            foreach (var c in cards.Where(c => Matches(c.FrontAstJson) || Matches(c.BackAstJson) || Matches(c.SourceTitleSnapshot)))
            {
                results.Add(new
                {
                    id = c.Id,
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
        var todayEntries = await db.AiUsageLedgers
            .Where(l => l.UserId == userId && l.CreatedAt >= todayUtc)
            .ToListAsync();

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
        api.MapGet("/lessons", async (SwmsDbContext db) =>
        {
            var lessons = await db.Lessons.OrderBy(l => l.SortOrder).ToListAsync();
            return Results.Ok(lessons.Select(l => new
            {
                l.Id,
                l.ChapterId,
                l.Title,
                l.Slug,
                l.CurrentVersion,
                l.PlainText,
                contentAst = JsonSerializer.Deserialize<object>(l.ContentAstJson),
                l.UpdatedAt
            }));
        });

        api.MapGet("/lessons/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return Results.NotFound();

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
                lesson.Title,
                lesson.Slug,
                lesson.CurrentVersion,
                lesson.PlainText,
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
            var docs = await db.Documents.Where(d => d.UploaderId == userId).OrderByDescending(d => d.CreatedAt).ToListAsync();
            return Results.Ok(docs);
        });

        api.MapGet("/documents/{id:guid}", async (Guid id, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null) return Results.NotFound();

            var annotations = await db.Annotations
                .Where(a => a.DocumentId == id && a.UserId == userId && a.Status != "archived")
                .ToListAsync();

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
                annotations
            });
        });

        api.MapPost("/documents/upload", async (UploadDocumentRequest req, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var quota = await db.UserQuotas.FirstOrDefaultAsync(q => q.UserId == userId);
            if (quota != null && quota.CurrentStorageBytes + req.FileSizeBytes > quota.StorageLimitBytes)
            {
                return Results.Json(new { error = "Vượt quá hạn mức dung lượng lưu trữ 50 MB của gói Free." }, statusCode: StatusCodes.Status413PayloadTooLarge);
            }

            var personalCourse = await db.Courses.FirstOrDefaultAsync(c => c.InstructorId == userId && c.Visibility == "personal")
                                 ?? await db.Courses.FirstAsync();

            var extractedText = string.IsNullOrWhiteSpace(req.ExtractedText)
                ? $"Nội dung trích xuất từ tài liệu {req.Title}. Hỗ trợ bôi đậm 5 màu ngữ nghĩa và tạo thẻ FSRS tức thì."
                : req.ExtractedText;

            var blockId = Guid.NewGuid().ToString();
            var ast = new
            {
                type = "doc",
                content = new object[]
                {
                    new
                    {
                        type = "heading",
                        attrs = new { level = 2, block_id = Guid.NewGuid().ToString() },
                        content = new[] { new { type = "text", text = req.Title } }
                    },
                    new
                    {
                        type = "paragraph",
                        attrs = new { block_id = blockId },
                        content = new[] { new { type = "text", text = extractedText } }
                    }
                }
            };

            var doc = new Document
            {
                Id = Guid.NewGuid(),
                CourseId = personalCourse.Id,
                UploaderId = userId,
                Title = req.Title,
                FileUrl = $"/uploads/{Guid.NewGuid():N}-{req.Title}",
                FileSizeBytes = Math.Max(1024, req.FileSizeBytes),
                TotalPages = Math.Max(1, req.TotalPages),
                MimeType = req.MimeType ?? "application/pdf",
                OcrStatus = "completed",
                ContentAstJson = JsonSerializer.Serialize(ast),
                PlainText = extractedText
            };

            db.Documents.Add(doc);
            if (quota != null)
            {
                quota.CurrentStorageBytes += doc.FileSizeBytes;
            }
            await db.SaveChangesAsync();

            return Results.Ok(doc);
        });
    }

    // ========================================================================
    // 4. ANNOTATIONS & 3-ACTION ORPHANED RECONCILIATION (DECISION 13 & 14)
    // ========================================================================
    private static void MapAnnotationEndpoints(RouteGroupBuilder api)
    {
        var annGroup = api.MapGroup("/annotations");

        annGroup.MapGet("/", async (Guid? lessonId, Guid? documentId, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var query = db.Annotations.Where(a => a.UserId == userId && a.Status != "archived");
            if (lessonId.HasValue) query = query.Where(a => a.LessonId == lessonId);
            if (documentId.HasValue) query = query.Where(a => a.DocumentId == documentId);
            var list = await query.OrderBy(a => a.CreatedAt).ToListAsync();
            return Results.Ok(list);
        });

        annGroup.MapPost("/", async (CreateAnnotationRequest req, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
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
                LessonId = req.LessonId,
                BlockId = req.LessonId.HasValue ? (req.BlockId ?? SeedDataInitializer.BlockPara1Id) : null,
                DocumentId = req.LessonId.HasValue ? null : req.DocumentId,
                PageNumber = req.LessonId.HasValue ? null : (req.PageNumber ?? 1),
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
            var list = await db.Notebooks
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();
            return Results.Ok(list);
        });

        nbGroup.MapPost("/by-source", async (AutoProvisionNotebookRequest req, HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
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
        });

        nbGroup.MapGet("/{id:guid}", async (Guid id, SwmsDbContext db) =>
        {
            var nb = await db.Notebooks.FirstOrDefaultAsync(n => n.Id == id);
            if (nb == null) return Results.NotFound();
            return await BuildNotebookDetailResponseAsync(db, nb);
        });

        nbGroup.MapPut("/{id:guid}", async (
            Guid id,
            SaveNotebookRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var nb = await db.Notebooks.FirstOrDefaultAsync(n => n.Id == id);
            if (nb == null) return Results.NotFound();

            var now = DateTimeOffset.UtcNow;

            // Optimistic Concurrency Control (OCC) Check (Decision B & ADR-002 & ADR-022)
            if (req.BaseVersion != nb.Version)
            {
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
                    ContentAstJson = req.ContentAstJson,
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
                    ClientContentAstJson = req.ContentAstJson,
                    ResolutionStatus = "kept_both",
                    CreatedAt = now,
                    ResolvedAt = now
                };
                db.NotebookConflictDrafts.Add(conflictDraft);

                await ExtractAndSyncNotebookLinksAsync(db, forkNotebook.Id, req.ContentAstJson);
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
            if (!string.IsNullOrWhiteSpace(req.Title))
            {
                nb.Title = req.Title.Trim();
            }
            nb.ContentAstJson = req.ContentAstJson;
            nb.Version += 1;
            nb.UpdatedAt = now;

            await ExtractAndSyncNotebookLinksAsync(db, nb.Id, req.ContentAstJson);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                nb.Id,
                nb.Title,
                nb.Version,
                nb.UpdatedAt,
                contentAst = JsonSerializer.Deserialize<object>(nb.ContentAstJson)
            });
        });

        nbGroup.MapGet("/{id:guid}/export-markdown", async (Guid id, SwmsDbContext db) =>
        {
            var nb = await db.Notebooks.FirstOrDefaultAsync(n => n.Id == id);
            if (nb == null) return Results.NotFound();

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

        var conflicts = await db.NotebookConflictDrafts
            .Where(c => c.NotebookId == nb.Id)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

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
            bool? includeAll,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var now = DateTimeOffset.UtcNow;

            var query = db.Flashcards.Where(f => f.UserId == userId && !f.IsArchived);
            if (includeAll != true)
            {
                query = query.Where(f => f.DueAt <= now.AddDays(1));
            }
            if (lessonId.HasValue) query = query.Where(f => f.LessonId == lessonId);
            if (documentId.HasValue) query = query.Where(f => f.DocumentId == documentId);

            if (tagId.HasValue)
            {
                var cardIdsWithTag = await db.FlashcardTags
                    .Where(ft => ft.TagId == tagId.Value)
                    .Select(ft => ft.FlashcardId)
                    .ToHashSetAsync();
                query = query.Where(f => cardIdsWithTag.Contains(f.Id));
            }

            var cards = await query.OrderBy(f => f.DueAt).ToListAsync();
            var allTags = await db.Tags.Where(t => t.UserId == userId).ToDictionaryAsync(t => t.Id);
            var cardTagLinks = await db.FlashcardTags.ToListAsync();

            var fsrsParams = await db.UserFsrsParameters.FirstOrDefaultAsync(p => p.UserId == userId);
            var weights = fsrsParams?.Weights ?? FsrsV5Engine.DefaultWeights;

            var enriched = cards.Select(c =>
            {
                var state = new FsrsCardState(c.Stability, c.Difficulty, c.Reps, c.Lapses, c.State, c.LastReviewAt, c.DueAt);
                var pAgain = FsrsV5Engine.Evaluate(state, 1, now, weights);
                var pHard = FsrsV5Engine.Evaluate(state, 2, now, weights);
                var pGood = FsrsV5Engine.Evaluate(state, 3, now, weights);
                var pEasy = FsrsV5Engine.Evaluate(state, 4, now, weights);

                var attachedTags = cardTagLinks
                    .Where(ft => ft.FlashcardId == c.Id && allTags.ContainsKey(ft.TagId))
                    .Select(ft => new { allTags[ft.TagId].Id, allTags[ft.TagId].Name, allTags[ft.TagId].ColorKey })
                    .ToList();

                return new
                {
                    c.Id,
                    c.LessonId,
                    c.DocumentId,
                    c.AnnotationId,
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
            var now = DateTimeOffset.UtcNow;

            var card = new Flashcard
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = req.LessonId,
                DocumentId = req.DocumentId,
                AnnotationId = req.AnnotationId,
                CardType = string.IsNullOrWhiteSpace(req.CardType) ? "cloze" : req.CardType,
                SourceTitleSnapshot = req.SourceTitleSnapshot ?? "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp",
                SourceQuoteSnapshot = req.SourceQuoteSnapshot ?? req.FrontText,
                FrontAstJson = JsonSerializer.Serialize(new { text = req.FrontText }),
                BackAstJson = JsonSerializer.Serialize(new { text = req.BackText }),
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

        fcGroup.MapPost("/sync-reviews", async (
            SyncReviewsBatchRequest req,
            HttpContext http,
            SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var serverNow = DateTimeOffset.UtcNow;
            var fsrsParams = await db.UserFsrsParameters.FirstOrDefaultAsync(p => p.UserId == userId);
            var weights = fsrsParams?.Weights ?? FsrsV5Engine.DefaultWeights;

            var syncedResults = new List<object>();

            foreach (var ev in req.Reviews)
            {
                // 1. Idempotency check on review_event_id (Decision J & ADR-018)
                var existingReview = await db.FlashcardReviews
                    .FirstOrDefaultAsync(r => r.ReviewEventId == ev.ReviewEventId);
                if (existingReview != null)
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
                        ReviewEventId = ev.ReviewEventId,
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
                        ReviewEventId = ev.ReviewEventId,
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
            }

            await db.SaveChangesAsync();
            return Results.Ok(new
            {
                syncedCount = syncedResults.Count,
                userActivated = user?.IsActivated ?? true,
                results = syncedResults
            });
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

        pqGroup.MapGet("/", async (HttpContext http, SwmsDbContext db) =>
        {
            var userId = ResolveUserId(http);
            var quizzes = await db.PracticeQuizzes
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.StartedAt)
                .ToListAsync();
            return Results.Ok(quizzes);
        });

        pqGroup.MapGet("/{id:guid}", async (Guid id, SwmsDbContext db) =>
        {
            var quiz = await db.PracticeQuizzes.FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return Results.NotFound();

            var items = await db.PracticeQuizItems
                .Where(i => i.QuizId == id)
                .OrderBy(i => i.CreatedAt)
                .ToListAsync();

            return Results.Ok(new
            {
                quiz.Id,
                quiz.Title,
                quiz.LessonId,
                quiz.DocumentId,
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
                    i.ClozeSolution,
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
            var quiz = await db.PracticeQuizzes.FirstOrDefaultAsync(q => q.Id == quizId);
            var item = await db.PracticeQuizItems.FirstOrDefaultAsync(i => i.Id == itemId && i.QuizId == quizId);
            if (quiz == null || item == null) return Results.NotFound();

            item.UserAnswerJson = JsonSerializer.Serialize(new { text = req.Answer });

            if (item.ItemType is "multiple_choice" or "cloze" or "short_answer")
            {
                var expected = (item.ClozeSolution ?? "").Trim();
                var actual = (req.Answer ?? "").Trim();
                item.IsCorrect = string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase) ||
                                 VietnameseTextNormalizer.Unaccent(actual).Contains(VietnameseTextNormalizer.Unaccent(expected));
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

            // Essay item -> Durable Transactional Outbox Queue (Decision I, ADR-009 & ADR-017)
            var idempKey = req.IdempotencyKey ?? Guid.NewGuid();
            var existingJob = await db.BackgroundJobs.FirstOrDefaultAsync(j => j.IdempotencyKey == idempKey);
            if (existingJob == null)
            {
                var questionText = ExtractQuestionFromPrompt(item.PromptAstJson);
                var payload = new
                {
                    quizId,
                    itemId,
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

                // Commit job to PostgreSQL BEFORE dispatching or returning 202 Accepted!
                db.BackgroundJobs.Add(existingJob);
                await db.SaveChangesAsync();

                await taskQueue.EnqueueAsync(new AiJobMessage(existingJob.Id));
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
                itemId = item.Id
            });
        });

        // Background Job Status & SSE Stream
        api.MapGet("/jobs/{id:guid}", async (Guid id, SwmsDbContext db) =>
        {
            var job = await db.BackgroundJobs.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null) return Results.NotFound();
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
            if (!await CheckAiQuotaAvailableAsync(db, userId))
            {
                return Results.Json(new { error = "Đã đạt giới hạn 50 AI Credits/ngày của gói Free. Tạo thẻ Cloze tức thì vẫn miễn phí 100%." }, statusCode: StatusCodes.Status429TooManyRequests);
            }

            var sysPrompt = "Bạn là trợ lý học thuật AI của hệ thống SWMS (Study Workspace Management System). Hãy giải thích ngắn gọn, súc tích bằng tiếng Việt khái niệm được bôi đậm trong tối đa 3 gạch đầu dòng dễ nhớ.";
            var userPrompt = $"Ngữ cảnh bài học: {req.ContextTitle ?? "Hệ điều hành"}\nĐoạn trích: \"{req.Excerpt}\"";

            var res = await gemini.CompleteChatAsync(sysPrompt, userPrompt);
            var explanation = !string.IsNullOrWhiteSpace(res.Content)
                ? res.Content
                : $"• **Khái niệm cốt lõi**: `{req.Excerpt}` là thành phần trọng tâm giúp tối ưu hóa hiệu năng ánh xạ bộ nhớ ảo sang khung trang vật lý.\n• **Cơ chế hoạt động**: Tận dụng tính cục bộ không gian & thời gian (Locality of Reference) để tra cứu trong $O(1)$ chu kỳ.\n• **Ứng dụng thực tế**: Giảm thiểu tối đa chi phí duyệt bảng trang đa cấp (Page Walk) trên kiến trúc x86-64.";

            db.AiUsageLedgers.Add(new AiUsageLedger
            {
                UserId = userId,
                Feature = "explain_concept",
                CreditsCharged = 1,
                Model = res.Model,
                PromptTokens = res.PromptTokens,
                CompletionTokens = res.CompletionTokens,
                TotalTokens = res.TotalTokens,
                CostUsd = Math.Round(res.TotalTokens * 0.0000005m, 6),
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                explanation,
                model = res.Model,
                usedLiveEndpoint = res.UsedLiveEndpoint,
                tokensUsed = res.TotalTokens,
                creditsCharged = 1
            });
        });

        aiGroup.MapPost("/generate-flashcard", async (
            AiInSituRequest req,
            HttpContext http,
            SwmsDbContext db,
            GeminiProxyClient gemini) =>
        {
            var userId = ResolveUserId(http);
            if (!await CheckAiQuotaAvailableAsync(db, userId))
            {
                return Results.Json(new { error = "Đã hết hạn mức AI hôm nay. Vui lòng dùng chế độ Cloze Tức thì (Miễn phí)." }, statusCode: StatusCodes.Status429TooManyRequests);
            }

            var sysPrompt = "Bạn là chuyên gia thiết kế thẻ ghi nhớ Spaced Repetition (FSRS). Từ đoạn trích được cung cấp, hãy tạo 1 câu hỏi Active Recall sắc bén (front) và câu trả lời ngắn gọn (back) bằng tiếng Việt. Trả về JSON thuần: {\"front\":\"...\",\"back\":\"...\"}";
            var userPrompt = $"Ngữ cảnh: {req.ContextTitle ?? "Hệ điều hành"}\nĐoạn trích: \"{req.Excerpt}\"";

            var res = await gemini.CompleteChatAsync(sysPrompt, userPrompt);
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

            db.AiUsageLedgers.Add(new AiUsageLedger
            {
                UserId = userId,
                Feature = "qa_generation",
                CreditsCharged = 1,
                Model = res.Model,
                PromptTokens = res.PromptTokens,
                CompletionTokens = res.CompletionTokens,
                TotalTokens = res.TotalTokens,
                CostUsd = Math.Round(res.TotalTokens * 0.0000005m, 6),
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                front,
                back,
                model = res.Model,
                usedLiveEndpoint = res.UsedLiveEndpoint,
                tokensUsed = res.TotalTokens,
                creditsCharged = 1
            });
        });
    }

    private static async Task<bool> CheckAiQuotaAvailableAsync(SwmsDbContext db, Guid userId)
    {
        var quota = await db.UserQuotas.FirstOrDefaultAsync(q => q.UserId == userId);
        if (quota == null) return true;

        var todayUtc = DateTimeOffset.UtcNow.Date;
        var usedCredits = await db.AiUsageLedgers
            .Where(l => l.UserId == userId && l.CreatedAt >= todayUtc)
            .SumAsync(l => l.CreditsCharged);

        return usedCredits < quota.DailyAiCreditLimit;
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
}
