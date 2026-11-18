import type { Metadata } from "next";
import { LandingPageV2 } from "@/components/landing/LandingPageV2";

export const metadata: Metadata = {
  title: "Studxy — Không gian Tự học & Ghi nhớ Thông minh",
  description:
    "Đọc tài liệu, ghi chú sổ tay và ôn thẻ nhớ lâu trong một nơi duy nhất. Biến mọi tài liệu thành kiến thức của riêng bạn.",
  keywords: [
    "Studxy",
    "SWMS",
    "Đọc sâu",
    "Chia đôi",
    "FSRS v5",
    "Thẻ nhớ",
    "Tự học",
    "Course Studio",
  ],
  openGraph: {
    title: "Studxy — Không gian Tự học & Ghi nhớ Thông minh",
    description:
      "Đọc tài liệu, ghi chú sổ tay và ôn thẻ nhớ lâu trong một nơi duy nhất. Biến mọi tài liệu thành kiến thức của riêng bạn.",
    type: "website",
    locale: "vi_VN",
  },
};

export default function HomePage() {
  return <LandingPageV2 />;
}
