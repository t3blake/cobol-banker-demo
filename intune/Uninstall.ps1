# ─────────────────────────────────────────────────────────────
# Woodgrove Bank Terminal — Intune Uninstall Script
# Runs in SYSTEM context. Removes app and shortcut.
# ─────────────────────────────────────────────────────────────

$ErrorActionPreference = "SilentlyContinue"

$appName    = "Woodgrove Bank Terminal"
$installDir = "$env:ProgramFiles\WoodgroveBank"
$shortcut   = Join-Path "$env:Public\Desktop" "$appName.lnk"

Write-Host "Uninstalling $appName..."

# Kill running instances
Get-Process "cobol-banker" -ErrorAction SilentlyContinue | Stop-Process -Force

# Remove desktop shortcut
if (Test-Path $shortcut) {
    Remove-Item $shortcut -Force
    Write-Host "  Removed desktop shortcut"
}

# Remove install directory
if (Test-Path $installDir) {
    Remove-Item $installDir -Recurse -Force
    Write-Host "  Removed $installDir"
}

Write-Host "$appName uninstalled."
exit 0
