# scripts/run_diag.ps1
$root = (Get-Item $PSScriptRoot).Parent.FullName
Set-Location $root

Write-Host "Cleaning up old processes..."
Get-NetTCPConnection -LocalPort 5080, 3000 -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 2

Write-Host "Starting Backend..."
$backendProc = Start-Process -FilePath "dotnet" -ArgumentList "run --project backend/src/Swms.Api/Swms.Api.csproj -c Release --urls http://127.0.0.1:5080" -WorkingDirectory $root -PassThru

for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Seconds 1
    try {
        $resp = Invoke-WebRequest -Uri "http://127.0.0.1:5080/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
        if ($resp.StatusCode -eq 200) {
            Write-Host "Backend ready."
            break
        }
    } catch {}
}

Write-Host "Starting Frontend..."
$frontendProc = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm.cmd run dev" -WorkingDirectory "$root\frontend" -PassThru

for ($i = 0; $i -lt 45; $i++) {
    Start-Sleep -Seconds 1
    try {
        $resp = Invoke-WebRequest -Uri "http://127.0.0.1:3000" -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
        if ($resp.StatusCode -eq 200) {
            Write-Host "Frontend ready."
            break
        }
    } catch {}
}

Write-Host "Running Diagnostic Test..."
Set-Location "$root\frontend"
& cmd.exe /c "npx.cmd playwright test e2e/challenger-diag.spec.ts --reporter=list"

Write-Host "Cleaning up..."
Stop-Process -Id $backendProc.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $frontendProc.Id -Force -ErrorAction SilentlyContinue
Get-NetTCPConnection -LocalPort 5080, 3000 -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
}
