using CobolBanker.Data;
using CobolBanker.Models;
using CobolBanker.Screens;

namespace CobolBanker.Commands;

public static class SystemAdminCommand
{
    public static void Run(Database db, Teller teller)
    {
        while (true)
        {
            Screen.Header("SYSTEM ADMINISTRATION");
            Screen.EmptyRow();
            Screen.Row("1. Reset Database to Defaults");
            Screen.Row("2. About / Version Info");
            Screen.EmptyRow();
            Screen.Row("0. Return to Main Menu");
            Screen.EmptyRow();
            Screen.BottomBorder();
            Screen.PrintLine();

            var choice = Screen.MenuChoice("ENTER SELECTION", 0, 2);

            switch (choice)
            {
                case 0: return;
                case 1: ResetDatabase(db); break;
                case 2: ShowAbout(db); break;
            }
        }
    }

    private static void ResetDatabase(Database db)
    {
        Screen.Header("DATABASE RESET");
        Screen.EmptyRow();
        Screen.Row("WARNING: This will erase ALL data and");
        Screen.Row("restore the database to its original");
        Screen.Row("demo state. This cannot be undone.");
        Screen.EmptyRow();
        Screen.BottomBorder();
        Screen.PrintLine();

        if (!Screen.Confirm("ARE YOU SURE YOU WANT TO RESET"))
        {
            Screen.WarningText("RESET CANCELLED");
            Screen.PressAnyKey();
            return;
        }

        // Double confirm — this is destructive
        Screen.PrintLine();
        if (!Screen.Confirm("FINAL CONFIRMATION - RESET ALL DATA"))
        {
            Screen.WarningText("RESET CANCELLED");
            Screen.PressAnyKey();
            return;
        }

        Screen.PrintLine();
        Screen.PrintLine("  Resetting database...");
        db.ResetDatabase();

        Screen.PrintLine();
        Screen.SuccessText("DATABASE RESET COMPLETE");
        Screen.PrintLine("  All data restored to demo defaults.");
        Screen.PressAnyKey();
    }

    private static void ShowAbout(Database db)
    {
        var schemaVersion = db.GetSchemaVersion();

        Screen.Header("ABOUT COBOL BANKER");
        Screen.EmptyRow();
        Screen.Row("COBOL BANKER", centered: true);
        Screen.Row("Legacy Banking Terminal Simulator", centered: true);
        Screen.EmptyRow();
        Screen.Divider();
        Screen.EmptyRow();
        Screen.Row($"Application Version:  {Screen.AppVersion}");
        Screen.Row($"Database Schema:      v{schemaVersion}");
        Screen.Row($"Runtime:              .NET {Environment.Version}");
        Screen.Row($"Platform:             {Environment.OSVersion}");
        Screen.EmptyRow();
        Screen.Divider();
        Screen.EmptyRow();
        Screen.Row("A demo tool for showcasing how AI");
        Screen.Row("agents interact with legacy terminal");
        Screen.Row("applications. Not a real banking system.");
        Screen.EmptyRow();
        Screen.Row("All data is fictional. No real PII.");
        Screen.EmptyRow();
        Screen.BottomBorder();
        Screen.PressAnyKey();
    }
}
