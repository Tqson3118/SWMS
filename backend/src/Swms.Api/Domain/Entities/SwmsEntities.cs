using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Swms.Api.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(50)]
    public string Role { get; set; } = "student"; // student, instructor, admin
    public int TokenVersion { get; set; } = 1;
    public int StreakDays { get; set; } = 1;
    public int OnboardingStep { get; set; } = 1; // 1..4 (4 = completed all 3 steps)
    public bool IsActivated { get; set; } = false;
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

[Table("refresh_tokens")]
public class RefreshToken
{
    [Key, Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Column("user_id")]
    public Guid UserId { get; set; }
    [Required, MaxLength(64), Column("token_hash")]
    public string TokenHash { get; set; } = string.Empty;
    [Column("family_id")]
    public Guid FamilyId { get; set; }
    [Column("is_revoked")]
    public bool IsRevoked { get; set; } = false;
    [Column("replaced_by_token_id")]
    public Guid? ReplacedByTokenId { get; set; }
    [Column("encrypted_replacement_token")]
    public byte[]? EncryptedReplacementToken { get; set; } // AES-256-GCM encrypted plaintext replacement token for 15-30s grace window
    [Column("device_info")]
    public string? DeviceInfo { get; set; }
    [MaxLength(45), Column("ip_address")]
    public string? IpAddress { get; set; }
    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Column("revoked_at")]
    public DateTimeOffset? RevokedAt { get; set; }
}

public class UserFsrsParameters
{
    [Key]
    public Guid UserId { get; set; }
    public double[] Weights { get; set; } =
    [
        0.4072, 1.1829, 3.1262, 15.4722, 7.2102,
        0.5316, 1.0651, 0.0234, 1.616, 0.1544,
        1.0824, 1.9813, 0.0953, 0.2975, 2.2042,
        0.2407, 2.9466, 0.5034, 0.6567
    ];
    public double DesiredRetention { get; set; } = 0.90;
    public int MaximumIntervalDays { get; set; } = 36500;
    public DateTimeOffset? OptimizedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class UserQuota
{
    [Key]
    public Guid UserId { get; set; }
    [MaxLength(30)]
    public string Tier { get; set; } = "free"; // free, pro, institution
    public int DailyAiTokenLimit { get; set; } = 50000;
    public int DailyAiCreditLimit { get; set; } = 50;
    public long StorageLimitBytes { get; set; } = 52428800; // 50 MB default
    public long CurrentStorageBytes { get; set; } = 19293798; // ~18.4 MB sample
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class AiUsageLedger
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [Required, MaxLength(50)]
    public string Feature { get; set; } = "explain_concept";
    public int CreditsCharged { get; set; } = 1;
    [Required, MaxLength(100)]
    public string Model { get; set; } = "gemini-3.8-flash";
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    [Column(TypeName = "numeric(10,6)")]
    public decimal CostUsd { get; set; }
    public string? ContextRefJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Course
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstructorId { get; set; }
    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required, MaxLength(30)]
    public string Visibility { get; set; } = "personal"; // personal, private, public
    public bool IsPublished { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class CourseEnrollment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    [Required, MaxLength(50)]
    public string Role { get; set; } = "student";
    public DateTimeOffset EnrolledAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Chapter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CourseId { get; set; }
    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Lesson
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChapterId { get; set; }
    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public int CurrentVersion { get; set; } = 1;
    [Required]
    public string ContentAstJson { get; set; } = "{\"type\":\"doc\",\"content\":[]}";
    [Required]
    public string PlainText { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class LessonVersion
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LessonId { get; set; }
    public int VersionNumber { get; set; }
    [Required]
    public string ContentAstJson { get; set; } = "{\"type\":\"doc\",\"content\":[]}";
    [Required]
    public string PlainText { get; set; } = string.Empty;
    public Guid PublishedBy { get; set; }
    public DateTimeOffset PublishedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? ChangeSummary { get; set; }
}

public class Document
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CourseId { get; set; }
    public Guid UploaderId { get; set; }
    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string FileUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; } = 1024;
    public int TotalPages { get; set; } = 1;
    [Required, MaxLength(100)]
    public string MimeType { get; set; } = "application/pdf";
    [Required, MaxLength(30)]
    public string OcrStatus { get; set; } = "pending"; // pending, processing, completed, failed
    public string? ContentAstJson { get; set; }
    public string? PlainText { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Annotation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    // Mutually exclusive target: Lesson + BlockId XOR Document + PageNumber
    public Guid? LessonId { get; set; }
    public Guid? BlockId { get; set; }
    public Guid? DocumentId { get; set; }
    public int? PageNumber { get; set; }
    public string? BoundingBoxesJson { get; set; }

    [Required]
    public string ExactQuote { get; set; } = string.Empty;
    [Required]
    public string OriginalExactQuote { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? PrefixQuote { get; set; }
    [MaxLength(100)]
    public string? SuffixQuote { get; set; }
    public int StartOffset { get; set; }
    public int EndOffset { get; set; }
    [Required, MaxLength(20)]
    public string ColorKey { get; set; } = "yellow"; // yellow, green, blue, purple, rose
    [MaxLength(100)]
    public string? SemanticLabel { get; set; }
    public string? NoteComment { get; set; }
    [Required, MaxLength(30)]
    public string Status { get; set; } = "anchored"; // anchored, needs_review, orphaned, archived
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Notebook
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [Required, MaxLength(255)]
    public string Title { get; set; } = "Untitled Notebook";
    public Guid? PrimarySourceLessonId { get; set; }
    public Guid? PrimarySourceDocumentId { get; set; }
    [Required]
    public string ContentAstJson { get; set; } = "{\"type\":\"doc\",\"content\":[]}";
    public int Version { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class NotebookLink
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NotebookId { get; set; }
    public Guid SourceBlockId { get; set; }
    [Required, MaxLength(30)]
    public string TargetType { get; set; } = "annotation"; // annotation, lesson, document
    public Guid? RefAnnotationId { get; set; }
    public Guid? RefLessonId { get; set; }
    public Guid? RefDocumentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class NotebookConflictDraft
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NotebookId { get; set; }
    public Guid? ForkedNotebookId { get; set; }
    public Guid UserId { get; set; }
    [MaxLength(100)]
    public string? DeviceId { get; set; }
    public int BaseVersion { get; set; }
    public int ServerVersion { get; set; }
    [Required]
    public string ClientContentAstJson { get; set; } = "{\"type\":\"doc\",\"content\":[]}";
    [Required, MaxLength(30)]
    public string ResolutionStatus { get; set; } = "kept_both"; // pending, kept_both, resolved_local, resolved_server
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
}

public class Tag
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    public string ColorKey { get; set; } = "blue";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Flashcard
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? LessonId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? AnnotationId { get; set; }
    [Required, MaxLength(30)]
    public string CardType { get; set; } = "cloze"; // standard, cloze, ai_generated
    public string? SourceTitleSnapshot { get; set; }
    public string? SourceQuoteSnapshot { get; set; }
    [Required]
    public string FrontAstJson { get; set; } = string.Empty;
    [Required]
    public string BackAstJson { get; set; } = string.Empty;
    public double Stability { get; set; } = 0.0;
    public double Difficulty { get; set; } = 0.0;
    public int Reps { get; set; } = 0;
    public int Lapses { get; set; } = 0;
    public int State { get; set; } = 0; // 0=New, 1=Learning, 2=Review, 3=Relearning
    public bool IsArchived { get; set; } = false;
    public DateTimeOffset? LastReviewAt { get; set; }
    public DateTimeOffset DueAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class FlashcardTag
{
    public Guid FlashcardId { get; set; }
    public Guid TagId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class FlashcardReview
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReviewEventId { get; set; } = Guid.NewGuid();
    public Guid FlashcardId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; } // 1=Again, 2=Hard, 3=Good, 4=Easy
    public int? ReviewDurationMs { get; set; }
    public double ScheduledDays { get; set; }
    public double ElapsedDays { get; set; }
    public double LastStability { get; set; }
    public double NewStability { get; set; }
    public double LastDifficulty { get; set; }
    public double NewDifficulty { get; set; }
    public DateTimeOffset ReviewedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class PracticeQuiz
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [Required, MaxLength(255)]
    public string Title { get; set; } = "Practice Quiz";
    public Guid? LessonId { get; set; }
    public Guid? DocumentId { get; set; }
    public int TotalItems { get; set; }
    public int CorrectItems { get; set; }
    [Required, MaxLength(30)]
    public string Status { get; set; } = "in_progress"; // in_progress, completed, abandoned
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public string? AiSummaryFeedback { get; set; }
}

public class PracticeQuizItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuizId { get; set; }
    [Required, MaxLength(30)]
    public string ItemType { get; set; } = "multiple_choice"; // multiple_choice, cloze, short_answer, essay
    [Required]
    public string PromptAstJson { get; set; } = string.Empty;
    public string? ChoicesJson { get; set; }
    public string? ClozeSolution { get; set; }
    public string? UserAnswerJson { get; set; }
    public bool? IsCorrect { get; set; }
    public string? AiRubricFeedbackJson { get; set; }
    public DateTimeOffset? EvaluatedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class BackgroundJob
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IdempotencyKey { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [Required, MaxLength(50)]
    public string JobType { get; set; } = string.Empty;
    [Required, MaxLength(30)]
    public string Status { get; set; } = "queued"; // queued, leased, running, completed, failed, cancelled
    public int Priority { get; set; } = 0;
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public DateTimeOffset? LeaseExpiresAt { get; set; }
    [MaxLength(100)]
    public string? LeasedByWorkerId { get; set; }
    public int ProgressPercent { get; set; } = 0;
    [Required]
    public string PayloadJson { get; set; } = "{}";
    public string? ResultJson { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
