# ─────────────────────────────────────────────────────────────
# Woodgrove Bank Terminal — Intune Detection Script
# Returns exit 0 if installed, exit 1 if not.
# ─────────────────────────────────────────────────────────────

$exePath = "C:\WoodgroveBank\cobol-banker.exe"
$dbPath  = "C:\WoodgroveBank\cobol-banker.db"

if ((Test-Path $exePath) -and (Test-Path $dbPath)) {
    Write-Output "Detected: $exePath"
    exit 0
} else {
    exit 1
}
