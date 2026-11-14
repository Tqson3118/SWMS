$root = "D:\03_Study\Studxy"

Write-Host "--- 1. Cleaning ports ---"
foreach ($port in @(5080, 3000)) {
    $conns = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if ($conns) {
        foreach ($c in $conns) {
            Stop-Process -Id $c.OwningProcess -Force -ErrorAction SilentlyContinue
        }
    }
}
Start-Sleep -Seconds 1

Write-Host "--- 2. Starting Backend API ---"
$backendExe = "$root\backend\src\Swms.Api\bin\Release\net10.0\Swms.Api.exe"
$backendProc = Start-Process -FilePath $backendExe -ArgumentList "--urls http://127.0.0.1:5080" -WorkingDirectory "$root\backend\src\Swms.Api" -PassThru

$backendReady = $false
for ($i = 0; $i -lt 25; $i++) {
    Start-Sleep -Milliseconds 500
    try {
        $res = Invoke-WebRequest -Uri "http://127.0.0.1:5080/health" -UseBasicParsing -TimeoutSec 1 -ErrorAction Stop
        if ($res.StatusCode -eq 200) { 
            $backendReady = $true
            Write-Host "Backend is READY on 5080"
            break 
        }
    } catch {}
}
if (-not $backendReady) { throw "Backend failed to start!" }

Write-Host "--- 3. Starting Frontend Dev Server ---"
$frontendProc = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npx.cmd next dev --port 3000" -WorkingDirectory "$root\frontend" -PassThru

$frontendReady = $false
for ($i = 0; $i -lt 45; $i++) {
    Start-Sleep -Seconds 1
    try {
        $res = Invoke-WebRequest -Uri "http://127.0.0.1:3000/workspace" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($res.StatusCode -eq 200) { 
            $frontendReady = $true
            Write-Host "Frontend is READY on 3000"
            break 
        }
    } catch {}
}
if (-not $frontendReady) { throw "Frontend failed to start!" }

Write-Host "--- 4. Running Playwright Comprehensive Tour ---"
Set-Location "$root\frontend"
$pwOutput = & cmd.exe /c "npx.cmd playwright test e2e/comprehensive-learning-tour.spec.ts --reporter=list" 2>&1
Write-Host ($pwOutput -join "`n")

Write-Host "--- 5. Copying screenshots to Brain artifact directory ---"
$dest = "C:\Users\Administrator\.gemini\antigravity\brain\04ee9dd0-2d59-4f51-ba9d-670aa8a2f735\showcase"
Copy-Item "$root\test-results\deep-tour-screenshots\*.png" -Destination $dest -Force -ErrorAction SilentlyContinue

# Find recorded video
$videoFile = Get-ChildItem -Recurse -Filter "video.webm" "$root\frontend\test-results\comprehensive-learning-tour-*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($videoFile) {
    Copy-Item $videoFile.FullName -Destination "$dest\deep_dive_interactive_learning_tour.webm" -Force
    Write-Host "Copied video to $dest\deep_dive_interactive_learning_tour.webm"
}

Write-Host "--- 6. Cleanup ---"
Stop-Process -Id $backendProc.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $frontendProc.Id -Force -ErrorAction SilentlyContinue
foreach ($port in @(5080, 3000)) {
    $conns = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if ($conns) {
        foreach ($c in $conns) {
            Stop-Process -Id $c.OwningProcess -Force -ErrorAction SilentlyContinue
        }
    }
}
Write-Host "Finished deep tour recording successfully!"
