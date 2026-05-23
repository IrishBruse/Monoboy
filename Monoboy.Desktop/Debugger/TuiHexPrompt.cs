namespace Monoboy.Desktop.Debugger;

using System;
using System.Globalization;
using System.Text;

/// <summary>Read a 16-bit hex value on the footer row.</summary>
static class TuiHexPrompt
{
    public static bool TryReadAddress(string prompt, out ushort address)
    {
        address = 0;
        int row = Math.Max(0, Console.WindowHeight - 1);
        int width = Math.Max(1, Console.WindowWidth);
        Console.SetCursorPosition(0, row);
        Console.CursorVisible = true;
        Console.Write(prompt);
        Console.Write(new string(' ', Math.Max(0, width - prompt.Length)));

        Console.SetCursorPosition(prompt.Length, row);
        var input = new StringBuilder();

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.CursorVisible = false;
                if (input.Length == 0)
                {
                    return false;
                }

                return ushort.TryParse(
                    input.ToString(),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out address);
            }

            if (key.Key == ConsoleKey.Escape)
            {
                Console.CursorVisible = false;
                return false;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (input.Length == 0)
                {
                    continue;
                }

                input.Length--;
                int x = prompt.Length + input.Length;
                Console.SetCursorPosition(x, row);
                Console.Write(' ');
                Console.SetCursorPosition(x, row);
                continue;
            }

            char c = key.KeyChar;
            if (!IsHexDigit(c))
            {
                continue;
            }

            if (input.Length >= 4)
            {
                continue;
            }

            input.Append(char.ToUpperInvariant(c));
            Console.Write(char.ToUpperInvariant(c));
        }
    }

    static bool IsHexDigit(char c) =>
        c is >= '0' and <= '9'
            or >= 'a' and <= 'f'
            or >= 'A' and <= 'F';
}
