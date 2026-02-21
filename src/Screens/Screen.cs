using System.Reflection;
using CobolBanker.Terminal;

namespace CobolBanker.Screens;

/// <summary>
/// Core rendering utilities for the green-screen terminal aesthetic.
/// All screen output flows through TerminalService → WPF MainWindow.
/// </summary>
public static class Screen
{
    public const int WIDTH = 60;

    // Box-drawing characters
    private const char TL = '╔'; // top-left
    private const char TR = '╗'; // top-right
    private const char BL = '╚'; // bottom-left
    private const char BR = '╝'; // bottom-right
    private const char H = '═';  // horizontal
    private const char V = '║';  // vertical
    private const char ML = '╠'; // middle-left
    private const char MR = '╣'; // middle-right

    public static string AppVersion
    {
        get
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v1.0.0";
        }
    }

    public static void Clear() => TerminalService.Clear();

    // ── Box rendering ───────────────────────────────────────────────

    public static void TopBorder()
        => TerminalService.WriteLine($"{TL}{new string(H, WIDTH - 2)}{TR}", TermColor.BrightGreen);

    public static void BottomBorder()
        => TerminalService.WriteLine($"{BL}{new string(H, WIDTH - 2)}{BR}", TermColor.BrightGreen);

    public static void Divider()
        => TerminalService.WriteLine($"{ML}{new string(H, WIDTH - 2)}{MR}", TermColor.BrightGreen);

    public static void Row(string text, bool centered = false)
    {
        string content;
        if (centered)
        {
            int pad = WIDTH - 4 - text.Length;
            int leftPad = pad / 2;
            int rightPad = pad - leftPad;
            content = $"{new string(' ', leftPad)}{text}{new string(' ', rightPad)}";
        }
        else
        {
            content = text.PadRight(WIDTH - 4);
        }

        // Truncate if too long
        if (content.Length > WIDTH - 4)
            content = content[..(WIDTH - 4)];

        TerminalService.Write($"{V}", TermColor.BrightGreen);
        TerminalService.Write($"  {content}  ");
        TerminalService.WriteLine($"{V}", TermColor.BrightGreen);
    }

    public static void EmptyRow() => Row("");

    // ── Styled text ─────────────────────────────────────────────────

    public static void ErrorText(string text)
        => TerminalService.WriteLine($"  *** {text} ***", TermColor.Red);

    public static void SuccessText(string text)
        => TerminalService.WriteLine($"  >>> {text} <<<", TermColor.BrightGreen);

    public static void WarningText(string text)
        => TerminalService.WriteLine($"  !!! {text} !!!", TermColor.Yellow);

    // ── Console-replacement convenience methods ─────────────────────

    public static void Print(string text)
        => TerminalService.Write(text);

    public static void PrintLine(string text = "")
        => TerminalService.WriteLine(text);

    // ── Input ───────────────────────────────────────────────────────

    public static string Prompt(string label)
        => TerminalService.ReadLine($"{label}: ");

    public static string PromptPassword(string label)
        => TerminalService.ReadPassword($"{label}: ");

    public static int MenuChoice(string prompt, int min, int max)
    {
        while (true)
        {
            var input = Prompt(prompt);
            if (int.TryParse(input, out int choice) && choice >= min && choice <= max)
                return choice;
            ErrorText($"INVALID SELECTION - ENTER {min}-{max}");
        }
    }

    public static bool Confirm(string message)
    {
        var input = Prompt($"{message} (Y/N)");
        return input.Equals("Y", StringComparison.OrdinalIgnoreCase);
    }

    public static void PressAnyKey()
    {
        TerminalService.WriteLine();
        TerminalService.WriteLine("  Press any key to continue...", TermColor.DimGreen);
        TerminalService.ReadKey();
    }

    // ── Header/footer helpers ───────────────────────────────────────

    public static void Header(string title, string? subtitle = null)
    {
        Clear();
        TopBorder();
        Row(title, centered: true);
        if (subtitle != null)
            Row(subtitle, centered: true);
        Divider();
    }

    public static void StatusBar(string tellerName, string branch)
    {
        var date = DateTime.Now.ToString("MM/dd/yy");
        var status = $"Teller: {tellerName} | Branch: {branch} | {date}";
        Divider();
        Row(status);
        BottomBorder();
    }

    // ── Table rendering ─────────────────────────────────────────────

    public static void TableHeader(params (string label, int width)[] columns)
    {
        var line = "  ";
        var separator = "  ";
        foreach (var (label, width) in columns)
        {
            line += label.PadRight(width);
            separator += new string('-', width);
        }
        TerminalService.WriteLine(line, TermColor.BrightGreen);
        TerminalService.WriteLine(separator, TermColor.DimGreen);
    }

    public static void TableRow(params (string value, int width)[] columns)
    {
        var line = "  ";
        foreach (var (value, width) in columns)
        {
            var val = value.Length > width - 1 ? value[..(width - 2)] + "~" : value;
            line += val.PadRight(width);
        }
        TerminalService.WriteLine(line);
    }
}
