import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

function validatedTarget(raw: string | undefined): string | undefined {
  if (!raw?.trim()) return undefined;
  let parsed: URL;
  try { parsed = new URL(raw.trim()); } catch { throw new Error('VITE_API_TARGET must be an HTTP loopback origin with an explicit port.'); }
  if (parsed.protocol !== 'http:' || !['localhost', '127.0.0.1', '[::1]', '::1'].includes(parsed.hostname) || !parsed.port || parsed.username || parsed.password || parsed.pathname !== '/' || parsed.search || parsed.hash) throw new Error('VITE_API_TARGET must be an HTTP loopback origin with an explicit port and no path, query, credentials, or hash.');
  return parsed.origin;
}

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const target = validatedTarget(env.VITE_API_TARGET);
  return {
    plugins: [react()],
    test: { environment: 'jsdom', setupFiles: './src/test/setup.ts', globals: true },
    server: { port: 5173, ...(target ? { proxy: { '/api': { target, changeOrigin: true } } } : {}) }
  };
});
