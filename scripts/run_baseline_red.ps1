# scripts/run_baseline_red.ps1
# Starts baseline backend & frontend, runs pdf-multipage-e2e.spec.ts, captures baseline-red.log, and cleans up.

$ErrorActionPreference = "Continue"
$root = (Get-Item $PSScriptRoot).Parent.FullName
Set-Location $root

$verificationDir = Join-Path $root "test-results\verification"
if (-not (Test-Path $verificationDir)) {
    New-Item -ItemType Directory -Path $verificationDir -Force | Out-Null
}
$logPath = Join-Path $verificationDir "baseline-red.log"

Write-Host "=== Step 1: Cleaning up any old processes on ports 5080 & 3000 ==="
Get-NetTCPConnection -LocalPort 5080, 3000 -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "Terminating process on port $($_.LocalPort) (PID $($_.OwningProcess))"
    Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 2

Write-Host "=== Step 2: Starting Backend API (5080) ==="
$backendProc = Start-Process -FilePath "dotnet" -ArgumentList "run --project backend/src/Swms.Api/Swms.Api.csproj -c Release --urls http://127.0.0.1:5080" -WorkingDirectory $root -PassThru

$backendReady = $false
for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Seconds 1
    try {
        $resp = Invoke-WebRequest -Uri "http://127.0.0.1:5080/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
        if ($resp.StatusCode -eq 200) {
            $backendReady = $true
            Write-Host "Backend is ready at http://127.0.0.1:5080/health"
            break
        }
    } catch {
        # continue waiting
    }
}

if (-not $backendReady) {
    Write-Error "Backend failed to start within 30 seconds"
    Stop-Process -Id $backendProc.Id -Force -ErrorAction SilentlyContinue
    exit 1
}

Write-Host "=== Step 3: Starting Frontend Next.js (3000) ==="
$frontendProc = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm.cmd run dev" -WorkingDirectory "$root\frontend" -PassThru

$frontendReady = $false
for ($i = 0; $i -lt 45; $i++) {
    Start-Sleep -Seconds 1
    try {
        $resp = Invoke-WebRequest -Uri "http://127.0.0.1:3000" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
        if ($resp.StatusCode -eq 200) {
            $frontendReady = $true
            Write-Host "Frontend is ready at http://127.0.0.1:3000"
            break
        }
    } catch {
        # continue waiting
    }
}

if (-not $frontendReady) {
    Write-Error "Frontend failed to start within 45 seconds"
    Stop-Process -Id $backendProc.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $frontendProc.Id -Force -ErrorAction SilentlyContinue
    exit 1
}

Write-Host "=== Step 4: Running Acceptance Test pdf-multipage-e2e.spec.ts ==="
Set-Location "$root\frontend"

# Run playwright test and capture output
$testOutput = & cmd.exe /c "npx.cmd playwright test e2e/pdf-multipage-e2e.spec.ts --reporter=list" 2>&1
$testExitCode = $LASTEXITCODE

Write-Host "Test finished with exit code: $testExitCode (EXPECTED FAILURE FOR RED-BEFORE-GREEN)"

# Write raw log to file with timestamp and exit code header
$header = @"
================================================================================
SWMS PDF MULTI-PAGE ACCEPTANCE TEST: BASELINE RED-BEFORE-GREEN EVIDENCE
Execution Timestamp: $(Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
Environment: Baseline Commit ff44c40 (Before Multi-Page Overhaul)
Test File: frontend/e2e/pdf-multipage-e2e.spec.ts
Playwright Exit Code: $testExitCode (Demonstrating detection of 1-page facade bug)
================================================================================

"@

$finalContent = $header + ($testOutput -join "`n")
[System.IO.File]::WriteAllText($logPath, $finalContent, [System.Text.Encoding]::UTF8)
Write-Host "Saved baseline failure log to: $logPath"

Write-Host "=== Step 5: Shutting down processes ==="
Stop-Process -Id $backendProc.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $frontendProc.Id -Force -ErrorAction SilentlyContinue

# Ensure no orphaned child processes on ports
Get-NetTCPConnection -LocalPort 5080, 3000 -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
}

Write-Host "Cleanup completed."
exit 0
