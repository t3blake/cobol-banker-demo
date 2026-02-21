using CobolBanker.Data;
using CobolBanker.Models;
using CobolBanker.Screens;

namespace CobolBanker.Commands;

public static class AccountMaintenanceCommand
{
    public static void Run(Database db, Teller teller)
    {
        while (true)
        {
            Screen.Header("ACCOUNT MAINTENANCE");
            Screen.EmptyRow();
            Screen.Row("1. Change Account Status");
            Screen.Row("2. Update Customer Contact Info");
            Screen.Row("3. Add Account Note");
            Screen.EmptyRow();
            Screen.Row("0. Return to Main Menu");
            Screen.EmptyRow();
            Screen.BottomBorder();
            Screen.PrintLine();

            var choice = Screen.MenuChoice("ENTER SELECTION", 0, 3);

            switch (choice)
            {
                case 0: return;
                case 1: ChangeAccountStatus(db, teller); break;
                case 2: UpdateContactInfo(db, teller); break;
                case 3: AddAccountNote(db, teller); break;
            }
        }
    }

    private static void ChangeAccountStatus(Database db, Teller teller)
    {
        Screen.Header("CHANGE ACCOUNT STATUS");
        Screen.EmptyRow();
        Screen.Row("Enter 0 to cancel");
        Screen.EmptyRow();
        Screen.BottomBorder();
        Screen.PrintLine();

        var acctInput = Screen.Prompt("ACCOUNT NUMBER");
        if (acctInput == "0") return;

        var account = db.GetAccount(acctInput);
        if (account == null)
        {
            Screen.ErrorText("ACCOUNT NOT FOUND");
            Screen.PressAnyKey();
            return;
        }

        var customer = db.GetCustomer(account.CustomerId);
        Screen.PrintLine();
        Screen.PrintLine($"  Account:  {account.AccountNumber} ({account.AccountType})");
        Screen.PrintLine($"  Owner:    {customer?.FullName ?? "Unknown"}");
        Screen.PrintLine($"  Balance:  ${account.Balance:N2}");
        Screen.PrintLine($"  Status:   {account.Status}");
        Screen.PrintLine();
        Screen.PrintLine("  Available statuses:");
        Screen.PrintLine("    1. Active");
        Screen.PrintLine("    2. Frozen");
        Screen.PrintLine("    3. Closed");
        Screen.PrintLine();

        var statusChoice = Screen.MenuChoice("NEW STATUS", 1, 3);
        var newStatus = statusChoice switch
        {
            1 => "Active",
            2 => "Frozen",
            3 => "Closed",
            _ => account.Status
        };

        if (newStatus == account.Status)
        {
            Screen.PrintLine($"  Account is already {account.Status}.");
            Screen.PressAnyKey();
            return;
        }

        Screen.PrintLine();
        if (!Screen.Confirm($"CHANGE STATUS FROM {account.Status.ToUpper()} TO {newStatus.ToUpper()}"))
        {
            Screen.WarningText("STATUS CHANGE CANCELLED");
            Screen.PressAnyKey();
            return;
        }

        db.UpdateAccountStatus(account.AccountNumber, newStatus);
        db.AddNote(account.AccountNumber, teller.Username,
            $"Account status changed from {account.Status} to {newStatus} by {teller.DisplayName}");

        Screen.PrintLine();
        Screen.SuccessText($"ACCOUNT STATUS CHANGED TO {newStatus.ToUpper()}");
        Screen.PressAnyKey();
    }

    private static void UpdateContactInfo(Database db, Teller teller)
    {
        Screen.Header("UPDATE CUSTOMER CONTACT INFO");
        Screen.EmptyRow();
        Screen.Row("Enter Customer ID or search by name");
        Screen.Row("Enter 0 to cancel");
        Screen.EmptyRow();
        Screen.BottomBorder();
        Screen.PrintLine();

        var input = Screen.Prompt("CUSTOMER ID");
        if (input == "0") return;

        // Try direct customer lookup
        var customer = db.GetCustomer(input);
        if (customer == null)
        {
            // Try searching
            var results = db.SearchCustomers(input);
            if (results.Count == 0)
            {
                Screen.ErrorText("CUSTOMER NOT FOUND");
                Screen.PressAnyKey();
                return;
            }
            if (results.Count == 1)
            {
                customer = results[0];
            }
            else
            {
                Screen.PrintLine();
                for (int i = 0; i < results.Count; i++)
                    Screen.PrintLine($"  {i + 1}. {results[i].CustomerId} - {results[i].FullName}");

                Screen.PrintLine();
                var sel = Screen.Prompt("SELECT CUSTOMER #");
                if (int.TryParse(sel, out int idx) && idx >= 1 && idx <= results.Count)
                    customer = results[idx - 1];
                else
                {
                    Screen.ErrorText("INVALID SELECTION");
                    Screen.PressAnyKey();
                    return;
                }
            }
        }

        Screen.PrintLine();
        Screen.PrintLine($"  Customer:  {customer.FullName} ({customer.CustomerId})");
        Screen.PrintLine($"  Phone:     {customer.Phone}");
        Screen.PrintLine($"  Address:   {customer.Address}");
        Screen.PrintLine();
        Screen.PrintLine("  Enter new values (press ENTER to keep current):");
        Screen.PrintLine();

        var newPhone = Screen.Prompt($"  PHONE [{customer.Phone}]");
        if (string.IsNullOrWhiteSpace(newPhone)) newPhone = customer.Phone;

        var newAddress = Screen.Prompt($"  ADDRESS [{customer.Address}]");
        if (string.IsNullOrWhiteSpace(newAddress)) newAddress = customer.Address;

        if (newPhone == customer.Phone && newAddress == customer.Address)
        {
            Screen.PrintLine("  No changes made.");
            Screen.PressAnyKey();
            return;
        }

        Screen.PrintLine();
        Screen.PrintLine("  Updated values:");
        if (newPhone != customer.Phone)
            Screen.PrintLine($"    Phone:   {customer.Phone} → {newPhone}");
        if (newAddress != customer.Address)
            Screen.PrintLine($"    Address: {customer.Address} → {newAddress}");
        Screen.PrintLine();

        if (!Screen.Confirm("CONFIRM CHANGES"))
        {
            Screen.WarningText("CHANGES CANCELLED");
            Screen.PressAnyKey();
            return;
        }

        db.UpdateCustomerContact(customer.CustomerId, newPhone, newAddress);

        // Add notes to all active accounts
        var changes = new List<string>();
        if (newPhone != customer.Phone) changes.Add($"phone updated to {newPhone}");
        if (newAddress != customer.Address) changes.Add($"address updated to {newAddress}");
        var noteText = $"Customer contact info updated: {string.Join(", ", changes)} by {teller.DisplayName}";

        var accounts = db.GetAccountsForCustomer(customer.CustomerId);
        foreach (var acct in accounts.Where(a => a.Status == "Active"))
        {
            db.AddNote(acct.AccountNumber, teller.Username, noteText);
        }

        Screen.PrintLine();
        Screen.SuccessText("CUSTOMER CONTACT INFO UPDATED");
        Screen.PressAnyKey();
    }

    private static void AddAccountNote(Database db, Teller teller)
    {
        Screen.Header("ADD ACCOUNT NOTE");
        Screen.EmptyRow();
        Screen.Row("Enter 0 to cancel");
        Screen.EmptyRow();
        Screen.BottomBorder();
        Screen.PrintLine();

        var acctInput = Screen.Prompt("ACCOUNT NUMBER");
        if (acctInput == "0") return;

        var account = db.GetAccount(acctInput);
        if (account == null)
        {
            Screen.ErrorText("ACCOUNT NOT FOUND");
            Screen.PressAnyKey();
            return;
        }

        var customer = db.GetCustomer(account.CustomerId);
        Screen.PrintLine();
        Screen.PrintLine($"  Account:  {account.AccountNumber} ({account.AccountType})");
        Screen.PrintLine($"  Owner:    {customer?.FullName ?? "Unknown"}");
        Screen.PrintLine($"  Status:   {account.Status}");
        Screen.PrintLine();

        // Show existing notes
        var existingNotes = db.GetNotes(account.AccountNumber);
        if (existingNotes.Count > 0)
        {
            Screen.PrintLine("  EXISTING NOTES:");
            foreach (var n in existingNotes.Take(3))
            {
                Screen.PrintLine($"    [{n.CreatedDate}] {n.CreatedBy}: {n.NoteText}");
            }
            if (existingNotes.Count > 3)
                Screen.PrintLine($"    ... and {existingNotes.Count - 3} more");
            Screen.PrintLine();
        }

        var noteText = Screen.Prompt("NOTE TEXT");
        if (string.IsNullOrWhiteSpace(noteText))
        {
            Screen.ErrorText("NOTE TEXT CANNOT BE EMPTY");
            Screen.PressAnyKey();
            return;
        }

        db.AddNote(account.AccountNumber, teller.Username, noteText);

        Screen.PrintLine();
        Screen.SuccessText("NOTE ADDED SUCCESSFULLY");
        Screen.PressAnyKey();
    }
}
