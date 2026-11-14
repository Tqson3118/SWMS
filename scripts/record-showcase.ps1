$root = "D:\03_Study\Studxy"

Write-Host "--- 1. Cleaning up ports 5080 and 3000 ---"
foreach ($port in @(5080, 3000)) {
    $conns = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if ($conns) {
        foreach ($c in $conns) {
            Stop-Process -Id $c.OwningProcess -Force -ErrorAction SilentlyContinue
        }
    }
}
Start-Sleep -Seconds 1

Write-Host "--- 2. Starting Backend Server ---"
$backendDll = "$root\backend\src\Swms.Api\bin\Release\net10.0\Swms.Api.dll"
$backendProc = Start-Process -FilePath "dotnet" -ArgumentList "`"$backendDll`" --urls http://127.0.0.1:5080" -WorkingDirectory "$root\backend\src\Swms.Api" -PassThru

$backendReady = $false
for ($i = 0; $i -lt 30; $i++) {
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

Write-Host "--- 3. Starting Frontend Next.js Server ---"
$env:NODE_OPTIONS = "--max-old-space-size=4096"
$frontendProc = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npx.cmd next dev --port 3000" -WorkingDirectory "$root\frontend" -PassThru

$frontendReady = $false
for ($i = 0; $i -lt 40; $i++) {
    Start-Sleep -Seconds 1
    try {
        $res = Invoke-WebRequest -Uri "http://127.0.0.1:3000" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($res.StatusCode -eq 200) { 
            $frontendReady = $true
            Write-Host "Frontend is READY on 3000"
            break 
        }
    } catch {}
}
if (-not $frontendReady) { throw "Frontend failed to start!" }

Write-Host "--- 4. Running Record & Showcase Playwright Spec ---"
Set-Location "$root\frontend"
$pwOutput = & cmd.exe /c "npx.cmd playwright test e2e/record-video-showcase.spec.ts --reporter=list" 2>&1
Write-Host ($pwOutput -join "`n")

Write-Host "--- 5. Cleanup ---"
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
Write-Host "Showcase recording completed!"
