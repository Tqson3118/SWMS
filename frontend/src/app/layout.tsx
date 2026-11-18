import type { Metadata } from "next";
import "./globals.css";
import { ThemeSynchronizer } from "@/components/layout/ThemeSynchronizer";

export const metadata: Metadata = {
  title: "Studxy — Không gian Tự học & Ghi nhớ Thông minh",
  description:
    "Đọc tài liệu, ghi chú sổ tay và ôn thẻ nhớ lâu trong một nơi duy nhất. Biến mọi tài liệu thành kiến thức của riêng bạn."
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
      <body className="min-h-screen bg-[var(--canvas-bg)] text-[var(--text-primary)] antialiased selection:bg-[#5E51E8]/20 font-sans">
        <ThemeSynchronizer />
        {children}
      </body>
    </html>
  );
}
