const backendBaseUrl = (
    process.env.BACKEND_INTERNAL_URL ||
    process.env.NEXT_PUBLIC_API_BASE_URL ||
    "http://127.0.0.1:5080"
).replace(/\/+$/, "");

const nextConfig = {
    reactStrictMode: true,
    pageExtensions: ["tsx", "ts"],
    webpack: (config) => {
        config.resolve.extensions = [".tsx", ".ts", ".jsx", ".js", ".json"];
        return config;
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
