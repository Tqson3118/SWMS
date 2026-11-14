# scripts/verify-m3-challenger.ps1
<#
.SYNOPSIS
    Empirical Challenger Verification Script for Milestone 3 (Dual-Layer Reader & Split Workspace Ergonomics).
.DESCRIPTION
    1. Cleans up ports 5080 and 3000.
    2. Builds backend in Release mode.
    3. Starts backend on http://127.0.0.1:5080 and polls health.
    4. Starts frontend on http://127.0.0.1:3000 and polls readiness.
    5. Executes:
       - npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g "R3-T1"
       - npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g "R3-T2"
       - npx.cmd playwright test e2e/challenger-m3-stress.spec.ts
    6. Ensures clean teardown in finally block.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Continue"
$root = (Get-Item $PSScriptRoot).Parent.FullName
Set-Location $root

function Write-Step {
    param([string]$Msg, [string]$Color = "Cyan")
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $Msg" -ForegroundColor $Color
}

function Stop-PortProcesses {
    param([int[]]$Ports)
    foreach ($port in $Ports) {
        $pidsToKill = [System.Collections.Generic.HashSet[int]]::new()
        try {
            $conns = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
            if ($conns) {
                foreach ($conn in $conns) {
                    $procId = $conn.OwningProcess
                    if ($procId -and $procId -gt 0 -and $procId -ne $PID) {
                        [void]$pidsToKill.Add($procId)
                    }
                }
            }
        } catch {}
        try {
            $netstatLines = netstat -ano | Select-String ":$port\s"
            foreach ($item in $netstatLines) {
                $tokens = ($item.ToString().Trim() -split '\s+')
                $procPid = $tokens[-1]
                if ($procPid -match '^\d+$' -and [int]$procPid -ne $PID -and [int]$procPid -gt 0) {
                    [void]$pidsToKill.Add([int]$procPid)
                }
            }
        } catch {}

        foreach ($p in $pidsToKill) {
            & taskkill /PID $p /T /F 2>$null | Out-Null
            Stop-Process -Id $p -Force -ErrorAction SilentlyContinue
        }

        for ($wait = 0; $wait -lt 15; $wait++) {
            $listening = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
            if (-not $listening) { break }
            Start-Sleep -Milliseconds 500
        }
    }
}

$backendProc = $null
$frontendProc = $null
$overallPass = $true

$runDir = Join-Path $root "test-results\challenger-m3"
if (-not (Test-Path $runDir)) {
    New-Item -ItemType Directory -Path $runDir -Force | Out-Null
}
$backendServiceLog = Join-Path $runDir "backend-service.log"
$backendServiceErr = Join-Path $runDir "backend-service-err.log"
$frontendServiceLog = Join-Path $runDir "frontend-service.log"
$frontendServiceErr = Join-Path $runDir "frontend-service-err.log"

try {
    Write-Step "Step 1: Clean up ports 5080 & 3000" "Yellow"
    Stop-PortProcesses @(5080, 3000)
    Start-Sleep -Seconds 2

    Write-Step "Step 2: Build Backend" "Yellow"
    $bBuild = & dotnet build backend/src/Swms.Api/Swms.Api.csproj -c Release 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host ($bBuild -join "`n") -ForegroundColor Red
        throw "Backend build failed."
    }
    Write-Step "Backend build succeeded." "Green"

    Write-Step "Step 3: Launch Backend on port 5080" "Yellow"
    $backendArgs = "run --project backend/src/Swms.Api/Swms.Api.csproj -c Release --urls http://127.0.0.1:5080"
    $backendProc = Start-Process -FilePath "dotnet" -ArgumentList $backendArgs -WorkingDirectory $root `
        -RedirectStandardOutput $backendServiceLog -RedirectStandardError $backendServiceErr -PassThru
    Write-Step "Backend process PID: $($backendProc.Id)" "Gray"

    $bReady = $false
    for ($i = 1; $i -le 30; $i++) {
        Start-Sleep -Seconds 1
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:5080/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
            if ($resp.StatusCode -eq 200) {
                $bReady = $true
                Write-Step "Backend ready after ${i}s" "Green"
                break
            }
        } catch {}
    }
    if (-not $bReady) { throw "Backend timed out." }

    Write-Step "Step 4: Launch Frontend Next.js Dev Server on port 3000" "Yellow"
    $frontendProc = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm.cmd run dev" -WorkingDirectory "$root\frontend" `
        -RedirectStandardOutput $frontendServiceLog -RedirectStandardError $frontendServiceErr -PassThru
    Write-Step "Frontend process PID: $($frontendProc.Id)" "Gray"

    $fReady = $false
    for ($i = 1; $i -le 45; $i++) {
        Start-Sleep -Seconds 1
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:3000" -TimeoutSec 10 -UseBasicParsing -ErrorAction Stop
            if ($resp.StatusCode -eq 200) {
                $fReady = $true
                Write-Step "Frontend ready after ${i}s" "Green"
                break
            }
        } catch {
            Write-Step "Waiting for frontend... (${i}s): $($_.Exception.Message)" "Gray"
        }
    }
    if (-not $fReady) { throw "Frontend timed out after 45s." }

    # Warm up /workspace page to let Next.js compile on-demand
    Write-Step "Warming up /workspace page..." "Gray"
    try {
        $wResp = Invoke-WebRequest -Uri "http://127.0.0.1:3000/workspace" -TimeoutSec 15 -UseBasicParsing -ErrorAction SilentlyContinue
        Write-Step "Warm up status: $($wResp.StatusCode)" "Gray"
    } catch {}

    Set-Location "$root\frontend"

    # Command 1: R3-T1
    Write-Step "`n========================================================" "Cyan"
    Write-Step "Running: npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g `"R3-T1`"" "Cyan"
    Write-Step "========================================================" "Cyan"
    & cmd.exe /c "npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g `"R3-T1`" --reporter=list"
    if ($LASTEXITCODE -ne 0) {
        Write-Step "FAILED: R3-T1 tests failed!" "Red"
        $overallPass = $false
    } else {
        Write-Step "PASSED: R3-T1 tests passed cleanly!" "Green"
    }

    # Command 2: R3-T2
    Write-Step "`n========================================================" "Cyan"
    Write-Step "Running: npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g `"R3-T2`"" "Cyan"
    Write-Step "========================================================" "Cyan"
    & cmd.exe /c "npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g `"R3-T2`" --reporter=list"
    if ($LASTEXITCODE -ne 0) {
        Write-Step "FAILED: R3-T2 tests failed!" "Red"
        $overallPass = $false
    } else {
        Write-Step "PASSED: R3-T2 tests passed cleanly!" "Green"
    }

    # Command 3: Challenger Stress Suite
    Write-Step "`n========================================================" "Cyan"
    Write-Step "Running: npx.cmd playwright test e2e/challenger-m3-stress.spec.ts" "Cyan"
    Write-Step "========================================================" "Cyan"
    & cmd.exe /c "npx.cmd playwright test e2e/challenger-m3-stress.spec.ts --reporter=list"
    if ($LASTEXITCODE -ne 0) {
        Write-Step "FAILED: Challenger M3 Stress tests failed!" "Red"
        $overallPass = $false
    } else {
        Write-Step "PASSED: Challenger M3 Stress tests passed cleanly!" "Green"
    }

    # Command 4: Challenger Empirical Suite (Split Workspace & Jump to Source & Hitboxes)
    Write-Step "`n========================================================" "Cyan"
    Write-Step "Running: npx.cmd playwright test e2e/challenger-m3-empirical.spec.ts" "Cyan"
    Write-Step "========================================================" "Cyan"
    & cmd.exe /c "npx.cmd playwright test e2e/challenger-m3-empirical.spec.ts --reporter=list"
    if ($LASTEXITCODE -ne 0) {
        Write-Step "FAILED: Challenger M3 Empirical tests failed!" "Red"
        $overallPass = $false
    } else {
        Write-Step "PASSED: Challenger M3 Empirical tests passed cleanly!" "Green"
    }
} catch {
    Write-Step "EXCEPTION ENCOUNTERED: $_" "Red"
    $overallPass = $false
} finally {
    Set-Location $root
    Write-Step "`n--- Cleaning up backend & frontend processes ---" "Yellow"
    if ($frontendProc) { Stop-Process -Id $frontendProc.Id -Force -ErrorAction SilentlyContinue }
    if ($backendProc) { Stop-Process -Id $backendProc.Id -Force -ErrorAction SilentlyContinue }
    Stop-PortProcesses @(5080, 3000)
    Write-Step "Cleanup complete." "Gray"
}

if ($overallPass) {
    Write-Step "`n>>> ALL MILESTONE 3 TESTS & ADVERSARIAL STRESS SUITES PASSED (EXIT CODE 0) <<<" "Green"
    exit 0
} else {
    Write-Step "`n>>> ONE OR MORE TESTS FAILED (EXIT CODE 1) <<<" "Red"
    exit 1
}
