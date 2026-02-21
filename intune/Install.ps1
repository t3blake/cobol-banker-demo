# ─────────────────────────────────────────────────────────────
# Woodgrove Bank Terminal — Intune Install Script
# Runs in SYSTEM context. Installs to Program Files and
# creates a desktop shortcut for all users.
# ─────────────────────────────────────────────────────────────

$ErrorActionPreference = "Stop"

$appName    = "Woodgrove Bank Terminal"
$installDir = "$env:ProgramFiles\WoodgroveBank"
$exeName    = "cobol-banker.exe"
$srcExe     = Join-Path $PSScriptRoot $exeName
$destExe    = Join-Path $installDir $exeName
$shortcut   = Join-Path "$env:Public\Desktop" "$appName.lnk"

# ── Install ──────────────────────────────────────────────────

Write-Host "Installing $appName..."

# Create install directory
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

# Copy the exe
Copy-Item -Path $srcExe -Destination $destExe -Force
Write-Host "  Copied $exeName to $installDir"

# Create desktop shortcut (Public Desktop = all users)
$ws = New-Object -ComObject WScript.Shell
$sc = $ws.CreateShortcut($shortcut)
$sc.TargetPath       = $destExe
$sc.WorkingDirectory = $installDir
$sc.Description      = $appName
$sc.Save()
Write-Host "  Created desktop shortcut: $shortcut"

Write-Host "$appName installed successfully."
exit 0
