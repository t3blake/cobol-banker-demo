using CobolBanker.Data;
using CobolBanker.Models;
using CobolBanker.Screens;

namespace CobolBanker.Commands;

public static class CustomerLookupCommand
{
    /// <summary>
    /// Customer lookup screen. Returns selected customer ID, or null if user backs out.
    /// </summary>
    public static string? Run(Database db, Teller teller)
    {
        while (true)
        {
            Screen.Header("CUSTOMER LOOKUP");
            Screen.EmptyRow();
            Screen.Row("Search by: Name, Account #, Phone,");
            Screen.Row("           or Customer ID");
            Screen.EmptyRow();
            Screen.Row("Enter 0 to return to Main Menu");
            Screen.EmptyRow();
            Screen.BottomBorder();
            Screen.PrintLine();

            var query = Screen.Prompt("SEARCH");
            if (query == "0" || query.Equals("back", StringComparison.OrdinalIgnoreCase))
                return null;

            if (string.IsNullOrWhiteSpace(query))
            {
                Screen.ErrorText("SEARCH TERM REQUIRED");
                Screen.PressAnyKey();
                continue;
            }

            var results = db.SearchCustomers(query);

            if (results.Count == 0)
            {
                Screen.PrintLine();
                Screen.ErrorText("NO MATCHING CUSTOMERS FOUND");
                Screen.PressAnyKey();
                continue;
            }

            // Display results
            Screen.PrintLine();
            Screen.PrintLine($"  {results.Count} CUSTOMER(S) FOUND:");
            Screen.PrintLine();

            Screen.TableHeader(
                ("#", 4),
                ("CUST ID", 10),
                ("NAME", 22),
                ("PHONE", 12)
            );

            for (int i = 0; i < results.Count; i++)
            {
                var c = results[i];
                Screen.TableRow(
                    ($"{i + 1}", 4),
                    (c.CustomerId, 10),
                    (c.FullName, 22),
                    (c.Phone, 12)
                );
            }

            Screen.PrintLine();
            var selection = Screen.Prompt("SELECT CUSTOMER # (0=Back)");
            if (selection == "0") continue;

            if (int.TryParse(selection, out int idx) && idx >= 1 && idx <= results.Count)
            {
                var selected = results[idx - 1];
                ShowCustomerDetail(db, selected);
                return selected.CustomerId;
            }

            Screen.ErrorText("INVALID SELECTION");
            Screen.PressAnyKey();
        }
    }

    public static void ShowCustomerDetail(Database db, Customer customer)
    {
        Screen.Header("CUSTOMER DETAIL");
        Screen.EmptyRow();
        Screen.Row($"Customer ID:  {customer.CustomerId}");
        Screen.Row($"Name:         {customer.FullName}");
        Screen.Row($"Phone:        {customer.Phone}");
        Screen.Row($"Address:      {customer.Address}");
        Screen.Row($"Since:        {customer.CreatedDate}");
        Screen.EmptyRow();
        Screen.Divider();
        Screen.Row("ACCOUNTS:");
        Screen.EmptyRow();

        var accounts = db.GetAccountsForCustomer(customer.CustomerId);
        if (accounts.Count == 0)
        {
            Screen.Row("  (No accounts on file)");
        }
        else
        {
            foreach (var acct in accounts)
            {
                var statusFlag = acct.Status != "Active" ? $" [{acct.Status.ToUpper()}]" : "";
                Screen.Row($"  {acct.AccountNumber}  {acct.AccountType,-14} ${acct.Balance,12:N2}{statusFlag}");
            }
        }

        Screen.EmptyRow();
        Screen.BottomBorder();
        Screen.PressAnyKey();
    }
}
