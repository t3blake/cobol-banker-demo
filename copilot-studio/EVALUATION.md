# COBOL Banker — Evaluation Test Plan

> **Usage:** Use these test cases in Copilot Studio's **Evaluation** feature to validate agent behavior after changing instructions, knowledge, or CUA tool settings. Each test has an input prompt, the expected agent behavior, and pass/fail criteria.

---

## How to Use

1. In Copilot Studio, go to **Evaluate** (left nav) → **+ New evaluation**.
2. Add test cases using the **Input** and **Expected output** columns below.
3. Run the evaluation against your agent.
4. Review results — each test should meet the pass criteria described.

> **Important:** Reset the database (Menu 8 → 1 → Y → Y) before running evaluations to ensure consistent seed data.

---

## Test Suite 1: Application Launch & Login

### Test 1.1 — Launch the app

| Field | Value |
|-------|-------|
| **Input** | Open the banking application. |
| **Expected behavior** | Agent uses Win+R → `C:\WoodgroveBank\cobol-banker.exe`, waits for the window titled "Woodgrove Bank — Terminal Emulator" to appear. |
| **Pass criteria** | App window is visible. Agent reports that the application is open. |

### Test 1.2 — Auto-login with default credentials

| Field | Value |
|-------|-------|
| **Input** | Log in to the system. |
| **Expected behavior** | Agent enters `teller1` at USERID prompt, `pass123` at PASSWORD prompt, dismisses "Press any key" screen. |
| **Pass criteria** | Agent reaches Main Menu and reports "ACCESS GRANTED" or similar confirmation. |

### Test 1.3 — Login with bad credentials

| Field | Value |
|-------|-------|
| **Input** | Log in with username `baduser` and password `wrong`. |
| **Expected behavior** | Agent enters the credentials, sees "INVALID CREDENTIALS" error, reports the failure to the user. |
| **Pass criteria** | Agent reads the error and reports it — does NOT silently retry with default creds. |

---

## Test Suite 2: Customer Lookup

### Test 2.1 — Lookup by name

| Field | Value |
|-------|-------|
| **Input** | Look up customer Jane Doe. |
| **Expected behavior** | Menu 1 → search "Jane Doe" → select result → read Customer Detail screen. |
| **Pass criteria** | Agent reports: Jane Doe, CUST002, 555-0102, 456 Oak Ave, 2 accounts (Checking + Savings) with balances. |

### Test 2.2 — Lookup by phone

| Field | Value |
|-------|-------|
| **Input** | Find the customer with phone number 555-0103. |
| **Expected behavior** | Menu 1 → search "555-0103" → select result → read detail. |
| **Pass criteria** | Agent identifies Robert Jones (CUST003) and reports his info including 3 accounts. |

### Test 2.3 — Lookup with no results

| Field | Value |
|-------|-------|
| **Input** | Look up a customer named "Mickey Mouse". |
| **Expected behavior** | Menu 1 → search "Mickey Mouse" → no results. |
| **Pass criteria** | Agent reports that no matching customer was found. Does not hallucinate data. |

---

## Test Suite 3: Account Inquiry

### Test 3.1 — Check a balance

| Field | Value |
|-------|-------|
| **Input** | What's the balance on account 1000100001? |
| **Expected behavior** | Menu 2 → enter `1000100001` → read account detail. |
| **Pass criteria** | Agent reports: Checking, John Smith, $4,523.67, Active status. Numbers match exactly. |

### Test 3.2 — Inquire on a closed account

| Field | Value |
|-------|-------|
| **Input** | Look up account 1000700002. |
| **Expected behavior** | Menu 2 → enter `1000700002` → read detail. |
| **Pass criteria** | Agent reports the account is **Closed** and shows the existing note about closure. |

---

## Test Suite 4: Fund Transfer

### Test 4.1 — Standard transfer

| Field | Value |
|-------|-------|
| **Input** | Transfer $2,000 from John Smith's savings (1000100002) to his checking (1000100001). |
| **Expected behavior** | Menu 3 → source `1000100002` → dest `1000100001` → amount `2000` → confirm `Y`. |
| **Pass criteria** | Agent reports "TRANSFER COMPLETED SUCCESSFULLY" with updated balances: Savings $10,750.00, Checking $6,523.67. |

### Test 4.2 — Transfer from frozen account (should fail)

| Field | Value |
|-------|-------|
| **Input** | First freeze account 1000200002, then try to transfer $100 from it to 1000200001. |
| **Expected behavior** | Menu 4 → freeze `1000200002` → then Menu 3 → attempt transfer → app shows error "ACCOUNT IS FROZEN". |
| **Pass criteria** | Agent reports the freeze succeeded, then reports the transfer failed due to account being frozen. Does NOT silently unfreeze. |

### Test 4.3 — Insufficient funds

| Field | Value |
|-------|-------|
| **Input** | Transfer $999,999 from account 1000800001 to 1000800002. |
| **Expected behavior** | Menu 3 → source `1000800001` → dest `1000800002` → amount `999999` → app shows "INSUFFICIENT FUNDS". |
| **Pass criteria** | Agent reports the error with the current balance ($920.15). Does not retry or reduce the amount without asking. |

---

## Test Suite 5: Account Maintenance

### Test 5.1 — Freeze an account

| Field | Value |
|-------|-------|
| **Input** | Freeze Jane Doe's savings account 1000200002. |
| **Expected behavior** | Menu 4 → option 1 → enter `1000200002` → select Frozen (2) → confirm Y. |
| **Pass criteria** | Agent reports "ACCOUNT STATUS CHANGED TO FROZEN". |

### Test 5.2 — Unfreeze an account

| Field | Value |
|-------|-------|
| **Input** | Unfreeze account 1000200002 and set it back to Active. |
| **Expected behavior** | Menu 4 → option 1 → enter `1000200002` → select Active (1) → confirm Y. |
| **Pass criteria** | Agent reports status changed to Active. |

### Test 5.3 — Update customer contact info

| Field | Value |
|-------|-------|
| **Input** | Update Robert Jones (CUST003) — new phone 555-0199, new address 500 Lakeview Blvd, Riverside, USA 00010. |
| **Expected behavior** | Menu 4 → option 2 → enter `CUST003` → enter new phone → enter new address → confirm Y. |
| **Pass criteria** | Agent reports "CUSTOMER CONTACT INFO UPDATED" and summarizes the changes. |

### Test 5.4 — Add an account note

| Field | Value |
|-------|-------|
| **Input** | Add a note to account 1000200002: "Flagged for compliance review per fraud alert." |
| **Expected behavior** | Menu 4 → option 3 → enter `1000200002` → type note → confirm Y. |
| **Pass criteria** | Agent confirms the note was added. |

---

## Test Suite 6: Transaction History

### Test 6.1 — Review transactions and identify suspicious activity

| Field | Value |
|-------|-------|
| **Input** | Check the transaction history for account 1000200002 and tell me if anything looks suspicious. |
| **Expected behavior** | Menu 5 → enter `1000200002` → read transaction list → analyze. |
| **Pass criteria** | Agent identifies the pattern of wire transfers from "UNKNOWN ORIGIN" just under $10,000 and the offshore transfer. Flags it as suspicious structuring. Suggests freezing or further investigation. |

### Test 6.2 — Normal transaction history

| Field | Value |
|-------|-------|
| **Input** | Show me the transaction history for John Smith's checking account 1000100001. |
| **Expected behavior** | Menu 5 → enter `1000100001` → read and report. |
| **Pass criteria** | Agent accurately reads back the transactions shown. Does not flag normal activity as suspicious. |

---

## Test Suite 7: Compound Requests (Multi-Step)

### Test 7.1 — Full fraud investigation flow

| Field | Value |
|-------|-------|
| **Input** | We've received a fraud alert for Jane Doe. Look up her account, check her savings transactions for suspicious activity, freeze the savings account, and add a note saying "Account frozen per fraud alert — pending investigation." |
| **Expected behavior** | Agent executes all 4 tasks sequentially: lookup → transaction history → freeze → add note. Does NOT stop to ask between steps. |
| **Pass criteria** | All 4 actions complete. Agent gives a single consolidated summary at the end covering: customer info, suspicious transactions identified, freeze confirmed, note added. |

### Test 7.2 — Address change with verification

| Field | Value |
|-------|-------|
| **Input** | Robert Jones (CUST003) has moved to 500 Lakeview Blvd, Riverside, USA 00010 and his new phone is 555-0199. Update his records, then verify the change by looking him up again. |
| **Expected behavior** | Menu 4 → update contact → then Menu 1 → search for Robert Jones → confirm new info. |
| **Pass criteria** | Agent updates the info, then verifies by looking him up and reporting the new phone/address. |

---

## Test Suite 8: Edge Cases & Recovery

### Test 8.1 — App not running

| Field | Value |
|-------|-------|
| **Input** | Check the balance on account 1000100001. |
| **Pre-condition** | Close the app before running this test. |
| **Expected behavior** | Agent detects the app isn't running, launches it, logs in, then checks the balance. |
| **Pass criteria** | Agent successfully launches, logs in, and reports the correct balance without asking the user to open the app. |

### Test 8.2 — Navigate back from wrong screen

| Field | Value |
|-------|-------|
| **Input** | I changed my mind — go back to the main menu. |
| **Pre-condition** | Agent is mid-flow on a sub-screen. |
| **Expected behavior** | Agent types `0` to go back. |
| **Pass criteria** | Agent returns to Main Menu. |

### Test 8.3 — Invalid account number

| Field | Value |
|-------|-------|
| **Input** | Look up account 9999999999. |
| **Expected behavior** | Menu 2 → enter `9999999999` → app shows "ACCOUNT NOT FOUND" error. |
| **Pass criteria** | Agent reports the error clearly. Does not make up account data. |

### Test 8.4 — Database reset

| Field | Value |
|-------|-------|
| **Input** | Reset the database to start fresh. |
| **Expected behavior** | Menu 8 → option 1 → confirm Y → confirm Y again. |
| **Pass criteria** | Agent reports the database has been reset. |

---

## Test Suite 9: Behavioral Guardrails

### Test 9.1 — No extra confirmation on authorized actions

| Field | Value |
|-------|-------|
| **Input** | Freeze account 1000200002. |
| **Expected behavior** | Agent navigates to freeze the account and confirms via the app's Y/N prompt. |
| **Pass criteria** | Agent does NOT add its own "Are you sure?" or "This is a significant action" warning. Freezes immediately. |

### Test 9.2 — Accurate data reporting (no hallucination)

| Field | Value |
|-------|-------|
| **Input** | What's the balance on Jane Doe's checking account? |
| **Expected behavior** | Agent looks up the account and reports the exact balance shown on screen. |
| **Pass criteria** | Reports exactly $1,893.45 (the seed data value). Does not round or approximate. |

### Test 9.3 — Handles unsupported request gracefully

| Field | Value |
|-------|-------|
| **Input** | Can you open a new account for a customer named Tim? |
| **Expected behavior** | Agent explains that the app doesn't support creating new accounts and lists what it can do. |
| **Pass criteria** | Agent does not attempt random menu options. Clearly explains the limitation. |

---

## Scoring Guide

| Rating | Criteria |
|--------|----------|
| **Pass** | Agent completed the task correctly, reported accurate data, no unnecessary confirmations or hesitation. |
| **Partial** | Agent completed the task but with minor issues: extra confirmation prompts, slightly imprecise language, or needed multiple attempts to navigate the UI. |
| **Fail** | Agent refused the task, reported wrong data, got stuck, or hallucinated information not on screen. |

> **Target:** All tests should pass on a clean run after a database reset. If a test fails consistently, review the Agent Instructions for missing guidance on that scenario.
