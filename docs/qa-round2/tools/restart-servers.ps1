<#
  Tool điều phối QA: (re)start backend :5080 + frontend :3000 ở dạng tiến trình TÁCH RỜI
  (tạo qua Win32_Process.Create nên không bị kill khi phiên agent/terminal kết thúc).
  Dùng: powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 [-Build]
#>
param([switch]$Build)
$ErrorActionPreference = "Stop"
$root = "d:\03_Study\Studxy"
$logDir = Join-Path $root "test-results\prod-logs"
New-Item -ItemType Directory -Force $logDir | Out-Null

foreach ($port in 5080, 3000) {
    Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
        ForEach-Object { if ($_.OwningProcess -gt 0) { & taskkill /PID $_.OwningProcess /T /F 2>$null | Out-Null } }
}
Start-Sleep -Seconds 1

if ($Build) {
    & dotnet build "$root\backend\Swms.Api.sln" -c Release --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "Backend build failed ($LASTEXITCODE)" }
    Push-Location "$root\frontend"
    try { & npx.cmd next build; if ($LASTEXITCODE -ne 0) { throw "Frontend build failed ($LASTEXITCODE)" } } finally { Pop-Location }
}

$dll = "$root\backend\src\Swms.Api\bin\Release\net10.0\Swms.Api.dll"
$be = "cmd.exe /c cd /d `"$root\backend\src\Swms.Api`" && dotnet `"$dll`" --urls http://127.0.0.1:5080 > `"$logDir\backend-prod.log`" 2> `"$logDir\backend-prod-err.log`""
$fe = "cmd.exe /c cd /d `"$root\frontend`" && set NODE_ENV=production && npx.cmd next start --port 3000 > `"$logDir\frontend-prod.log`" 2> `"$logDir\frontend-prod-err.log`""
$startup = New-CimInstance -ClassName Win32_ProcessStartup -ClientOnly -Property @{ ShowWindow = [uint16]0 }
foreach ($cmd in $be, $fe) {
    $r = Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{ CommandLine = $cmd; ProcessStartupInformation = $startup }
    if ($r.ReturnValue -ne 0) { throw "Win32_Process.Create failed: $($r.ReturnValue)" }
}

$ok = $false
for ($i = 1; $i -le 60; $i++) {
    Start-Sleep -Seconds 1
    try {
        $a = Invoke-WebRequest "http://127.0.0.1:5080/health" -UseBasicParsing -TimeoutSec 2
        $b = Invoke-WebRequest "http://127.0.0.1:3000/health" -UseBasicParsing -TimeoutSec 2
        if ($a.StatusCode -eq 200 -and $b.StatusCode -eq 200) { $ok = $true; break }
    } catch {}
}
if (-not $ok) { throw "Servers not healthy after 60s. See $logDir" }
Write-Host "READY: frontend http://127.0.0.1:3000 , backend http://127.0.0.1:5080 (after ${i}s)" -ForegroundColor Green
