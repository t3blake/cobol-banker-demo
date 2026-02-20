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
| 2 | **Single self-contained binary** | No installers, no runtimes, no config files required. Copy the exe → run it. |
| 3 | **Deterministic, text-based UI** | All interaction is through text input/output so a Copilot agent (via terminal) can read and drive it. No mouse, no GUI. |
| 4 | **Look the part** | The UI should feel like an old IBM 3270 / AS/400 terminal — fixed-width layouts, bordered screens, menu-driven navigation. |
| 5 | **In-memory data only** | All data lives in memory for the session. No database, no files. Pre-seeded with sample data on startup. |
| 6 | **Safe for demos** | No real PII, no network calls, no destructive operations. Everything resets on restart. |

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

## 4. Proposed Feature Set

### 4.1 Authentication (Simulated)

- Simple username/password prompt at launch
- Hardcoded valid credentials (e.g., `teller1` / `pass123`)
- Displays associate name and branch after login

### 4.2 Main Menu

A numbered menu like:

```
╔══════════════════════════════════════════╗
║      COBOL BANKER — MAIN TERMINAL       ║
║         FIRST NATIONAL BANK             ║
╠══════════════════════════════════════════╣
║  1. Customer Lookup                     ║
║  2. Account Inquiry                     ║
║  3. Fund Transfer                       ║
║  4. Account Maintenance                 ║
║  5. Transaction History                 ║
║  6. Log Off                             ║
╚══════════════════════════════════════════╝
ENTER SELECTION: _
```

### 4.3 Customer Lookup

- Search by account number, name, or SSN (last 4)
- Display matching customer(s) in a table
- Select a customer to view details

### 4.4 Account Inquiry

- Show account type, balance, status, last activity date
- Pre-seeded with 5–10 sample customer accounts

### 4.5 Fund Transfer

- Transfer between two accounts (same customer or cross-customer)
- Prompt for source, destination, amount
- Show confirmation screen with details before executing
- Display success/failure message

### 4.6 Account Maintenance

- Change account status (active / frozen / closed)
- Update customer contact info (phone, address)
- Add a note to the account

### 4.7 Transaction History

- Show last N transactions for an account in a table
- Pre-seeded with sample transaction data

---

## 5. Architecture

### 5.1 Technology Choice

> **Decision needed:** Which language/framework to use for the exe?

| Option | Pros | Cons |
|--------|------|------|
| **Go** | Single binary, fast compile, easy cross-compile, good terminal libs (`tcell`, `bubbletea`) | Less common in enterprise demos |
| **C# (.NET AOT)** | Familiar to enterprise audiences, AOT produces single exe | Larger binary, AOT can be finicky |
| **Rust** | Single binary, fast, strong terminal libs (`ratatui`) | Steeper learning curve |
| **Python + PyInstaller** | Fast to develop, familiar | Larger binary, slower startup, feels less "real" |

### 5.2 Application Layers (Keep It Flat)

```
┌─────────────────────────────────┐
│         Screen Renderer         │  ← Draws bordered text UI to stdout
├─────────────────────────────────┤
│         Input Handler           │  ← Reads stdin, routes to commands
├─────────────────────────────────┤
│         Business Logic          │  ← Lookup, transfer, maintenance
├─────────────────────────────────┤
│         In-Memory Data Store    │  ← Seed data, structs/maps
└─────────────────────────────────┘
```

No frameworks, no DI containers, no ORMs. Just a handful of source files:

| File | Responsibility |
|------|---------------|
| `main` | Entry point, login flow, main menu loop |
| `screens` | Functions that render each screen (borders, tables, prompts) |
| `data` | Seed data definitions and in-memory store |
| `models` | Structs/types for Customer, Account, Transaction |
| `commands` | Business logic for each menu option |

### 5.3 Navigation Model

- **Menu-driven with numbered choices** (no free-form commands)
- Each screen prints its content, then waits for input
- "Back" or "0" returns to the previous menu
- Invalid input re-prompts with an error line

### 5.4 Data Model (In-Memory)

```
Customer
  ├── ID (string, e.g., "CUST001")
  ├── First Name
  ├── Last Name
  ├── SSN (masked, last 4 stored)
  ├── Phone
  ├── Address
  └── Accounts[]

Account
  ├── Account Number (string, e.g., "1000234567")
  ├── Type (Checking / Savings / Money Market)
  ├── Balance (decimal)
  ├── Status (Active / Frozen / Closed)
  ├── Opened Date
  └── Transactions[]

Transaction
  ├── Date
  ├── Description
  ├── Amount (+/-)
  └── Running Balance
```

---

## 6. What This App Is NOT

- Not a real banking system — no security, no persistence
- Not a TUI framework showcase — no mouse, no scrolling, no colors beyond basic ANSI
- Not an API — the only interface is stdin/stdout in a terminal
- Not multi-user — single session, single terminal

---

## 7. Open Questions

1. **Language choice** — Which option from 5.1 do you prefer?
2. **Color / ANSI** — Should we use green-on-black ANSI coloring to really sell the "green screen" look, or keep it plain text for maximum agent compatibility?
3. **Scope** — Is the feature set above the right size, or should we trim/add anything?
4. **Agent interaction model** — Should the app have any special affordances for agent interaction (e.g., a machine-readable mode), or should the agent just read the terminal output as-is?

---

*This document is a living draft. Let's iterate until we're aligned, then start building.*
