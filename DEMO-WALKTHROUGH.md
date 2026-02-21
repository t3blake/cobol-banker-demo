# COBOL Banker — Demo Walkthrough

> **Audience:** CSAs running live demos for customers.
> **Pre-req:** Launch `cobol-banker.exe` — the green-screen terminal window appears automatically.
> **Reset between demos:** Log in → Menu option `8` (System Admin) → `1` (Reset Database) → confirm twice. This restores all seed data to its original state.

---

## Quick Reference — Teller Credentials

| Username | Password | Name | Branch |
|----------|----------|------|--------|
| `teller1` | `pass123` | J. Smith | Downtown |
| `teller2` | `pass123` | M. Johnson | Westside |
| `admin` | `admin` | Admin User | HQ |

Use **teller1** for all three scenarios unless you want variety.

---

## Quick Reference — Key Customers & Accounts

| Customer | ID | Phone | Accounts | Notes |
|----------|----|-------|----------|-------|
| Jane Doe | CUST002 | 555-0102 | Checking `1000200001` ($1,893.45) / Savings `1000200002` ($45,200.00) | Savings has suspicious wire transfers — use for **Scenario 1** |
| Robert Jones | CUST003 | 555-0103 | Checking `1000300001` / Savings `1000300002` / Money Market `1000300003` | Three accounts, good for **Scenario 2** (address change) |
| John Smith | CUST001 | 555-0101 | Checking `1000100001` ($4,523.67) / Savings `1000100002` ($12,750.00) | Normal customer, good for **Scenario 3** (fund transfer) |
| Mary Williams | CUST004 | 555-0104 | Checking `1000400001` ($2,341.89) / Savings `1000400002` ($8,900.00) | Checking has structuring pattern — alternate for Scenario 1 |

---

## Scenario 1: Find and Freeze a Flagged Account

> **Narrative:** "A fraud alert has come in for a customer with suspicious wire transfer activity. We need to locate the account, review the transactions, freeze the account, and add an investigation note."

### Setup
If demoing with an agent, frame it as:
> "We've received a fraud alert for a customer named **Jane Doe**. Can you look up her account, check for suspicious activity, freeze the account, and add a note?"

### Step-by-Step

| Step | Action | What to type | What to expect |
|------|--------|-------------|----------------|
| 1 | **Log in** | `teller1` then `pass123` | "ACCESS GRANTED — Welcome, J. Smith" |
| 2 | **Go to Customer Lookup** | `1` | Customer Lookup screen |
| 3 | **Search for the customer** | `Jane Doe` | 1 result: Jane Doe, CUST002, 555-0102 |
| 4 | **Select her** | `1` | Customer Detail — shows 2 accounts (Checking + Savings) |
| 5 | **Press any key**, return to menu | *(any key)* | Back at Main Menu |
| 6 | **Go to Transaction History** | `5` | Transaction History screen |
| 7 | **Look up the savings account** | `1000200002` | Transaction list appears |

**What you'll see in the transactions (talk through these):**

```
DATE         DESCRIPTION                      AMOUNT       BALANCE
2026-02-18   WIRE TRANSFER - OFFSHORE ACCT    +4,800.00    $45,200.00
2026-02-11   WIRE TRANSFER - UNKNOWN ORIGIN   +9,900.00    $40,400.00
2026-02-06   TRANSFER FROM CHECKING           +1,000.00    $30,500.00
2026-02-02   WIRE TRANSFER - UNKNOWN ORIGIN   +9,700.00    $29,500.00
2026-01-28   WIRE TRANSFER - UNKNOWN ORIGIN   +9,500.00    $19,800.00
2026-01-22   WIRE TRANSFER - UNKNOWN ORIGIN   +9,800.00    $10,300.00
2026-01-15   TRANSFER FROM CHECKING           +500.00      $500.00
```

> **Talking point:** "Notice the pattern — multiple wire transfers from unknown origins, each just under $10,000, and one from an offshore account. This is a textbook suspicious activity pattern."

| Step | Action | What to type | What to expect |
|------|--------|-------------|----------------|
| 8 | **Press any key**, return to menu | *(any key)* | Back at Main Menu |
| 9 | **Go to Account Maintenance** | `4` | Account Maintenance submenu |
| 10 | **Change Account Status** | `1` | Prompts for account number |
| 11 | **Enter the savings account** | `1000200002` | Shows account info: Active, $45,200.00 |
| 12 | **Select Frozen** | `2` | Confirmation prompt |
| 13 | **Confirm** | `Y` | "ACCOUNT STATUS CHANGED TO FROZEN" |
| 14 | **Press any key**, back to Maintenance | *(any key)* | Back at Account Maintenance submenu |
| 15 | **Add an account note** | `3` | Prompts for account number |
| 16 | **Enter the same account** | `1000200002` | Shows account info + existing notes |
| 17 | **Type the note** | `Account frozen per fraud alert — pending investigation` | Confirmation |
| 18 | **Done** | Press any key, then `0` to return to Main Menu | Back at Main Menu |

> **Talking point:** "The account is now frozen with a full audit trail. A real investigator would pick this up from here."

---

## Scenario 2: Process a Customer Address Change

> **Narrative:** "A customer has called in to update their mailing address and phone number. We need to look them up, verify their identity, and update their contact information."

### Setup
If demoing with an agent, frame it as:
> "Customer **Robert Jones** is on the phone. He's moved to a new address: **500 Lakeview Blvd, Riverside, USA 00010** and has a new phone number: **555-0199**. Can you update his records?"

### Step-by-Step

| Step | Action | What to type | What to expect |
|------|--------|-------------|----------------|
| 1 | **Log in** (if not already) | `teller1` then `pass123` | "ACCESS GRANTED" |
| 2 | **Go to Customer Lookup** | `1` | Customer Lookup screen |
| 3 | **Search by phone** | `555-0103` | 1 result: Robert Jones, CUST003 |
| 4 | **Select him** | `1` | Customer Detail — current address: "789 Pine Rd, Springfield, USA 00003", phone: 555-0103, 3 accounts listed |
| 5 | **Press any key**, return to menu | *(any key)* | Back at Main Menu |
| 6 | **Go to Account Maintenance** | `4` | Account Maintenance submenu |
| 7 | **Update Customer Contact Info** | `2` | Prompts for Customer ID or name |
| 8 | **Enter his customer ID** | `CUST003` | Shows current contact: Robert Jones, 555-0103, 789 Pine Rd... |
| 9 | **Enter new phone** | `555-0199` | (press Enter to keep current, or type new value) |
| 10 | **Enter new address** | `500 Lakeview Blvd, Riverside, USA 00010` | Shows summary of changes |
| 11 | **Confirm** | `Y` | "CUSTOMER CONTACT INFO UPDATED" |
| 12 | **Done** | Press any key | Back at Account Maintenance |

> **Talking point:** "The contact info is updated across all of Robert's accounts, and a note was automatically added to each active account documenting the change."

### Optional Verification Step
To show the update persisted:

| Step | Action | What to type | What to expect |
|------|--------|-------------|----------------|
| 13 | Return to Main Menu | `0` | Main Menu |
| 14 | **Customer Lookup** | `1` | Lookup screen |
| 15 | **Search by new phone** | `555-0199` | Robert Jones now appears with the new phone number |
| 16 | **Select him** | `1` | Updated address and phone shown in Customer Detail |

---

## Scenario 3: Transfer Funds Between Accounts

> **Narrative:** "A customer wants to transfer $2,000 from their savings account to their checking account."

### Setup
If demoing with an agent, frame it as:
> "Customer **John Smith** would like to transfer **$2,000** from his savings to his checking account. Can you process that?"

### Step-by-Step

| Step | Action | What to type | What to expect |
|------|--------|-------------|----------------|
| 1 | **Log in** (if not already) | `teller1` then `pass123` | "ACCESS GRANTED" |
| 2 | **Go to Customer Lookup** | `1` | Customer Lookup screen |
| 3 | **Search for the customer** | `John Smith` | 1 result: John Smith, CUST001, 555-0101 |
| 4 | **Select him** | `1` | Customer Detail — 2 accounts listed |

**What you'll see (note these for the transfer):**

```
Accounts:
  1000100001  Checking        $  4,523.67
  1000100002  Savings         $ 12,750.00
```

| Step | Action | What to type | What to expect |
|------|--------|-------------|----------------|
| 5 | **Press any key**, return to menu | *(any key)* | Back at Main Menu |
| 6 | **Go to Fund Transfer** | `3` | Fund Transfer screen |
| 7 | **Enter source account** (Savings) | `1000100002` | Shows: Savings, John Smith, $12,750.00 |
| 8 | **Enter destination account** (Checking) | `1000100001` | Shows: Checking, John Smith, $4,523.67 |
| 9 | **Enter transfer amount** | `2000` | Transfer Confirmation screen |

**What you'll see on the confirmation screen:**

```
FROM:
  1000100002 (Savings)
  John Smith
  Current Balance: $12,750.00
  New Balance:     $10,750.00

TO:
  1000100001 (Checking)
  John Smith
  Current Balance: $4,523.67
  New Balance:     $6,523.67

TRANSFER AMOUNT: $2,000.00
```

| Step | Action | What to type | What to expect |
|------|--------|-------------|----------------|
| 10 | **Confirm the transfer** | `Y` | "TRANSFER COMPLETED SUCCESSFULLY" with updated balances |
| 11 | **Done** | Press any key | Returns to Main Menu |

> **Talking point:** "The transfer is complete — both balances are updated, transaction records are created on both accounts, and audit notes are automatically added."

### Optional Verification Step
To show the transfer posted:

| Step | Action | What to type | What to expect |
|------|--------|-------------|----------------|
| 12 | **Go to Transaction History** | `5` | Transaction History screen |
| 13 | **Check the savings account** | `1000100002` | Top transaction: "TRANSFER TO 1000100001 -2,000.00" |

---

## Tips for a Smooth Demo

1. **Always reset first.** Before any demo, go to System Admin (`8`) → Reset Database (`1`) → confirm twice. This ensures clean seed data every time.

2. **Demo order matters.** Run Scenario 3 (transfer) before Scenario 1 (freeze) if you're doing multiple — freezing Jane Doe's account prevents transfers involving it.

3. **Tab between scenarios.** You don't need to log off between scenarios. After finishing one, just press any key and you're back at the Main Menu.

4. **If the agent gets stuck on a screen,** the input prompt is always at the bottom. The agent should type `0` or `back` to return to the previous menu.

5. **Keep the cheat sheet handy.** The account numbers are the hardest thing to remember:
   - Jane Doe's savings (fraud): **1000200002**
   - John Smith's savings → checking: **1000100002** → **1000100001**
   - Robert Jones's customer ID: **CUST003**

6. **The "wow" moment** in each scenario:
   - **Scenario 1:** The agent reads the transaction history, identifies the suspicious pattern, and takes action — all without human guidance.
   - **Scenario 2:** The agent updates multiple fields and the change propagates to all accounts with audit notes.
   - **Scenario 3:** The agent navigates a multi-step confirmation flow with calculated projected balances.
