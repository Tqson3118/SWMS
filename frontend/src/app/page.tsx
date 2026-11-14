import type { Metadata } from "next";
import { LandingPageV2 } from "@/components/landing/LandingPageV2";

export const metadata: Metadata = {
  title: "Studxy — Nền tảng Học tập Thông minh",
  description:
    "Hợp nhất Deep Reader, PKM Notebook và thuật toán FSRS v5 trong một không gian duy nhất. Biến mọi tài liệu thành kiến thức bền vững.",
  keywords: [
    "Studxy",
    "SWMS",
    "Deep Reader",
    "PKM",
    "FSRS v5",
    "Spaced Repetition",
    "Active Recall",
    "Course Studio",
  ],
  openGraph: {
    title: "Studxy — Nền tảng Học tập Thông minh",
    description:
      "Hợp nhất Deep Reader, PKM Notebook và thuật toán FSRS v5. Biến mọi tài liệu thành kiến thức bền vững.",
    type: "website",
    locale: "vi_VN",
  },
};

export default function HomePage() {
  return <LandingPageV2 />;
}
