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
            Console.WriteLine();

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
        Console.WriteLine();

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
        Console.WriteLine();
        Console.WriteLine($"  Account:  {account.AccountNumber} ({account.AccountType})");
        Console.WriteLine($"  Owner:    {customer?.FullName ?? "Unknown"}");
        Console.WriteLine($"  Balance:  ${account.Balance:N2}");
        Console.WriteLine($"  Status:   {account.Status}");
        Console.WriteLine();
        Console.WriteLine("  Available statuses:");
        Console.WriteLine("    1. Active");
        Console.WriteLine("    2. Frozen");
        Console.WriteLine("    3. Closed");
        Console.WriteLine();

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
            Console.WriteLine($"  Account is already {account.Status}.");
            Screen.PressAnyKey();
            return;
        }

        Console.WriteLine();
        if (!Screen.Confirm($"CHANGE STATUS FROM {account.Status.ToUpper()} TO {newStatus.ToUpper()}"))
        {
            Screen.WarningText("STATUS CHANGE CANCELLED");
            Screen.PressAnyKey();
            return;
        }

        db.UpdateAccountStatus(account.AccountNumber, newStatus);
        db.AddNote(account.AccountNumber, teller.Username,
            $"Account status changed from {account.Status} to {newStatus} by {teller.DisplayName}");

        Console.WriteLine();
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
        Console.WriteLine();

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
                Console.WriteLine();
                for (int i = 0; i < results.Count; i++)
                    Console.WriteLine($"  {i + 1}. {results[i].CustomerId} - {results[i].FullName}");

                Console.WriteLine();
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

        Console.WriteLine();
        Console.WriteLine($"  Customer:  {customer.FullName} ({customer.CustomerId})");
        Console.WriteLine($"  Phone:     {customer.Phone}");
        Console.WriteLine($"  Address:   {customer.Address}");
        Console.WriteLine();
        Console.WriteLine("  Enter new values (press ENTER to keep current):");
        Console.WriteLine();

        var newPhone = Screen.Prompt($"  PHONE [{customer.Phone}]");
        if (string.IsNullOrWhiteSpace(newPhone)) newPhone = customer.Phone;

        var newAddress = Screen.Prompt($"  ADDRESS [{customer.Address}]");
        if (string.IsNullOrWhiteSpace(newAddress)) newAddress = customer.Address;

        if (newPhone == customer.Phone && newAddress == customer.Address)
        {
            Console.WriteLine("  No changes made.");
            Screen.PressAnyKey();
            return;
        }

        Console.WriteLine();
        Console.WriteLine("  Updated values:");
        if (newPhone != customer.Phone)
            Console.WriteLine($"    Phone:   {customer.Phone} → {newPhone}");
        if (newAddress != customer.Address)
            Console.WriteLine($"    Address: {customer.Address} → {newAddress}");
        Console.WriteLine();

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

        Console.WriteLine();
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
        Console.WriteLine();

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
        Console.WriteLine();
        Console.WriteLine($"  Account:  {account.AccountNumber} ({account.AccountType})");
        Console.WriteLine($"  Owner:    {customer?.FullName ?? "Unknown"}");
        Console.WriteLine($"  Status:   {account.Status}");
        Console.WriteLine();

        // Show existing notes
        var existingNotes = db.GetNotes(account.AccountNumber);
        if (existingNotes.Count > 0)
        {
            Console.WriteLine("  EXISTING NOTES:");
            foreach (var n in existingNotes.Take(3))
            {
                Console.WriteLine($"    [{n.CreatedDate}] {n.CreatedBy}: {n.NoteText}");
            }
            if (existingNotes.Count > 3)
                Console.WriteLine($"    ... and {existingNotes.Count - 3} more");
            Console.WriteLine();
        }

        var noteText = Screen.Prompt("NOTE TEXT");
        if (string.IsNullOrWhiteSpace(noteText))
        {
            Screen.ErrorText("NOTE TEXT CANNOT BE EMPTY");
            Screen.PressAnyKey();
            return;
        }

        db.AddNote(account.AccountNumber, teller.Username, noteText);

        Console.WriteLine();
        Screen.SuccessText("NOTE ADDED SUCCESSFULLY");
        Screen.PressAnyKey();
    }
}
