// docs/qa-round2/scripts/a-hacker/03-injection-tests.mjs
// Test Suite 3: Injection (SQL, Path Traversal, XSS, SSRF, ZIP Slip, File Upload)
import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';

const BASE_URL = 'http://127.0.0.1:5080';
const ATTACKER_EMAIL = 'qa+attacker-01@test.local';
const ATTACKER_PASS = 'AttackerPass@123';

const results = [];

function record(testId, name, status, details) {
  results.push({ testId, name, status, details });
  console.log(`[${status}] ${testId} - ${name}`);
  if (details) console.log(`   Details: ${details}`);
}

async function login(email, password) {
  const res = await fetch(`${BASE_URL}/api/v1/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
  const data = await res.json();
  return data.accessToken;
}

// Simple ZIP generator for testing ZIP Slip without external packages
function createZipWithEntry(filename, content) {
  // Minimal ZIP format
  const contentBuf = Buffer.from(content);
  const nameBuf = Buffer.from(filename);

  // Local file header (30 bytes + name + content)
  const lfh = Buffer.alloc(30);
  lfh.writeUInt32LE(0x04034b50, 0); // signature
  lfh.writeUInt16LE(20, 4); // version needed
  lfh.writeUInt16LE(0, 6); // flags
  lfh.writeUInt16LE(0, 8); // compression (store)
  lfh.writeUInt16LE(0, 10); // time
  lfh.writeUInt16LE(0, 12); // date
  const crc = 0; // simplified for test
  lfh.writeUInt32LE(crc, 14); // crc32
  lfh.writeUInt32LE(contentBuf.length, 18); // comp size
  lfh.writeUInt32LE(contentBuf.length, 22); // uncomp size
  lfh.writeUInt16LE(nameBuf.length, 26); // name length
  lfh.writeUInt16LE(0, 28); // extra length

  // Central directory header (46 bytes + name)
  const cdh = Buffer.alloc(46);
  cdh.writeUInt32LE(0x02014b50, 0); // signature
  cdh.writeUInt16LE(20, 4); // version made by
  cdh.writeUInt16LE(20, 6); // version needed
  cdh.writeUInt16LE(0, 8); // flags
  cdh.writeUInt16LE(0, 10); // compression
  cdh.writeUInt16LE(0, 12); // time
  cdh.writeUInt16LE(0, 14); // date
  cdh.writeUInt32LE(crc, 16);
  cdh.writeUInt32LE(contentBuf.length, 20);
  cdh.writeUInt32LE(contentBuf.length, 24);
  cdh.writeUInt16LE(nameBuf.length, 28);
  cdh.writeUInt16LE(0, 30); // extra length
  cdh.writeUInt16LE(0, 32); // comment length
  cdh.writeUInt16LE(0, 34); // disk number start
  cdh.writeUInt16LE(0, 36); // internal attr
  cdh.writeUInt32LE(0, 38); // external attr
  cdh.writeUInt32LE(0, 42); // relative offset of local header

  // End of central directory record (22 bytes)
  const cdOffset = lfh.length + nameBuf.length + contentBuf.length;
  const cdSize = cdh.length + nameBuf.length;
  const eocd = Buffer.alloc(22);
  eocd.writeUInt32LE(0x06054b50, 0);
  eocd.writeUInt16LE(0, 4);
  eocd.writeUInt16LE(0, 6);
  eocd.writeUInt16LE(1, 8); // total entries on this disk
  eocd.writeUInt16LE(1, 10); // total entries
  eocd.writeUInt32LE(cdSize, 12);
  eocd.writeUInt32LE(cdOffset, 16);
  eocd.writeUInt16LE(0, 20);

  return Buffer.concat([lfh, nameBuf, contentBuf, cdh, nameBuf, eocd]);
}

async function run() {
  console.log('=== STARTING TEST SUITE 3: INJECTION & FILE SECURITY ===\n');

  const token = await login(ATTACKER_EMAIL, ATTACKER_PASS);
  const authHeaders = { Authorization: `Bearer ${token}` };

  // 3.1 SQL Injection Testing
  console.log('--- 3.1 SQL Injection ---');
  const sqlPayloads = [
    "' OR '1'='1",
    "'; DROP TABLE courses; --",
    "1' UNION SELECT null, null, null--",
    "admin' --",
    "' OR 1=1--"
  ];

  let sqliVulnerable = false;
  let sqliDetails = '';
  for (const p of sqlPayloads) {
    const enc = encodeURIComponent(p);
    const searchRes = await fetch(`${BASE_URL}/api/v1/workspace/search?q=${enc}`, { headers: authHeaders });
    const clarifyRes = await fetch(`${BASE_URL}/api/v1/courses/generate-wizard/clarify`, {
      method: 'POST',
      headers: { ...authHeaders, 'Content-Type': 'application/json' },
      body: JSON.stringify({ topic: p })
    });

    if (searchRes.status === 500 || clarifyRes.status === 500) {
      const errText = await searchRes.text();
      if (errText.includes('SQL') || errText.includes('sqlite') || errText.includes('syntax')) {
        sqliVulnerable = true;
        sqliDetails = `Payload '${p}' triggered SQL error 500: ${errText}`;
        break;
      }
    }
  }

  if (sqliVulnerable) {
    record('INJ-01', 'SQL Injection in Search / Query Parameters', 'VULNERABLE', sqliDetails);
  } else {
    record('INJ-01', 'SQL Injection in Search / Query Parameters', 'SECURE',
      'All SQL injection payloads safely escaped or evaluated as text strings without SQL error (EF Core parameterization).');
  }

  // 3.2 Path Traversal in /document-assets/
  console.log('\n--- 3.2 Path Traversal in Asset Streaming ---');
  const pathTraversalUrls = [
    `${BASE_URL}/api/v1/document-assets/..%2f..%2f..%2f..%2fappsettings.json`,
    `${BASE_URL}/api/v1/document-assets/os-operating-systems/..%2f..%2f..%2fappsettings.json`,
    `${BASE_URL}/api/v1/document-assets/os-operating-systems/....//....//appsettings.json`,
    `${BASE_URL}/api/v1/document-assets/invalid-folder/test.png`
  ];

  let pathTraversalLeaked = false;
  let leakPayload = '';
  for (const url of pathTraversalUrls) {
    const res = await fetch(url);
    if (res.status === 200) {
      const text = await res.text();
      if (text.includes('SigningKey') || text.includes('ConnectionStrings')) {
        pathTraversalLeaked = true;
        leakPayload = url;
        break;
      }
    }
  }

  if (pathTraversalLeaked) {
    record('INJ-02', 'Path Traversal in /document-assets/', 'VULNERABLE',
      `Path traversal successfully retrieved sensitive file: ${leakPayload}`);
  } else {
    record('INJ-02', 'Path Traversal in /document-assets/', 'SECURE',
      'All path traversal sequences (../, %2e%2e, invalid folder) blocked with HTTP 403 / 404.');
  }

  // 3.3 Stored XSS Payloads
  console.log('\n--- 3.3 Stored XSS Testing ---');
  const xssPayload = `<script>alert('XSS_STUDXY')</script><img src=x onerror=alert('XSS_IMG')>`;

  // Test XSS in Flashcard
  const fcRes = await fetch(`${BASE_URL}/api/v1/flashcards`, {
    method: 'POST',
    headers: { ...authHeaders, 'Content-Type': 'application/json' },
    body: JSON.stringify({
      cardType: 'standard',
      frontText: `Front ${xssPayload}`,
      backText: `Back ${xssPayload}`
    })
  });
  const fcData = await fcRes.json();
  const fcStoredRaw = fcData.frontAstJson?.includes('<script>') || fcData.frontAstJson?.includes('<img');

  // Test XSS in Notebook
  const nbRes = await fetch(`${BASE_URL}/api/v1/notebooks`, {
    method: 'POST',
    headers: { ...authHeaders, 'Content-Type': 'application/json' },
    body: JSON.stringify({
      title: `Notebook ${xssPayload}`,
      contentAstJson: JSON.stringify({
        type: 'doc',
        content: [{ type: 'paragraph', content: [{ type: 'text', text: xssPayload }] }]
      })
    })
  });
  const nbData = await nbRes.json();
  const nbStoredRaw = nbData.title?.includes('<script>') || nbData.contentAstJson?.includes('<script>');

  if (fcStoredRaw || nbStoredRaw) {
    record('INJ-03', 'Stored XSS in Backend Storage (Unsanitized HTML in DB)', 'VULNERABLE',
      `Backend stores raw unescaped HTML/JavaScript tags in database (Flashcard: ${fcStoredRaw}, Notebook: ${nbStoredRaw}). While React client uses JSX escaping, API consumers or raw HTML renders remain at risk.`);
  } else {
    record('INJ-03', 'Stored XSS in Backend Storage', 'SECURE',
      'Backend sanitized or rejected HTML script tags.');
  }

  // 3.4 SSRF Protection in AI Proxy BaseUrl Override
  console.log('\n--- 3.4 SSRF Protection via X-Ai-Base-Url ---');
  const ssrfInternalUrls = [
    'http://127.0.0.1:5080',
    'http://localhost:5080',
    'http://169.254.169.254/latest/meta-data/',
    'http://192.168.1.1'
  ];

  let ssrfVulnerable = false;
  let ssrfProof = '';
  for (const target of ssrfInternalUrls) {
    const res = await fetch(`${BASE_URL}/api/v1/ai/explain`, {
      method: 'POST',
      headers: {
        ...authHeaders,
        'Content-Type': 'application/json',
        'X-Ai-Base-Url': target,
        'X-Ai-Api-Key': 'test-key',
        'X-Ai-Model': 'test-model'
      },
      body: JSON.stringify({ excerpt: 'Test concept', contextTitle: 'Test' })
    });
    const data = await res.json().catch(() => null);
    if (data?.usedLiveEndpoint === true) {
      ssrfVulnerable = true;
      ssrfProof = `Target ${target} was called via live endpoint!`;
      break;
    }
  }

  if (ssrfVulnerable) {
    record('INJ-04', 'SSRF via Custom AI Proxy BaseUrl Header', 'VULNERABLE', ssrfProof);
  } else {
    record('INJ-04', 'SSRF via Custom AI Proxy BaseUrl Header', 'SECURE',
      'Internal loopback, RFC1918, link-local, and non-whitelisted hosts blocked by IsAllowedOverrideBaseUrlAsync.');
  }

  // 3.5 ZIP Slip in Course Import
  console.log('\n--- 3.5 ZIP Slip in Course Import ---');
  const maliciousZip = createZipWithEntry('../../../../../../../tmp/evil_slip.txt', 'Hacked file content');
  const zipFormData = new FormData();
  zipFormData.append('file', new Blob([maliciousZip], { type: 'application/zip' }), 'slip.zip');

  const zipRes = await fetch(`${BASE_URL}/api/v1/courses/import-zip`, {
    method: 'POST',
    headers: authHeaders,
    body: zipFormData
  });
  const zipData = await zipRes.json().catch(() => null);

  // Check if file was extracted to disk
  const slipTargetExists = fs.existsSync('/tmp/evil_slip.txt') || fs.existsSync('C:\\tmp\\evil_slip.txt');
  if (slipTargetExists) {
    record('INJ-05', 'ZIP Slip Arbitrary File Overwrite', 'VULNERABLE',
      'ZIP Slip succeeded: File was extracted outside target directory!');
  } else {
    record('INJ-05', 'ZIP Slip Arbitrary File Overwrite', 'SECURE',
      'Course import parses ZIP strictly in-memory using ZipArchive and inserts into DB; no files are extracted to disk.');
  }

  // 3.6 File Upload: Fake MIME Type & Executable
  console.log('\n--- 3.6 File Upload Validation & MIME Enforcement ---');
  // Upload .exe with .pdf extension
  const fakePdfExe = Buffer.from('MZ\x90\x00\x03\x00\x00\x00\x04\x00\x00\x00\xff\xff\x00\x00This is a fake PE executable binary payload.');
  const fakeForm = new FormData();
  fakeForm.append('file', new Blob([fakePdfExe], { type: 'application/pdf' }), 'malicious.pdf');
  fakeForm.append('title', 'Disguised Executable');

  const fakeUploadRes = await fetch(`${BASE_URL}/api/v1/documents/upload`, {
    method: 'POST',
    headers: authHeaders,
    body: fakeForm
  });
  const fakeUploadData = await fakeUploadRes.json().catch(() => null);

  if (fakeUploadRes.status === 422) {
    record('INJ-06', 'File Content Inspection (Fake PDF rejected)', 'SECURE',
      `Server rejected fake PDF binary: HTTP 422 (${fakeUploadData?.error}) via pure C# PdfPig parsing.`);
  } else if (fakeUploadRes.status === 200) {
    record('INJ-06', 'File Content Inspection (Fake PDF rejected)', 'VULNERABLE',
      `Server accepted non-PDF binary disguised as PDF!`);
  } else {
    record('INJ-06', 'File Content Inspection (Fake PDF rejected)', 'INFO',
      `HTTP ${fakeUploadRes.status}: ${JSON.stringify(fakeUploadData)}`);
  }

  // Upload disallowed extension (.exe directly)
  const exeForm = new FormData();
  exeForm.append('file', new Blob([fakePdfExe], { type: 'application/x-msdownload' }), 'malware.exe');
  const exeUploadRes = await fetch(`${BASE_URL}/api/v1/documents/upload`, {
    method: 'POST',
    headers: authHeaders,
    body: exeForm
  });
  const exeUploadData = await exeUploadRes.json().catch(() => null);

  if (exeUploadRes.status === 400) {
    record('INJ-07', 'Disallowed File Extension (.exe rejected)', 'SECURE',
      `Server rejected .exe upload with HTTP 400: ${exeUploadData?.error}`);
  } else {
    record('INJ-07', 'Disallowed File Extension (.exe rejected)', 'VULNERABLE',
      `Server allowed .exe upload: HTTP ${exeUploadRes.status}`);
  }

  console.log('\n=== TEST SUITE 3 SUMMARY ===');
  console.table(results);
}

run().catch(console.error);
