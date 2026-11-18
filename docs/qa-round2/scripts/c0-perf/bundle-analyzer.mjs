import fs from 'node:fs';
import path from 'node:path';

const NEXT_DIR = path.resolve('frontend/.next');
const APP_MANIFEST_PATH = path.join(NEXT_DIR, 'app-build-manifest.json');
const BUILD_MANIFEST_PATH = path.join(NEXT_DIR, 'build-manifest.json');
const OUTPUT_JSON_PATH = path.resolve('docs/qa-round2/data/c0-perf/bundle-analysis.json');

function analyzeBundles() {
  if (!fs.existsSync(APP_MANIFEST_PATH)) {
    throw new Error(`Cannot find app-build-manifest at ${APP_MANIFEST_PATH}`);
  }

  const appManifest = JSON.parse(fs.readFileSync(APP_MANIFEST_PATH, 'utf8'));
  const buildManifest = fs.existsSync(BUILD_MANIFEST_PATH)
    ? JSON.parse(fs.readFileSync(BUILD_MANIFEST_PATH, 'utf8'))
    : { pages: {} };

  // Shared chunks (shared across routes)
  const sharedChunks = new Set();
  const rootFiles = appManifest.pages['/layout'] || [];
  for (const f of rootFiles) {
    sharedChunks.add(f);
  }

  // Calculate size of each chunk file
  const chunkSizes = {};
  const staticDir = path.join(NEXT_DIR, 'static');
  function scanDir(dir) {
    if (!fs.existsSync(dir)) return;
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    for (const ent of entries) {
      const full = path.join(dir, ent.name);
      if (ent.isDirectory()) {
        scanDir(full);
      } else if (ent.isFile() && ent.name.endsWith('.js')) {
        const rel = path.relative(NEXT_DIR, full).replace(/\\/g, '/');
        const stat = fs.statSync(full);
        chunkSizes[rel] = stat.size;
      }
    }
  }
  scanDir(staticDir);

  // Inspect each chunk to find which heavy third-party libraries it contains
  const chunkLibraryMap = {};
  const knownLibs = [
    { name: 'pdfjs-dist', pattern: /pdfjs|pdf\.js|getDocument|GlobalWorkerOptions/i },
    { name: 'gsap', pattern: /gsap|TweenMax|ScrollTrigger|Power2|TimelineLite/i },
    { name: 'lucide-react', pattern: /lucide|createLucideIcon/i },
    { name: 'react / react-dom', pattern: /react\.element|useSyncExternalStore|react-dom/i },
    { name: 'zustand', pattern: /zustand|createStore/i },
  ];

  for (const [relPath, size] of Object.entries(chunkSizes)) {
    const full = path.join(NEXT_DIR, relPath);
    try {
      const content = fs.readFileSync(full, 'utf8');
      const detected = [];
      for (const lib of knownLibs) {
        if (lib.pattern.test(content)) {
          detected.push(lib.name);
        }
      }
      chunkLibraryMap[relPath] = detected;
    } catch {
      chunkLibraryMap[relPath] = [];
    }
  }

  // Route breakdown
  const routeStats = {};
  for (const [route, files] of Object.entries(appManifest.pages)) {
    const fileList = [...files];
    let routeBytes = 0;
    const chunks = [];

    for (const file of fileList) {
      const size = chunkSizes[file] || 0;
      routeBytes += size;
      chunks.push({
        file,
        sizeBytes: size,
        sizeKB: +(size / 1024).toFixed(2),
        libs: chunkLibraryMap[file] || [],
      });
    }

    routeStats[route] = {
      route,
      totalBytes: routeBytes,
      totalKB: +(routeBytes / 1024).toFixed(2),
      chunkCount: fileList.length,
      chunks: chunks.sort((a, b) => b.sizeBytes - a.sizeBytes),
    };
  }

  // Sort routes by total size
  const sortedRoutes = Object.values(routeStats).sort((a, b) => b.totalBytes - a.totalBytes);

  // Top chunks
  const topChunks = Object.entries(chunkSizes)
    .map(([file, size]) => ({
      file,
      sizeBytes: size,
      sizeKB: +(size / 1024).toFixed(2),
      libs: chunkLibraryMap[file] || [],
    }))
    .sort((a, b) => b.sizeBytes - a.sizeBytes)
    .slice(0, 20);

  const result = {
    analyzedAt: new Date().toISOString(),
    totalJsChunksCount: Object.keys(chunkSizes).length,
    totalJsChunksBytes: Object.values(chunkSizes).reduce((a, b) => a + b, 0),
    totalJsChunksKB: +(Object.values(chunkSizes).reduce((a, b) => a + b, 0) / 1024).toFixed(2),
    sortedRoutes,
    topChunks,
  };

  fs.mkdirSync(path.dirname(OUTPUT_JSON_PATH), { recursive: true });
  fs.writeFileSync(OUTPUT_JSON_PATH, JSON.stringify(result, null, 2), 'utf8');

  console.log('=== BUNDLE ANALYSIS COMPLETED ===');
  console.log(`Total JS static files: ${result.totalJsChunksCount} (${result.totalJsChunksKB} KB)`);
  console.log('\nTop 10 Heaviest Routes:');
  sortedRoutes.slice(0, 10).forEach((r, i) => {
    console.log(`${(i + 1).toString().padStart(2)}. ${r.route.padEnd(35)} ${r.totalKB.toFixed(1)} KB (${r.chunkCount} chunks)`);
  });

  console.log('\nTop 8 Heaviest Chunks:');
  topChunks.slice(0, 8).forEach((c, i) => {
    console.log(`${(i + 1).toString().padStart(2)}. ${c.file.padEnd(50)} ${c.sizeKB.toFixed(1)} KB [${c.libs.join(', ') || 'custom'}]`);
  });

  return result;
}

try {
  analyzeBundles();
} catch (err) {
  console.error(err);
  process.exit(1);
}
