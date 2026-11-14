# scripts/verify-m3-challenger-it2.ps1
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

$runDir = Join-Path $root "test-results\challenger-m3-it2"
if (-not (Test-Path $runDir)) {
    New-Item -ItemType Directory -Path $runDir -Force | Out-Null
}
$backendLog = Join-Path $runDir "backend.log"
$backendErr = Join-Path $runDir "backend-err.log"
$frontendLog = Join-Path $runDir "frontend.log"
$frontendErr = Join-Path $runDir "frontend-err.log"

try {
    Write-Step "Step 1: Clean up ports 5080 & 3000" "Yellow"
    Stop-PortProcesses @(5080, 3000)
    Start-Sleep -Seconds 2

    Write-Step "Step 2: Launch Backend on port 5080" "Yellow"
    $backendArgs = "run --project backend/src/Swms.Api/Swms.Api.csproj -c Release --urls http://127.0.0.1:5080"
    $backendProc = Start-Process -FilePath "dotnet" -ArgumentList $backendArgs -WorkingDirectory $root `
        -RedirectStandardOutput $backendLog -RedirectStandardError $backendErr -PassThru
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

    Write-Step "Step 3: Launch Frontend Next.js Dev Server on port 3000" "Yellow"
    $frontendProc = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm.cmd run dev" -WorkingDirectory "$root\frontend" `
        -RedirectStandardOutput $frontendLog -RedirectStandardError $frontendErr -PassThru
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
        } catch {}
    }
    if (-not $fReady) { throw "Frontend timed out after 45s." }

    # Warm up /workspace page
    Write-Step "Warming up /workspace page..." "Gray"
    try {
        $wResp = Invoke-WebRequest -Uri "http://127.0.0.1:3000/workspace" -TimeoutSec 15 -UseBasicParsing -ErrorAction SilentlyContinue
        Write-Step "Warm up status: $($wResp.StatusCode)" "Gray"
    } catch {}

    Set-Location "$root\frontend"

    # Command 1: Challenger Adversarial Suite (Clamping, Double-Click Reset, Viewport Overflow, Module Resolution)
    Write-Step "`n========================================================" "Cyan"
    Write-Step "Running: npx.cmd playwright test e2e/challenger-m3-it2-empirical.spec.ts" "Cyan"
    Write-Step "========================================================" "Cyan"
    & cmd.exe /c "npx.cmd playwright test e2e/challenger-m3-it2-empirical.spec.ts --reporter=list"
    if ($LASTEXITCODE -ne 0) {
        Write-Step "FAILED: Challenger M3 it2 empirical tests failed!" "Red"
        $overallPass = $false
    } else {
        Write-Step "PASSED: Challenger M3 it2 empirical tests passed cleanly!" "Green"
    }

    # Command 2: Project R3 Suite (10 tests)
    Write-Step "`n========================================================" "Cyan"
    Write-Step "Running: npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g `"R3-T`"" "Cyan"
    Write-Step "========================================================" "Cyan"
    & cmd.exe /c "npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g `"R3-T`" --reporter=list"
    if ($LASTEXITCODE -ne 0) {
        Write-Step "FAILED: Project R3 tests failed!" "Red"
        $overallPass = $false
    } else {
        Write-Step "PASSED: Project R3 tests passed cleanly!" "Green"
    }
} catch {
    Write-Step "EXCEPTION: $_" "Red"
    $overallPass = $false
} finally {
    Set-Location $root
    Write-Step "`n--- Tearing down backend & frontend processes ---" "Yellow"
    if ($frontendProc) { Stop-Process -Id $frontendProc.Id -Force -ErrorAction SilentlyContinue }
    if ($backendProc) { Stop-Process -Id $backendProc.Id -Force -ErrorAction SilentlyContinue }
    Stop-PortProcesses @(5080, 3000)
    Write-Step "Cleanup complete." "Gray"
}

if ($overallPass) {
    Write-Step "`n>>> ALL EMPIRICAL CHALLENGES PASSED (EXIT CODE 0) <<<" "Green"
    exit 0
} else {
    Write-Step "`n>>> VERIFICATION FAILED (EXIT CODE 1) <<<" "Red"
    exit 1
}
