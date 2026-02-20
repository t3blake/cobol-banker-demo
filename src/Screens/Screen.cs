using System.Reflection;

namespace CobolBanker.Screens;

/// <summary>
/// Core rendering utilities for the green-screen terminal aesthetic.
/// All screen output flows through here — ANSI green-on-black, box-drawing, fixed-width layouts.
/// </summary>
public static class Screen
{
    // ANSI escape codes
    private const string ESC = "\x1b[";
    private const string GREEN = $"{ESC}32m";
    private const string BRIGHT_GREEN = $"{ESC}92m";
    private const string DIM_GREEN = $"{ESC}2;32m";
    private const string RED = $"{ESC}91m";
    private const string YELLOW = $"{ESC}93m";
    private const string BLACK_BG = $"{ESC}40m";
    private const string RESET = $"{ESC}0m";
    private const string BOLD = $"{ESC}1m";
    private const string DIM = $"{ESC}2m";
    private const string CLEAR_SCREEN = $"{ESC}2J{ESC}H";

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

    /// <summary>
    /// Initialize the terminal for green-screen mode.
    /// </summary>
    public static void Init()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        // Enable virtual terminal processing on Windows
        Console.Write($"{BLACK_BG}{GREEN}");
        Console.CursorVisible = true;
        Clear();
    }

    /// <summary>
    /// Reset terminal to normal colors on exit.
    /// </summary>
    public static void Shutdown()
    {
        Console.Write(RESET);
        Console.Clear();
        Console.CursorVisible = true;
    }

    public static void Clear()
    {
        Console.Write($"{BLACK_BG}{GREEN}{CLEAR_SCREEN}");
    }

    // ── Box rendering ───────────────────────────────────────────────

    public static void TopBorder()
    {
        Console.WriteLine($"{BRIGHT_GREEN}{TL}{new string(H, WIDTH - 2)}{TR}{GREEN}");
    }

    public static void BottomBorder()
    {
        Console.WriteLine($"{BRIGHT_GREEN}{BL}{new string(H, WIDTH - 2)}{BR}{GREEN}");
    }

    public static void Divider()
    {
        Console.WriteLine($"{BRIGHT_GREEN}{ML}{new string(H, WIDTH - 2)}{MR}{GREEN}");
    }

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

        Console.WriteLine($"{BRIGHT_GREEN}{V}{GREEN}  {content}  {BRIGHT_GREEN}{V}{GREEN}");
    }

    public static void EmptyRow()
    {
        Row("");
    }

    // ── Styled text ─────────────────────────────────────────────────

    public static void BrightText(string text)
    {
        Console.Write($"{BRIGHT_GREEN}{BOLD}{text}{RESET}{BLACK_BG}{GREEN}");
    }

    public static void DimText(string text)
    {
        Console.Write($"{DIM_GREEN}{text}{RESET}{BLACK_BG}{GREEN}");
    }

    public static void ErrorText(string text)
    {
        Console.WriteLine($"{RED}  *** {text} ***{RESET}{BLACK_BG}{GREEN}");
    }

    public static void SuccessText(string text)
    {
        Console.WriteLine($"{BRIGHT_GREEN}{BOLD}  >>> {text} <<<{RESET}{BLACK_BG}{GREEN}");
    }

    public static void WarningText(string text)
    {
        Console.WriteLine($"{YELLOW}  !!! {text} !!!{RESET}{BLACK_BG}{GREEN}");
    }

    // ── Input ───────────────────────────────────────────────────────

    public static string Prompt(string label)
    {
        Console.Write($"{GREEN}{label}: {BRIGHT_GREEN}");
        var input = Console.ReadLine()?.Trim() ?? "";
        Console.Write(GREEN);
        return input;
    }

    public static string PromptPassword(string label)
    {
        Console.Write($"{GREEN}{label}: {BRIGHT_GREEN}");
        var password = "";
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        }
        Console.Write(GREEN);
        return password;
    }

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
        Console.WriteLine();
        DimText("  Press any key to continue...");
        Console.ReadKey(intercept: true);
        Console.WriteLine();
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
        Console.WriteLine($"{BRIGHT_GREEN}{BOLD}{line}{RESET}{BLACK_BG}{GREEN}");
        Console.WriteLine($"{DIM_GREEN}{separator}{RESET}{BLACK_BG}{GREEN}");
    }

    public static void TableRow(params (string value, int width)[] columns)
    {
        var line = "  ";
        foreach (var (value, width) in columns)
        {
            var val = value.Length > width - 1 ? value[..(width - 2)] + "~" : value;
            line += val.PadRight(width);
        }
        Console.WriteLine(line);
    }
}
