using CobolBanker.Data;
using CobolBanker.Models;
using CobolBanker.Screens;

namespace CobolBanker.Commands;

public static class TransactionHistoryCommand
{
    public static void Run(Database db, Teller teller)
    {
        while (true)
        {
            Screen.Header("TRANSACTION HISTORY");
            Screen.EmptyRow();
            Screen.Row("Enter Account Number to view history");
            Screen.Row("Enter 0 to return to Main Menu");
            Screen.EmptyRow();
            Screen.BottomBorder();
            Screen.PrintLine();

            var input = Screen.Prompt("ACCOUNT NUMBER");
            if (input == "0" || input.Equals("back", StringComparison.OrdinalIgnoreCase))
                return;

            var account = db.GetAccount(input);
            if (account == null)
            {
                Screen.ErrorText("ACCOUNT NOT FOUND");
                Screen.PressAnyKey();
                continue;
            }

            var customer = db.GetCustomer(account.CustomerId);
            var transactions = db.GetTransactions(input, 25);

            Screen.Header("TRANSACTION HISTORY");
            Screen.EmptyRow();
            Screen.Row($"Account:  {account.AccountNumber}  ({account.AccountType})");
            Screen.Row($"Owner:    {customer?.FullName ?? "Unknown"}");
            Screen.Row($"Status:   {account.Status}");
            Screen.Row($"Balance:  ${account.Balance:N2}");
            Screen.EmptyRow();
            Screen.BottomBorder();

            Screen.PrintLine();

            if (transactions.Count == 0)
            {
                Screen.PrintLine("  (No transactions on file)");
            }
            else
            {
                Screen.TableHeader(
                    ("DATE", 12),
                    ("DESCRIPTION", 28),
                    ("AMOUNT", 12),
                    ("BALANCE", 12)
                );

                foreach (var t in transactions)
                {
                    var sign = t.Amount >= 0 ? "+" : "";
                    Screen.TableRow(
                        (t.Date, 12),
                        (t.Description, 28),
                        ($"{sign}{t.Amount:N2}", 12),
                        ($"${t.RunningBalance:N2}", 12)
                    );
                }

                Screen.PrintLine();
                Screen.PrintLine($"  SHOWING {transactions.Count} MOST RECENT TRANSACTIONS");
            }

            Screen.PressAnyKey();
        }
    }
}
