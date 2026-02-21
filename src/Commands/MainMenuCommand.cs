using CobolBanker.Models;
using CobolBanker.Screens;

namespace CobolBanker.Commands;

public static class MainMenuCommand
{
    public static int Run(Teller teller)
    {
        Screen.Header("COBOL BANKER — MAIN TERMINAL", $"WOODGROVE BANK  {Screen.AppVersion}");
        Screen.EmptyRow();
        Screen.Row("1. Customer Lookup");
        Screen.Row("2. Account Inquiry");
        Screen.Row("3. Fund Transfer");
        Screen.Row("4. Account Maintenance");
        Screen.Row("5. Transaction History");
        Screen.EmptyRow();
        Screen.Row("8. System Administration");
        Screen.Row("9. Log Off");
        Screen.EmptyRow();
        Screen.StatusBar(teller.DisplayName, teller.Branch);
        Screen.PrintLine();

        return Screen.MenuChoice("ENTER SELECTION", 1, 9);
    }
}
