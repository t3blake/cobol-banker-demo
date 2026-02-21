using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CobolBanker.Commands;
using CobolBanker.Data;
using CobolBanker.Terminal;

namespace CobolBanker;

public partial class MainWindow : Window
{
    private TaskCompletionSource<string>? _inputTcs;
    private TaskCompletionSource<bool>? _keyTcs;
    private bool _passwordMode;
    private string _realPassword = "";
    private bool _waitingForKey;

    // Green-screen color palette
    private static readonly SolidColorBrush GreenBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x33, 0xFF, 0x33)));
    private static readonly SolidColorBrush BrightGreenBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x66, 0xFF, 0x66)));
    private static readonly SolidColorBrush DimGreenBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x00, 0x99, 0x00)));
    private static readonly SolidColorBrush RedBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xFF, 0x44, 0x44)));
    private static readonly SolidColorBrush YellowBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00)));

    private static SolidColorBrush Freeze(SolidColorBrush brush) { brush.Freeze(); return brush; }

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Wire up the terminal service callbacks
        TerminalService.WriteAction = OnWrite;
        TerminalService.ClearAction = OnClear;
        TerminalService.ReadLineFunc = OnReadLine;
        TerminalService.ReadPasswordFunc = OnReadPassword;
        TerminalService.ReadKeyAction = OnReadKey;

        // Start the application logic on a background thread
        var appThread = new Thread(RunApp) { IsBackground = true, Name = "AppLogic" };
        appThread.Start();
    }

    // ── Application logic (runs on background thread) ────────────

    private void RunApp()
    {
        try
        {
            using var db = new Database();

            while (true)
            {
                var teller = LoginCommand.Run(db);
                if (teller == null) break;

                var running = true;
                while (running)
                {
                    var choice = MainMenuCommand.Run(teller);
                    switch (choice)
                    {
                        case 1: CustomerLookupCommand.Run(db, teller); break;
                        case 2: AccountInquiryCommand.Run(db, teller); break;
                        case 3: FundTransferCommand.Run(db, teller); break;
                        case 4: AccountMaintenanceCommand.Run(db, teller); break;
                        case 5: TransactionHistoryCommand.Run(db, teller); break;
                        case 8: SystemAdminCommand.Run(db, teller); break;
                        case 9:
                            Screens.Screen.Clear();
                            Screens.Screen.SuccessText("LOGGED OFF SUCCESSFULLY");
                            Screens.Screen.PressAnyKey();
                            running = false;
                            break;
                        default:
                            Screens.Screen.ErrorText("FUNCTION NOT AVAILABLE");
                            Screens.Screen.PressAnyKey();
                            break;
                    }
                }
            }
        }
        catch (Exception) { /* Window closing or fatal error */ }

        // Close the window when app logic exits
        try { Dispatcher.BeginInvoke(Close); } catch { }
    }

    // ── Terminal service callbacks ────────────────────────────────

    private void OnWrite(string text, TermColor color)
    {
        Dispatcher.BeginInvoke(() =>
        {
            var brush = color switch
            {
                TermColor.BrightGreen => BrightGreenBrush,
                TermColor.DimGreen => DimGreenBrush,
                TermColor.Red => RedBrush,
                TermColor.Yellow => YellowBrush,
                _ => GreenBrush
            };

            OutputText.Inlines.Add(new Run(text) { Foreground = brush });
            OutputScroll.ScrollToEnd();
        });
    }

    private void OnClear()
    {
        Dispatcher.Invoke(() => OutputText.Inlines.Clear());
    }

    private string OnReadLine(string prompt)
    {
        _inputTcs = new TaskCompletionSource<string>();
        _passwordMode = false;

        Dispatcher.BeginInvoke(() =>
        {
            PromptLabel.Text = prompt;
            InputBox.Clear();
            InputPanel.Visibility = Visibility.Visible;
            OutputScroll.ScrollToEnd();

            // Schedule keyboard focus after WPF finishes layout for the newly-visible panel
            Dispatcher.BeginInvoke(DispatcherPriority.Input, () => Keyboard.Focus(InputBox));
        });

        return _inputTcs.Task.Result;
    }

    private string OnReadPassword(string prompt)
    {
        _inputTcs = new TaskCompletionSource<string>();
        _passwordMode = true;
        _realPassword = "";

        Dispatcher.BeginInvoke(() =>
        {
            PromptLabel.Text = prompt;
            InputBox.Clear();
            InputPanel.Visibility = Visibility.Visible;
            OutputScroll.ScrollToEnd();

            Dispatcher.BeginInvoke(DispatcherPriority.Input, () => Keyboard.Focus(InputBox));
        });

        return _inputTcs.Task.Result;
    }

    private void OnReadKey()
    {
        _keyTcs = new TaskCompletionSource<bool>();

        Dispatcher.BeginInvoke(() =>
        {
            _waitingForKey = true;
            InputPanel.Visibility = Visibility.Collapsed;
            Focus();
        });

        _keyTcs.Task.Wait();
    }

    // ── Input event handlers ─────────────────────────────────────

    private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var text = _passwordMode ? _realPassword : InputBox.Text?.Trim() ?? "";
            var echoText = _passwordMode ? new string('*', _realPassword.Length) : text;

            // Echo prompt + input to the output area
            OutputText.Inlines.Add(new Run(PromptLabel.Text + echoText + "\n") { Foreground = GreenBrush });
            InputPanel.Visibility = Visibility.Collapsed;
            InputBox.Clear();
            _realPassword = "";
            OutputScroll.ScrollToEnd();

            _inputTcs?.TrySetResult(text);
            e.Handled = true;
        }
        else if (_passwordMode && e.Key == Key.Back)
        {
            if (_realPassword.Length > 0)
            {
                _realPassword = _realPassword[..^1];
                if (InputBox.Text.Length > 0)
                {
                    InputBox.Text = InputBox.Text[..^1];
                    InputBox.CaretIndex = InputBox.Text.Length;
                }
            }
            e.Handled = true;
        }
    }

    private void InputBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (_passwordMode)
        {
            _realPassword += e.Text;
            InputBox.Text += new string('*', e.Text.Length);
            InputBox.CaretIndex = InputBox.Text.Length;
            e.Handled = true;
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_waitingForKey)
        {
            _waitingForKey = false;
            _keyTcs?.TrySetResult(true);
            e.Handled = true;
        }
    }

    private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_waitingForKey)
        {
            _waitingForKey = false;
            _keyTcs?.TrySetResult(true);
            e.Handled = true;
        }
        else if (InputPanel.Visibility == Visibility.Visible)
        {
            // Click anywhere → redirect focus to the input box
            Keyboard.Focus(InputBox);
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // Unblock any waiting input so the background thread can exit
        _inputTcs?.TrySetResult("");
        _keyTcs?.TrySetResult(true);
    }
}
