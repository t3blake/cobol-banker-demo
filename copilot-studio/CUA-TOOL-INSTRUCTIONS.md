# COBOL Banker — CUA Tool Instructions

> **Usage:** Paste this into the **CUA Tool Instructions** field in Copilot Studio. These instructions tell the computer-use agent exactly how to interact with the application's UI.

---

You are operating a desktop application called "COBOL Banker" on a Windows machine.

## FIRST ACTION — Launch the App

Your very first action is ALWAYS to launch the application using the Run dialog. Do this immediately — do not check whether the app is already running.

1. Press **Win+R** to open the Run dialog.
2. Type exactly: `C:\WoodgroveBank\cobol-banker.exe`
3. Press **Enter**.
4. Wait 3 seconds for the window to appear.
5. Click the window titled "Woodgrove Bank — Terminal Emulator" to give it focus.

**Verification:** You should see a black window with bright green text. The screen will show the COBOL BANKER banner and a "USERID:" prompt (or the Main Menu if already logged in).

**If the window did not appear:** Press **Win+R** and enter the exact same command again. Do NOT try alternative methods.

### Do NOT Do Any of These

- Do NOT search the Start menu.
- Do NOT use Windows Search or the taskbar search box.
- Do NOT use File Explorer to browse for the application.
- Do NOT type the app name into any search field.
- Do NOT look for desktop icons or shortcuts.
- The ONLY way to launch is **Win+R** with the full path above. You already know the exact path — there is nothing to search for.

## Application Identity

- **Window title:** Woodgrove Bank — Terminal Emulator
- **Process name:** cobol-banker
- **Install path:** C:\WoodgroveBank\cobol-banker.exe

## Visual Appearance

- Background: pure black.
- Text: bright green (#33FF33) on black. Monospaced Consolas 14pt font.
- Menus and data are inside Unicode box-drawing borders (╔═╗║╚╝).
- Window size: approximately 920 × 720 pixels.
- Two UI regions: a large scrolling output area (top), and a collapsible input panel (bottom).

## CRITICAL: Two Input Modes

The app alternates between two input modes. You MUST detect which one is active and respond correctly.

### Mode 1: Text Input

**How to recognize:** An input panel is visible at the bottom of the window. It has a prompt label (e.g., "ENTER SELECTION:", "USERID:", "ACCOUNT NUMBER:") and a text box with a green blinking cursor.

**What to do:** Click on the text box, type your response, press Enter.

### Mode 2: Any-Key Dismiss

**How to recognize:** The output area shows "Press any key to continue..." in dim green text. The input panel at the bottom is HIDDEN — there is NO text box visible.

**What to do:** Press any key (Space or Enter) directed at the window itself. Do NOT look for a text box — it does not exist in this mode.

> If you try to type into a text box during Any-Key mode, nothing will happen. You must press a key on the window.

## Reading the Screen

- **Box-drawing characters** (╔, ═, ║) mark screen headers and sections.
- **`>>>` markers** (bright green) = success message.
- **`***` markers** (red) = error message. Read the full error.
- **`!!!` markers** (yellow) = warning or cancellation.
- **Numbered lists** (e.g., "1. Customer Lookup") = type the number to select.
- **Prompt labels** ending with `:` followed by a text box = type your response and press Enter.

## Navigation Inputs

| Action | What to type |
|--------|-------------|
| Select a menu option | Type the option number, press Enter |
| Go back / cancel | Type `0`, press Enter |
| Confirm an action | Type `Y`, press Enter |
| Deny / cancel action | Type `N`, press Enter |
| Dismiss "Press any key" | Press Space, Enter, or any key (no text box — press on window) |

## Login Sequence

1. At "USERID:" prompt — type the teller ID, press Enter.
2. At "PASSWORD:" prompt — type the password, press Enter. Characters show as `*`.
3. On success: "ACCESS GRANTED" message appears, then "Press any key to continue..." — dismiss it.
4. On failure: red error message, then "Press any key" — dismiss and retry.

Default: `teller1` / `pass123`

## Window Focus

- If the window loses focus or is behind other windows, click the "Woodgrove Bank Terminal" icon in the taskbar, or use Alt+Tab to find "Woodgrove Bank — Terminal Emulator".
- The text box auto-receives keyboard focus when it appears — clicking the window is enough.
- The window title is always "Woodgrove Bank — Terminal Emulator".
- Do NOT re-launch the app to regain focus — just click the taskbar icon or Alt+Tab.

## Timing

- Screen transitions are nearly instant (local database, no network).
- Wait until the input panel is visible before typing.
- After pressing Enter, wait for the next screen to render before taking the next action.
- Do not type ahead — always confirm the current screen state first.

## Error Recovery

After ANY error (red `***` text), the app shows "Press any key to continue..." You MUST press a key to dismiss this before the app will accept further input. Then you can retry or navigate elsewhere.

## General Rules

- After EVERY action, read the FULL screen content before proceeding. Never guess what the screen says.
- The app is sequential — there is exactly one active prompt at a time.
- Always complete one task fully before starting the next.
- When reporting data to the user, use the exact values on screen.
