export interface CourseLesson {
  id: string;
  lessonId?: string;
  chapterId?: string;
  courseId?: string;
  title: string;
  slug?: string;
  durationMinutes?: number;
  estimatedMinutes?: number;
  isCompleted?: boolean;
  isBookmarked?: boolean;
  needsReview?: boolean;
  sortOrder?: number;
  practiceActivityCount?: number;
  practicePassedCount?: number;
  quizId?: string | null;
}

export interface CourseChapter {
  id: string;
  chapterId?: string;
  title: string;
  sortOrder?: number;
  lessons: CourseLesson[];
}

export interface WeakChapterItem {
  chapterId: string;
  title: string;
  completionPercent: number;
  reason?: string;
  nextLessonId?: string;
}

export interface CourseResumeState {
  lastLessonId?: string | null;
  lastLessonTitle?: string | null;
  readPercent?: number;
  totalStudiedMinutes?: number;
  lastAccessedAt?: string | null;
}

export interface CourseReviewItem {
  id: string;
  userName: string;
  rating: number;
  comment: string;
  createdAt?: string | null;
}

export interface CourseDetail {
  id: string;
  courseId?: string;
  title: string;
  slug?: string;
  description?: string;
  visibility?: string;
  isPublished?: boolean;
  isPinned?: boolean;
  isArchived?: boolean;
  dailyLessonGoal?: number;
  targetCompletionDate?: string | null;
  isSystemCourse?: boolean;
  canDelete?: boolean;
  category?: string;
  level?: string;
  learningOutcomes?: string[];
  prerequisites?: string[];
  targetAudience?: string;
  labLabel?: string;
  hasAudio?: boolean;
  bookmarkedLessonCount?: number;
  needsReviewLessonCount?: number;
  chapterCount?: number;
  lessonCount?: number;
  totalLessons: number;
  completedLessons: number;
  completedLessonIds?: string[];
  progressPercent: number;
  totalEstimatedMinutes?: number;
  flashcardCount?: number;
  dueFlashcardCount?: number;
  masteredFlashcardCount?: number;
  practiceTotalActivities?: number;
  practicePassedActivities?: number;
  practiceAverageScore?: number;
  quizCount?: number;
  tagId?: string | null;
  tagName?: string | null;
  quizId?: string | null;
  lessonQuizMap?: Record<string, string>;
  firstLessonId?: string | null;
  weakChapters?: WeakChapterItem[];
  chapters: CourseChapter[];
}

export interface WorkspaceNotebookAsset {
  id: string;
  title: string;
  version?: number;
  primarySourceLessonId?: string | null;
  primarySourceDocumentId?: string | null;
  updatedAt?: string | null;
}

export interface WorkspaceAnnotationAsset {
  id: string;
  lessonId?: string | null;
  documentId?: string | null;
  pageNumber?: number | null;
  exactQuote?: string;
  colorKey?: string;
  semanticLabel?: string;
  noteComment?: string;
  updatedAt?: string | null;
}

export interface WorkspaceFlashcardAsset {
  id: string;
  lessonId?: string | null;
  documentId?: string | null;
  cardType?: string;
  sourceTitleSnapshot?: string;
  frontText?: string;
  backText?: string;
  state: number;
  reps: number;
  dueAt: string;
}

export interface WorkspaceLessonAsset {
  id: string;
  title: string;
  slug?: string;
  chapterId?: string;
  sortOrder?: number;
}

export interface WorkspaceQuizAsset {
  id: string;
  title: string;
  lessonId?: string | null;
  documentId?: string | null;
  totalItems: number;
  correctItems: number;
  status: string;
  startedAt?: string | null;
  completedAt?: string | null;
}

export interface WorkspaceAssetsData {
  courseId: string;
  notebooks: WorkspaceNotebookAsset[];
  annotations: WorkspaceAnnotationAsset[];
  flashcards: {
    total: number;
    due: number;
    mastered: number;
    items: WorkspaceFlashcardAsset[];
  };
  bookmarkedLessons: WorkspaceLessonAsset[];
  needsReviewLessons: WorkspaceLessonAsset[];
  quizzes: WorkspaceQuizAsset[];
  practiceSummary: {
    totalActivities: number;
    passedActivities: number;
    averageScore: number;
  };
}

export interface CourseSearchResultItem {
  lessonId: string;
  chapterId: string;
  chapterTitle: string;
  lessonTitle: string;
  snippet?: string;
  matchCount: number;
}

export interface CourseSearchResponse {
  query: string;
  totalMatches: number;
  results: CourseSearchResultItem[];
}
