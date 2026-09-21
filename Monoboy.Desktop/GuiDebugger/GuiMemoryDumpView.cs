namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.Text;

using Monoboy;

using Raylib_cs;

/// <summary>
/// Hex memory dump for the GUI debugger pane.
/// Theme: <see cref="GuiDebuggerTheme.PanelBackground"/>, <see cref="GuiDebuggerTheme.Value"/>,
/// <see cref="GuiDebuggerTheme.Label"/> (address column), <see cref="GuiDebuggerTheme.HexDimZero"/>,
/// <see cref="GuiDebuggerTheme.PanelBorder"/> (separators and scrollbar).
/// </summary>
public static class GuiMemoryDumpView
{
    const int FontSize = 13;
    const int LineHeight = 18;
    const int ScrollbarWidth = 8;
    const int BytesPerRow = 16;
    const int TotalRows = 0x10000 / BytesPerRow;

    static readonly Color AddressGrey = new(0x8A, 0x8A, 0x9E, 255);

    public static void Draw(Rectangle area, Emulator emulator, ref int scrollRows)
    {
        int areaX = (int)area.X;
        int areaY = (int)area.Y;
        int areaW = Math.Max(0, (int)area.Width);
        int areaH = Math.Max(0, (int)area.Height);
        if (areaW <= 0 || areaH <= 0)
        {
            return;
        }

        Raylib.DrawRectangle(areaX, areaY, areaW, areaH, GuiDebuggerTheme.PanelBackground);

        int visibleRows = Math.Max(1, areaH / LineHeight);
        int maxScroll = Math.Max(0, TotalRows - visibleRows);
        scrollRows = Math.Clamp(scrollRows, 0, maxScroll);

        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), area))
        {
            float wheel = Raylib.GetMouseWheelMove();
            if (wheel != 0)
            {
                scrollRows -= (int)Math.Sign(wheel);
                scrollRows = Math.Clamp(scrollRows, 0, maxScroll);
            }
        }

        Raylib.BeginScissorMode(areaX, areaY, areaW - ScrollbarWidth, areaH);
        for (int row = 0; row < visibleRows; row++)
        {
            int rowIndex = scrollRows + row;
            if (rowIndex >= TotalRows)
            {
                break;
            }

            int y = areaY + row * LineHeight;
            DrawHexRow(areaX + 2, y, emulator, (ushort)(rowIndex * BytesPerRow));
        }

        Raylib.EndScissorMode();

        DrawScrollbar(
            areaX + areaW - ScrollbarWidth,
            areaY,
            ScrollbarWidth,
            areaH,
            scrollRows,
            maxScroll);
    }

    static void DrawHexRow(int x, int y, Emulator emulator, ushort rowAddr)
    {
        int cursorX = x;

        string addr = $"{rowAddr:X4}  ";
        GuiDebuggerFont.Draw(addr, cursorX, y + 2, FontSize, AddressGrey);
        cursorX += GuiDebuggerFont.Measure(addr, FontSize);

        var ascii = new StringBuilder(BytesPerRow);
        for (int col = 0; col < BytesPerRow; col++)
        {
            if (col == 8)
            {
                DrawSeparator(ref cursorX, y, "| ");
            }

            ushort absAddr = (ushort)(rowAddr + col);
            byte val = emulator.Read(absAddr);
            Color byteColor = val == 0 ? GuiDebuggerTheme.HexDimZero : GuiDebuggerTheme.Value;
            string cell = $"{val:X2} ";
            GuiDebuggerFont.Draw(cell, cursorX, y + 2, FontSize, byteColor);
            cursorX += GuiDebuggerFont.Measure(cell, FontSize);

            if (val >= 0x20 && val <= 0x7E)
            {
                ascii.Append((char)val);
            }
            else
            {
                ascii.Append('.');
            }
        }

        DrawSeparator(ref cursorX, y, "| ");
        DrawAsciiRun(cursorX, y, ascii.ToString());
    }

    static void DrawSeparator(ref int cursorX, int y, string text)
    {
        GuiDebuggerFont.Draw(text, cursorX, y + 2, FontSize, GuiDebuggerTheme.PanelBorder);
        cursorX += GuiDebuggerFont.Measure(text, FontSize);
    }

    static void DrawAsciiRun(int x, int y, string run)
    {
        int cx = x;
        for (int i = 0; i < run.Length; i++)
        {
            char c = run[i];
            bool dim = c == '.';
            Color color = dim ? GuiDebuggerTheme.HexDimZero : GuiDebuggerTheme.Value;
            string ch = c.ToString();
            GuiDebuggerFont.Draw(ch, cx, y + 2, FontSize, color);
            cx += GuiDebuggerFont.Measure(ch, FontSize);
        }
    }

    static void DrawScrollbar(int x, int y, int width, int height, int scrollRows, int maxScrollRows)
    {
        Raylib.DrawRectangle(x, y, width, height, GuiDebuggerTheme.ScrollbarTrack);
        Raylib.DrawLine(x, y, x, y + height, GuiDebuggerTheme.PanelBorder);

        if (height <= 4 || maxScrollRows <= 0)
        {
            return;
        }

        int thumbH = Math.Max(8, height / 16);
        float t = Math.Clamp(scrollRows / (float)maxScrollRows, 0f, 1f);
        int thumbY = y + (int)((height - thumbH) * t);
        Raylib.DrawRectangle(x + 1, thumbY, Math.Max(1, width - 2), thumbH, GuiDebuggerTheme.ScrollbarThumb);
    }
}
