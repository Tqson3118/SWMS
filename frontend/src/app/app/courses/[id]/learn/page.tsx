"use client";

import React, { Suspense } from "react";
import { useParams, useSearchParams } from "next/navigation";
import { CourseLearningWorkspace } from "@/components/learn/CourseLearningWorkspace";

function LearnPageInner() {
  const params = useParams();
  const searchParams = useSearchParams();
  const courseId = String(params.id || "standalone");
  const lessonParam = searchParams.get("lesson") || searchParams.get("lessonId");
  const docParam = searchParams.get("doc");
  const notebookParam = searchParams.get("notebook");
  const openNotesInitially =
    searchParams.get("notes") === "open" || searchParams.get("split") === "true";

  return (
    <CourseLearningWorkspace
      courseId={courseId}
      initialLessonId={lessonParam}
      initialDocId={docParam}
      initialNotebookId={notebookParam}
      openNotesInitially={openNotesInitially}
    />
  );
}

export default function LearnPage() {
  return (
    <Suspense
      fallback={
        <div className="flex items-center justify-center h-screen bg-bg-primary">
          <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
        </div>
      }
    >
      <LearnPageInner />
    </Suspense>
  );
}

