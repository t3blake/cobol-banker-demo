using CobolBanker.Data;
using CobolBanker.Models;
using CobolBanker.Screens;

namespace CobolBanker.Commands;

public static class FundTransferCommand
{
    public static void Run(Database db, Teller teller)
    {
        while (true)
        {
            Screen.Header("FUND TRANSFER");
            Screen.EmptyRow();
            Screen.Row("Transfer funds between accounts");
            Screen.Row("Enter 0 at any prompt to cancel");
            Screen.EmptyRow();
            Screen.BottomBorder();
            Screen.PrintLine();

            // Step 1: Source account
            var sourceInput = Screen.Prompt("SOURCE ACCOUNT NUMBER");
            if (sourceInput == "0") return;

            var sourceAcct = db.GetAccount(sourceInput);
            if (sourceAcct == null)
            {
                Screen.ErrorText("SOURCE ACCOUNT NOT FOUND");
                Screen.PressAnyKey();
                continue;
            }

            if (sourceAcct.Status != "Active")
            {
                Screen.ErrorText($"SOURCE ACCOUNT IS {sourceAcct.Status.ToUpper()} - TRANSFER NOT ALLOWED");
                Screen.PressAnyKey();
                continue;
            }

            var sourceCustomer = db.GetCustomer(sourceAcct.CustomerId);
            Screen.PrintLine($"  Source: {sourceAcct.AccountNumber} ({sourceAcct.AccountType})");
            Screen.PrintLine($"  Owner:  {sourceCustomer?.FullName ?? "Unknown"}");
            Screen.PrintLine($"  Balance: ${sourceAcct.Balance:N2}");
            Screen.PrintLine();

            // Step 2: Destination account
            var destInput = Screen.Prompt("DESTINATION ACCOUNT NUMBER");
            if (destInput == "0") return;

            if (destInput == sourceInput)
            {
                Screen.ErrorText("SOURCE AND DESTINATION CANNOT BE THE SAME");
                Screen.PressAnyKey();
                continue;
            }

            var destAcct = db.GetAccount(destInput);
            if (destAcct == null)
            {
                Screen.ErrorText("DESTINATION ACCOUNT NOT FOUND");
                Screen.PressAnyKey();
                continue;
            }

            if (destAcct.Status != "Active")
            {
                Screen.ErrorText($"DESTINATION ACCOUNT IS {destAcct.Status.ToUpper()} - TRANSFER NOT ALLOWED");
                Screen.PressAnyKey();
                continue;
            }

            var destCustomer = db.GetCustomer(destAcct.CustomerId);
            Screen.PrintLine($"  Dest:   {destAcct.AccountNumber} ({destAcct.AccountType})");
            Screen.PrintLine($"  Owner:  {destCustomer?.FullName ?? "Unknown"}");
            Screen.PrintLine($"  Balance: ${destAcct.Balance:N2}");
            Screen.PrintLine();

            // Step 3: Amount
            var amountInput = Screen.Prompt("TRANSFER AMOUNT");
            if (amountInput == "0") return;

            // Strip $ if they typed it
            amountInput = amountInput.TrimStart('$');

            if (!decimal.TryParse(amountInput, out decimal amount) || amount <= 0)
            {
                Screen.ErrorText("INVALID AMOUNT - MUST BE A POSITIVE NUMBER");
                Screen.PressAnyKey();
                continue;
            }

            if (amount > sourceAcct.Balance)
            {
                Screen.ErrorText("INSUFFICIENT FUNDS IN SOURCE ACCOUNT");
                Screen.PressAnyKey();
                continue;
            }

            // Step 4: Confirmation screen
            Screen.PrintLine();
            Screen.Header("TRANSFER CONFIRMATION");
            Screen.EmptyRow();
            Screen.Row("FROM:");
            Screen.Row($"  {sourceAcct.AccountNumber} ({sourceAcct.AccountType})");
            Screen.Row($"  {sourceCustomer?.FullName ?? "Unknown"}");
            Screen.Row($"  Current Balance: ${sourceAcct.Balance:N2}");
            Screen.Row($"  New Balance:     ${sourceAcct.Balance - amount:N2}");
            Screen.EmptyRow();
            Screen.Row("TO:");
            Screen.Row($"  {destAcct.AccountNumber} ({destAcct.AccountType})");
            Screen.Row($"  {destCustomer?.FullName ?? "Unknown"}");
            Screen.Row($"  Current Balance: ${destAcct.Balance:N2}");
            Screen.Row($"  New Balance:     ${destAcct.Balance + amount:N2}");
            Screen.EmptyRow();
            Screen.Divider();
            Screen.Row($"TRANSFER AMOUNT: ${amount:N2}");
            Screen.EmptyRow();
            Screen.BottomBorder();
            Screen.PrintLine();

            if (!Screen.Confirm("CONFIRM TRANSFER"))
            {
                Screen.WarningText("TRANSFER CANCELLED");
                Screen.PressAnyKey();
                continue;
            }

            // Step 5: Execute
            try
            {
                db.TransferFunds(sourceAcct.AccountNumber, destAcct.AccountNumber, amount, teller.Username);
                Screen.PrintLine();
                Screen.SuccessText("TRANSFER COMPLETED SUCCESSFULLY");

                // Show updated balances
                var updatedSource = db.GetAccount(sourceAcct.AccountNumber)!;
                var updatedDest = db.GetAccount(destAcct.AccountNumber)!;
                Screen.PrintLine($"  {updatedSource.AccountNumber} New Balance: ${updatedSource.Balance:N2}");
                Screen.PrintLine($"  {updatedDest.AccountNumber} New Balance: ${updatedDest.Balance:N2}");
            }
            catch (Exception ex)
            {
                Screen.ErrorText($"TRANSFER FAILED: {ex.Message}");
            }

            Screen.PressAnyKey();
            return;
        }
    }
}
