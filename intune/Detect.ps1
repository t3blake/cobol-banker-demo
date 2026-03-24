# ─────────────────────────────────────────────────────────────
# Woodgrove Bank Terminal — Intune Detection Script
# Returns exit 0 if installed, exit 1 if not.
# ─────────────────────────────────────────────────────────────

$exePath = "C:\WoodgroveBank\cobol-banker.exe"

if (Test-Path $exePath) {
    Write-Output "Detected: $exePath"
    exit 0
} else {
    exit 1
}
