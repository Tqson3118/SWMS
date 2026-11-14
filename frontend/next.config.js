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
                destination: "http://127.0.0.1:5080/api/v1/:path*"
            }
        ];
    }
};
export default nextConfig;
