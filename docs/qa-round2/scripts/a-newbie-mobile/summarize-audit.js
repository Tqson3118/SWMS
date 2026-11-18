const fs = require('fs');
const path = require('path');

const data = JSON.parse(
  fs.readFileSync(
    path.join(__dirname, 'tap-target-audit-results.json'),
    'utf8'
  )
);

console.log('| Viewport | Tuyến đường (Route) | Tổng tương tác | < 44×44px | < 36×36px (Critical) | Tràn ngang? | Bị MobileNav che? |');
console.log('|---|---|---|---|---|---|---|');
for (const item of data) {
  const isOverflow = item.overflowCheck.isOverflowing
    ? `CÓ (${item.overflowCheck.scrollWidth}px)`
    : 'Không';
  const obscuredCount = item.mobileNavObscurity?.obscuredElements?.length || 0;
  const isObscured = obscuredCount > 0 ? `CÓ (${obscuredCount} phần tử)` : 'Không';
  console.log(
    `| ${item.vpName} | \`${item.route}\` | ${item.tapTargetCheck.totalInteractive} | ${item.tapTargetCheck.sub44Count} | ${item.tapTargetCheck.sub36Count} | ${isOverflow} | ${isObscured} |`
  );
}

console.log('\n--- CÁC PHẦN TỬ TAP TARGET < 36px ĐIỂN HÌNH ---');
for (const item of data) {
  const sub36 = item.tapTargetCheck.issues.filter(i => i.severity === 'Critical_Sub36');
  if (sub36.length > 0) {
    console.log(`\n[${item.vpName.toUpperCase()} - ${item.route}]`);
    for (const issue of sub36) {
      console.log(` - <${issue.tagName}> [${issue.testId || issue.text}] kích thước: ${issue.width}x${issue.height}px (tại x:${issue.left}, y:${issue.top})`);
    }
  }
}

console.log('\n--- CÁC VẤN ĐỀ VỀ KHOẢNG CÁCH (SPACING < 6px) ---');
for (const item of data) {
  if (item.tapTargetCheck.spacingIssues.length > 0) {
    console.log(`\n[${item.vpName.toUpperCase()} - ${item.route}]`);
    for (const sp of item.tapTargetCheck.spacingIssues) {
      console.log(` - Giữa [${sp.elemA.tag}: "${sp.elemA.text || sp.elemA.testId}"] và [${sp.elemB.tag}: "${sp.elemB.text || sp.elemB.testId}"] cách ${sp.edgeDistX}px ngang, ${sp.edgeDistY}px dọc`);
    }
  }
}

console.log('\n--- CÁC PHẦN TỬ BỊ MOBILENAV CHE KHUẤT Ở ĐÁY MÀN HÌNH ---');
for (const item of data) {
  if (item.mobileNavObscurity?.obscuredElements?.length > 0) {
    console.log(`\n[${item.vpName.toUpperCase()} - ${item.route}]`);
    for (const obs of item.mobileNavObscurity.obscuredElements) {
      console.log(` - <${obs.tagName}> [${obs.testId || obs.text}] đè lấn: ${obs.overlapPx}px`);
    }
  }
}
