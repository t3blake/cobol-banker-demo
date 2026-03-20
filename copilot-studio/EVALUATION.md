# COBOL Banker — Evaluation Test Plan

> **Usage:** Use these test cases with Copilot Studio's **Evaluation** feature to validate agent behavior after changing instructions, knowledge, or CUA tool settings. Each test has an input prompt, the expected agent behavior, and pass/fail criteria.

---

## Setting Up Evaluation in Copilot Studio

### Prerequisites

- Your agent is fully configured (Agent Instructions, CUA Tool Instructions, and Knowledge file are all applied — see [../README.md](../README.md)).
- COBOL Banker is installed on the target Windows 365 machine (`C:\WoodgroveBank\cobol-banker.exe`).
- The database has been reset to seed data (Menu 8 → 1 → Y → Y).

### Step-by-Step: Create an Evaluation

The test cases are split into 4 CSV files designed to be run as separate evaluations. This avoids overloading the CUA connection — running too many CUA tests in a single batch can exhaust the connection token and cause cascading failures.

| CSV File | Tests | What It Covers | Run Time |
|----------|-------|---------------|----------|
| [`evaluation-1-smoke.csv`](evaluation-1-smoke.csv) | 5 | Launch, login, basic lookup, balance check, bad login | ~5 min |
| [`evaluation-2-readonly.csv`](evaluation-2-readonly.csv) | 7 | Lookups, inquiries, transaction history, error handling | ~10 min |
| [`evaluation-3-write.csv`](evaluation-3-write.csv) | 7 | Transfers, freezes, contact updates, notes, DB reset | ~15 min |
| [`evaluation-4-compound.csv`](evaluation-4-compound.csv) | 4 | Multi-step workflows, behavioral guardrails | ~15 min |

**To import a batch:**

1. Open your agent in **Copilot Studio** (https://copilotstudio.microsoft.com).
2. In the left navigation, click **Evaluate**.
3. Click **+ New evaluation** → **Import**.
4. Select one of the CSV files above.
5. Name it to match the file (e.g., "1 — Smoke Test", "2 — Read-Only").
6. Configure test methods if desired. The default test method is applied automatically on import.
7. Click **Run** to execute the batch.
8. **Wait for the batch to finish before running the next one.** Don't run multiple evaluations simultaneously — they share the same CUA connection.

**Run order:** Always run in numbered order (1 → 2 → 3 → 4). Reset the database before batches 3 and 4 since they modify data.

**Option B — Add test cases manually:**

1. Open your agent in **Copilot Studio** → **Evaluate** → **+ New evaluation**.
2. For each test case in this document, add a row:
   - **Input**: Copy the prompt from the **Input** field.
   - **Expected output**: Copy the **Pass criteria** text.
3. Click **Run** and review results.

### Tips for Effective Evaluations

- **Reset the database before every evaluation run.** Log in → Menu 8 → option 1 → Y → Y. Without this, balances and statuses from previous tests will cause false failures.
- **Run tests that modify data last.** Tests that freeze accounts or transfer funds change the database state. Run read-only tests (lookups, balance checks, transaction history) first.
- **Run compound tests separately** if they conflict. Test 7.1 (fraud flow) freezes Jane Doe's savings — any subsequent test on that account will see "Frozen" instead of "Active".
- **Use focused evaluations for iteration.** You don't need to run all 22 tests every time. After changing Agent Instructions, run the behavioral guardrail tests (Suite 9). After changing CUA Tool Instructions, run the launch/navigation tests (Suites 1, 8).

### When to Run Evaluations

| Trigger | Which tests to run |
|---------|--------------------|
| Changed **Agent Instructions** | Suites 7 (compound), 9 (guardrails), plus any scenario you modified |
| Changed **CUA Tool Instructions** | Suites 1 (launch/login), 8 (edge cases/recovery) |
| Changed **Knowledge** file | Suites 2 (lookup), 3 (inquiry), 6 (transaction history) |
| Updated the **app itself** (new build) | All suites — full regression |
| Before a **live demo** | Quick smoke test: Tests 1.2, 2.1, 4.1, 5.1 (login, lookup, transfer, freeze) |

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

---

## Troubleshooting Common Failures

| Symptom | Likely cause | Fix |
|---------|-------------|-----|
| Agent refuses to freeze/close an account | LLM safety training overriding instructions | Strengthen the "This Is a Demo Environment" section in Agent Instructions — add explicit mention of the refused action |
| Agent adds "Are you sure?" before acting | Double-confirmation habit | Add/reinforce "Do not add your own layer of confirmation" in Agent Instructions |
| Agent can't find or launch the app | CUA can't locate the window or path | Check CUA Tool Instructions — verify the window title, exe path, and launch method are correct |
| Agent types into the wrong input mode | Confused between text-box input and any-key dismiss | Verify the "Two Input Modes" section in CUA Tool Instructions clearly distinguishes the two |
| Agent reports wrong balance or data | Stale database (previous test changed data) | Reset the database before the evaluation run |
| Agent gets stuck on a screen | Missing navigation recovery instructions | Add the stuck screen to the Recovery section in Agent Instructions |
| Agent hallucinates customer data | Knowledge file not uploaded or outdated | Re-upload KNOWLEDGE.md as a Knowledge source in Copilot Studio |
| Agent works in test but fails in demo | Different machine, app not installed, or different screen resolution | Verify app is installed on the demo machine and CUA can see the window at the expected size |
