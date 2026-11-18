// docs/qa-round2/scripts/a-hacker/06-info-disclosure-tests.mjs
// Test Suite 6: Information Disclosure & Debug / Demo Endpoints

const BACKEND_URL = 'http://127.0.0.1:5080';
const FRONTEND_URL = 'http://127.0.0.1:3000';

const results = [];

function record(testId, name, status, details) {
  results.push({ testId, name, status, details });
  console.log(`[${status}] ${testId} - ${name}`);
  if (details) console.log(`   Details: ${details}`);
}

async function run() {
  console.log('=== STARTING TEST SUITE 6: INFORMATION DISCLOSURE ===\n');

  // 6.1 Swagger / OpenAPI Endpoint Exposure
  console.log('--- 6.1 Swagger / OpenAPI Documentation ---');
  const swaggerPaths = [
    '/swagger',
    '/swagger/index.html',
    '/swagger/v1/swagger.json',
    '/openapi/v1.json',
    '/api-docs'
  ];

  let swaggerExposed = false;
  let exposedUrl = '';
  for (const p of swaggerPaths) {
    const res = await fetch(`${BACKEND_URL}${p}`);
    if (res.status === 200) {
      swaggerExposed = true;
      exposedUrl = `${BACKEND_URL}${p}`;
      break;
    }
  }

  if (swaggerExposed) {
    record('INFO-01', 'Swagger UI / OpenAPI Definition Publicly Exposed', 'VULNERABLE',
      `Swagger/OpenAPI is accessible in production at ${exposedUrl}`);
  } else {
    record('INFO-01', 'Swagger UI / OpenAPI Documentation', 'SECURE',
      'Swagger / OpenAPI endpoints are disabled or not registered in production.');
  }

  // 6.2 Stack Trace Leaks on Malformed Requests
  console.log('\n--- 6.2 Stack Trace Disclosure on Malformed Requests ---');
  const malformedTests = [
    { name: 'Invalid JSON Syntax', url: `${BACKEND_URL}/api/v1/auth/login`, method: 'POST', body: '{ email: "test"' },
    { name: 'Invalid GUID Format', url: `${BACKEND_URL}/api/v1/courses/not-a-valid-guid-1234`, method: 'GET' },
    { name: 'Type Mismatch', url: `${BACKEND_URL}/api/v1/documents/00000000-0000-0000-0000-000000000001/pages/notanumber`, method: 'GET' }
  ];

  let stackTraceLeaked = false;
  let leakInfo = '';
  for (const t of malformedTests) {
    const res = await fetch(t.url, {
      method: t.method,
      headers: { 'Content-Type': 'application/json' },
      body: t.body
    });
    const text = await res.text();
    if (text.includes('System.Exception') || text.includes('at Swms.Api') || text.includes('StackTrace') || text.includes('.cs:line')) {
      stackTraceLeaked = true;
      leakInfo = `${t.name}: Leaked C# stack trace in response! (${text.slice(0, 150)}...)`;
      break;
    }
  }

  if (stackTraceLeaked) {
    record('INFO-02', 'C# Exception Stack Trace Disclosure', 'VULNERABLE', leakInfo);
  } else {
    record('INFO-02', 'C# Exception Stack Trace Disclosure', 'SECURE',
      'Malformed requests handled gracefully without leaking server stack traces (standard 400/404/405).');
  }

  // 6.3 Sensitive Static Files Disclosure (.env, .git, db)
  console.log('\n--- 6.3 Sensitive Static Files Disclosure ---');
  const sensitiveFiles = [
    { target: 'Frontend', url: `${FRONTEND_URL}/.env` },
    { target: 'Frontend', url: `${FRONTEND_URL}/.env.local` },
    { target: 'Frontend', url: `${FRONTEND_URL}/.git/HEAD` },
    { target: 'Backend', url: `${BACKEND_URL}/.env` },
    { target: 'Backend', url: `${BACKEND_URL}/appsettings.json` },
    { target: 'Backend', url: `${BACKEND_URL}/swms_local_dev.db` }
  ];

  let sensitiveFileLeaked = false;
  let leakedFileDetails = '';
  for (const f of sensitiveFiles) {
    const res = await fetch(f.url);
    if (res.status === 200) {
      const content = await res.text();
      // Verify it's actually the file, not Next.js fallback HTML
      if (!content.includes('<!DOCTYPE html>') && (content.includes('KEY') || content.includes('ref:') || content.includes('SQLite'))) {
        sensitiveFileLeaked = true;
        leakedFileDetails = `${f.target} served sensitive file at ${f.url}`;
        break;
      }
    }
  }

  if (sensitiveFileLeaked) {
    record('INFO-03', 'Sensitive Static File Exposure (.env, .git, database)', 'VULNERABLE', leakedFileDetails);
  } else {
    record('INFO-03', 'Sensitive Static File Exposure (.env, .git, database)', 'SECURE',
      'Sensitive files (.env, .git, db, appsettings.json) are not served as static files (404/Not Found).');
  }

  // 6.4 Hardcoded Demo Passwords in Client-Side JavaScript
  console.log('\n--- 6.4 Client-Side Bundle Inspection (Demo Accounts) ---');
  // Check if frontend HTML or JS chunk exposes demo credentials
  const authHtmlRes = await fetch(`${FRONTEND_URL}/auth`);
  const authHtml = await authHtmlRes.text();
  
  // Extract script src tags from HTML
  const scriptMatches = [...authHtml.matchAll(/src="(\/_next\/static\/chunks\/[^"]+)"/g)].map(m => m[1]);
  let leaksPlaintextCredentials = false;
  let leakedScriptUrl = '';

  for (const scriptPath of scriptMatches) {
    const sRes = await fetch(`${FRONTEND_URL}${scriptPath}`);
    const sText = await sRes.text();
    if (sText.includes('Studxy@2026') || sText.includes('MinhAnhPassword123!')) {
      leaksPlaintextCredentials = true;
      leakedScriptUrl = `${FRONTEND_URL}${scriptPath}`;
      break;
    }
  }

  if (leaksPlaintextCredentials) {
    record('INFO-04', 'Hardcoded User Passwords in Client-Side Production JS Chunks', 'VULNERABLE',
      `The production JavaScript chunk (${leakedScriptUrl}) embeds plaintext passwords for "son.tran@studxy.vn" ("Studxy@2026") and "minh.anh@studxy.vn" ("MinhAnhPassword123!"). Any visitor can view source to extract credentials.`);
  } else {
    record('INFO-04', 'Hardcoded User Passwords in Client-Side Bundle', 'SECURE',
      'Plaintext credentials not detected in public JS chunks.');
  }

  // 6.5 Configuration Secrets in appsettings.json
  console.log('\n--- 6.5 Source Code / Config Secrets Audit ---');
  // Record vulnerability for committed secrets
  record('INFO-05', 'Hardcoded JWT Signing Key & AI Proxy API Key in appsettings.json', 'VULNERABLE',
    'appsettings.json commits production fallback secrets into version control: Jwt:SigningKey ("SWMS-v1-Enterprise-Jwt-HmacSha256-Secret-Key-2026-Studxy-Workspace!"), AiProxy:ApiKey ("sk-5a76..."), and Postgres credentials.');

  console.log('\n=== TEST SUITE 6 SUMMARY ===');
  console.table(results);
}

run().catch(console.error);
