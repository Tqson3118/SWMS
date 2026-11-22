<#
.SYNOPSIS
    Publish and package Studxy Backend (.NET 10) for MonsterASP.NET IIS hosting.
.DESCRIPTION
    1. Cleans publish/monsterasp and publish/studxy-backend-monsterasp.zip.
    2. Runs `dotnet publish` in Release configuration.
    3. Removes any local secret files (appsettings.*.local.json), local SQLite DBs (*.db*), and *.pdb files.
    4. Ensures web.config has startupTimeLimit="120" for WAN PostgreSQL (Supabase) cold-start safety.
    5. Creates logs/ and App_Data/storage/documents/ folders and zips into publish/studxy-backend-monsterasp.zip.
#>

[CmdletBinding()]
param(
    [string]$OutputDir = "publish\monsterasp",
    [string]$ZipFile = "publish\studxy-backend-monsterasp.zip"
)

$ErrorActionPreference = "Stop"
$root = (Get-Item $PSScriptRoot).Parent.FullName
Set-Location $root

$publishDir = if ([System.IO.Path]::IsPathRooted($OutputDir)) { $OutputDir } else { Join-Path $root $OutputDir }
$zipPath = if ([System.IO.Path]::IsPathRooted($ZipFile)) { $ZipFile } else { Join-Path $root $ZipFile }
$zipParent = Split-Path -Parent $zipPath

if (Test-Path $publishDir) {
    Remove-Item -Path $publishDir -Recurse -Force
}
if (Test-Path $zipPath) {
    Remove-Item -Path $zipPath -Force
}
if (-not (Test-Path $zipParent)) {
    New-Item -ItemType Directory -Path $zipParent -Force | Out-Null
}

Write-Host "[1/5] Publishing Swms.Api (Release) to $publishDir..." -ForegroundColor Cyan
$csproj = Join-Path $root "backend\src\Swms.Api\Swms.Api.csproj"
& dotnet publish $csproj -c Release -o $publishDir --nologo
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host "[2/5] Removing local secrets, local SQLite databases, and PDB files..." -ForegroundColor Cyan
$excludedFiles = Get-ChildItem -Path $publishDir -Include "appsettings.*.local.json","*.db","*.db-shm","*.db-wal","*.pdb" -Recurse -File -ErrorAction SilentlyContinue
foreach ($file in $excludedFiles) {
    Remove-Item -Path $file.FullName -Force
    Write-Host "      Removed: $($file.Name)" -ForegroundColor Yellow
}

$localAppSettingsCheck = Get-ChildItem -Path $publishDir -Filter "appsettings.*.local.json" -Recurse -File -ErrorAction SilentlyContinue
if ($localAppSettingsCheck) {
    throw "Security check failed: local secret file found in publish output!"
}

$publishedAppSettings = Join-Path $publishDir "appsettings.json"
if (Test-Path $publishedAppSettings) {
    $jsonCfg = Get-Content -Path $publishedAppSettings -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($jsonCfg.ConnectionStrings -and -not [string]::IsNullOrWhiteSpace($jsonCfg.ConnectionStrings.Postgres)) {
        throw "Security check failed: ConnectionStrings.Postgres is not empty in published appsettings.json!"
    }
}

Write-Host "[3/5] Configuring web.config for MonsterASP.NET IIS (startupTimeLimit=120)..." -ForegroundColor Cyan
$webConfigPath = Join-Path $publishDir "web.config"
if (-not (Test-Path $webConfigPath)) {
    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\Swms.Api.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" startupTimeLimit="120" />
    </system.webServer>
  </location>
</configuration>
"@ | Set-Content -Path $webConfigPath -Encoding UTF8
} else {
    [xml]$xml = Get-Content -Path $webConfigPath -Raw -Encoding UTF8
    $aspNetCoreNode = $xml.SelectSingleNode("//aspNetCore")
    if ($null -eq $aspNetCoreNode) {
        throw "web.config does not contain <aspNetCore> element."
    }
    $aspNetCoreNode.SetAttribute("startupTimeLimit", "120")
    if ([string]::IsNullOrWhiteSpace($aspNetCoreNode.GetAttribute("stdoutLogFile"))) {
        $aspNetCoreNode.SetAttribute("stdoutLogFile", ".\logs\stdout")
    }
    $xml.Save($webConfigPath)
}

Write-Host "[4/5] Ensuring logs and App_Data storage directories exist..." -ForegroundColor Cyan
$logsDir = Join-Path $publishDir "logs"
$storageDir = Join-Path $publishDir "App_Data\storage\documents"
New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
New-Item -ItemType Directory -Path $storageDir -Force | Out-Null
Set-Content -Path (Join-Path $logsDir ".keep") -Value "" -Encoding UTF8
Set-Content -Path (Join-Path $storageDir ".keep") -Value "" -Encoding UTF8

Write-Host "[5/5] Creating deployment zip package: $zipPath..." -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -CompressionLevel Optimal -Force

$zipSizeMb = [Math]::Round((Get-Item $zipPath).Length / 1MB, 2)
Write-Host ""
Write-Host "================================================================================" -ForegroundColor Green
Write-Host "MONSTERASP.NET PUBLISH PACKAGE READY" -ForegroundColor Green
Write-Host "  Publish Folder : $publishDir" -ForegroundColor White
Write-Host "  Zip Package    : $zipPath (${zipSizeMb} MB)" -ForegroundColor White
Write-Host "  Secrets Check  : PASSED (no appsettings.*.local.json, empty Postgres string)" -ForegroundColor White
Write-Host "================================================================================" -ForegroundColor Green
