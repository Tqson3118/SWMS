param(
    [string]$SourcePath = "D:\03_Study\Studxy\document\PersonalFinance",
    [string]$DestinationZip = "D:\03_Study\Studxy\frontend\e2e\fixtures\personal-finance-course.zip"
)

$destDir = Split-Path $DestinationZip
if (-not (Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
}

if (Test-Path $DestinationZip) {
    Remove-Item $DestinationZip -Force
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($SourcePath, $DestinationZip)

Write-Host "Created zip package: $DestinationZip"
Get-Item $DestinationZip | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
