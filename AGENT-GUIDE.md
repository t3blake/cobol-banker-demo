# AGENT-GUIDE.md — Computer-Use Agent Reference for COBOL Banker

> **Purpose:** This document is the authoritative reference for building a Windows 365 Agent (computer-use / "computer-do" style) that interacts with the COBOL Banker desktop application. It describes every screen, every input flow, every edge case, and every piece of seed data the agent will encounter.

---

## 1. Application Overview

**COBOL Banker** is a WPF desktop application that simulates a legacy green-screen banking terminal. It is installed locally on a Windows machine and launched via a desktop shortcut or exe. The app is a single-window GUI — not a real console — disguised to look like a mainframe terminal.

- **Exe name:** `cobol-banker.exe`
- **Window title:** `Woodgrove Bank — Terminal Emulator`
- **Install path (Intune):** `C:\Program Files\WoodgroveBank\cobol-banker.exe`
- **Desktop shortcut:** `C:\Users\Public\Desktop\Woodgrove Bank Terminal.lnk`
- **Database:** `cobol-banker.db` (SQLite, auto-created next to the exe on first run)

### Why This App Exists

This is a demo prop. The goal is to show how a Windows 365 Agent can operate a locally-installed legacy application — reading the screen, typing inputs, clicking when needed — just like a human would. The agent should be able to log in, look up customers, check balances, transfer funds, change account statuses, and navigate all menus.

---

## 2. Window Structure & Visual Recognition

### Layout

```
┌──────────────────────────────────────────────────────┐
│                                                      │
│   ╔══════════════════════════════════════════════╗    │  ← Output area
│   ║          COBOL BANKER — MAIN TERMINAL        ║    │    (ScrollViewer
│   ║        WOODGROVE BANK  v1.1.0                ║    │     + TextBlock)
│   ╠══════════════════════════════════════════════╣    │
│   ║  1. Customer Lookup                          ║    │
│   ║  2. Account Inquiry                          ║    │
│   ║  ...                                         ║    │
│   ╚══════════════════════════════════════════════╝    │
│                                                      │
├──────────────────────────────────────────────────────┤
│ ENTER SELECTION: [___________]                       │  ← Input panel
└──────────────────────────────────────────────────────┘    (appears when
                                                            input is needed)
```

### Visual Properties

| Property | Value |
|---|---|
| Window size | 920 × 720 px (default), minimum 720 × 500 |
| Background | Solid black (`#000000`) |
| Font | **Consolas 14pt** — monospaced, critical for layout |
| Primary text color | Green (`#33FF33`) |
| Bright/border color | Bright green (`#66FF66`) |
| Dim text color | Dim green (`#009900`) — used for "Press any key" |
| Error text color | Red (`#FF4444`) |
| Warning text color | Yellow (`#FFD700`) |
| Box-drawing chars | `╔ ╗ ╚ ╝ ═ ║ ╠ ╣` — Unicode, bright green |
| Content width | 60 fixed-width characters inside the box |

### Two Main UI Regions

1. **Output Area** (top, fills most of the window)
   - WPF `TextBlock` inside a `ScrollViewer`
   - All terminal output appears here as colored `Run` elements
   - Text is **never cleared from view** except when `Screen.Clear()` is called (which wipes all `Inlines`)
   - Auto-scrolls to bottom on new content
   - Has a thin `#336633` border

2. **Input Panel** (bottom, collapsible)
   - Only visible when the app is waiting for user input
   - Contains a prompt label (e.g., `ENTER SELECTION: `) and a `TextBox`
   - The `TextBox` has green text on black background, blending with the terminal aesthetic
   - Submit input by pressing **Enter**
   - Panel disappears after input is submitted

### How to Detect "Waiting for Input" State

The agent must distinguish between two states:

| State | Visual Cue | Agent Action |
|---|---|---|
| **Waiting for text input** | Input panel visible at bottom with a prompt label and blinking green cursor | Type text into the input box, press Enter |
| **Waiting for "any key"** | Text reads `Press any key to continue...` in dim green; input panel is **hidden** | Press any key (Space, Enter, etc.) or click anywhere in the window |

> **Critical:** When the "Press any key" prompt is shown, the input panel is collapsed. The agent must send a keypress to the window itself, not to a textbox.

---

## 3. Application Flow — Every Screen, Step by Step

### 3.1 Login Screen

**Trigger:** App launch (automatic first screen)

**Screen content:**
```
╔══════════════════════════════════════════════════════════╗
║                      COBOL BANKER                        ║
║                    WOODGROVE BANK                        ║
║                       v1.1.0                             ║
╠══════════════════════════════════════════════════════════╣
║                                                          ║
║  TELLER AUTHENTICATION REQUIRED                          ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝
```

**Input sequence:**
1. Prompt: `USERID: ` → Agent types username, presses Enter
2. Prompt: `PASSWORD: ` → Agent types password, presses Enter (characters display as `*`)

**Outcomes:**
- **Success:** Shows `>>> ACCESS GRANTED <<<` in bright green, then teller name, branch, login time. Then `Press any key to continue...`
- **Failure:** Shows `*** INVALID CREDENTIALS (ATTEMPT X/3) ***` in red. Then `Press any key to continue...`
- **3 failures:** Shows `*** MAXIMUM ATTEMPTS EXCEEDED - TERMINAL LOCKED ***`, then app exits after keypress

**Teller credentials:**

| Username | Password | Display Name | Branch |
|---|---|---|---|
| `teller1` | `pass123` | J. Smith | Downtown |
| `teller2` | `pass123` | M. Johnson | Westside |
| `admin` | `admin` | Admin User | HQ |

> **Recommendation for agent:** Use `teller1` / `pass123` for normal operations.

---

### 3.2 Main Menu

**Trigger:** After successful login (and after returning from any command)

**Screen content:**
```
╔══════════════════════════════════════════════════════════╗
║             COBOL BANKER — MAIN TERMINAL                 ║
║             WOODGROVE BANK  v1.1.0                       ║
╠══════════════════════════════════════════════════════════╣
║                                                          ║
║  1. Customer Lookup                                      ║
║  2. Account Inquiry                                      ║
║  3. Fund Transfer                                        ║
║  4. Account Maintenance                                  ║
║  5. Transaction History                                  ║
║                                                          ║
║  8. System Administration                                ║
║  9. Log Off                                              ║
║                                                          ║
╠══════════════════════════════════════════════════════════╣
║  Teller: J. Smith | Branch: Downtown | 02/21/26          ║
╚══════════════════════════════════════════════════════════╝

```

**Input:** `ENTER SELECTION: ` → Agent types a single digit (`1`–`5`, `8`, or `9`), presses Enter

**Menu map:**
| Input | Destination |
|---|---|
| `1` | Customer Lookup |
| `2` | Account Inquiry |
| `3` | Fund Transfer |
| `4` | Account Maintenance |
| `5` | Transaction History |
| `8` | System Administration |
| `9` | Log Off (returns to login screen) |
| `6`, `7` | Shows `*** FUNCTION NOT AVAILABLE ***` |

---

### 3.3 Customer Lookup (Menu 1)

**Purpose:** Search for customers by name, account number, phone, or customer ID.

**Screen header:** `CUSTOMER LOOKUP`

**Input:** `SEARCH: ` → Agent types a search term, presses Enter

**Search types that work:**
- Full or partial name: `Smith`, `Jane`, `Doe`
- Customer ID: `CUST001`
- Account number: `1000100001`
- Phone number: `555-0101`
- Enter `0` to return to Main Menu

**Results screen:**
- Shows numbered list of matching customers with CUST ID, NAME, PHONE
- Prompt: `SELECT CUSTOMER # (0=Back): ` → Type row number to select, or `0`

**Customer detail screen (after selection):**
- Shows customer ID, full name, phone, address, member-since date
- Lists all accounts with account number, type, balance, and status flags
- Then `Press any key to continue...`

---

### 3.4 Account Inquiry (Menu 2)

**Purpose:** View detailed account information by account number or customer ID.

**Screen header:** `ACCOUNT INQUIRY`

**Input:** `ACCOUNT/CUSTOMER ID: ` → Agent types an account number or customer ID, presses Enter

**Behaviors:**
- If **account number** is entered → shows account detail directly
- If **customer ID** is entered and customer has **1 account** → shows that account detail
- If **customer ID** is entered and customer has **multiple accounts** → shows a numbered list, agent picks one
- Enter `0` to return to Main Menu

**Account detail screen shows:**
- Account number, type, status, balance, opened date
- Owner name and phone
- **Last 5 transactions** in a table (date, description, amount, running balance)
- Account notes (if any exist)

---

### 3.5 Fund Transfer (Menu 3)

**Purpose:** Transfer money between two accounts. Multi-step flow with confirmation.

**Screen header:** `FUND TRANSFER`

**Step-by-step flow:**

| Step | Prompt | Agent Action | Notes |
|---|---|---|---|
| 1 | `SOURCE ACCOUNT NUMBER: ` | Type source account number | Must be Active status |
| — | (Shows source account info) | Read to verify | |
| 2 | `DESTINATION ACCOUNT NUMBER: ` | Type destination account number | Must be different from source, must be Active |
| — | (Shows destination account info) | Read to verify | |
| 3 | `TRANSFER AMOUNT: ` | Type dollar amount (e.g., `500` or `500.00`) | Cannot exceed source balance. Leading `$` is stripped automatically |
| — | Shows confirmation screen | Review FROM/TO/amounts | |
| 4 | `CONFIRM TRANSFER (Y/N): ` | Type `Y` or `N` | Case-insensitive |

**Success output:** `>>> TRANSFER COMPLETED SUCCESSFULLY <<<` + updated balances for both accounts

**Error conditions the agent may encounter:**
- `*** SOURCE ACCOUNT NOT FOUND ***`
- `*** SOURCE ACCOUNT IS FROZEN - TRANSFER NOT ALLOWED ***`
- `*** DESTINATION ACCOUNT NOT FOUND ***`
- `*** SOURCE AND DESTINATION CANNOT BE THE SAME ***`
- `*** INSUFFICIENT FUNDS IN SOURCE ACCOUNT ***`
- `*** INVALID AMOUNT - MUST BE A POSITIVE NUMBER ***`

> Enter `0` at any prompt to cancel and return to Main Menu.

---

### 3.6 Account Maintenance (Menu 4)

**Screen header:** `ACCOUNT MAINTENANCE`

**Sub-menu:**
| Input | Action |
|---|---|
| `1` | Change Account Status |
| `2` | Update Customer Contact Info |
| `3` | Add Account Note |
| `0` | Return to Main Menu |

#### 3.6.1 Change Account Status

| Step | Prompt | Agent Action |
|---|---|---|
| 1 | `ACCOUNT NUMBER: ` | Type account number |
| — | Shows current account info | Read status |
| 2 | `NEW STATUS: ` | Type `1` (Active), `2` (Frozen), or `3` (Closed) |
| 3 | `CHANGE STATUS FROM X TO Y (Y/N): ` | Type `Y` to confirm |

**Success:** `>>> ACCOUNT STATUS CHANGED TO [STATUS] <<<`  
A note is automatically added to the account recording the change.

#### 3.6.2 Update Customer Contact Info

| Step | Prompt | Agent Action |
|---|---|---|
| 1 | `CUSTOMER ID: ` | Type customer ID or search term |
| — | Shows current phone/address | Read values |
| 2 | `PHONE [current]: ` | Type new phone or press Enter to keep |
| 3 | `ADDRESS [current]: ` | Type new address or press Enter to keep |
| 4 | `CONFIRM CHANGES (Y/N): ` | Type `Y` to confirm |

**Success:** `>>> CUSTOMER CONTACT INFO UPDATED <<<`  
Notes added to all active accounts.

#### 3.6.3 Add Account Note

| Step | Prompt | Agent Action |
|---|---|---|
| 1 | `ACCOUNT NUMBER: ` | Type account number |
| — | Shows account info and existing notes | Read context |
| 2 | `NOTE TEXT: ` | Type the note content |
| 3 | `SAVE NOTE (Y/N): ` | Type `Y` to confirm |

**Success:** `>>> NOTE SAVED SUCCESSFULLY <<<`

---

### 3.7 Transaction History (Menu 5)

**Screen header:** `TRANSACTION HISTORY`

**Input:** `ACCOUNT NUMBER: ` → Type account number, press Enter (or `0` to go back)

**Output:** Shows up to 25 most recent transactions in a table:
```
  DATE        DESCRIPTION                  AMOUNT      BALANCE
  ----------  --------------------------  ----------  ----------
  2026-02-15  DIRECT DEPOSIT - EMPLOYER   +3,200.00   $4,523.67
  2026-02-14  DEBIT CARD - RESTAURANT       -62.30    $1,323.67
  ...
```

Also shows account owner, type, status, and current balance above the table.

---

### 3.8 System Administration (Menu 8)

**Screen header:** `SYSTEM ADMINISTRATION`

**Sub-menu:**
| Input | Action |
|---|---|
| `1` | Reset Database to Defaults |
| `2` | About / Version Info |
| `0` | Return to Main Menu |

#### Reset Database
- **Double confirmation** required (two `Y/N` prompts)
- Drops all tables and re-seeds everything to original demo state
- Useful for resetting after demo runs

#### About / Version Info
- Shows app version, database schema version, .NET runtime, OS info
- Read-only; press any key to return

---

## 4. Seed Data Reference

### 4.1 Customers

| Customer ID | Name | Phone | Address | Since |
|---|---|---|---|---|
| CUST001 | John Smith | 555-0101 | 123 Main St, Anytown, USA 00001 | 2020-03-15 |
| CUST002 | Jane Doe | 555-0102 | 456 Oak Ave, Anytown, USA 00002 | 2019-07-22 |
| CUST003 | Robert Jones | 555-0103 | 789 Pine Rd, Springfield, USA 00003 | 2021-01-10 |
| CUST004 | Mary Williams | 555-0104 | 321 Elm St, Shelbyville, USA 00004 | 2018-11-05 |
| CUST005 | James Brown | 555-0105 | 654 Maple Dr, Anytown, USA 00005 | 2022-06-18 |
| CUST006 | Patricia Davis | 555-0106 | 987 Cedar Ln, Springfield, USA 00006 | 2020-09-30 |
| CUST007 | Michael Miller | 555-0107 | 147 Birch Ct, Shelbyville, USA 00007 | 2017-04-12 |
| CUST008 | Linda Wilson | 555-0108 | 258 Walnut Way, Anytown, USA 00008 | 2023-02-28 |

### 4.2 Accounts

| Account # | Customer | Type | Balance | Status |
|---|---|---|---|---|
| 1000100001 | John Smith (CUST001) | Checking | $4,523.67 | Active |
| 1000100002 | John Smith (CUST001) | Savings | $12,750.00 | Active |
| 1000200001 | Jane Doe (CUST002) | Checking | $1,893.45 | Active |
| 1000200002 | Jane Doe (CUST002) | Savings | $45,200.00 | Active |
| 1000300001 | Robert Jones (CUST003) | Checking | $8,734.12 | Active |
| 1000300002 | Robert Jones (CUST003) | Savings | $25,000.00 | Active |
| 1000300003 | Robert Jones (CUST003) | Money Market | $150,000.00 | Active |
| 1000400001 | Mary Williams (CUST004) | Checking | $2,341.89 | Active |
| 1000400002 | Mary Williams (CUST004) | Savings | $8,900.00 | Active |
| 1000500001 | James Brown (CUST005) | Checking | $15,678.34 | Active |
| 1000600001 | Patricia Davis (CUST006) | Checking | $3,421.56 | Active |
| 1000600002 | Patricia Davis (CUST006) | Savings | $67,500.00 | Active |
| 1000700001 | Michael Miller (CUST007) | Checking | $11,234.78 | Active |
| 1000700002 | Michael Miller (CUST007) | Savings | $5,600.00 | **Closed** |
| 1000800001 | Linda Wilson (CUST008) | Checking | $920.15 | Active |
| 1000800002 | Linda Wilson (CUST008) | Savings | $3,200.00 | Active |

### 4.3 Accounts with Notable Characteristics

| Account | What's Interesting | Demo Use |
|---|---|---|
| 1000200002 (Jane Doe Savings) | Transaction history shows multiple wire transfers from "UNKNOWN ORIGIN" and "OFFSHORE ACCT" — structuring pattern under $10K | Fraud investigation demo |
| 1000400001 (Mary Williams Checking) | Cash deposits just under $5K followed by wire transfers out — classic structuring | Suspicious activity demo |
| 1000700002 (Michael Miller Savings) | Status is **Closed** | Test frozen/closed account handling |
| 1000300003 (Robert Jones Money Market) | $150K balance | High-value account demo |

### 4.4 Existing Account Notes

| Account | Note |
|---|---|
| 1000200002 | "Customer called - verified wire transfer from family overseas. No flag needed at this time." (by teller2) |
| 1000700002 | "Account closed per customer request. Remaining balance transferred to checking." (by teller1) |
| 1000400001 | "Unusual cash deposit pattern noted. Flagged for compliance review." (by teller2) |

---

## 5. Input Patterns & Keyboard Interaction

### General Rules

| Action | How |
|---|---|
| Submit text input | Type into the input box, press **Enter** |
| Dismiss "Press any key" | Press **any key** (Space, Enter, etc.) or **click** anywhere in the window |
| Navigate back | Type `0` or `back` at most prompts |
| Cancel multi-step flow | Type `0` at any prompt in the flow |
| Confirm action | Type `Y` (case-insensitive) |
| Deny action | Type `N` (or anything other than `Y`) |

### Input Box Behavior

- The input box appears **only** when the app needs text input
- It always receives keyboard focus automatically
- The prompt label (e.g., `SEARCH: `) appears to the left of the text box
- After pressing Enter, the prompt + typed text are echoed into the output area and the input box disappears
- **Password fields** show `*` characters instead of the actual text

### Timing Considerations

- Screen transitions are nearly instantaneous (no network calls, local SQLite)
- The only delay is rendering — typically <100ms
- The agent should wait until the input panel becomes visible before typing
- After pressing Enter, wait for the next screen state before acting

---

## 6. Recommended Agent Scenarios

These are the three demo scenarios the app was designed to support. They exercise the full breadth of the terminal.

### Scenario A: Customer Service Workflow

> "A customer calls to check their balance and transfer funds."

**Steps:**
1. Login as `teller1` / `pass123`
2. From Main Menu, select `1` (Customer Lookup)
3. Search for `John Smith`
4. Select customer #1 from results
5. Note the accounts and balances on the customer detail screen
6. Press any key, then `0` to return to Main Menu
7. Select `3` (Fund Transfer)
8. Source account: `1000100001` (John Smith Checking, $4,523.67)
9. Destination account: `1000100002` (John Smith Savings, $12,750.00)
10. Amount: `500`
11. Confirm: `Y`
12. Verify success message and updated balances

### Scenario B: Fraud Investigation

> "Compliance flagged an account for suspicious wire activity."

**Steps:**
1. Login as `teller1` / `pass123`
2. Select `5` (Transaction History)
3. Enter account `1000200002` (Jane Doe Savings)
4. Observe multiple "WIRE TRANSFER - UNKNOWN ORIGIN" entries, all just under $10K
5. Press any key, then back to Main Menu
6. Select `4` (Account Maintenance) → `1` (Change Account Status)
7. Enter account `1000200002`
8. Change status to `2` (Frozen)
9. Confirm: `Y`
10. Verify `>>> ACCOUNT STATUS CHANGED TO FROZEN <<<`
11. Return to Account Maintenance → `3` (Add Account Note)
12. Enter account `1000200002`
13. Note: `Multiple structured wire transfers detected. Account frozen pending investigation.`
14. Confirm: `Y`

### Scenario C: Address Update + Multi-Account Review

> "A customer moved and needs their contact info updated."

**Steps:**
1. Login as `teller1` / `pass123`
2. Select `4` (Account Maintenance) → `2` (Update Customer Contact Info)
3. Enter `CUST003` (Robert Jones)
4. New phone: `555-9999`
5. New address: `100 New Boulevard, Anytown, USA 00010`
6. Confirm: `Y`
7. Return to Main Menu → `2` (Account Inquiry)
8. Enter `CUST003` — see 3 accounts listed
9. Select account `1` to view the checking detail
10. Verify the notes reflect the contact info change

---

## 7. Agent Implementation Tips

### Visual Recognition Strategy

The app renders all text as WPF `TextBlock.Inlines` (colored `Run` elements). For screen-reading:

- **Look for box-drawing characters** (`╔`, `═`, `║`) to identify screen headers/sections
- **Look for the prompt pattern** `LABEL: ` followed by a text input — this means the app is waiting for input
- **Look for `Press any key to continue...`** in dim green — this means the app is waiting for any keypress
- **Look for `>>>` markers** for success, `***` for errors, `!!!` for warnings
- **Numbered lists** (e.g., `1. Customer Lookup`) indicate menu choices — the agent types the number

### Error Recovery

| Error Message Pattern | Meaning | Recovery |
|---|---|---|
| `*** INVALID CREDENTIALS ***` | Wrong username/password | Retry (up to 3 attempts) |
| `*** INVALID SELECTION - ENTER X-Y ***` | Typed a number outside range | Re-enter a valid number |
| `*** ACCOUNT NOT FOUND ***` | Typed wrong account number | Re-enter correct number |
| `*** INSUFFICIENT FUNDS ***` | Transfer amount > balance | Enter smaller amount |
| `*** ACCOUNT IS FROZEN ***` | Can't transfer from/to frozen account | Use Account Maintenance to reactivate first |
| `!!! TRANSFER CANCELLED !!!` | Agent typed `N` at confirm | Not an error; retry the flow if transfer was intended |

After every error, the app shows `Press any key to continue...` — the agent must press a key before the flow continues.

### Database Reset

If a demo has been run and data has been modified, the agent (or human) can reset to pristine state:
1. Main Menu → `8` (System Administration)
2. `1` (Reset Database to Defaults)
3. Confirm twice with `Y`

This restores all seed data to the exact values documented in Section 4.

### Window Focus

- The app window title is always `Woodgrove Bank — Terminal Emulator`
- The exe process name is `cobol-banker`
- If the window loses focus, the agent needs to bring it back to foreground before typing
- The input box auto-receives keyboard focus when it appears, so clicking the window is sufficient to restore input capability

---

## 8. Technical Architecture (For Agent Developers)

### How the App Works Internally

```
┌──────────────────┐       ┌───────────────────┐       ┌──────────────┐
│  Background       │       │  TerminalService   │       │  WPF Window  │
│  Thread           │──────▶│  (Static Bridge)   │──────▶│  (UI Thread) │
│                   │       │                    │       │              │
│  LoginCommand     │  Write│  WriteAction       │Invoke │  OutputText  │
│  MainMenuCommand  │  ────▶│  ClearAction       │──────▶│  .Inlines    │
│  CustomerLookup   │       │  ReadLineFunc      │       │              │
│  AccountInquiry   │  Read │  ReadPasswordFunc  │  TCS  │  InputPanel  │
│  FundTransfer     │  ◀────│  ReadKeyAction     │◀──────│  InputBox    │
│  AccountMaint     │       │                    │       │              │
│  TransactionHist  │       └───────────────────┘       └──────────────┘
│  SystemAdmin      │
└──────────────────┘
```

- All business logic runs on a **background thread** — the UI never blocks
- Input is collected via `TaskCompletionSource` — the background thread blocks on `.Task.Result` while WPF collects input
- `Screen.Clear()` wipes all output `Inlines` — the agent will see a blank screen briefly before new content renders
- The app is **stateful and sequential** — there is always exactly one active screen, one prompt, one expected input at a time

### Key Files

| File | Purpose |
|---|---|
| `src/MainWindow.xaml` | Window layout, font/color settings |
| `src/MainWindow.xaml.cs` | UI callbacks, input handling, thread bridge |
| `src/Terminal/TerminalService.cs` | Static delegates connecting threads |
| `src/Screens/Screen.cs` | All rendering: boxes, headers, tables, prompts |
| `src/Commands/*.cs` | 8 command files, one per major function |
| `src/Data/Database.cs` | SQLite schema, seed data, all CRUD queries |
| `src/Models/*.cs` | Customer, Account, Transaction, AccountNote, Teller |

### Building & Running

```powershell
# Add .NET SDK to path (if installed to user profile)
$env:Path = "$env:USERPROFILE\.dotnet;$env:Path"

# Run in development
cd src
dotnet run

# Publish self-contained exe
dotnet publish -c Release -r win-x64 --self-contained /p:PublishSingleFile=true -o ..\dist

# Published exe
..\dist\cobol-banker.exe
```

---

## 9. Screen State Machine

The full app state machine for the agent to track:

```
[App Launch]
    │
    ▼
[LOGIN] ──(3 failures)──▶ [EXIT]
    │
    ▼ (success)
[MAIN MENU] ◀──────────────────────────────────────────┐
    │                                                   │
    ├──1──▶ [CUSTOMER LOOKUP] ──▶ [CUSTOMER DETAIL] ───┘
    │            │                                      │
    │            └──0──────────────────────────────────┘
    │                                                   │
    ├──2──▶ [ACCOUNT INQUIRY] ──▶ [ACCOUNT DETAIL] ────┘
    │            │                                      │
    │            └──0──────────────────────────────────┘
    │                                                   │
    ├──3──▶ [FUND TRANSFER] (multi-step) ──────────────┘
    │            │                                      │
    │            └──0 (cancel at any step)─────────────┘
    │                                                   │
    ├──4──▶ [ACCOUNT MAINTENANCE] ◀────────────────────┐
    │        ├──1──▶ [CHANGE STATUS] ──────────────────┘│
    │        ├──2──▶ [UPDATE CONTACT] ─────────────────┘│
    │        ├──3──▶ [ADD NOTE] ───────────────────────┘│
    │        └──0──────────────────────────────────────┘
    │                                                   │
    ├──5──▶ [TRANSACTION HISTORY] ─────────────────────┘
    │            │                                      │
    │            └──0──────────────────────────────────┘
    │                                                   │
    ├──8──▶ [SYSTEM ADMIN] ◀───────────────────────────┐
    │        ├──1──▶ [RESET DB] (2 confirms) ──────────┘│
    │        ├──2──▶ [ABOUT] ──────────────────────────┘│
    │        └──0──────────────────────────────────────┘
    │                                                   │
    └──9──▶ [LOG OFF] ──▶ [LOGIN]
```

Every leaf node either loops back to its parent (via `0` or after completion) or returns to Main Menu. There are no dead ends.

---

## 10. Quick-Reference Cheat Sheet

```
LOGIN:          teller1 / pass123
MAIN MENU:      1-5, 8, 9
GO BACK:        0 (or "back")
CONFIRM:        Y
DENY:           N
PRESS ANY KEY:  Space / Enter / any key / mouse click

GOOD TEST CUSTOMER:   John Smith (CUST001) — normal, 2 accounts
FRAUD CUSTOMER:       Jane Doe (CUST002) — suspicious wires on savings
SUSPICIOUS CUSTOMER:  Mary Williams (CUST004) — structuring pattern
CLOSED ACCOUNT:       1000700002 (Michael Miller Savings)
HIGH-VALUE ACCOUNT:   1000300003 (Robert Jones Money Market, $150K)
NEWEST CUSTOMER:      Linda Wilson (CUST008) — low balance

RESET EVERYTHING:     Menu 8 → 1 → Y → Y
```
