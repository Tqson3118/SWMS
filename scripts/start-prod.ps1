<#
.SYNOPSIS
    Production Startup Script for Studxy (SWMS v2.0).
.DESCRIPTION
    1. Cleans up any existing processes on ports 5080 and 3000.
    2. Builds Backend (.NET 10 Release) and Frontend (Next.js production build) unless -NoBuild is passed.
    3. Launches Swms.Api.dll on http://127.0.0.1:5080 and polls /health.
    4. Launches `next start --port 3000` on http://127.0.0.1:3000 and polls readiness.
    5. Optionally opens http://127.0.0.1:3000/app in the default browser.
#>

[CmdletBinding()]
param(
    [switch]$NoBuild,
    [switch]$OpenBrowser
)

$ErrorActionPreference = "Stop"
$root = (Get-Item $PSScriptRoot).Parent.FullName
Set-Location $root

function Stop-PortListeners {
    param([int[]]$Ports)
    foreach ($port in $Ports) {
        $pidsToKill = [System.Collections.Generic.HashSet[int]]::new()
        try {
            $conns = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
            if ($conns) {
                foreach ($conn in $conns) {
                    if ($conn.OwningProcess -gt 0 -and $conn.OwningProcess -ne $PID) {
                        [void]$pidsToKill.Add($conn.OwningProcess)
                    }
                }
            }
        } catch {}

        foreach ($p in $pidsToKill) {
            Write-Host "[Cleanup] Stopping existing process on port $port (PID: $p)..." -ForegroundColor Yellow
            & taskkill /PID $p /T /F 2>$null | Out-Null
            Stop-Process -Id $p -Force -ErrorAction SilentlyContinue
        }

        for ($w = 0; $w -lt 10; $w++) {
            $listening = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
            if (-not $listening) { break }
            Start-Sleep -Milliseconds 300
        }
    }
}

$logDir = Join-Path $root "test-results\prod-logs"
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

Stop-PortListeners @(5080, 3000)

if (-not $NoBuild) {
    Write-Host "[1/4] Building Backend (.NET 10 Release)..." -ForegroundColor Cyan
    & dotnet build backend/Swms.Api.sln -c Release --nologo -v q
    if ($LASTEXITCODE -ne 0) {
        throw "Backend Release build failed with exit code $LASTEXITCODE"
    }

    Write-Host "[2/4] Building Frontend (Next.js Production)..." -ForegroundColor Cyan
    Push-Location (Join-Path $root "frontend")
    try {
        & npx.cmd next build
        if ($LASTEXITCODE -ne 0) {
            throw "Frontend production build failed with exit code $LASTEXITCODE"
        }
    } finally {
        Pop-Location
    }
} else {
    Write-Host "[1-2/4] Skipping builds (-NoBuild enabled)..." -ForegroundColor Yellow
}

$backendDll = Join-Path $root "backend\src\Swms.Api\bin\Release\net10.0\Swms.Api.dll"
if (-not (Test-Path $backendDll)) {
    throw "Backend Release binary not found at $backendDll"
}

$backendOut = Join-Path $logDir "backend-prod.log"
$backendErr = Join-Path $logDir "backend-prod-err.log"
$frontendOut = Join-Path $logDir "frontend-prod.log"
$frontendErr = Join-Path $logDir "frontend-prod-err.log"

Write-Host "[3/4] Starting Backend API on http://127.0.0.1:5080..." -ForegroundColor Cyan
$backendProc = Start-Process -FilePath "dotnet" `
    -ArgumentList "`"$backendDll`" --urls http://127.0.0.1:5080" `
    -WorkingDirectory (Join-Path $root "backend\src\Swms.Api") `
    -RedirectStandardOutput $backendOut `
    -RedirectStandardError $backendErr `
    -WindowStyle Hidden `
    -PassThru

$backendReady = $false
for ($i = 1; $i -le 30; $i++) {
    Start-Sleep -Seconds 1
    if ($backendProc.HasExited) {
        throw "Backend exited early with code $($backendProc.ExitCode). Check $backendErr"
    }
    try {
        $resp = Invoke-WebRequest -Uri "http://127.0.0.1:5080/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
        if ($resp.StatusCode -eq 200) {
            $backendReady = $true
            Write-Host "      Backend READY (HTTP 200 after ${i}s, PID: $($backendProc.Id))" -ForegroundColor Green
            break
        }
    } catch {}
}
if (-not $backendReady) {
    throw "Backend failed to pass /health check within 30s."
}

Write-Host "[4/4] Starting Frontend Production Server on http://127.0.0.1:3000..." -ForegroundColor Cyan
$env:NODE_ENV = "production"
$env:NODE_OPTIONS = "--max-old-space-size=4096"
$frontendProc = Start-Process -FilePath "cmd.exe" `
    -ArgumentList "/c npx.cmd next start --port 3000" `
    -WorkingDirectory (Join-Path $root "frontend") `
    -RedirectStandardOutput $frontendOut `
    -RedirectStandardError $frontendErr `
    -WindowStyle Hidden `
    -PassThru

$frontendReady = $false
for ($i = 1; $i -le 30; $i++) {
    Start-Sleep -Seconds 1
    if ($frontendProc.HasExited) {
        throw "Frontend exited early with code $($frontendProc.ExitCode). Check $frontendErr"
    }
    try {
        $resp = Invoke-WebRequest -Uri "http://127.0.0.1:3000/" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
        $healthProxy = Invoke-WebRequest -Uri "http://127.0.0.1:3000/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
        if ($resp.StatusCode -eq 200 -and $healthProxy.StatusCode -eq 200) {
            $frontendReady = $true
            Write-Host "      Frontend READY (HTTP 200 after ${i}s, PID: $($frontendProc.Id))" -ForegroundColor Green
            break
        }
    } catch {}
}
if (-not $frontendReady) {
    throw "Frontend failed to respond with HTTP 200 within 30s."
}

Write-Host ""
Write-Host "================================================================================" -ForegroundColor Green
Write-Host "STUDXY (SWMS v2.0) PRODUCTION SERVER RUNNING" -ForegroundColor Green
Write-Host "  Frontend App     : http://127.0.0.1:3000/app" -ForegroundColor White
Write-Host "  Landing Page     : http://127.0.0.1:3000/" -ForegroundColor White
Write-Host "  Auth Portal      : http://127.0.0.1:3000/auth" -ForegroundColor White
Write-Host "  Workspace Classic: http://127.0.0.1:3000/workspace" -ForegroundColor White
Write-Host "  Health Endpoint  : http://127.0.0.1:3000/health (proxied to :5080)" -ForegroundColor White
Write-Host "  Backend PID      : $($backendProc.Id) | Logs: $backendOut" -ForegroundColor Gray
Write-Host "  Frontend PID     : $($frontendProc.Id) | Logs: $frontendOut" -ForegroundColor Gray
Write-Host "================================================================================" -ForegroundColor Green

if ($OpenBrowser) {
    Start-Process "http://127.0.0.1:3000/app"
}
