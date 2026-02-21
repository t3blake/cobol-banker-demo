# Intune Win32 Packaging

## Contents

| File | Purpose |
|------|---------|
| `Install.ps1` | Install script — copies exe to `%ProgramFiles%\WoodgroveBank`, creates Public Desktop shortcut |
| `Uninstall.ps1` | Uninstall script — kills process, removes folder and shortcut |
| `Detect.ps1` | Detection script — checks for exe in install path |

## Pre-built Package

Run the packaging steps below, or use the pre-built `output/Install.intunewin` if available.

## Packaging Steps

1. **Publish the exe** (from repo root):
   ```powershell
   dotnet publish src -c Release -r win-x64 --self-contained /p:PublishSingleFile=true -o dist
   ```

2. **Stage the source folder:**
   ```powershell
   New-Item -ItemType Directory -Path intune\source -Force
   Copy-Item dist\cobol-banker.exe intune\source\
   Copy-Item intune\Install.ps1 intune\source\
   Copy-Item intune\Uninstall.ps1 intune\source\
   ```

3. **Run IntuneWinAppUtil:**
   ```powershell
   .\intune\IntuneWinAppUtil.exe -c intune\source -s Install.ps1 -o intune\output -q
   ```
   Output: `intune\output\Install.intunewin`

## Intune Configuration

When creating the Win32 app in Intune:

| Setting | Value |
|---------|-------|
| **App package file** | `Install.intunewin` |
| **Name** | Woodgrove Bank Terminal |
| **Description** | Legacy banking terminal simulator for demo purposes |
| **Publisher** | Woodgrove Bank (Demo) |
| **Install command** | `powershell.exe -ExecutionPolicy Bypass -File Install.ps1` |
| **Uninstall command** | `powershell.exe -ExecutionPolicy Bypass -File Uninstall.ps1` |
| **Install behavior** | System |
| **Detection rule type** | Use a custom detection script |
| **Detection script** | Upload `Detect.ps1` |
| **OS architecture** | 64-bit |
| **Minimum OS** | Windows 10 1903 |

### Assignment
Assign to a device group or user group as **Required** or **Available**.
