using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;

namespace Swms.Api.Infrastructure.Database;

public class SwmsDbContext : DbContext
{
    public SwmsDbContext(DbContextOptions<SwmsDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserFsrsParameters> UserFsrsParameters => Set<UserFsrsParameters>();
    public DbSet<UserQuota> UserQuotas => Set<UserQuota>();
    public DbSet<AiUsageLedger> AiUsageLedgers => Set<AiUsageLedger>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonVersion> LessonVersions => Set<LessonVersion>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentPage> DocumentPages => Set<DocumentPage>();
    public DbSet<Annotation> Annotations => Set<Annotation>();
    public DbSet<Notebook> Notebooks => Set<Notebook>();
    public DbSet<NotebookLink> NotebookLinks => Set<NotebookLink>();
    public DbSet<NotebookConflictDraft> NotebookConflictDrafts => Set<NotebookConflictDraft>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<FlashcardTag> FlashcardTags => Set<FlashcardTag>();
    public DbSet<FlashcardReview> FlashcardReviews => Set<FlashcardReview>();
    public DbSet<PracticeQuiz> PracticeQuizzes => Set<PracticeQuiz>();
    public DbSet<PracticeQuizItem> PracticeQuizItems => Set<PracticeQuizItem>();
    public DbSet<BackgroundJob> BackgroundJobs => Set<BackgroundJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var tableNames = new Dictionary<Type, string>
        {
            [typeof(User)] = "users",
            [typeof(RefreshToken)] = "refresh_tokens",
            [typeof(UserFsrsParameters)] = "user_fsrs_parameters",
            [typeof(UserQuota)] = "user_quotas",
            [typeof(AiUsageLedger)] = "ai_usage_ledger",
            [typeof(Course)] = "courses",
            [typeof(CourseEnrollment)] = "course_enrollments",
            [typeof(Chapter)] = "chapters",
            [typeof(Lesson)] = "lessons",
            [typeof(LessonVersion)] = "lesson_versions",
            [typeof(Document)] = "documents",
            [typeof(DocumentPage)] = "document_pages",
            [typeof(Annotation)] = "annotations",
            [typeof(Notebook)] = "notebooks",
            [typeof(NotebookLink)] = "notebook_links",
            [typeof(NotebookConflictDraft)] = "notebook_conflict_drafts",
            [typeof(Tag)] = "tags",
            [typeof(Flashcard)] = "flashcards",
            [typeof(FlashcardTag)] = "flashcard_tags",
            [typeof(FlashcardReview)] = "flashcard_reviews",
            [typeof(PracticeQuiz)] = "practice_quizzes",
            [typeof(PracticeQuizItem)] = "practice_quiz_items",
            [typeof(BackgroundJob)] = "background_jobs"
        };

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (tableNames.TryGetValue(entityType.ClrType, out var tableName))
            {
                entityType.SetTableName(tableName);
            }

            foreach (var property in entityType.GetProperties())
            {
                var propName = property.Name;
                if (propName.EndsWith("Json", StringComparison.Ordinal))
                {
                    property.SetColumnName(ToSnakeCase(propName[..^4]));
                    property.SetColumnType("jsonb");
                }
                else
                {
                    property.SetColumnName(ToSnakeCase(propName));
                }
            }
        }

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(r => r.TokenHash).IsUnique();
        modelBuilder.Entity<Course>().HasIndex(c => c.Slug).IsUnique();
        modelBuilder.Entity<CourseEnrollment>().HasIndex(ce => new { ce.UserId, ce.CourseId }).IsUnique();
        modelBuilder.Entity<Lesson>().HasIndex(l => new { l.ChapterId, l.Slug }).IsUnique();
        modelBuilder.Entity<LessonVersion>().HasIndex(lv => new { lv.LessonId, lv.VersionNumber }).IsUnique();
        modelBuilder.Entity<Tag>().HasIndex(t => new { t.UserId, t.Name }).IsUnique();
        modelBuilder.Entity<FlashcardTag>().HasKey(ft => new { ft.FlashcardId, ft.TagId });
        modelBuilder.Entity<FlashcardReview>().HasIndex(fr => fr.ReviewEventId).IsUnique();
        modelBuilder.Entity<BackgroundJob>().HasIndex(bj => bj.IdempotencyKey).IsUnique();
        modelBuilder.Entity<DocumentPage>()
            .HasOne(dp => dp.Document)
            .WithMany(d => d.Pages)
            .HasForeignKey(dp => dp.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<DocumentPage>().HasIndex(dp => new { dp.DocumentId, dp.PageNumber }).IsUnique();
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new System.Text.StringBuilder(name.Length + 8);
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceRelationalIntegrityRules();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        EnforceRelationalIntegrityRules();
        return base.SaveChanges();
    }

    private void EnforceRelationalIntegrityRules()
    {
        // 1. Validate chk_annotation_target_exclusive on added/modified Annotations
        var changedAnnotations = ChangeTracker.Entries<Annotation>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        foreach (var ann in changedAnnotations)
        {
            var isLessonTarget = ann.LessonId.HasValue && ann.BlockId.HasValue && !ann.DocumentId.HasValue && !ann.PageNumber.HasValue;
            var isDocTarget = ann.DocumentId.HasValue && ann.PageNumber.HasValue && ann.PageNumber > 0 && !ann.LessonId.HasValue && !ann.BlockId.HasValue;
            if (!isLessonTarget && !isDocTarget)
            {
                throw new InvalidOperationException(
                    "chk_annotation_target_exclusive violation: Annotation must target strictly EITHER (LessonId + BlockId) XOR (DocumentId + PageNumber > 0).");
            }
            if (ann.EndOffset < ann.StartOffset)
            {
                throw new InvalidOperationException("chk_annotation_offsets violation: EndOffset must be >= StartOffset.");
            }
        }

        // 2. Enforce ON DELETE RESTRICT on Flashcard -> FlashcardReviews (Decision D & ADR-004)
        var deletedFlashcards = ChangeTracker.Entries<Flashcard>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity)
            .ToList();

        foreach (var card in deletedFlashcards)
        {
            var hasReviews = FlashcardReviews.Any(r => r.FlashcardId == card.Id);
            if (hasReviews)
            {
                throw new InvalidOperationException(
                    $"ON DELETE RESTRICT violation (ADR-004): Flashcard {card.Id} has historical review logs in flashcard_reviews and cannot be hard-deleted. Use soft-delete (is_archived = true).");
            }
        }

        // 3. Enforce ON DELETE SET NULL when an Annotation is deleted (Decision D & ADR-004)
        var deletedAnnotations = ChangeTracker.Entries<Annotation>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity.Id)
            .ToHashSet();

        if (deletedAnnotations.Count > 0)
        {
            var linkedCards = Flashcards.Where(f => f.AnnotationId.HasValue && deletedAnnotations.Contains(f.AnnotationId.Value)).ToList();
            foreach (var card in linkedCards)
            {
                card.AnnotationId = null;
                card.UpdatedAt = DateTimeOffset.UtcNow;
            }

            var linkedNotebookLinks = NotebookLinks.Where(nl => nl.RefAnnotationId.HasValue && deletedAnnotations.Contains(nl.RefAnnotationId.Value)).ToList();
            NotebookLinks.RemoveRange(linkedNotebookLinks);
        }

        // 4. Enforce ON DELETE SET NULL when a Lesson is deleted
        var deletedLessons = ChangeTracker.Entries<Lesson>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity.Id)
            .ToHashSet();

        if (deletedLessons.Count > 0)
        {
            foreach (var card in Flashcards.Where(f => f.LessonId.HasValue && deletedLessons.Contains(f.LessonId.Value)).ToList())
            {
                card.LessonId = null;
            }
            foreach (var nb in Notebooks.Where(n => n.PrimarySourceLessonId.HasValue && deletedLessons.Contains(n.PrimarySourceLessonId.Value)).ToList())
            {
                nb.PrimarySourceLessonId = null;
            }
        }

        // 5. Enforce ON DELETE CASCADE when a Document is deleted
        var deletedDocuments = ChangeTracker.Entries<Document>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity.Id)
            .ToHashSet();

        if (deletedDocuments.Count > 0)
        {
            var linkedPages = DocumentPages.Where(dp => deletedDocuments.Contains(dp.DocumentId)).ToList();
            if (linkedPages.Count > 0)
            {
                DocumentPages.RemoveRange(linkedPages);
            }
        }
    }
}
