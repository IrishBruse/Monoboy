namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.Numerics;

using Raylib_cs;

/// <summary>Flat dark dropdown used by the debugger toolbar (Run / PPU).</summary>
static class GuiDebuggerMenu
{
    public const int RowHeight = 24;
    public const int FontSize = 13;

    public static int MeasureWidth(string[] labels, string[] shortcuts)
    {
        int max = 0;
        for (int i = 0; i < labels.Length; i++)
        {
            int w = GuiDebuggerFont.Measure(labels[i], FontSize)
                + 28
                + GuiDebuggerFont.Measure(shortcuts[i], FontSize)
                + 16;
            if (w > max)
            {
                max = w;
            }
        }

        return Math.Max(180, max);
    }

    public static Rectangle Bounds(int x, int y, int width, int count) =>
        new(x, y, width, count * RowHeight);

    /// <returns>Hover row index, or -1.</returns>
    public static int Draw(Rectangle bounds, string[] labels, string[] shortcuts, Vector2 mouse)
    {
        Raylib.DrawRectangleRec(bounds, GuiDebuggerTheme.MenuBackground);

        int hoverIndex = -1;
        for (int i = 0; i < labels.Length; i++)
        {
            var row = new Rectangle(bounds.X, bounds.Y + (i * RowHeight), bounds.Width, RowHeight);
            bool hover = Raylib.CheckCollisionPointRec(mouse, row);
            if (hover)
            {
                hoverIndex = i;
                Raylib.DrawRectangleRec(row, GuiDebuggerTheme.MenuHover);
            }

            int textY = (int)row.Y + ((RowHeight - FontSize) / 2);
            GuiDebuggerFont.Draw(labels[i], (int)row.X + 10, textY, FontSize, GuiDebuggerTheme.Value);
            int sw = GuiDebuggerFont.Measure(shortcuts[i], FontSize);
            GuiDebuggerFont.Draw(shortcuts[i], (int)(row.X + row.Width - sw - 10), textY, FontSize, GuiDebuggerTheme.Label);
        }

        return hoverIndex;
    }

    public static int HitRow(Rectangle bounds, int count, Vector2 mouse)
    {
        if (!Raylib.CheckCollisionPointRec(mouse, bounds))
        {
            return -1;
        }

        int row = (int)((mouse.Y - bounds.Y) / RowHeight);
        if (row < 0 || row >= count)
        {
            return -1;
        }

        return row;
    }
}
