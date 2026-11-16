import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
function proxyOrigin(raw: string | undefined): string {
  if (!raw) throw new Error('VITE_API_ORIGIN is required for the development server and must be http://127.0.0.1:<port>.');
  let value: URL;
  try { value = new URL(raw); } catch { throw new Error('VITE_API_ORIGIN must be http://127.0.0.1:<port>.'); }
  if (value.protocol !== 'http:' || value.hostname !== '127.0.0.1' || !value.port || value.username || value.password || value.pathname !== '/' || value.search || value.hash) throw new Error('VITE_API_ORIGIN must be a loopback HTTP origin without credentials or a path.');
  return value.origin;
}
export default defineConfig(({ mode, command }) => { const env = loadEnv(mode, process.cwd(), ''); return { plugins: [react()], server: command === 'serve' ? { proxy: { '/api': { target: proxyOrigin(env.VITE_API_ORIGIN), changeOrigin: false } } } : undefined }; });
