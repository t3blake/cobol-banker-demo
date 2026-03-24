# ─────────────────────────────────────────────────────────────
# Woodgrove Bank Terminal — Package Script
# Builds the app from source and creates release artifacts:
#   1. WoodgroveBank.zip   — manual install (exe + db + Install.ps1)
#   2. Install.intunewin   — Intune Win32 package
# ─────────────────────────────────────────────────────────────

$ErrorActionPreference = "Stop"

$intuneDir  = $PSScriptRoot
$repoRoot   = Split-Path $intuneDir -Parent
$srcDir     = Join-Path $repoRoot "src"
$distDir    = Join-Path $repoRoot "dist"
$srcStaging = Join-Path $intuneDir "source"
$outDir     = Join-Path $intuneDir "output"
$zipPath    = Join-Path $distDir "ManualInstall.zip"

# ── Build ────────────────────────────────────────────────────

Write-Host "Building COBOL Banker..." -ForegroundColor Cyan
Push-Location $srcDir
dotnet publish -c Release -r win-x64 --self-contained /p:PublishSingleFile=true -o $distDir
Pop-Location

if (-not (Test-Path (Join-Path $distDir "cobol-banker.exe"))) {
    Write-Error "Build failed — cobol-banker.exe not found in $distDir"
    exit 1
}

Write-Host "Build complete." -ForegroundColor Green

# ── Zip Package ──────────────────────────────────────────────

Write-Host "Creating WoodgroveBank.zip..." -ForegroundColor Cyan

$zipStaging = Join-Path $env:TEMP "WoodgroveBank-zip-staging"
if (Test-Path $zipStaging) { Remove-Item $zipStaging -Recurse -Force }
New-Item -ItemType Directory -Path $zipStaging | Out-Null

Copy-Item (Join-Path $distDir "cobol-banker.exe") $zipStaging
Copy-Item (Join-Path $distDir "cobol-banker.db")  $zipStaging
Copy-Item (Join-Path $intuneDir "Install.ps1")    $zipStaging
Copy-Item (Join-Path $intuneDir "Uninstall.ps1")  $zipStaging

if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $zipStaging "*") -DestinationPath $zipPath

Remove-Item $zipStaging -Recurse -Force

Write-Host "  Created: $zipPath" -ForegroundColor Green

# ── Intune Package ───────────────────────────────────────────

$intuneUtil = Join-Path $intuneDir "IntuneWinAppUtil.exe"

if (Test-Path $intuneUtil) {
    Write-Host "Creating Install.intunewin..." -ForegroundColor Cyan

    # Stage files
    if (-not (Test-Path $srcStaging)) { New-Item -ItemType Directory -Path $srcStaging | Out-Null }
    Copy-Item (Join-Path $distDir "cobol-banker.exe") $srcStaging -Force
    Copy-Item (Join-Path $distDir "cobol-banker.db")  $srcStaging -Force
    Copy-Item (Join-Path $intuneDir "Install.ps1")    $srcStaging -Force
    Copy-Item (Join-Path $intuneDir "Uninstall.ps1")  $srcStaging -Force

    if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

    & $intuneUtil -c $srcStaging -s Install.ps1 -o $outDir -q

    Write-Host "  Created: $outDir\Install.intunewin" -ForegroundColor Green
} else {
    Write-Host "  Skipping Intune package (IntuneWinAppUtil.exe not found at $intuneUtil)" -ForegroundColor Yellow
}

# ── Summary ──────────────────────────────────────────────────

Write-Host ""
Write-Host "Release artifacts:" -ForegroundColor Cyan
Write-Host "  Zip:    $zipPath"
if (Test-Path $intuneUtil) {
    Write-Host "  Intune: $outDir\Install.intunewin"
}
Write-Host ""
Write-Host "Upload these to a GitHub release." -ForegroundColor Cyan
