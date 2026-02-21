namespace CobolBanker.Terminal;

/// <summary>
/// Color palette for terminal output — maps to green-screen aesthetic.
/// </summary>
public enum TermColor
{
    Green,       // Normal text
    BrightGreen, // Borders, bold text, success
    DimGreen,    // Muted/dim text
    Red,         // Errors
    Yellow       // Warnings
}

/// <summary>
/// Static bridge between the app logic thread (commands, screens)
/// and the WPF UI. All terminal I/O flows through here.
/// </summary>
public static class TerminalService
{
    // These are set by MainWindow on startup
    public static Action<string, TermColor>? WriteAction;
    public static Action? ClearAction;
    public static Func<string, string>? ReadLineFunc;
    public static Func<string, string>? ReadPasswordFunc;
    public static Action? ReadKeyAction;

    public static void Write(string text, TermColor color = TermColor.Green)
        => WriteAction?.Invoke(text, color);

    public static void WriteLine(string text = "", TermColor color = TermColor.Green)
        => WriteAction?.Invoke(text + "\n", color);

    public static void Clear()
        => ClearAction?.Invoke();

    public static string ReadLine(string prompt)
        => ReadLineFunc?.Invoke(prompt) ?? "";

    public static string ReadPassword(string prompt)
        => ReadPasswordFunc?.Invoke(prompt) ?? "";

    public static void ReadKey()
        => ReadKeyAction?.Invoke();
}
