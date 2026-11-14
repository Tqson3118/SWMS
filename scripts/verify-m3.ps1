# scripts/verify-m3.ps1
param(
    [string]$Mode = "dev"
)

$ErrorActionPreference = "Continue"
$root = (Get-Item $PSScriptRoot).Parent.FullName
Set-Location $root

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
            Write-Host "Cleaning up process on port $port (PID: $p)" -ForegroundColor Yellow
            & taskkill /PID $p /T /F 2>$null | Out-Null
            Stop-Process -Id $p -Force -ErrorAction SilentlyContinue
        }
    }
}

Write-Host "=== SWMS Milestone 3 Verification Runner ===" -ForegroundColor Cyan
Stop-PortProcesses @(5080, 3000)
Start-Sleep -Seconds 1

$backendProc = $null
$frontendProc = $null
$backendLog = "$root\backend-verify.log"
$frontendLog = "$root\frontend-verify.log"

try {
    Write-Host "`n1. Starting Backend on http://127.0.0.1:5080..." -ForegroundColor Yellow
    $backendArgs = "run --project backend/src/Swms.Api/Swms.Api.csproj -c Release --urls http://127.0.0.1:5080"
    $backendProc = Start-Process -FilePath "dotnet" -ArgumentList $backendArgs -WorkingDirectory $root `
        -RedirectStandardOutput $backendLog -RedirectStandardError "$backendLog.err" -PassThru

    $backendReady = $false
    for ($i = 1; $i -le 30; $i++) {
        Start-Sleep -Seconds 1
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:5080/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
            if ($resp.StatusCode -eq 200) {
                $backendReady = $true
                Write-Host "Backend is READY (HTTP 200 after ${i}s)" -ForegroundColor Green
                break
            }
        } catch {}
    }
    if (-not $backendReady) {
        throw "Backend failed to become ready within 30s"
    }

    Write-Host "`n2. Starting Frontend on http://127.0.0.1:3000 ($Mode mode)..." -ForegroundColor Yellow
    $frontendCmd = if ($Mode -eq "start") { "npx.cmd next start --port 3000" } else { "npx.cmd next dev --port 3000" }
    $frontendProc = Start-Process -FilePath "cmd.exe" -ArgumentList "/c $frontendCmd" -WorkingDirectory "$root\frontend" `
        -RedirectStandardOutput $frontendLog -RedirectStandardError "$frontendLog.err" -PassThru

    $frontendReady = $false
    for ($i = 1; $i -le 45; $i++) {
        Start-Sleep -Seconds 1
        if ($frontendProc.HasExited) {
            throw "Frontend process exited unexpectedly with code $($frontendProc.ExitCode)"
        }
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:3000" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
            if ($resp.StatusCode -eq 200) {
                $frontendReady = $true
                Write-Host "Frontend is READY (HTTP 200 after ${i}s)" -ForegroundColor Green
                break
            }
        } catch {}
    }
    if (-not $frontendReady) {
        throw "Frontend failed to become ready within 45s"
    }

    Write-Host "`n3. Running Playwright R3 tests: npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g 'R3-T'..." -ForegroundColor Yellow
    Set-Location "$root\frontend"
    $r3Cmd = "npx.cmd playwright test e2e/r5-visual-interactive-e2e.spec.ts -g `"R3-T`" --reporter=list"
    $r3Output = & cmd.exe /c $r3Cmd 2>&1
    $r3ExitCode = $LASTEXITCODE
    $r3Output | ForEach-Object { Write-Host $_ }
    if ($r3ExitCode -ne 0) {
        throw "Playwright R3 tests FAILED with exit code $r3ExitCode"
    }
    Write-Host "Playwright R3 tests PASSED (exit code 0)" -ForegroundColor Green

    Write-Host "`n4. Running Playwright Workspace tests: npx.cmd playwright test e2e/swms-workspace.spec.ts..." -ForegroundColor Yellow
    $wsCmd = "npx.cmd playwright test e2e/swms-workspace.spec.ts --reporter=list"
    $wsOutput = & cmd.exe /c $wsCmd 2>&1
    $wsExitCode = $LASTEXITCODE
    $wsOutput | ForEach-Object { Write-Host $_ }
    if ($wsExitCode -ne 0) {
        throw "Playwright workspace tests FAILED with exit code $wsExitCode"
    }
    Write-Host "Playwright workspace tests PASSED (exit code 0)" -ForegroundColor Green

    Write-Host "`n=== ALL PLAYWRIGHT VERIFICATIONS PASSED ===" -ForegroundColor Green
} finally {
    Set-Location $root
    Write-Host "`nTearing down processes..." -ForegroundColor Gray
    if ($backendProc -and -not $backendProc.HasExited) {
        Stop-Process -Id $backendProc.Id -Force -ErrorAction SilentlyContinue
        & taskkill /PID $backendProc.Id /T /F 2>$null | Out-Null
    }
    if ($frontendProc -and -not $frontendProc.HasExited) {
        Stop-Process -Id $frontendProc.Id -Force -ErrorAction SilentlyContinue
        & taskkill /PID $frontendProc.Id /T /F 2>$null | Out-Null
    }
    Start-Sleep -Seconds 1
    Stop-PortProcesses @(5080, 3000)
    Write-Host "Cleanup completed." -ForegroundColor Gray
}
