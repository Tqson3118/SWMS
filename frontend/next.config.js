const backendBaseUrl = (
    process.env.BACKEND_INTERNAL_URL ||
    process.env.NEXT_PUBLIC_API_BASE_URL ||
    "http://127.0.0.1:5080"
).replace(/\/+$/, "");

const nextConfig = {
    reactStrictMode: true,
    poweredByHeader: false,
    pageExtensions: ["tsx", "ts"],
    webpack: (config) => {
        config.resolve.extensions = [".tsx", ".ts", ".jsx", ".js", ".json"];
        return config;
    },
    async headers() {
        return [
            {
                source: "/:path*",
                headers: [
                    {
                        key: "X-Frame-Options",
                        value: "SAMEORIGIN",
                    },
                    {
                        key: "X-Content-Type-Options",
                        value: "nosniff",
                    },
                    {
                        key: "Referrer-Policy",
                        value: "strict-origin-when-cross-origin",
                    },
                    {
                        key: "Content-Security-Policy",
                        value: "default-src 'self' http://127.0.0.1:5080 http://localhost:5080 https://accounts.google.com; script-src 'self' 'unsafe-inline' 'unsafe-eval' blob: https://accounts.google.com; worker-src 'self' blob:; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://accounts.google.com; font-src 'self' data: https://fonts.gstatic.com; img-src 'self' data: blob: https:; connect-src 'self' http://127.0.0.1:5080 http://localhost:5080 https://accounts.google.com; frame-src 'self' https://accounts.google.com https://www.youtube.com https://www.youtube-nocookie.com; frame-ancestors 'self';",
                    },
                ],
            },
        ];
    },
    async rewrites() {
        return [
            {
                source: "/api/v1/:path*",
                destination: `${backendBaseUrl}/api/v1/:path*`
            },
            {
                source: "/health",
                destination: `${backendBaseUrl}/health`
            }
        ];
    }
};
export default nextConfig;
