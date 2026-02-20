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
            Screen.Row("FIRST NATIONAL BANK", centered: true);
            Screen.Row($"{Screen.AppVersion}", centered: true);
            Screen.Divider();
            Screen.EmptyRow();
            Screen.Row("TELLER AUTHENTICATION REQUIRED");
            Screen.EmptyRow();
            Screen.BottomBorder();
            Console.WriteLine();

            var username = Screen.Prompt("  USERID");
            var password = Screen.PromptPassword("  PASSWORD");

            var teller = db.AuthenticateTeller(username, password);
            if (teller != null)
            {
                Console.WriteLine();
                Screen.SuccessText("ACCESS GRANTED");
                Console.WriteLine($"  Welcome, {teller.DisplayName}");
                Console.WriteLine($"  Branch: {teller.Branch}");
                Console.WriteLine($"  Login Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss}");
                Screen.PressAnyKey();
                return teller;
            }

            Console.WriteLine();
            Screen.ErrorText($"INVALID CREDENTIALS (ATTEMPT {attempt}/{MAX_ATTEMPTS})");
            if (attempt < MAX_ATTEMPTS)
                Screen.PressAnyKey();
        }

        Console.WriteLine();
        Screen.ErrorText("MAXIMUM ATTEMPTS EXCEEDED - TERMINAL LOCKED");
        Screen.PressAnyKey();
        return null;
    }
}
