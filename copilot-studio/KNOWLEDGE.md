# COBOL Banker — Knowledge Reference

> **Usage:** Upload this file as a Knowledge source in Copilot Studio. The agent will retrieve relevant sections when answering questions about customers, accounts, or application features.

---

## Application Overview

COBOL Banker is a desktop application simulating a legacy green-screen banking terminal (Woodgrove Bank). It runs as a local Windows application with a green-on-black terminal aesthetic.

- **App name on screen:** COBOL BANKER / WOODGROVE BANK
- **Window title:** Woodgrove Bank — Terminal Emulator
- **Install location:** C:\WoodgroveBank\cobol-banker.exe
- **Desktop shortcut label:** Woodgrove Bank Terminal

---

## Login Credentials

| Username | Password | Display Name | Branch |
|----------|----------|--------------|--------|
| `teller1` | `pass123` | J. Smith | Downtown |
| `teller2` | `pass123` | M. Johnson | Westside |
| `admin` | `admin` | Admin User | HQ |

Use `teller1` / `pass123` for standard operations.

---

## Main Menu Options

| # | Function | Description |
|---|----------|-------------|
| 1 | Customer Lookup | Search by name, customer ID, account number, or phone |
| 2 | Account Inquiry | View account details, last 5 transactions, notes |
| 3 | Fund Transfer | Transfer money between two accounts |
| 4 | Account Maintenance | Change status, update contact info, add notes |
| 5 | Transaction History | View up to 25 recent transactions |
| 8 | System Administration | Reset database, view app version |
| 9 | Log Off | Return to login screen |

Options 6 and 7 show "FUNCTION NOT AVAILABLE."

---

## Customers

| Customer ID | Name | Phone | Address | Member Since |
|-------------|------|-------|---------|--------------|
| CUST001 | John Smith | 555-0101 | 123 Main St, Anytown, USA 00001 | 2020-03-15 |
| CUST002 | Jane Doe | 555-0102 | 456 Oak Ave, Anytown, USA 00002 | 2019-07-22 |
| CUST003 | Robert Jones | 555-0103 | 789 Pine Rd, Springfield, USA 00003 | 2021-01-10 |
| CUST004 | Mary Williams | 555-0104 | 321 Elm St, Shelbyville, USA 00004 | 2018-11-05 |
| CUST005 | James Brown | 555-0105 | 654 Maple Dr, Anytown, USA 00005 | 2022-06-18 |
| CUST006 | Patricia Davis | 555-0106 | 987 Cedar Ln, Springfield, USA 00006 | 2020-09-30 |
| CUST007 | Michael Miller | 555-0107 | 147 Birch Ct, Shelbyville, USA 00007 | 2017-04-12 |
| CUST008 | Linda Wilson | 555-0108 | 258 Walnut Way, Anytown, USA 00008 | 2023-02-28 |

---

## Accounts

| Account # | Customer | Type | Balance | Status |
|-----------|----------|------|---------|--------|
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
| 1000700002 | Michael Miller (CUST007) | Savings | $5,600.00 | Closed |
| 1000800001 | Linda Wilson (CUST008) | Checking | $920.15 | Active |
| 1000800002 | Linda Wilson (CUST008) | Savings | $3,200.00 | Active |

---

## Notable Accounts

| Account | What's Interesting | Demo Use |
|---------|-------------------|----------|
| 1000200002 (Jane Doe Savings) | Transaction history shows multiple wire transfers from "UNKNOWN ORIGIN" and "OFFSHORE ACCT" — structuring pattern under $10K | Fraud investigation demo |
| 1000400001 (Mary Williams Checking) | Cash deposits just under $5K followed by wire transfers out — classic structuring | Suspicious activity demo |
| 1000700002 (Michael Miller Savings) | Status is **Closed** | Test closed account handling |
| 1000300003 (Robert Jones Money Market) | $150K balance | High-value account demo |

---

## Existing Account Notes

| Account | Note |
|---------|------|
| 1000200002 | "Customer called - verified wire transfer from family overseas. No flag needed at this time." (by teller2) |
| 1000700002 | "Account closed per customer request. Remaining balance transferred to checking." (by teller1) |
| 1000400001 | "Unusual cash deposit pattern noted. Flagged for compliance review." (by teller2) |

---

## Menu Workflows — Detailed Steps

### Customer Lookup (Menu 1)

1. At Main Menu, type `1` and press Enter.
2. At `SEARCH:` prompt, enter a search term (name, customer ID, account number, or phone). Type `0` to go back.
3. Results show a numbered list of matching customers.
4. At `SELECT CUSTOMER #:` prompt, type the row number to view details, or `0` to go back.
5. Customer detail shows: ID, name, phone, address, member-since date, and all accounts with balances.
6. Press any key to return to Main Menu.

### Account Inquiry (Menu 2)

1. At Main Menu, type `2` and press Enter.
2. At `ACCOUNT/CUSTOMER ID:` prompt, enter an account number or customer ID. Type `0` to go back.
3. If a customer ID has multiple accounts, a numbered list appears — select one.
4. Account detail shows: account number, type, status, balance, opened date, owner info, last 5 transactions, and any notes.
5. Press any key to return to Main Menu.

### Fund Transfer (Menu 3)

1. At Main Menu, type `3` and press Enter.
2. At `SOURCE ACCOUNT NUMBER:` prompt, enter the source account. Type `0` to cancel.
3. Source account info is displayed for verification.
4. At `DESTINATION ACCOUNT NUMBER:` prompt, enter the destination account.
5. Destination account info is displayed.
6. At `TRANSFER AMOUNT:` prompt, enter the dollar amount (e.g., `500` or `500.00`).
7. A confirmation summary appears.
8. At `CONFIRM TRANSFER (Y/N):` prompt, type `Y` to confirm or `N` to cancel.
9. On success: `>>> TRANSFER COMPLETED SUCCESSFULLY <<<` with updated balances.

**Possible errors:** ACCOUNT NOT FOUND, ACCOUNT IS FROZEN, INSUFFICIENT FUNDS, INVALID AMOUNT, SOURCE AND DESTINATION CANNOT BE THE SAME.

### Account Maintenance (Menu 4)

Sub-menu:
- `1` — Change Account Status
- `2` — Update Customer Contact Info
- `3` — Add Account Note
- `0` — Return to Main Menu

**Change Status:** Enter account number → see current info → enter new status (`1`=Active, `2`=Frozen, `3`=Closed) → confirm Y/N.

**Update Contact:** Enter customer ID → see current phone/address → enter new phone (or Enter to keep) → enter new address (or Enter to keep) → confirm Y/N.

**Add Note:** Enter account number → see account info and existing notes → type note text → confirm Y/N.

### Transaction History (Menu 5)

1. At Main Menu, type `5` and press Enter.
2. Enter account number (or `0` to go back).
3. Shows up to 25 recent transactions: date, description, amount, running balance.

### System Administration (Menu 8)

Sub-menu:
- `1` — Reset Database (requires double confirmation: Y, then Y again)
- `2` — About / Version Info
- `0` — Return to Main Menu

---

## Demo Scenarios

### Scenario A: Customer Balance Check & Fund Transfer

> "A customer calls to check their balance and transfer funds."

1. Login as `teller1` / `pass123`
2. Menu `1` → search `John Smith` → select customer #1 → note accounts
3. Press any key → back to Main Menu
4. Menu `3` → source `1000100001` → destination `1000100002` → amount `500` → confirm `Y`
5. Verify success and updated balances

### Scenario B: Fraud Investigation & Account Freeze

> "Compliance flagged Jane Doe's savings for suspicious wire activity."

1. Login as `teller1` / `pass123`
2. Menu `5` → account `1000200002` → observe suspicious wire transfers
3. Press any key → Main Menu
4. Menu `4` → `1` → account `1000200002` → status `2` (Frozen) → confirm `Y`
5. Menu `4` → `3` → account `1000200002` → note: `Multiple structured wire transfers detected. Account frozen pending investigation.` → confirm `Y`

### Scenario C: Address Update

> "Robert Jones moved and needs contact info updated."

1. Login as `teller1` / `pass123`
2. Menu `4` → `2` → `CUST003` → phone `555-9999` → address `100 New Boulevard, Anytown, USA 00010` → confirm `Y`
3. Menu `2` → `CUST003` → select account 1 → verify note reflects the change

---

## Screen State Machine

```
[Launch App] → [Login] → [Main Menu]

Main Menu branches:
  1 → Customer Lookup → Customer Detail → Main Menu
  2 → Account Inquiry → Account Detail → Main Menu
  3 → Fund Transfer (multi-step) → Main Menu
  4 → Account Maintenance → sub-menu (1/2/3/0) → Main Menu
  5 → Transaction History → Main Menu
  8 → System Admin → sub-menu (1/2/0) → Main Menu
  9 → Log Off → Login

At any prompt: type 0 to go back.
After 3 failed logins: app exits.
Every completed action returns to Main Menu. There are no dead ends.
```

---

## Error Messages

| Pattern | Meaning |
|---------|---------|
| `>>> ... <<<` | Success (bright green) |
| `*** ... ***` | Error (red) |
| `!!! ... !!!` | Warning/cancellation (yellow) |
| `Press any key to continue...` | Dismiss prompt before continuing |

---

## Database Reset

To restore all data to original seed values:
Main Menu → `8` → `1` → `Y` → `Y`
