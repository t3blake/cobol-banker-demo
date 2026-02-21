using CobolBanker.Data;
using CobolBanker.Models;
using CobolBanker.Screens;

namespace CobolBanker.Commands;

public static class LoginCommand
{
    public static Teller? Run(Database db)
    {
        const int MAX_ATTEMPTS = 3;

        for (int attempt = 1; attempt <= MAX_ATTEMPTS; attempt++)
        {
            Screen.Clear();
            Screen.TopBorder();
            Screen.Row("COBOL BANKER", centered: true);
            Screen.Row("WOODGROVE BANK", centered: true);
            Screen.Row($"{Screen.AppVersion}", centered: true);
            Screen.Divider();
            Screen.EmptyRow();
            Screen.Row("TELLER AUTHENTICATION REQUIRED");
            Screen.EmptyRow();
            Screen.BottomBorder();
            Screen.PrintLine();

            var username = Screen.Prompt("  USERID");
            var password = Screen.PromptPassword("  PASSWORD");

            var teller = db.AuthenticateTeller(username, password);
            if (teller != null)
            {
                Screen.PrintLine();
                Screen.SuccessText("ACCESS GRANTED");
                Screen.PrintLine($"  Welcome, {teller.DisplayName}");
                Screen.PrintLine($"  Branch: {teller.Branch}");
                Screen.PrintLine($"  Login Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss}");
                Screen.PressAnyKey();
                return teller;
            }

            Screen.PrintLine();
            Screen.ErrorText($"INVALID CREDENTIALS (ATTEMPT {attempt}/{MAX_ATTEMPTS})");
            if (attempt < MAX_ATTEMPTS)
                Screen.PressAnyKey();
        }

        Screen.PrintLine();
        Screen.ErrorText("MAXIMUM ATTEMPTS EXCEEDED - TERMINAL LOCKED");
        Screen.PressAnyKey();
        return null;
    }
}
