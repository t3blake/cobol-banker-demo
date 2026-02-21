# ─────────────────────────────────────────────────────────────
# Woodgrove Bank Terminal — Intune Detection Script
# Returns exit 0 if installed, exit 1 if not.
# ─────────────────────────────────────────────────────────────

$exePath = "$env:ProgramFiles\WoodgroveBank\cobol-banker.exe"

if (Test-Path $exePath) {
    Write-Host "Detected: $exePath"
    exit 0
} else {
    exit 1
}
