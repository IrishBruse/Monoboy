namespace Monoboy.Desktop.GuiDebugger;

using System;

using Raylib_cs;

/// <summary>Shared vertical scrollbar: wheel, track click, and thumb drag.</summary>
static class GuiScroll
{
    public const int Width = 10;
    const int WheelLines = 3;

    static int _dragId;
    static bool _dragging;

    public static float ConsumeWheel() => Raylib.GetMouseWheelMove();

    public static void ApplyWheel(Rectangle hit, float wheel, ref int scroll, int maxScroll)
    {
        if (wheel == 0 || maxScroll <= 0)
        {
            return;
        }

        if (!Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), hit))
        {
            return;
        }

        int delta = (int)MathF.Round(wheel * WheelLines);
        if (delta == 0)
        {
            delta = Math.Sign(wheel) * WheelLines;
        }

        scroll = Math.Clamp(scroll - delta, 0, maxScroll);
    }

    public static void Draw(int id, Rectangle bar, ref int scroll, int maxScroll)
    {
        Raylib.DrawRectangleRec(bar, GuiDebuggerTheme.ScrollbarTrack);
        if (bar.Height <= 4)
        {
            return;
        }

        int thumbH = maxScroll <= 0
            ? (int)bar.Height
            : Math.Max(16, (int)(bar.Height * bar.Height / (bar.Height + (maxScroll * 18))));
        thumbH = Math.Min(thumbH, (int)bar.Height);
        float t = maxScroll <= 0 ? 0 : Math.Clamp(scroll / (float)maxScroll, 0f, 1f);
        int thumbY = (int)bar.Y + (int)((bar.Height - thumbH) * t);
        var thumb = new Rectangle(bar.X + 1, thumbY, Math.Max(1, bar.Width - 2), thumbH);

        var mouse = Raylib.GetMousePosition();
        bool overThumb = Raylib.CheckCollisionPointRec(mouse, thumb);
        bool overBar = Raylib.CheckCollisionPointRec(mouse, bar);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && overBar)
        {
            _dragging = true;
            _dragId = id;
            if (!overThumb && maxScroll > 0)
            {
                float rel = (mouse.Y - bar.Y - (thumbH * 0.5f)) / Math.Max(1, bar.Height - thumbH);
                scroll = Math.Clamp((int)MathF.Round(rel * maxScroll), 0, maxScroll);
            }
        }

        if (_dragging && _dragId == id)
        {
            if (!Raylib.IsMouseButtonDown(MouseButton.Left) || maxScroll <= 0)
            {
                _dragging = false;
            }
            else
            {
                float rel = (mouse.Y - bar.Y - (thumbH * 0.5f)) / Math.Max(1, bar.Height - thumbH);
                scroll = Math.Clamp((int)MathF.Round(rel * maxScroll), 0, maxScroll);
            }
        }

        bool hot = overThumb || (_dragging && _dragId == id);
        Raylib.DrawRectangleRec(thumb, hot ? GuiDebuggerTheme.ScrollbarThumbHover : GuiDebuggerTheme.ScrollbarThumb);
    }
}
