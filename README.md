# COBOL Banker

A legacy "green screen" banking terminal simulator built for demoing how a Copilot Studio computer-use agent can interact with a locally installed desktop application.

<img width="901" height="708" alt="COBOL Banker terminal window" src="https://github.com/user-attachments/assets/674b2d13-c5f7-46be-a50c-b2c5c9b3e151" />

---

## Quick Start — Set Up Your Own Demo

Follow these steps to get COBOL Banker running on a Windows 365 Cloud PC with a Copilot Studio computer-use agent.

### Prerequisites

- A **Windows 365 Cloud PC** (or any Windows machine for local testing)
- **Microsoft Intune** access (only if deploying via Intune — not needed for manual install)
- **Copilot Studio** access (to create the agent)
- **Git** (to clone this repo)

---

### Step 1: Deploy the App

Go to the [latest release](https://github.com/t3blake/cobol-banker-demo/releases/latest) and choose one of these install methods:

#### Option A: Manual Install (quickest)

1. Download **ManualInstall.zip** from the release.
2. Extract the zip.
3. Right-click **Install.ps1** → **Run with PowerShell** (or run from an admin terminal: `powershell -ExecutionPolicy Bypass -File Install.ps1`).
4. The script installs to `C:\WoodgroveBank\` and creates a desktop shortcut called "Woodgrove Bank Terminal".

> **Even simpler:** If you just want to test locally, extract `cobol-banker.exe` and `cobol-banker.db` into `C:\WoodgroveBank\` manually. The agent instructions expect this exact path.

#### Option B: Deploy via Intune

1. Download **Install.intunewin** from the release.

2. In the **Microsoft Intune admin center**, go to **Apps → Windows → Add → Windows app (Win32)**.

3. Upload `Install.intunewin` as the app package file.

4. Configure the app:

   | Setting | Value |
   |---------|-------|
   | **Name** | Woodgrove Bank Terminal |
   | **Description** | Legacy banking terminal simulator for demo purposes |
   | **Publisher** | Woodgrove Bank (Demo) |
   | **Install command** | `powershell.exe -ExecutionPolicy Bypass -File Install.ps1` |
   | **Uninstall command** | `powershell.exe -ExecutionPolicy Bypass -File Uninstall.ps1` |
   | **Install behavior** | System |
   | **OS architecture** | 64-bit |
   | **Minimum OS** | Windows 10 1903 |

5. For the **detection rule**, choose **"Use a custom detection script"** and upload [`intune/Detect.ps1`](intune/Detect.ps1) from this repo.

6. **Assign** the app to a device group or user group as **Required**.

7. Wait for the app to install on the target machine. It installs to `C:\WoodgroveBank\` and creates a desktop shortcut called "Woodgrove Bank Terminal".

---

### Step 2: Create the Copilot Studio Agent

1. In **Copilot Studio**, create a new agent.

2. Configure the three instruction areas using the files in the [`copilot-studio/`](copilot-studio/) folder:

   | Copilot Studio Field | File to Use | How |
   |---------------------|-------------|-----|
   | **Knowledge** | [`copilot-studio/KNOWLEDGE.md`](copilot-studio/KNOWLEDGE.md) | Upload as a file |
   | **Agent Instructions** | [`copilot-studio/AGENT-INSTRUCTIONS.md`](copilot-studio/AGENT-INSTRUCTIONS.md) | Copy/paste the contents (skip the header) |
   | **CUA Tool Instructions** | [`copilot-studio/CUA-TOOL-INSTRUCTIONS.md`](copilot-studio/CUA-TOOL-INSTRUCTIONS.md) | Copy/paste the contents (skip the header) |

   > **Why three files?** Knowledge is retrieved on demand (the agent searches it when needed), while Instructions are always in context. Splitting them avoids tripling your token usage and gives each part a clear role:
   > - **Knowledge** = reference data (customers, accounts, menus, scenarios)
   > - **Agent Instructions** = how the agent should behave and interpret requests
   > - **CUA Tool Instructions** = how the computer-use tool interacts with the app's UI

3. Enable the **Computer Use** tool on the agent.

4. Point the agent at the **Windows 365 Cloud PC** where the app is deployed.

5. **Set the model** to **Claude Sonnet 4.5** (or newer). This is a vision-heavy, instruction-following task — fast chat-tier models with strong vision outperform reasoning models here. Avoid reasoning-class models (GPT-5 Reasoning, Claude Opus) as they add latency with no benefit for UI navigation.

   | Model | Recommendation |
   |-------|---------------|
   | Claude Sonnet 4.5 / 4.6 | Recommended — fast, accurate vision, strong instruction compliance |
   | GPT-4.1 | Good alternative — solid vision, fast |
   | GPT-5 Chat | Worth testing — newer, potentially better vision |
   | GPT-5 Auto / Reasoning | Not recommended — reasoning overhead adds latency with no benefit |
   | Claude Opus 4.6 | Not recommended — overkill for menu navigation |

6. **Disable web search.** The agent has everything it needs in the Knowledge file and instructions. Web search adds latency, risks injecting irrelevant context, and would be a compliance concern in any banking demo narrative.

> **Tip — Stale connections:** The Windows 365 / CUA connection token can expire when the Cloud PC session disconnects or restarts. If the agent prompts you to re-authenticate: go to **Settings → Connections** in Copilot Studio, find the Windows 365 connection, and click to refresh it. Do this proactively before demos.

---

### Step 3: Test It

Try these prompts with your agent:

| Prompt | What It Tests |
|--------|--------------|
| "Log in to the banking terminal and look up customer Jane Doe" | App launch, login, customer search |
| "Check the transaction history on account 1000200002 and tell me if anything looks suspicious" | Navigation, reading data, analysis |
| "Freeze Jane Doe's savings account and add a note about suspicious wire transfers" | Multi-step workflow, account maintenance |
| "Transfer $500 from John Smith's checking to his savings" | Fund transfer with confirmation |

See [DEMO-WALKTHROUGH.md](DEMO-WALKTHROUGH.md) for fully scripted demo scenarios with talking points.

---

### Step 4: Reset Between Demos

After each demo, the data may have changed (frozen accounts, transferred funds, etc.). To restore everything to the original seed data:

1. Log in to the app
2. Menu option `8` (System Administration)
3. Option `1` (Reset Database to Defaults)
4. Confirm twice with `Y`

Or tell the agent: *"Reset the banking database to its default state."*

---

### Step 5: Evaluate the Agent (Optional)

Use the built-in **Evaluation** feature in Copilot Studio to validate the agent works correctly.

1. In Copilot Studio, open your agent and click **Evaluate** in the left nav.
2. Click **+ New evaluation** → **Import**.
3. Import the test batches one at a time (run in order, wait for each to finish):

   | CSV File | Tests | What It Covers |
   |----------|-------|---------------|
   | [`evaluation-1-smoke.csv`](copilot-studio/evaluation-1-smoke.csv) | 5 | Launch, login, basic lookup, balance check |
   | [`evaluation-2-readonly.csv`](copilot-studio/evaluation-2-readonly.csv) | 7 | Lookups, transaction history, error handling |
   | [`evaluation-3-write.csv`](copilot-studio/evaluation-3-write.csv) | 7 | Transfers, freezes, contact updates, notes |
   | [`evaluation-4-compound.csv`](copilot-studio/evaluation-4-compound.csv) | 4 | Multi-step workflows, behavioral guardrails |

4. Reset the database (Step 4) before running batches 3 and 4.
5. For detailed expected behavior and troubleshooting, see [`copilot-studio/EVALUATION.md`](copilot-studio/EVALUATION.md).

> **Important:** Don't run multiple evaluations simultaneously — they share the same CUA connection. Run one batch, wait for it to finish, then start the next.

---

## Repository Structure

```
├── README.md                  ← You are here
├── AGENT-GUIDE.md             ← Full technical reference (all screens, flows, data)
├── DEMO-WALKTHROUGH.md        ← Scripted demo scenarios with talking points
├── DESIGN.md                  ← Architecture & design decisions
├── copilot-studio/            ← Ready-to-paste Copilot Studio content
│   ├── KNOWLEDGE.md           ← Upload as Knowledge file
│   ├── AGENT-INSTRUCTIONS.md  ← Paste into Agent Instructions
│   ├── CUA-TOOL-INSTRUCTIONS.md ← Paste into CUA Tool Instructions
│   ├── EVALUATION.md          ← Test plan & troubleshooting guide
│   ├── evaluation-1-smoke.csv ← Smoke test (5 tests)
│   ├── evaluation-2-readonly.csv ← Read-only tests (7 tests)
│   ├── evaluation-3-write.csv ← Write/state-change tests (7 tests)
│   └── evaluation-4-compound.csv ← Compound & behavioral tests (4 tests)
├── src/                       ← C# / .NET 8 WPF source code
├── dist/                      ← Pre-built exe + database
└── intune/                    ← Intune deployment packaging
    ├── Install.ps1
    ├── Uninstall.ps1
    ├── Detect.ps1
    ├── IntuneWinAppUtil.exe
    ├── source/                ← Staging folder for packaging
    └── output/                ← Built .intunewin package
```

---

## Building from Source (Optional)

You only need this if you're modifying the app itself. For demo purposes, use the pre-built release.

```powershell
# Install .NET 8 SDK if needed
# https://dotnet.microsoft.com/download/dotnet/8.0

# Run in development mode
cd src
dotnet run

# Publish self-contained exe
dotnet publish -c Release -r win-x64 --self-contained /p:PublishSingleFile=true -o ..\dist

# Rebuild the Intune package
Copy-Item dist\cobol-banker.exe intune\source\
Copy-Item dist\cobol-banker.db  intune\source\
Copy-Item intune\Install.ps1    intune\source\
Copy-Item intune\Uninstall.ps1  intune\source\
.\intune\IntuneWinAppUtil.exe -c intune\source -s Install.ps1 -o intune\output -q
```

---

## Key Details

| Property | Value |
|----------|-------|
| App window title | Woodgrove Bank — Terminal Emulator |
| Desktop shortcut name | Woodgrove Bank Terminal |
| Install path | `C:\WoodgroveBank\` |
| Exe name | `cobol-banker.exe` |
| Default login | `teller1` / `pass123` |
| Database | SQLite, local file, resets via in-app menu |

---

## Additional Documentation

- [AGENT-GUIDE.md](AGENT-GUIDE.md) — Complete technical reference for every screen, input flow, edge case, and seed data. The authoritative source.
- [DEMO-WALKTHROUGH.md](DEMO-WALKTHROUGH.md) — Three fully scripted demo scenarios (fraud investigation, address change, fund transfer) with talking points for live demos.
- [copilot-studio/EVALUATION.md](copilot-studio/EVALUATION.md) — 22-test evaluation plan with Copilot Studio setup instructions, scoring guide, and troubleshooting common failures.
- [DESIGN.md](DESIGN.md) — Architecture decisions, tech stack rationale, database schema, and versioning strategy.
- [intune/README.md](intune/README.md) — Intune packaging details and configuration reference.

## License

MIT
