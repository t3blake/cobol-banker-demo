using CobolBanker.Data;
using CobolBanker.Models;
using CobolBanker.Screens;

namespace CobolBanker.Commands;

public static class AccountInquiryCommand
{
    public static void Run(Database db, Teller teller)
    {
        while (true)
        {
            Screen.Header("ACCOUNT INQUIRY");
            Screen.EmptyRow();
            Screen.Row("Enter Account Number or Customer ID");
            Screen.Row("Enter 0 to return to Main Menu");
            Screen.EmptyRow();
            Screen.BottomBorder();
            Console.WriteLine();

            var input = Screen.Prompt("ACCOUNT/CUSTOMER ID");
            if (input == "0" || input.Equals("back", StringComparison.OrdinalIgnoreCase))
                return;

            // Try as account number first
            var account = db.GetAccount(input);
            if (account != null)
            {
                ShowAccountDetail(db, account);
                continue;
            }

            // Try as customer ID — show all their accounts
            var customer = db.GetCustomer(input);
            if (customer != null)
            {
                var accounts = db.GetAccountsForCustomer(customer.CustomerId);
                if (accounts.Count == 0)
                {
                    Screen.ErrorText("CUSTOMER HAS NO ACCOUNTS ON FILE");
                    Screen.PressAnyKey();
                    continue;
                }

                if (accounts.Count == 1)
                {
                    ShowAccountDetail(db, accounts[0]);
                    continue;
                }

                // Multiple accounts — let user pick
                Console.WriteLine();
                Console.WriteLine($"  ACCOUNTS FOR {customer.FullName}:");
                Console.WriteLine();
                Screen.TableHeader(
                    ("#", 4),
                    ("ACCOUNT #", 14),
                    ("TYPE", 14),
                    ("BALANCE", 14),
                    ("STATUS", 10)
                );

                for (int i = 0; i < accounts.Count; i++)
                {
                    var a = accounts[i];
                    Screen.TableRow(
                        ($"{i + 1}", 4),
                        (a.AccountNumber, 14),
                        (a.AccountType, 14),
                        ($"${a.Balance:N2}", 14),
                        (a.Status, 10)
                    );
                }

                Console.WriteLine();
                var sel = Screen.Prompt("SELECT ACCOUNT # (0=Back)");
                if (sel == "0") continue;
                if (int.TryParse(sel, out int idx) && idx >= 1 && idx <= accounts.Count)
                {
                    ShowAccountDetail(db, accounts[idx - 1]);
                }
                else
                {
                    Screen.ErrorText("INVALID SELECTION");
                    Screen.PressAnyKey();
                }
                continue;
            }

            Screen.ErrorText("ACCOUNT OR CUSTOMER NOT FOUND");
            Screen.PressAnyKey();
        }
    }

    private static void ShowAccountDetail(Database db, Account account)
    {
        var customer = db.GetCustomer(account.CustomerId);
        var notes = db.GetNotes(account.AccountNumber);
        var txns = db.GetTransactions(account.AccountNumber, 5);

        Screen.Header("ACCOUNT DETAIL");
        Screen.EmptyRow();
        Screen.Row($"Account #:    {account.AccountNumber}");
        Screen.Row($"Type:         {account.AccountType}");
        Screen.Row($"Status:       {account.Status}");
        Screen.Row($"Balance:      ${account.Balance:N2}");
        Screen.Row($"Opened:       {account.OpenedDate}");
        Screen.EmptyRow();
        if (customer != null)
        {
            Screen.Row($"Owner:        {customer.FullName} ({customer.CustomerId})");
            Screen.Row($"Phone:        {customer.Phone}");
        }
        Screen.EmptyRow();
        Screen.Divider();
        Screen.Row("RECENT TRANSACTIONS:");
        Screen.EmptyRow();

        if (txns.Count == 0)
        {
            Screen.Row("  (No transactions on file)");
        }
        else
        {
            foreach (var t in txns)
            {
                var sign = t.Amount >= 0 ? "+" : "";
                Screen.Row($"  {t.Date}  {sign}{t.Amount:N2}  {t.Description}");
            }
        }

        if (notes.Count > 0)
        {
            Screen.EmptyRow();
            Screen.Divider();
            Screen.Row("ACCOUNT NOTES:");
            Screen.EmptyRow();
            foreach (var n in notes)
            {
                Screen.Row($"  [{n.CreatedDate}] {n.CreatedBy}:");
                // Wrap long notes
                var words = n.NoteText.Split(' ');
                var line = "    ";
                foreach (var word in words)
                {
                    if (line.Length + word.Length + 1 > Screen.WIDTH - 6)
                    {
                        Screen.Row(line);
                        line = "    " + word;
                    }
                    else
                    {
                        line += (line == "    " ? "" : " ") + word;
                    }
                }
                if (line.Trim().Length > 0) Screen.Row(line);
            }
        }

        Screen.EmptyRow();
        Screen.BottomBorder();
        Screen.PressAnyKey();
    }
}
