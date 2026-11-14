# scripts/verify-m3-it2-challenger.ps1
[CmdletBinding()]
param()

$ErrorActionPreference = "Continue"
$root = (Get-Item $PSScriptRoot).Parent.FullName
Set-Location $root

function Write-Step {
    param([string]$Msg)
    Write-Output "[$(Get-Date -Format 'HH:mm:ss')] $Msg"
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
$backendServiceLog = Join-Path $runDir "backend-service.log"
$backendServiceErr = Join-Path $runDir "backend-service-err.log"
$frontendServiceLog = Join-Path $runDir "frontend-service.log"
$frontendServiceErr = Join-Path $runDir "frontend-service-err.log"

try {
    Write-Step "Step 1: Clean up ports 5080 & 3000"
    Stop-PortProcesses @(5080, 3000)
    Start-Sleep -Seconds 2

    Write-Step "Step 2: Build Backend"
    $bBuild = & dotnet build backend/src/Swms.Api/Swms.Api.csproj -c Release 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Output ($bBuild -join "`n")
        throw "Backend build failed."
    }
    Write-Step "Backend build succeeded."

    Write-Step "Step 3: Launch Backend on port 5080"
    $backendArgs = "run --project backend/src/Swms.Api/Swms.Api.csproj -c Release --urls http://127.0.0.1:5080"
    $backendProc = Start-Process -FilePath "dotnet" -ArgumentList $backendArgs -WorkingDirectory $root `
        -RedirectStandardOutput $backendServiceLog -RedirectStandardError $backendServiceErr -PassThru
    Write-Step "Backend process PID: $($backendProc.Id)"

    $bReady = $false
    for ($i = 1; $i -le 30; $i++) {
        Start-Sleep -Seconds 1
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:5080/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
            if ($resp.StatusCode -eq 200) {
                $bReady = $true
                Write-Step "Backend ready after ${i}s"
                break
            }
        } catch {}
    }
    if (-not $bReady) { throw "Backend timed out." }

    Write-Step "Step 4: Launch Frontend Next.js Dev Server on port 3000"
    $frontendProc = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm.cmd run dev" -WorkingDirectory "$root\frontend" `
        -RedirectStandardOutput $frontendServiceLog -RedirectStandardError $frontendServiceErr -PassThru
    Write-Step "Frontend process PID: $($frontendProc.Id)"

    $fReady = $false
    for ($i = 1; $i -le 45; $i++) {
        Start-Sleep -Seconds 1
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:3000" -TimeoutSec 10 -UseBasicParsing -ErrorAction Stop
            if ($resp.StatusCode -eq 200) {
                $fReady = $true
                Write-Step "Frontend ready after ${i}s"
                break
            }
        } catch {}
    }
    if (-not $fReady) { throw "Frontend timed out after 45s." }

    Write-Step "Warming up /workspace page..."
    for ($w = 1; $w -le 15; $w++) {
        try {
            $wResp = Invoke-WebRequest -Uri "http://127.0.0.1:3000/workspace" -TimeoutSec 15 -UseBasicParsing -ErrorAction Stop
            if ($wResp.StatusCode -eq 200) {
                Write-Step "Workspace page ready (HTTP 200) on attempt $w"
                break
            }
        } catch {
            Start-Sleep -Seconds 1
        }
    }

    Set-Location "$root\frontend"

    # Command 1: Official R3-T suite
    Write-Step "`n========================================================"
    Write-Step "Running: npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g `"R3-T`""
    Write-Step "========================================================"
    $r3Cmd = "npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g `"R3-T`" --reporter=list"
    $r3Out = & cmd.exe /c $r3Cmd 2>&1
    $r3Code = $LASTEXITCODE
    $r3Out | ForEach-Object { Write-Output $_ }
    if ($r3Code -ne 0) {
        Write-Step "FAILED: R3-T tests failed with exit code $r3Code!"
        $overallPass = $false
    } else {
        Write-Step "PASSED: R3-T tests passed cleanly!"
    }

    # Command 2: Challenger Adversarial Iteration 2 Suite (Dedicated Stress & Missing element handling)
    Write-Step "`n========================================================"
    Write-Step "Running: npx.cmd playwright test e2e/challenger-m3-adversarial-it2.spec.ts"
    Write-Step "========================================================"
    $it2Cmd = "npx.cmd playwright test e2e/challenger-m3-adversarial-it2.spec.ts --reporter=list"
    $it2Out = & cmd.exe /c $it2Cmd 2>&1
    $it2Code = $LASTEXITCODE
    $it2Out | ForEach-Object { Write-Output $_ }
    if ($it2Code -ne 0) {
        Write-Step "FAILED: Challenger M3 Adversarial IT2 tests failed with exit code $it2Code!"
        $overallPass = $false
    } else {
        Write-Step "PASSED: Challenger M3 Adversarial IT2 tests passed cleanly!"
    }

} catch {
    Write-Step "EXCEPTION ENCOUNTERED: $_"
    $overallPass = $false
} finally {
    Set-Location $root
    Write-Step "`n--- Cleaning up backend & frontend processes ---"
    if ($frontendProc -and -not $frontendProc.HasExited) {
        & taskkill /PID $frontendProc.Id /T /F 2>$null | Out-Null
        Stop-Process -Id $frontendProc.Id -Force -ErrorAction SilentlyContinue
    }
    if ($backendProc -and -not $backendProc.HasExited) {
        & taskkill /PID $backendProc.Id /T /F 2>$null | Out-Null
        Stop-Process -Id $backendProc.Id -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 1
    Stop-PortProcesses @(5080, 3000)
    Write-Step "Cleanup complete."
}

if ($overallPass) {
    Write-Step "`n>>> ALL MILESTONE 3 TESTS & ADVERSARIAL STRESS SUITES PASSED (EXIT CODE 0) <<<"
    exit 0
} else {
    Write-Step "`n>>> ONE OR MORE TESTS FAILED (EXIT CODE 1) <<<"
    exit 1
}
