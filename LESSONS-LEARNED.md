# COBOL Banker — Lessons Learned

A living document capturing what we've learned while building, tuning, and demoing the COBOL Banker app and its Copilot Studio computer-use agent. Aimed at anyone setting up their own instance — whether you're a colleague running the demo or exploring CUA capabilities on your own.

> **Contributing:** Add new entries to the relevant section with a date and brief description. Keep entries concise and actionable — focus on what you tried, what happened, and what worked.

---

## Start with a Design Doc

**The single most impactful thing we did was writing a formal design document before writing any code or agent instructions.**

The [DESIGN.md](DESIGN.md) captures the project goals, design principles, demo scenarios, feature set, database schema, and distribution strategy — all decided up front. This grounding paid off in several ways:

- **Fewer dead ends in code.** When implementation questions came up ("should we support mouse input?", "how should the reset work?"), the design doc already had the answer or the design principle that pointed to one.
- **Agent instructions wrote themselves.** Because the demo scenarios, menu structure, and data model were already defined, the agent instructions in `copilot-studio/` were essentially a translation of the design doc into agent-facing language.
- **Consistent seed data.** Defining the customers, accounts, and suspicious-activity patterns in the design doc meant the code, the agent knowledge file, and the evaluation tests all stayed aligned.

**Recommendation:** Even if your demo app is simple, write down what it does, who it's for, and what the agent needs to accomplish *before* you start building. A few hours of design saves days of rework.

---

## CUA Agent Tuning

These lessons come from iterating on the Copilot Studio computer-use agent. CUA agents interact with your app via screenshots and keyboard/mouse input — they're powerful but sensitive to how you write instructions.

### Put the Most Critical Action First

CUA agents weight early instructions heavily. If the first thing the agent needs to do is launch the app, that instruction should be the **first section** in your CUA Tool Instructions — not buried after "Application Identity" or "Visual Appearance."

We restructured [CUA-TOOL-INSTRUCTIONS.md](copilot-studio/CUA-TOOL-INSTRUCTIONS.md) to open with a "FIRST ACTION — Launch the App" section, placed immediately after the opening line. This alone reduced wasted steps at the start of every conversation.

**Before:** Application Identity → Visual Appearance → Launching the App (third section)
**After:** FIRST ACTION — Launch the App (first section) → Application Identity → Visual Appearance

### Give One Path, Not Three

Our original launch instructions offered three methods: Win+R (Run dialog), taskbar pin, and desktop shortcut. The intent was reliability through fallbacks. The result was the opposite — the agent would ignore all three and try its own approach (searching the Start menu, typing the app name into Windows Search, etc.).

**What worked:** Collapse to a single, deterministic method. We kept only the Win+R approach (`C:\WoodgroveBank\cobol-banker.exe`) because it requires zero visual scanning — the agent just presses a key combo and types a known path. The taskbar and desktop shortcut methods require the agent to visually locate an icon, which is unreliable.

**Takeaway:** For CUA agents, one clear path beats multiple fallbacks. Fewer choices = more reliable execution.

### Use Explicit "Do NOT" Instructions

CUA agents respond strongly to negative instructions. Without them, the agent improvises — and its improvisations are often worse than no action at all.

After adding a "Do NOT" block to our launch instructions, the Start menu searching stopped immediately:

```
- Do NOT search the Start menu.
- Do NOT use Windows Search or the taskbar search box.
- Do NOT use File Explorer to browse for the application.
- Do NOT type the app name into any search field.
- Do NOT look for desktop icons or shortcuts.
```

**Takeaway:** If you see the agent doing something undesirable, add an explicit prohibition. "Do NOT do X" is often more effective than "Do Y instead" — use both together.

### Make Launch Unconditional

Our original instructions said "If COBOL Banker is not already running, use one of these methods..." This conditional forced the agent to first determine whether the app was running — a visual assessment it did poorly, often concluding the app wasn't running even when it was visible, or vice versa.

**What worked:** Remove the conditional entirely. "Your very first action is ALWAYS to launch the application." If the app is already running, a second instance launches to the login screen — harmless, and the agent can proceed immediately. Eliminating the decision point eliminated the confusion.

### Tell the Agent What Success Looks Like

After a launch or navigation action, tell the agent exactly what it should see on screen. This gives it a verification checkpoint and prevents it from proceeding blindly or retrying unnecessarily.

```
Verification: You should see a black window with bright green text.
The screen will show the COBOL BANKER banner and a "USERID:" prompt.
```

Pair this with a retry instruction: "If the window did not appear, press Win+R and enter the exact same command again. Do NOT try alternative methods."

### Keep CUA Tool Instructions and Agent Instructions Aligned

The CUA Tool Instructions tell the agent *how to physically interact with the app*. The Agent Instructions tell the agent *what tasks to perform and how to reason*. Both files reference app launching, error recovery, and navigation — if they give conflicting guidance, the agent gets confused.

We caught a case where the Agent Instructions' "Recovery" section described launch differently than the CUA Tool Instructions' "Launching the App" section. After aligning both to use identical language (Win+R only, same "Do NOT" rules), recovery behavior improved.

**Recommendation:** After editing one file, grep for the same topic in the other file and make sure they match.

---

## App Design for CUA Agents

### Two Clearly Distinct Input Modes

COBOL Banker has two input states: a text box prompt (type and press Enter) and a "Press any key to continue..." dismiss screen (no text box, just press a key on the window). These look very different visually, which helps the CUA agent detect which mode it's in.

If you're building your own demo app, make input states visually unambiguous. A CUA agent that can't tell whether to type in a text box or press a key on the window will get stuck in loops.

### Consistent Visual Markers

We use `>>>` for success, `***` for errors, and `!!!` for warnings throughout the app. These consistent markers let us write simple CUA instructions ("if you see `***`, read the error") rather than teaching the agent to recognize every possible error screen individually.

### Keep Screens Sequential

COBOL Banker is deliberately single-threaded in its UI flow — there's exactly one active prompt at any time, and navigation always returns to the Main Menu. This simplicity is a feature for CUA agents. They don't handle concurrent dialogs, pop-ups, or non-linear navigation well.

---

## Evaluation & Testing

### Reset the Database Before Every Test Run

The evaluation tests assume seed data. If a previous test frozen an account or transferred funds, subsequent tests will fail with unexpected state. Always reset (Menu 8 → 1 → Y → Y) before running evaluation batches that modify data.

### Split Evaluations Into Small Batches

Running too many CUA tests in a single Copilot Studio evaluation batch can exhaust the CUA connection token, causing cascading failures. We split our 23 tests into 4 batches (smoke, read-only, write, compound) and run them sequentially. See [EVALUATION.md](copilot-studio/EVALUATION.md) for the full plan.

### Test the Launch Separately

App launching is the most failure-prone step for CUA agents. We made "Open the banking application" the very first test case so launch problems surface immediately, before you spend time debugging test failures that are actually launch failures.

---

## Deployment

### Avoid Spaces in Install Paths

The app was originally installed to `C:\Program Files\WoodgroveBank\`. We moved it to `C:\WoodgroveBank\` because CUA agents don't reliably handle spaces or quoting in file paths. When the agent types a path into the Win+R dialog, a space can cause the command to fail silently — no error, just nothing happens. Removing spaces from the path eliminated an entire class of launch failures.

**Takeaway:** If a CUA agent needs to type a file path, keep it short and space-free.

### Intune Install Creates the Exact Paths the Agent Expects

The agent instructions reference `C:\WoodgroveBank\cobol-banker.exe` as a hardcoded path. The [Intune install script](intune/Install.ps1) creates exactly this path. If you change one, change the other — a mismatch means the agent's Win+R command will fail silently (no error dialog, just nothing happens).

### Desktop Shortcut Label ≠ Window Title

The desktop shortcut is labeled "Woodgrove Bank Terminal" but the window title is "Woodgrove Bank — Terminal Emulator." This distinction matters for CUA agents that search by text. Our instructions reference both in the appropriate contexts — the shortcut label for taskbar refocusing, the window title for Alt+Tab and focus verification.

---

## Agent Behavior & Safety

### Explicitly Authorize Demo Actions

By default, the CUA agent refused to freeze accounts, transfer large amounts, and perform other actions it deemed "high risk" — even though the app uses entirely fake data. The model's built-in safety training overrides your instructions unless you explicitly address it.

The fix was the ["IMPORTANT: This Is a Demo Environment"](copilot-studio/AGENT-INSTRUCTIONS.md) section in Agent Instructions — an upfront block that states: this is a simulation with fake data, you are fully authorized, do not refuse or add extra confirmation. The key phrase is **"Do not add your own layer of 'are you sure?' on top"** — without this, the agent adds confirmation prompts the user never asked for.

**Takeaway:** If your demo app simulates sensitive operations, you need explicit authorization language in your agent instructions. Don't assume the agent will infer that fake data means it's safe to act.

### Split Instructions to Match Copilot Studio's Input Fields

We originally had a single monolithic guide ([AGENT-GUIDE.md](AGENT-GUIDE.md) — 700+ lines) covering everything: UI mechanics, agent behavior, reference data, demo scenarios. When we pasted this into Copilot Studio, the agent had too much context in the wrong places — the CUA tool was processing fraud analysis patterns, and the agent instructions were full of pixel-level UI details.

Splitting into three files that map directly to Copilot Studio's three input fields made a noticeable difference:

| Copilot Studio Field | File | What Goes Here |
|---------------------|------|---------------|
| **Knowledge** | `KNOWLEDGE.md` | Reference data — customers, accounts, balances |
| **Agent Instructions** | `AGENT-INSTRUCTIONS.md` | Behavior — how to reason, what tone to use, when to act |
| **CUA Tool Instructions** | `CUA-TOOL-INSTRUCTIONS.md` | UI mechanics — how to click, type, read screens |

**Takeaway:** Structure your documentation to match the tool's input model. Each field has a different purpose — mixing concerns dilutes the signal.

---

*Last updated: 2026-03-24*
