namespace Monoboy.Desktop.Debugger;

using Raylib_cs;

/// <summary>Monospace font for debugger panes; set once from <see cref="GuiDebugger"/>.</summary>
internal static class GuiDebuggerFont
{
    public static Font Font;
    public static bool UseMono;

    public static void Draw(string text, int x, int y, int size, Color color)
    {
        if (!UseMono)
        {
            Raylib.DrawText(text, x, y, size, color);
            return;
        }

        Raylib.DrawTextEx(Font, text, new(x, y), size, 1, color);
    }

    public static int Measure(string text, int size) =>
        UseMono
            ? (int)Raylib.MeasureTextEx(Font, text, size, 1).X
            : Raylib.MeasureText(text, size);
}
