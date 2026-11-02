import type { Metadata } from "next";
import "./globals.css";
import { ThemeSynchronizer } from "@/components/layout/ThemeSynchronizer";

export const metadata: Metadata = {
  title: "SWMS — Không gian Học tập Thống nhất cho Cá nhân Chủ quyền",
  description:
    "Study Workspace Management System: Deep Reader (68ch), Split Workspace PKM, Native FSRS v5 Active Recall & Formative AI Rubric Quizzes."
};

const THEME_INLINE_SCRIPT = `(function(){try{var s=localStorage.getItem("swms_theme");var d=s?s==="dark":window.matchMedia("(prefers-color-scheme: dark)").matches;if(d){document.documentElement.classList.add("dark");if(document.body){document.body.classList.add("dark")}}else{document.documentElement.classList.remove("dark");if(document.body){document.body.classList.remove("dark")}}}catch(e){}})();`;

export default function RootLayout({
  children
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi" suppressHydrationWarning>
      <head>
        <script dangerouslySetInnerHTML={{ __html: THEME_INLINE_SCRIPT }} />
      </head>
      <body className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 antialiased selection:bg-indigo-500/20">
        <ThemeSynchronizer />
        {children}
      </body>
    </html>
  );
}
