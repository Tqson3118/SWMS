import { LandingPageClient } from "@/components/landing/LandingPageClient";
export const metadata = {
    title: "SWMS — Không gian Học tập Thống nhất cho Cá nhân Chủ quyền",
    description: "Nền tảng quản trị học tập hợp nhất: Đọc sâu công thái học (68ch), Ghi chép song song (Split PKM), Thuật toán FSRS v5 tối ưu trí nhớ và Luyện thi Formative AI Quiz.",
    keywords: [
        "SWMS",
        "Studxy",
        "Sovereign Learning",
        "Deep Reader",
        "Split PKM",
        "FSRS v5",
        "Spaced Repetition",
        "Active Recall",
        "Personal Knowledge Management"
    ],
    openGraph: {
        title: "SWMS — Hệ sinh thái Học tập Thống nhất cho Cá nhân Chủ quyền",
        description: "Hợp nhất Đọc sâu, Ghi chú song song và Khoa học Trí nhớ FSRS v5 trong một không gian duy nhất.",
        type: "website",
        locale: "vi_VN"
    }
};
export default function HomePage() {
    return <LandingPageClient />;
}
