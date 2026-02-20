using CobolBanker.Commands;
using CobolBanker.Data;
using CobolBanker.Screens;

// ── COBOL Banker — Legacy Banking Terminal Simulator ───────────────
// Entry point: Initialize terminal, login, main menu loop.

Screen.Init();

try
{
    using var db = new Database();

    // Login loop — restart login on logoff
    while (true)
    {
        var teller = LoginCommand.Run(db);
        if (teller == null)
        {
            // Failed login — exit app
            break;
        }

        // Main menu loop
        var running = true;
        while (running)
        {
            var choice = MainMenuCommand.Run(teller);

            switch (choice)
            {
                case 1:
                    CustomerLookupCommand.Run(db, teller);
                    break;
                case 2:
                    AccountInquiryCommand.Run(db, teller);
                    break;
                case 3:
                    FundTransferCommand.Run(db, teller);
                    break;
                case 4:
                    AccountMaintenanceCommand.Run(db, teller);
                    break;
                case 5:
                    TransactionHistoryCommand.Run(db, teller);
                    break;
                case 8:
                    SystemAdminCommand.Run(db, teller);
                    break;
                case 9:
                    // Log off — back to login screen
                    Screen.Clear();
                    Screen.SuccessText("LOGGED OFF SUCCESSFULLY");
                    Screen.PressAnyKey();
                    running = false;
                    break;
                default:
                    Screen.ErrorText("FUNCTION NOT AVAILABLE");
                    Screen.PressAnyKey();
                    break;
            }
        }
    }
}
catch (Exception ex)
{
    Screen.ErrorText($"SYSTEM ERROR: {ex.Message}");
    Screen.PressAnyKey();
}
finally
{
    Screen.Shutdown();
}
