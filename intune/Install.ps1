# ─────────────────────────────────────────────────────────────
# Woodgrove Bank Terminal — Intune Install Script
# Runs in SYSTEM context. Installs to C:\WoodgroveBank and
# creates a desktop shortcut for all users.
# ─────────────────────────────────────────────────────────────

$ErrorActionPreference = "Stop"

$appName    = "Woodgrove Bank Terminal"
$installDir = "C:\WoodgroveBank"
$exeName    = "cobol-banker.exe"
$dbName     = "cobol-banker.db"
$srcExe     = Join-Path $PSScriptRoot $exeName
$srcDb      = Join-Path $PSScriptRoot $dbName
$destExe    = Join-Path $installDir $exeName
$destDb     = Join-Path $installDir $dbName
$shortcut   = Join-Path "$env:Public\Desktop" "$appName.lnk"

# ── Install ──────────────────────────────────────────────────

Write-Host "Installing $appName..."

# Create install directory
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

# Copy the exe and database
Copy-Item -Path $srcExe -Destination $destExe -Force
Write-Host "  Copied $exeName to $installDir"

Copy-Item -Path $srcDb -Destination $destDb -Force
Write-Host "  Copied $dbName to $installDir"

# Create desktop shortcut (Public Desktop = all users)
$ws = New-Object -ComObject WScript.Shell
$sc = $ws.CreateShortcut($shortcut)
$sc.TargetPath       = $destExe
$sc.WorkingDirectory = $installDir
$sc.Description      = $appName
$sc.Save()
Write-Host "  Created desktop shortcut: $shortcut"

# Pin to taskbar (makes the app easier for computer-use agents to find)
$taskbarDir = "$env:AppData\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar"
if (Test-Path $taskbarDir) {
    $taskbarShortcut = Join-Path $taskbarDir "$appName.lnk"
    $tc = $ws.CreateShortcut($taskbarShortcut)
    $tc.TargetPath       = $destExe
    $tc.WorkingDirectory = $installDir
    $tc.Description      = $appName
    $tc.Save()
    Write-Host "  Created taskbar pin: $taskbarShortcut"
}

Write-Host "$appName installed successfully."
exit 0
