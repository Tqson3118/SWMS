import fs from 'node:fs';
import path from 'node:path';

const NEXT_DIR = path.resolve('frontend/.next');
const APP_MANIFEST_PATH = path.join(NEXT_DIR, 'app-build-manifest.json');
const BUILD_MANIFEST_PATH = path.join(NEXT_DIR, 'build-manifest.json');
const OUTPUT_JSON_PATH = path.resolve('docs/qa-round2/data/c1-perf/bundle-analysis.json');

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

  // Find heavy library distribution
  const heavyLibUsage = {};
  for (const lib of knownLibs) {
    heavyLibUsage[lib.name] = [];
  }

  for (const [route, stats] of Object.entries(routeStats)) {
    for (const chunk of stats.chunks) {
      for (const libName of chunk.libs) {
        if (!heavyLibUsage[libName].some(r => r.route === route)) {
          heavyLibUsage[libName].push({
            route,
            chunk: chunk.file,
            chunkSizeKB: chunk.sizeKB,
          });
        }
      }
    }
  }

  // Chunks > 50KB
  const largeChunks = Object.entries(chunkSizes)
    .filter(([_, size]) => size > 50 * 1024)
    .map(([file, size]) => ({
      file,
      sizeBytes: size,
      sizeKB: +(size / 1024).toFixed(2),
      libs: chunkLibraryMap[file] || [],
    }))
    .sort((a, b) => b.sizeBytes - a.sizeBytes);

  const report = {
    timestamp: new Date().toISOString(),
    totalAnalyzedChunks: Object.keys(chunkSizes).length,
    largeChunksCount50KBPlus: largeChunks.length,
    largeChunks,
    routesCount: sortedRoutes.length,
    sortedRoutes,
    heavyLibUsage,
  };

  fs.mkdirSync(path.dirname(OUTPUT_JSON_PATH), { recursive: true });
  fs.writeFileSync(OUTPUT_JSON_PATH, JSON.stringify(report, null, 2), 'utf8');

  console.log('=== BUNDLE ANALYSIS COMPLETE ===');
  console.log(`Analyzed ${sortedRoutes.length} routes, ${Object.keys(chunkSizes).length} chunks.`);
  console.log(`Saved output to: ${OUTPUT_JSON_PATH}`);
  console.log('\nTop 5 Largest Routes (Total Download JS):');
  for (const r of sortedRoutes.slice(0, 5)) {
    console.log(`  - ${r.route.padEnd(35)} : ${r.totalKB.toFixed(1)} KB (${r.chunkCount} chunks)`);
  }
}

analyzeBundles();
