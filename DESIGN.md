# COBOL Banker — Legacy Banking Terminal Simulator

## Design Document (Draft v0.1)

---

## 1. Project Goal

Build a single-file local executable that mimics a **legacy "green screen" banking terminal**. The app will be used in demos to show how a Copilot agent can interact with a locally installed command-line application — reading screen output, issuing commands, and performing workflows on behalf of a user.

The name "COBOL Banker" is a playful nod to the mainframe era — the app itself won't be written in COBOL, but it evokes the exact kind of system we're simulating.

---

## 2. Design Principles

| # | Principle | Rationale |
|---|-----------|-----------|
| 1 | **Keep it dead-simple** | This is a demo prop, not production software. Minimal dependencies, minimal abstractions. |
| 2 | **Drop-and-go portable** | Must be trivial for a coworker to get running. Preferred: a zip they extract and double-click. An installer is acceptable if it's a single "Next → Finish" experience with no decisions. No external services, no runtimes to pre-install. |
| 3 | **Deterministic, text-based UI** | All interaction is through text input/output so a Copilot agent (via terminal) can read and drive it. No mouse, no GUI. |
| 4 | **Authentic green-screen aesthetics** | This is the whole point. The UI must feel like a real legacy IBM 3270 / AS/400 terminal — green-on-black ANSI colors, fixed-width box-drawing, blinking cursors if possible. If someone walks past the screen, they should think "that's an old banking system." |
| 5 | **Persistent local data** | Data survives across sessions using an embedded SQLite database. This makes the demo more realistic and lets an audience see that actions (from a human or agent) have lasting effects. |
| 6 | **Resettable** | A built-in "reset to factory defaults" option re-seeds the database to its original state. Demo can always start clean. |
| 7 | **Safe for demos** | No real PII, no network dependency, no destructive OS-level operations. |
| 8 | **No real PII — ever** | All names, addresses, phone numbers, and account numbers must be obviously fake. Use public-domain names (e.g., "John Smith", "Jane Doe"), 555-xxxx phone numbers, clearly fake addresses (e.g., "123 Main St, Anytown, USA"). **No SSNs** — not even fake ones. Customer lookup uses account numbers, names, and phone numbers only. |
| 9 | **Versioned and extensible** | The app must clearly identify its version and be structured so new features/screens can be added without reworking existing code. |

---

## 3. Target User Workflow (Demo Scenario)

A **banking associate** sits at their terminal and performs common daily tasks:

1. Log in to the system
2. Look up a customer account
3. View account details / balance
4. Perform basic account actions
5. Log out

The Copilot agent will be shown driving these steps through the terminal.

---

## 4. Demo Scenarios & Feature Set

Features are driven by **three core demo scenarios** — realistic workflows a banking associate would recognize. Each scenario maps to the terminal features it requires.

---

### Scenario 1: "Find and Freeze a Flagged Account"

> **Story:** A fraud alert comes in. The agent navigates the terminal to locate the customer, reviews suspicious transaction history, freezes the account, and adds an investigation note.

**Steps the agent performs:**
1. Log in as a teller
2. Navigate to Customer Lookup
3. Search by customer name
4. Select the customer from results
5. View account details — note the status is "Active"
6. View transaction history — see suspicious activity
7. Navigate to Account Maintenance
8. Change account status to "Frozen"
9. Add a note: "Account frozen per fraud alert — pending investigation"
10. Confirm the action

**Features required:**
- Login screen
- Customer Lookup (search by name, account #, phone number)
- Account Inquiry (type, balance, status, last activity)
- Transaction History (tabular list of recent transactions)
- Account Maintenance → Change Status
- Account Maintenance → Add Note

---

### Scenario 2: "Process a Customer Address Change"

> **Story:** A customer calls in to update their mailing address and phone number. The agent handles it through the terminal.

**Steps the agent performs:**
1. Log in as a teller
2. Navigate to Customer Lookup
3. Search by last-4 of phone number (555-xxxx)
4. Select the customer
5. View current contact information
6. Navigate to Account Maintenance → Update Contact Info
7. Update address and phone number
8. Confirm the changes
9. Add a note: "Address and phone updated per customer request"

**Features required:**
- Login screen
- Customer Lookup (search by phone number)
- Customer Detail view (shows current contact info)
- Account Maintenance → Update Contact Info (address, phone)
- Account Maintenance → Add Note

---

### Scenario 3: "Transfer Funds Between Accounts"

> **Story:** A customer requests a transfer from their savings to their checking account. The agent walks through the multi-step transfer flow.

**Steps the agent performs:**
1. Log in as a teller
2. Navigate to Customer Lookup
3. Search and select the customer
4. View their accounts — note source (Savings) and destination (Checking) account numbers and balances
5. Navigate to Fund Transfer
6. Enter source account number
7. Enter destination account number
8. Enter transfer amount
9. Review the confirmation screen (shows both accounts, amount, new projected balances)
10. Confirm the transfer
11. See success message with updated balances

**Features required:**
- Login screen
- Customer Lookup
- Account Inquiry (view multiple accounts for a customer)
- Fund Transfer (multi-step: source → dest → amount → confirm → result)

---

### 4.1 Consolidated Feature Map

Derived from the three scenarios above:

| Feature | Scenario 1 | Scenario 2 | Scenario 3 | Priority |
|---------|:----------:|:----------:|:----------:|----------|
| Login / Logout | ✓ | ✓ | ✓ | Must have |
| Main Menu | ✓ | ✓ | ✓ | Must have |
| Customer Lookup (name, acct#, phone) | ✓ | ✓ | ✓ | Must have |
| Customer Detail view | ✓ | ✓ | ✓ | Must have |
| Account Inquiry (balance, status) | ✓ | | ✓ | Must have |
| Transaction History | ✓ | | | Must have |
| Fund Transfer (multi-step) | | | ✓ | Must have |
| Change Account Status | ✓ | | | Must have |
| Update Contact Info | | ✓ | | Must have |
| Add Account Note | ✓ | ✓ | | Must have |
| System Reset (re-seed DB) | — | — | — | Must have |
| Version / About screen | — | — | — | Must have |

### 4.2 Main Menu (Updated)

```
╔══════════════════════════════════════════════════╗
║         COBOL BANKER — MAIN TERMINAL             ║
║           FIRST NATIONAL BANK  v1.0.0            ║
╠══════════════════════════════════════════════════╣
║  1. Customer Lookup                              ║
║  2. Account Inquiry                              ║
║  3. Fund Transfer                                ║
║  4. Account Maintenance                          ║
║  5. Transaction History                          ║
║  8. System Administration                        ║
║  9. Log Off                                      ║
╚══════════════════════════════════════════════════╝
║  Teller: J. Smith | Branch: Downtown | 02/20/26  ║
╚══════════════════════════════════════════════════╝
ENTER SELECTION: _
```

### 4.3 System Administration Submenu

```
╔══════════════════════════════════════════════════╗
║           SYSTEM ADMINISTRATION                  ║
╠══════════════════════════════════════════════════╣
║  1. Reset Database to Defaults                   ║
║  2. About / Version Info                         ║
║  0. Return to Main Menu                          ║
╚══════════════════════════════════════════════════╝
ENTER SELECTION: _
```

The "About" screen displays: app name, version number, build date, and schema version.

---

## 5. Architecture

### 5.1 Technology Choice

> **Decision: C# / .NET 8+ with Native AOT publishing**

| Aspect | Detail |
|--------|--------|
| **Language** | C# (.NET 8 or 9) |
| **Publish mode** | `dotnet publish -c Release -r win-x64 --self-contained /p:PublishAot=true` — produces a single native exe, no .NET runtime needed on target machine |
| **SQLite library** | `Microsoft.Data.Sqlite` (first-party, lightweight, works with AOT) |
| **Terminal rendering** | Raw `Console.Write` with ANSI escape codes — no TUI framework needed for menu-driven screens |
| **Project type** | Console application, single project, no solution file complexity |

**Why C# / .NET:**
- Familiar to enterprise demo audiences and the coworkers who'll run it
- AOT produces a true native exe — no runtime pre-install required
- `Microsoft.Data.Sqlite` is a first-party, well-supported SQLite wrapper
- `Console` class + ANSI escapes give us full green-screen control
- Strong tooling in VS Code with C# Dev Kit

**AOT considerations:**
- Some reflection-heavy libraries don't work with AOT — we'll avoid those
- `Microsoft.Data.Sqlite` with the `SQLitePCLRaw.bundle_e_sqlite3` provider works under AOT
- Binary size will be ~15-30 MB (acceptable for a demo tool)

### 5.2 Data Layer — SQLite

An embedded **SQLite** database is the persistence layer. Rationale:

| Requirement | How SQLite meets it |
|-------------|--------------------|
| Portable | Single `.db` file lives next to the exe — no server, no connection strings |
| Persistent | Data survives restarts; demo audience sees lasting effects of actions |
| Resettable | Drop all tables + re-seed, or just delete the file and let the app recreate it |
| Real-world feel | SQL under the hood makes the app feel like a real system, not a toy |
| Zero install | Every language above has SQLite support with no external dependencies |

**Behavior on startup:**
1. Look for `cobol-banker.db` in the same directory as the exe
2. If missing → create it, run schema migrations, seed sample data
3. If present → open it, verify schema version, run any pending migrations

**Why not Azure / cloud?**
Considered, but rejected for now:
- Adds a network dependency (demo fails if Wi-Fi is flaky)
- Adds deployment/provisioning complexity
- The core demo story is "agent interacts with a *local* legacy app" — cloud undermines that narrative
- If we later want a cloud variant (e.g., to show agent bridging local + cloud), we can add it as a separate mode without changing the core app

### 5.3 Application Layers (Keep It Flat)

```
┌─────────────────────────────────┐
│         Screen Renderer         │  ← Draws bordered text UI to stdout
├─────────────────────────────────┤
│         Input Handler           │  ← Reads stdin, routes to commands
├─────────────────────────────────┤
│         Business Logic          │  ← Lookup, transfer, maintenance
├─────────────────────────────────┤
│         Data Access (SQLite)    │  ← Queries, inserts, updates
├─────────────────────────────────┤
│         SQLite Database File    │  ← cobol-banker.db
└─────────────────────────────────┘
```

No frameworks, no DI containers, no ORMs. Just a handful of source files:

| File | Responsibility |
|------|---------------|
| `main` | Entry point, login flow, main menu loop |
| `screens` | Functions that render each screen (borders, tables, prompts) |
| `db` | Database connection, schema creation, migrations, seed data |
| `models` | Structs/types for Customer, Account, Transaction |
| `commands` | Business logic for each menu option |

### 5.4 Versioning Strategy

**App version** follows [Semantic Versioning](https://semver.org/): `MAJOR.MINOR.PATCH`

| Component | When to bump |
|-----------|--------------|
| MAJOR | Breaking changes to DB schema or fundamental UX flow |
| MINOR | New features / screens / demo scenarios added |
| PATCH | Bug fixes, seed data tweaks, cosmetic changes |

**Where the version lives:**

| Location | Purpose |
|----------|---------|
| `.csproj` → `<Version>` property | Single source of truth. Set once, flows everywhere. |
| Assembly metadata | Baked into the exe at compile time via `<Version>` |
| Login / Main Menu screen | Displayed to the user (e.g., `v1.0.0` in the header) |
| About screen | Full version detail: version, build date, schema version |
| `Schema_Version` table | Tracks DB schema version separately for migration logic |

**How to update the version:**
1. Change `<Version>` in the `.csproj` file — that's it
2. The app reads it at runtime via `Assembly.GetEntryAssembly().GetName().Version`
3. CI/build scripts can also set it via `/p:Version=x.y.z` on the `dotnet publish` command

**DB schema version** is separate from app version — tracked in the `Schema_Version` table. On startup, the app compares the DB schema version to what it expects and runs any pending migration scripts. This means a newer exe can upgrade an older `.db` file in place.

### 5.5 Extensibility

The project is structured so new features can be added without modifying existing code:

```
Adding a new screen / feature:
  1. Add a new command handler in commands/
  2. Add a new screen renderer in screens/
  3. Add a menu entry in the parent menu
  4. (If needed) Add a DB migration + update schema version
  5. Bump the minor version in .csproj
```

Key patterns that support this:
- **Each screen is a self-contained function** — renders output, reads input, returns a result
- **Menu entries are data-driven** — adding a menu item means adding to an array, not restructuring a switch statement
- **DB migrations are sequential scripts** — `migration_001.sql`, `migration_002.sql`, etc., run in order
- **No global state** — the DB connection and current teller session are passed explicitly

### 5.6 Navigation Model

- **Menu-driven with numbered choices** (no free-form commands)
- Each screen prints its content, then waits for input
- "Back" or "0" returns to the previous menu
- Invalid input re-prompts with an error line

### 5.7 Database Schema

```sql
-- Core tables
Customers
  customer_id    TEXT PRIMARY KEY   -- e.g., "CUST001"
  first_name     TEXT
  last_name      TEXT
  phone          TEXT               -- 555-xxxx format, obviously fake
  address        TEXT               -- e.g., "123 Main St, Anytown, USA"
  created_date   TEXT

Accounts
  account_number TEXT PRIMARY KEY   -- e.g., "1000234567"
  customer_id    TEXT FK → Customers
  account_type   TEXT               -- Checking / Savings / Money Market
  balance        REAL
  status         TEXT               -- Active / Frozen / Closed
  opened_date    TEXT

Transactions
  transaction_id INTEGER PRIMARY KEY AUTOINCREMENT
  account_number TEXT FK → Accounts
  date           TEXT
  description    TEXT
  amount         REAL               -- positive = credit, negative = debit
  running_balance REAL

-- Audit / notes
Account_Notes
  note_id        INTEGER PRIMARY KEY AUTOINCREMENT
  account_number TEXT FK → Accounts
  created_by     TEXT               -- teller username
  created_date   TEXT
  note_text      TEXT

-- Teller credentials (for simulated login)
Tellers
  username       TEXT PRIMARY KEY
  password       TEXT               -- plaintext, this is a demo
  display_name   TEXT
  branch         TEXT

-- Schema versioning
Schema_Version
  version        INTEGER
  applied_date   TEXT
```

**Seed data:** ~8-10 customers, 15-20 accounts, 50+ transactions, 2-3 teller logins. Enough to feel real without being overwhelming.

### 5.8 Distribution Strategy

> **Decision: Ship a pre-seeded database in the zip.**

The distribution artifact is a **zip file** containing:

```
cobol-banker/
  cobol-banker.exe      ← the binary
  cobol-banker.db        ← pre-seeded SQLite database with demo content
  README.txt             ← one-paragraph quick start
```

**Why pre-seeded, not auto-generated:**
- Coworkers unzip, double-click the exe, and immediately have a working demo with realistic data
- No "first run" setup delay or output noise — the terminal goes straight to the login screen
- Seed data can be carefully curated to tell a coherent story (e.g., customers with interesting account states, pending transfers, notes from prior sessions)
- The "reset" command inside the app restores the DB to this exact curated state

**Fallback behavior:** If the `.db` file is missing or corrupt, the app auto-creates a fresh one with the same seed data and prints a notice. This means the exe still works standalone — the pre-seeded DB is a convenience, not a hard dependency.

**Installer option (if needed later):** If the zip approach causes friction (e.g., people don't know where to extract it, or antivirus flags loose exes), we can wrap it in a lightweight installer (e.g., Inno Setup or WiX) that does one thing: extract files to `C:\COBOLBanker\` and create a Start Menu shortcut. Single "Install" button, no config screens.

---

## 6. What This App Is NOT

- Not a real banking system — no real security, no encryption, demo-grade data only
- Not a TUI framework showcase — no mouse, no scrolling, no colors beyond basic ANSI
- Not an API — the only interface is stdin/stdout in a terminal
- Not multi-user — single session, single terminal
- Not cloud-dependent — runs fully offline with local SQLite

---

## 7. Decisions Made

| # | Topic | Decision |
|---|-------|----------|
| D1 | **Data persistence** | SQLite embedded database, single `.db` file next to the exe |
| D2 | **Distribution** | Zip file with exe + pre-seeded `.db` + README. Installer only if zip causes friction. |
| D3 | **Seed data** | Ship pre-loaded with curated demo content. App can also self-seed if DB is missing. |
| D4 | **Cloud / Azure** | Not needed. Fully local. Door open for future cloud mode if compelling. |
| D5 | **Language / runtime** | C# / .NET 8+ with Native AOT publish. Single exe, no runtime required on target. |
| D6 | **Green-screen aesthetics** | Yes — ANSI green-on-black coloring, box-drawing characters, retro look is a core principle. |
| D7 | **Agent interaction model** | Pure legacy terminal via stdin/stdout. No special machine-readable mode. Designed for Windows 365 Agent "computer-do" style screen-reading interaction. |
| D8 | **Demo scenarios** | Three core scenarios: (1) Find & freeze flagged account, (2) Process address change, (3) Fund transfer. Feature set derived from these. |
| D9 | **Versioning** | Semantic versioning via `.csproj` `<Version>`. Displayed on main menu and About screen. DB schema versioned separately with migration support. |
| D10 | **Extensibility** | Menu-driven architecture, self-contained screen functions, data-driven menus, sequential DB migrations. New features = new files, not rewrites. |

## 8. Open Questions

*None at this time — ready to build v1.0.0.*

## 9. Version History

| Version | What's Included |
|---------|----------------|
| **v1.0.0** (planned) | Login, Customer Lookup, Account Inquiry, Transaction History, Fund Transfer, Account Maintenance (status, contact, notes), System Admin (reset, about). Supports all 3 demo scenarios. |

---

*This document is a living draft. Let's iterate until we're aligned, then start building.*
