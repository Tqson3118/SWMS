$destDir = "C:\Users\Administrator\.gemini\antigravity\brain\04ee9dd0-2d59-4f51-ba9d-670aa8a2f735\showcase"
if (-not (Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
}

Copy-Item "test-results\pf-tour-screenshots\*.png" -Destination $destDir -Force
Copy-Item "D:\03_Study\Studxy\frontend\test-results\personal-finance-upload-to-7dc6f--to-End-Learning-Experience\video.webm" -Destination (Join-Path $destDir "personal_finance_real_course_upload_walkthrough.webm") -Force

Write-Host "Showcase files copied successfully:"
Get-Item "$destDir\personal_finance_real_course_upload_walkthrough.webm" | Select-Object Name, Length
Get-ChildItem -Path $destDir -Filter "*pf*" | Select-Object Name, Length
