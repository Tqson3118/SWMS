// docs/qa-round2/scripts/a-hacker/02-idor-tests.mjs
// Test Suite 2: IDOR / BOLA (Broken Object Level Authorization)
import fs from 'node:fs';

const BASE_URL = 'http://127.0.0.1:5080';
const VICTIM_EMAIL = 'qa+victim-01@test.local';
const VICTIM_PASS = 'VictimPass@123';
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

async function run() {
  console.log('=== STARTING TEST SUITE 2: IDOR / BOLA ===\n');

  const victimToken = await login(VICTIM_EMAIL, VICTIM_PASS);
  const attackerToken = await login(ATTACKER_EMAIL, ATTACKER_PASS);

  const victimHeaders = { Authorization: `Bearer ${victimToken}`, 'Content-Type': 'application/json' };
  const attackerHeaders = { Authorization: `Bearer ${attackerToken}`, 'Content-Type': 'application/json' };

  // Setup Victim Resources:
  // 1. Create a private course via wizard commit
  console.log('Setting up victim resources...');
  const createCourseRes = await fetch(`${BASE_URL}/api/v1/courses/generate-wizard/commit`, {
    method: 'POST',
    headers: victimHeaders,
    body: JSON.stringify({
      title: 'Khóa học Bí Mật của Victim',
      topic: 'Bảo mật thông tin nội bộ',
      chapters: [
        {
          title: 'Chương 1: Tài liệu mật',
          sortOrder: 1,
          lessons: [
            {
              title: 'Bài 1: Mật khẩu và Token quản trị',
              summary: 'Nội dung tuyệt mật không công khai',
              estimatedMinutes: 10
            }
          ]
        }
      ]
    })
  });
  const courseData = await createCourseRes.json();
  const victimCourseId = courseData.courseId || courseData.id;

  // Get chapter and lesson IDs
  const getCourseRes = await fetch(`${BASE_URL}/api/v1/courses/${victimCourseId}`, { headers: victimHeaders });
  const getCourseData = await getCourseRes.json();
  const victimChapterId = getCourseData.chapters?.[0]?.id;
  const victimLessonId = getCourseData.chapters?.[0]?.lessons?.[0]?.id;

  console.log(`Victim Course: ${victimCourseId}`);
  console.log(`Victim Chapter: ${victimChapterId}`);
  console.log(`Victim Lesson: ${victimLessonId}`);

  // Create victim notebook
  const createNbRes = await fetch(`${BASE_URL}/api/v1/notebooks`, {
    method: 'POST',
    headers: victimHeaders,
    body: JSON.stringify({
      title: 'Sổ tay bí mật cá nhân của Victim',
      contentAstJson: JSON.stringify({
        type: 'doc',
        content: [{ type: 'paragraph', content: [{ type: 'text', text: 'Nhật ký bí mật của victim: PIN=9988' }] }]
      })
    })
  });
  const nbData = await createNbRes.json();
  const victimNbId = nbData.id;
  console.log(`Victim Notebook: ${victimNbId}`);

  // Create victim flashcard
  const createFcRes = await fetch(`${BASE_URL}/api/v1/flashcards`, {
    method: 'POST',
    headers: victimHeaders,
    body: JSON.stringify({
      lessonId: victimLessonId,
      cardType: 'standard',
      frontText: 'Câu hỏi riêng tư của victim',
      backText: 'Câu trả lời nhạy cảm'
    })
  });
  const fcData = await createFcRes.json();
  const victimFcId = fcData.id;
  console.log(`Victim Flashcard: ${victimFcId}`);

  // Create victim annotation
  const createAnnRes = await fetch(`${BASE_URL}/api/v1/annotations`, {
    method: 'POST',
    headers: victimHeaders,
    body: JSON.stringify({
      lessonId: victimLessonId,
      exactQuote: 'Mật khẩu và Token quản trị',
      startOffset: 0,
      endOffset: 27,
      colorKey: 'yellow',
      noteComment: 'Ghi chú nhạy cảm của victim'
    })
  });
  const annData = await createAnnRes.json();
  const victimAnnId = annData.id;
  console.log(`Victim Annotation: ${victimAnnId}`);

  // Upload victim document
  const formData = new FormData();
  const samplePdfBytes = Buffer.from('%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n2 0 obj<</Type/Pages/Count 1/Kids[3 0 R]>>endobj\n3 0 obj<</Type/Page/MediaBox[0 0 300 144]/Parent 2 0 R/Resources<<>>>>endobj\nxref\n0 4\n0000000000 65535 f\n0000000010 00000 n\n0000000057 00000 n\n0000000115 00000 n\ntrailer<</Size 4/Root 1 0 R>>\nstartxref\n198\n%%EOF');
  formData.append('file', new Blob([samplePdfBytes], { type: 'application/pdf' }), 'secret_victim.pdf');
  formData.append('title', 'Tài liệu mật của Victim');

  const uploadDocRes = await fetch(`${BASE_URL}/api/v1/documents/upload`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${victimToken}` },
    body: formData
  });
  const docData = await uploadDocRes.json();
  const victimDocId = docData.id;
  console.log(`Victim Document: ${victimDocId}`);

  console.log('\n--- ATTACKER IDOR TESTS STARTING ---\n');

  // 2.1 IDOR on Course: Read
  const aGetCourse = await fetch(`${BASE_URL}/api/v1/courses/${victimCourseId}`, { headers: attackerHeaders });
  if (aGetCourse.status === 200) {
    record('IDOR-01', 'Read Private Course of Other User', 'VULNERABLE',
      `Attacker was able to read victim private course (HTTP 200).`);
  } else if (aGetCourse.status === 403 || aGetCourse.status === 404) {
    record('IDOR-01', 'Read Private Course of Other User', 'SECURE',
      `Blocked with HTTP ${aGetCourse.status}`);
  }

  // 2.2 IDOR on Course: Update / Tamper
  const aPutCourse = await fetch(`${BASE_URL}/api/v1/courses/${victimCourseId}`, {
    method: 'PUT',
    headers: attackerHeaders,
    body: JSON.stringify({ title: 'Hacked by Attacker!' })
  });
  if (aPutCourse.status === 200) {
    record('IDOR-02', 'Update Private Course of Other User', 'VULNERABLE',
      `Attacker modified victim course title (HTTP 200).`);
  } else {
    record('IDOR-02', 'Update Private Course of Other User', 'SECURE',
      `Blocked with HTTP ${aPutCourse.status}`);
  }

  // 2.3 IDOR on Course: Publish state toggle
  const aPublishCourse = await fetch(`${BASE_URL}/api/v1/courses/${victimCourseId}/publish`, {
    method: 'POST',
    headers: attackerHeaders,
    body: JSON.stringify({ visibility: 'public', isPublished: true })
  });
  if (aPublishCourse.status === 200) {
    record('IDOR-03', 'Publish Other User Private Course', 'VULNERABLE',
      `Attacker forcefully published victim private course!`);
  } else {
    record('IDOR-03', 'Publish Other User Private Course', 'SECURE',
      `Blocked with HTTP ${aPublishCourse.status}`);
  }

  // 2.4 IDOR on Course: Export ZIP
  const aExportCourse = await fetch(`${BASE_URL}/api/v1/courses/${victimCourseId}/export-zip`, { headers: attackerHeaders });
  if (aExportCourse.status === 200) {
    record('IDOR-04', 'Export ZIP of Other User Private Course', 'VULNERABLE',
      `Attacker downloaded full ZIP export of victim private course!`);
  } else {
    record('IDOR-04', 'Export ZIP of Other User Private Course', 'SECURE',
      `Blocked with HTTP ${aExportCourse.status}`);
  }

  // 2.5 IDOR on Chapter: Update / Delete
  if (victimChapterId) {
    const aPutChapter = await fetch(`${BASE_URL}/api/v1/chapters/${victimChapterId}`, {
      method: 'PUT',
      headers: attackerHeaders,
      body: JSON.stringify({ title: 'Hacked Chapter' })
    });
    if (aPutChapter.status === 200) {
      record('IDOR-05', 'Update Chapter in Other User Course', 'VULNERABLE',
        `Attacker modified chapter in victim course (HTTP 200).`);
    } else {
      record('IDOR-05', 'Update Chapter in Other User Course', 'SECURE',
        `Blocked with HTTP ${aPutChapter.status}`);
    }
  }

  // 2.6 IDOR on Lesson: Read
  if (victimLessonId) {
    const aGetLesson = await fetch(`${BASE_URL}/api/v1/lessons/${victimLessonId}`, { headers: attackerHeaders });
    if (aGetLesson.status === 200) {
      record('IDOR-06', 'Read Lesson of Other User Private Course', 'VULNERABLE',
        `Attacker read victim private lesson content (HTTP 200).`);
    } else {
      record('IDOR-06', 'Read Lesson of Other User Private Course', 'SECURE',
        `Blocked with HTTP ${aGetLesson.status}`);
    }

    // 2.7 IDOR on Lesson: Update
    const aPutLesson = await fetch(`${BASE_URL}/api/v1/lessons/${victimLessonId}`, {
      method: 'PUT',
      headers: attackerHeaders,
      body: JSON.stringify({ title: 'Defaced Lesson Title' })
    });
    if (aPutLesson.status === 200) {
      record('IDOR-07', 'Update Lesson of Other User Course', 'VULNERABLE',
        `Attacker modified victim lesson (HTTP 200).`);
    } else {
      record('IDOR-07', 'Update Lesson of Other User Course', 'SECURE',
        `Blocked with HTTP ${aPutLesson.status}`);
    }

    // 2.8 IDOR on Lesson: Complete state
    const aCompleteLesson = await fetch(`${BASE_URL}/api/v1/lessons/${victimLessonId}/complete`, {
      method: 'POST',
      headers: attackerHeaders
    });
    if (aCompleteLesson.status === 200) {
      record('IDOR-08', 'Mark Complete on Other User Private Lesson', 'VULNERABLE',
        `Attacker marked complete on victim private lesson (HTTP 200).`);
    } else {
      record('IDOR-08', 'Mark Complete on Other User Private Lesson', 'SECURE',
        `Blocked with HTTP ${aCompleteLesson.status}`);
    }
  }

  // 2.9 IDOR on Document: Read metadata
  if (victimDocId) {
    const aGetDoc = await fetch(`${BASE_URL}/api/v1/documents/${victimDocId}`, { headers: attackerHeaders });
    if (aGetDoc.status === 200) {
      record('IDOR-09', 'Read Document Metadata of Other User', 'VULNERABLE',
        `Attacker read victim document metadata (HTTP 200).`);
    } else {
      record('IDOR-09', 'Read Document Metadata of Other User', 'SECURE',
        `Blocked with HTTP ${aGetDoc.status}`);
    }

    // 2.10 IDOR on Document: Download raw file / stream
    const aGetDocFile = await fetch(`${BASE_URL}/api/v1/documents/${victimDocId}/file`, { headers: attackerHeaders });
    if (aGetDocFile.status === 200) {
      record('IDOR-10', 'Download Document File of Other User', 'VULNERABLE',
        `Attacker downloaded raw file of victim document!`);
    } else {
      record('IDOR-10', 'Download Document File of Other User', 'SECURE',
        `Blocked with HTTP ${aGetDocFile.status}`);
    }

    // 2.11 IDOR on Document: Read document pages
    const aGetDocPages = await fetch(`${BASE_URL}/api/v1/documents/${victimDocId}/pages`, { headers: attackerHeaders });
    if (aGetDocPages.status === 200) {
      record('IDOR-11', 'Read Document Pages of Other User', 'VULNERABLE',
        `Attacker read extracted pages of victim document!`);
    } else {
      record('IDOR-11', 'Read Document Pages of Other User', 'SECURE',
        `Blocked with HTTP ${aGetDocPages.status}`);
    }
  }

  // 2.12 IDOR on Notebook: Read
  if (victimNbId) {
    const aGetNb = await fetch(`${BASE_URL}/api/v1/notebooks/${victimNbId}`, { headers: attackerHeaders });
    if (aGetNb.status === 200) {
      record('IDOR-12', 'Read Notebook of Other User', 'VULNERABLE',
        `Attacker read victim private notebook content!`);
    } else {
      record('IDOR-12', 'Read Notebook of Other User', 'SECURE',
        `Blocked with HTTP ${aGetNb.status}`);
    }

    // 2.13 IDOR on Notebook: Tamper / Overwrite
    const aPutNb = await fetch(`${BASE_URL}/api/v1/notebooks/${victimNbId}`, {
      method: 'PUT',
      headers: attackerHeaders,
      body: JSON.stringify({
        baseVersion: 1,
        title: 'Tampered Notebook',
        contentAstJson: JSON.stringify({ type: 'doc', content: [] })
      })
    });
    if (aPutNb.status === 200) {
      record('IDOR-13', 'Overwrite Notebook of Other User', 'VULNERABLE',
        `Attacker modified victim notebook!`);
    } else {
      record('IDOR-13', 'Overwrite Notebook of Other User', 'SECURE',
        `Blocked with HTTP ${aPutNb.status}`);
    }

    // 2.14 IDOR on Notebook: Export Markdown
    const aExportNb = await fetch(`${BASE_URL}/api/v1/notebooks/${victimNbId}/export-markdown`, { headers: attackerHeaders });
    if (aExportNb.status === 200) {
      record('IDOR-14', 'Export Markdown of Other User Notebook', 'VULNERABLE',
        `Attacker downloaded markdown export of victim private notebook!`);
    } else {
      record('IDOR-14', 'Export Markdown of Other User Notebook', 'SECURE',
        `Blocked with HTTP ${aExportNb.status}`);
    }
  }

  // 2.15 IDOR on Flashcard: Delete
  if (victimFcId) {
    const aDelFc = await fetch(`${BASE_URL}/api/v1/flashcards/${victimFcId}`, {
      method: 'DELETE',
      headers: attackerHeaders
    });
    if (aDelFc.status === 200) {
      record('IDOR-15', 'Delete Flashcard of Other User', 'VULNERABLE',
        `Attacker deleted victim flashcard!`);
    } else {
      record('IDOR-15', 'Delete Flashcard of Other User', 'SECURE',
        `Blocked with HTTP ${aDelFc.status}`);
    }

    // 2.16 IDOR on Flashcard: Undo review
    const aUndoFc = await fetch(`${BASE_URL}/api/v1/flashcards/${victimFcId}/undo-last-review`, {
      method: 'POST',
      headers: attackerHeaders
    });
    if (aUndoFc.status === 200) {
      record('IDOR-16', 'Undo Flashcard Review of Other User', 'VULNERABLE',
        `Attacker triggered undo review on victim flashcard!`);
    } else {
      record('IDOR-16', 'Undo Flashcard Review of Other User', 'SECURE',
        `Blocked with HTTP ${aUndoFc.status}`);
    }
  }

  // 2.17 IDOR on Annotation: Reconcile / Delete
  if (victimAnnId) {
    const aRecAnn = await fetch(`${BASE_URL}/api/v1/annotations/${victimAnnId}/reconcile`, {
      method: 'POST',
      headers: attackerHeaders,
      body: JSON.stringify({ action: 'delete' })
    });
    if (aRecAnn.status === 200) {
      record('IDOR-17', 'Reconcile/Delete Annotation of Other User', 'VULNERABLE',
        `Attacker deleted victim annotation via reconcile endpoint!`);
    } else {
      record('IDOR-17', 'Reconcile/Delete Annotation of Other User', 'SECURE',
        `Blocked with HTTP ${aRecAnn.status}`);
    }

    const aDelAnn = await fetch(`${BASE_URL}/api/v1/annotations/${victimAnnId}`, {
      method: 'DELETE',
      headers: attackerHeaders
    });
    if (aDelAnn.status === 200) {
      record('IDOR-18', 'Delete Annotation of Other User', 'VULNERABLE',
        `Attacker deleted victim annotation!`);
    } else {
      record('IDOR-18', 'Delete Annotation of Other User', 'SECURE',
        `Blocked with HTTP ${aDelAnn.status}`);
    }
  }

  // Cleanup: Delete victim course
  if (victimCourseId) {
    await fetch(`${BASE_URL}/api/v1/courses/${victimCourseId}`, {
      method: 'DELETE',
      headers: victimHeaders
    });
  }

  console.log('\n=== TEST SUITE 2 SUMMARY ===');
  console.table(results);
}

run().catch(console.error);
