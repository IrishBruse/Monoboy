namespace Monoboy.Desktop.GuiDebugger;

using System;

using Raylib_cs;

/// <summary>Monospace font for debugger panes; set once from <see cref="GuiDebugger"/>.</summary>
internal static class GuiDebuggerFont
{
    public const int Spacing = 0;

    public static Font Font;
    public static bool UseMono;

    public static void Draw(string text, int x, int y, int size, Color color)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (!UseMono)
        {
            Raylib.DrawText(text, x, y, size, color);
            return;
        }

        Raylib.DrawTextEx(Font, text, new(x, y), size, Spacing, color);
    }

    public static int Measure(string text, int size)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        return UseMono
            ? (int)MathF.Ceiling(Raylib.MeasureTextEx(Font, text, size, Spacing).X)
            : Raylib.MeasureText(text, size);
    }

    public static int CharWidth(int size)
    {
        int w = Measure("0", size);
        return w > 0 ? w : Math.Max(1, size * 6 / 10);
    }
}
