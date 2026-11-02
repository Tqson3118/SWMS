# scripts/verify.ps1
<#
.SYNOPSIS
    One-Command Automated Verification Runner for SWMS Multi-Page PDF & Study Tools.
.DESCRIPTION
    Automated end-to-end verification runner that:
    1. Cleans up any leftover processes on ports 5080 and 3000.
    2. Builds the backend in Release configuration.
    3. Starts the backend API in the background on http://127.0.0.1:5080.
    4. Polls http://127.0.0.1:5080/health until ready (up to 30 seconds).
    5. Starts the frontend Next.js server in the background on http://127.0.0.1:3000.
    6. Polls http://127.0.0.1:3000 until ready (up to 45 seconds).
    7. Runs backend unit and integration tests (verifies all 37 pass).
    8. Runs all positive Playwright E2E suites:
       - e2e/pdf-multipage-e2e.spec.ts (Core multi-page acceptance spec)
       - e2e/qa-e2e-suite.spec.ts (Full QA regression suite)
       - e2e/swms-landing.spec.ts (Public landing & auxiliary views)
       - e2e/swms-workspace.spec.ts (Core study loop & multi-viewport)
       - e2e/adversarial-viewport-stress.spec.ts (Viewport stress & zero-overflow)
       - e2e/challenger-m1-stress.spec.ts (M1 adversarial & chaos stress)
       - e2e/challenger-m2-stress.spec.ts (M2 adversarial & interaction stress)
       - e2e/swms-challenger-m1-2.spec.ts (M1 multi-viewport adversarial suite)
       (Excludes historical pre-fix bug reproduction specs).
    9. Collects detailed logs and summaries into test-results/verification/<timestamp>/.
    10. In a finally block, cleanly tears down all spawned backend and frontend processes,
        guaranteeing no orphaned processes remain on ports 5080 or 3000.
    11. Exits with code 0 on full success, or non-zero on failure.
#>

[CmdletBinding()]
param(
    [string]$OutputDir = "",
    [string]$LogFile = "",
    [string[]]$Suites = @()
)

$ErrorActionPreference = "Continue"
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

$root = (Get-Item $PSScriptRoot).Parent.FullName
Set-Location $root

# If OutputDir was passed as a spec file by mistake, move it to Suites
if ($OutputDir -and ($OutputDir -match '\.spec\.ts$' -or $OutputDir -match '^e2e[/\\]')) {
    $Suites += $OutputDir
    $OutputDir = ""
}

# Initialize logging directory
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$timestampIso = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $runDir = Join-Path $root "test-results\verification\$timestamp"
} else {
    if ([System.IO.Path]::IsPathRooted($OutputDir)) {
        $runDir = $OutputDir
    } else {
        $runDir = Join-Path $root $OutputDir
    }
}

if (-not (Test-Path $runDir)) {
    New-Item -ItemType Directory -Path $runDir -Force | Out-Null
}

$summaryLog = Join-Path $runDir "verification-summary.log"
$backendBuildLog = Join-Path $runDir "backend-build.log"
$backendServiceLog = Join-Path $runDir "backend-service.log"
$backendServiceErr = Join-Path $runDir "backend-service-err.log"
$frontendServiceLog = Join-Path $runDir "frontend-service.log"
$frontendServiceErr = Join-Path $runDir "frontend-service-err.log"
$backendTestsLog = Join-Path $runDir "backend-tests.log"
$playwrightLog = Join-Path $runDir "playwright-e2e.log"

function Write-Log {
    param([string]$Message, [string]$Color = "White")
    $line = "[$(Get-Date -Format 'HH:mm:ss')] $Message"
    Write-Host $line -ForegroundColor $Color
    Add-Content -Path $summaryLog -Value $line -Encoding utf8
}

function Stop-PortProcesses {
    param([int[]]$Ports)
    foreach ($port in $Ports) {
        # Method 1: Get-NetTCPConnection
        try {
            $conns = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
            if ($conns) {
                foreach ($conn in $conns) {
                    $procId = $conn.OwningProcess
                    if ($procId -and $procId -gt 0 -and $procId -ne $PID) {
                        Write-Log "Cleaning up process on port $port (PID: $procId)" "Yellow"
                        Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
                    }
                }
            }
        } catch {}

        # Method 2: netstat check fallback
        try {
            $netstatLines = netstat -ano | Select-String ":$port\s"
            foreach ($item in $netstatLines) {
                $tokens = ($item.ToString().Trim() -split '\s+')
                $procPid = $tokens[-1]
                if ($procPid -match '^\d+$' -and [int]$procPid -ne $PID -and [int]$procPid -gt 0) {
                    Write-Log "Cleaning up process on port $port from netstat (PID: $procPid)" "Yellow"
                    Stop-Process -Id ([int]$procPid) -Force -ErrorAction SilentlyContinue
                }
            }
        } catch {}
    }
}

Write-Log "================================================================================" "Cyan"
Write-Log "SWMS AUTOMATED VERIFICATION RUNNER (Milestone M4)" "Cyan"
Write-Log "Timestamp: $timestampIso" "Cyan"
Write-Log "Working Root: $root" "Cyan"
Write-Log "Log Directory: $runDir" "Cyan"
Write-Log "================================================================================" "Cyan"

$backendProc = $null
$frontendProc = $null
$overallSuccess = $true
$failureReasons = [System.Collections.Generic.List[string]]::new()

try {
    # -------------------------------------------------------------------------
    # Step A: Clean up any leftover processes on ports 5080 and 3000
    # -------------------------------------------------------------------------
    Write-Log "`n--- Step A: Pre-Flight Cleanup on Ports 5080 & 3000 ---" "Yellow"
    Stop-PortProcesses @(5080, 3000)
    Start-Sleep -Seconds 2

    # -------------------------------------------------------------------------
    # Step B: Build Backend (Release)
    # -------------------------------------------------------------------------
    Write-Log "`n--- Step B: Building Backend (Release) ---" "Yellow"
    $buildOutput = & dotnet build backend/src/Swms.Api/Swms.Api.csproj -c Release 2>&1
    $buildExitCode = $LASTEXITCODE
    $buildOutput | Out-File -FilePath $backendBuildLog -Encoding utf8
    if ($buildExitCode -ne 0) {
        Write-Log "Backend build FAILED with exit code $buildExitCode" "Red"
        $failureReasons.Add("Backend build failed (exit code $buildExitCode)")
        $overallSuccess = $false
        throw "Backend build failed."
    }
    Write-Log "Backend build succeeded." "Green"

    # -------------------------------------------------------------------------
    # Step C: Start Backend in Background (5080)
    # -------------------------------------------------------------------------
    Write-Log "`n--- Step C: Starting Backend API on http://127.0.0.1:5080 ---" "Yellow"
    $backendArgs = "run --project backend/src/Swms.Api/Swms.Api.csproj -c Release --urls http://127.0.0.1:5080"
    $backendProc = Start-Process -FilePath "dotnet" -ArgumentList $backendArgs -WorkingDirectory $root `
        -RedirectStandardOutput $backendServiceLog -RedirectStandardError $backendServiceErr -PassThru
    Write-Log "Backend process launched (PID: $($backendProc.Id))" "Gray"

    # -------------------------------------------------------------------------
    # Step D: Poll Backend Health Check (up to 30s)
    # -------------------------------------------------------------------------
    Write-Log "`n--- Step D: Polling Backend Health (http://127.0.0.1:5080/health) ---" "Yellow"
    $backendReady = $false
    for ($i = 1; $i -le 30; $i++) {
        Start-Sleep -Seconds 1
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:5080/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
            if ($resp.StatusCode -eq 200) {
                $backendReady = $true
                Write-Log "Backend is READY (HTTP 200 after ${i}s)" "Green"
                break
            }
        } catch {}
    }
    if (-not $backendReady) {
        Write-Log "Backend failed to become ready within 30 seconds" "Red"
        $failureReasons.Add("Backend health check timed out after 30 seconds")
        $overallSuccess = $false
        throw "Backend health check failed."
    }

    # -------------------------------------------------------------------------
    # Step E: Start Frontend in Background (3000)
    # -------------------------------------------------------------------------
    Write-Log "`n--- Step E: Starting Frontend Next.js Dev Server on http://127.0.0.1:3000 ---" "Yellow"
    $frontendProc = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm.cmd run dev" -WorkingDirectory "$root\frontend" `
        -RedirectStandardOutput $frontendServiceLog -RedirectStandardError $frontendServiceErr -PassThru
    Write-Log "Frontend process launched (PID: $($frontendProc.Id))" "Gray"

    # -------------------------------------------------------------------------
    # Step F: Poll Frontend Readiness (up to 45s)
    # -------------------------------------------------------------------------
    Write-Log "`n--- Step F: Polling Frontend Readiness (http://127.0.0.1:3000) ---" "Yellow"
    $frontendReady = $false
    for ($i = 1; $i -le 45; $i++) {
        Start-Sleep -Seconds 1
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:3000" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
            if ($resp.StatusCode -eq 200) {
                $frontendReady = $true
                Write-Log "Frontend is READY (HTTP 200 after ${i}s)" "Green"
                break
            }
        } catch {}
    }
    if (-not $frontendReady) {
        Write-Log "Frontend failed to become ready within 45 seconds" "Red"
        $failureReasons.Add("Frontend readiness check timed out after 45 seconds")
        $overallSuccess = $false
        throw "Frontend readiness check failed."
    }

    # -------------------------------------------------------------------------
    # Step G: Run Backend Unit & Integration Tests (dotnet test)
    # -------------------------------------------------------------------------
    Write-Log "`n--- Step G: Running Backend Tests (Swms.Tests) ---" "Yellow"
    $testOutput = & dotnet test backend/tests/Swms.Tests/Swms.Tests.csproj -c Release 2>&1
    $testExitCode = $LASTEXITCODE
    $testOutput | Out-File -FilePath $backendTestsLog -Encoding utf8

    if ($testExitCode -ne 0) {
        Write-Log "Backend tests FAILED with exit code $testExitCode" "Red"
        $failureReasons.Add("Backend tests failed (exit code $testExitCode)")
        $overallSuccess = $false
    } else {
        $joined = $testOutput -join "`n"
        if ($joined -match 'Passed!\s+-\s+Failed:\s+0,\s+Passed:\s+(\d+),\s+Skipped:\s+0,\s+Total:\s+(\d+)') {
            $passedCount = [int]$Matches[1]
            $totalCount = [int]$Matches[2]
            Write-Log "Backend tests PASSED: $passedCount / $totalCount tests passed." "Green"
            if ($passedCount -lt 37) {
                Write-Log "WARNING: Expected at least 37 passed tests, got $passedCount" "Yellow"
                $failureReasons.Add("Backend tests passed count ($passedCount) is less than expected 37")
                $overallSuccess = $false
            }
        } else {
            Write-Log "Backend tests PASSED with exit code 0." "Green"
        }
    }

    # -------------------------------------------------------------------------
    # Step H: Run Playwright E2E Test Suites
    # -------------------------------------------------------------------------
    Write-Log "`n--- Step H: Running Playwright E2E Suites ---" "Yellow"
    Set-Location "$root\frontend"

    if ($Suites.Count -eq 0) {
        $targetSuites = @(
            "e2e/pdf-multipage-e2e.spec.ts",
            "e2e/qa-e2e-suite.spec.ts",
            "e2e/swms-landing.spec.ts",
            "e2e/swms-workspace.spec.ts",
            "e2e/adversarial-viewport-stress.spec.ts",
            "e2e/challenger-m1-stress.spec.ts",
            "e2e/challenger-m2-stress.spec.ts",
            "e2e/swms-challenger-m1-2.spec.ts"
        )
    } else {
        $targetSuites = $Suites
    }

    Write-Log "Suites to execute ($($targetSuites.Count)):" "Cyan"
    foreach ($s in $targetSuites) {
        Write-Log "  - $s" "Gray"
    }

    $suitesJoined = $targetSuites -join " "
    $pwCmd = "npx.cmd playwright test $suitesJoined --reporter=list"
    Write-Log "Executing: cmd.exe /c `"$pwCmd`"" "Cyan"

    $pwOutput = & cmd.exe /c $pwCmd 2>&1
    $pwExitCode = $LASTEXITCODE
    $pwOutput | Out-File -FilePath $playwrightLog -Encoding utf8

    # Echo playwright output to console and summary
    foreach ($line in $pwOutput) {
        if ($line -match '^\s*ok\s+') {
            Write-Host $line -ForegroundColor Green
        } elseif ($line -match '^\s*\d+\s+passed') {
            Write-Host $line -ForegroundColor Green
            Add-Content -Path $summaryLog -Value $line -Encoding utf8
        } elseif ($line -match '^\s*\d+\s+failed') {
            Write-Host $line -ForegroundColor Red
            Add-Content -Path $summaryLog -Value $line -Encoding utf8
        } else {
            Write-Host $line
        }
    }

    if ($pwExitCode -ne 0) {
        Write-Log "Playwright E2E suites FAILED with exit code $pwExitCode" "Red"
        $failureReasons.Add("Playwright E2E suites failed (exit code $pwExitCode)")
        $overallSuccess = $false
    } else {
        Write-Log "All Playwright E2E suites PASSED (exit code 0)." "Green"
    }

} catch {
    Write-Log "EXCEPTION ENCOUNTERED: $_" "Red"
    $failureReasons.Add("Exception: $_")
    $overallSuccess = $false
} finally {
    # -------------------------------------------------------------------------
    # Step J: Teardown & Process Cleanup Invariant
    # -------------------------------------------------------------------------
    Set-Location $root
    Write-Log "`n--- Step J: Process Cleanup & Teardown ---" "Yellow"

    if ($backendProc -and -not $backendProc.HasExited) {
        Write-Log "Stopping backend process (PID: $($backendProc.Id))" "Gray"
        Stop-Process -Id $backendProc.Id -Force -ErrorAction SilentlyContinue
        & taskkill /PID $backendProc.Id /T /F 2>$null | Out-Null
    }

    if ($frontendProc -and -not $frontendProc.HasExited) {
        Write-Log "Stopping frontend process (PID: $($frontendProc.Id))" "Gray"
        Stop-Process -Id $frontendProc.Id -Force -ErrorAction SilentlyContinue
        & taskkill /PID $frontendProc.Id /T /F 2>$null | Out-Null
    }

    Start-Sleep -Seconds 1
    Stop-PortProcesses @(5080, 3000)

    $stopwatch.Stop()
    $elapsedSec = [math]::Round($stopwatch.Elapsed.TotalSeconds, 2)

    Write-Log "`n================================================================================" "Cyan"
    Write-Log "VERIFICATION RUN COMPLETE" "Cyan"
    Write-Log "Duration: ${elapsedSec}s" "Cyan"
    Write-Log "Logs directory: $runDir" "Cyan"

    if (-not [string]::IsNullOrWhiteSpace($LogFile)) {
        $destPath = if ([System.IO.Path]::IsPathRooted($LogFile)) { $LogFile } else { Join-Path $root $LogFile }
        $destDir = [System.IO.Path]::GetDirectoryName($destPath)
        if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }
        Copy-Item -Path $summaryLog -Destination $destPath -Force
        Write-Log "Summary log mirrored to: $destPath" "Cyan"
    }

    if ($overallSuccess) {
        Write-Log "STATUS: ALL CHECKS PASSED (EXIT CODE 0)" "Green"
        Write-Log "================================================================================" "Cyan"
        exit 0
    } else {
        Write-Log "STATUS: VERIFICATION FAILED (EXIT CODE 1)" "Red"
        Write-Log "Failure reasons:" "Red"
        foreach ($r in $failureReasons) {
            Write-Log "  - $r" "Red"
        }
        Write-Log "================================================================================" "Cyan"
        exit 1
    }
}
