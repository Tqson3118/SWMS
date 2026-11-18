import { LandingPageClient } from "@/components/landing/LandingPageClient";
export const metadata = {
    title: "Studxy — Không gian Tự học & Ghi nhớ Thông minh",
    description: "Đọc tài liệu, ghi chú sổ tay và ôn thẻ nhớ lâu trong một nơi duy nhất. Biến mọi tài liệu thành kiến thức của riêng bạn.",
    keywords: [
        "SWMS",
        "Studxy",
        "Sovereign Learning",
        "Đọc sâu",
        "Chia đôi",
        "FSRS v5",
        "Sổ tay ghi chú",
        "Ôn thẻ nhớ"
    ],
    openGraph: {
        title: "Studxy — Không gian Tự học & Ghi nhớ Thông minh",
        description: "Đọc tài liệu, ghi chú sổ tay và ôn thẻ nhớ lâu trong một nơi duy nhất. Biến mọi tài liệu thành kiến thức của riêng bạn.",
        type: "website",
        locale: "vi_VN"
    }
};
export default function HomePage() {
    return <LandingPageClient />;
}
