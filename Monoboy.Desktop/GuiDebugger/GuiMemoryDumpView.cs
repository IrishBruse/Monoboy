namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.Numerics;
using System.Text;

using ImGuiNET;

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
    const int BytesPerRow = 16;
    const int TotalRows = 0x10000 / BytesPerRow;

    static Vector4 C(Color color) =>
        new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    public static void Draw(Emulator emulator)
    {
        Vector2 avail = ImGui.GetContentRegionAvail();
        if (avail.X <= 0 || avail.Y <= 0)
        {
            return;
        }

        ImGui.BeginChild("MemoryDump", new Vector2(Math.Max(1f, avail.X), Math.Max(1f, avail.Y)), ImGuiChildFlags.None);
        float lineH = ImGui.GetTextLineHeightWithSpacing();

        ImGuiListClipperPtr clipper = GuiImGuiClipper.Create();
        try
        {
            clipper.Begin(TotalRows, lineH);
            while (clipper.Step())
            {
                for (int rowIndex = clipper.DisplayStart; rowIndex < clipper.DisplayEnd; rowIndex++)
                {
                    DrawHexRow(emulator, (ushort)(rowIndex * BytesPerRow));
                }
            }

            clipper.End();
        }
        finally
        {
            clipper.Destroy();
        }
        ImGui.EndChild();
    }

    static void DrawHexRow(Emulator emulator, ushort rowAddr)
    {
        DrawRun($"{rowAddr:X4}  ", GuiDebuggerTheme.Address);

        var ascii = new StringBuilder(BytesPerRow);
        for (int col = 0; col < BytesPerRow; col++)
        {
            if (col == 8)
            {
                DrawSeparator("| ");
            }

            ushort absAddr = (ushort)(rowAddr + col);
            byte val = emulator.Read(absAddr);
            Color byteColor = val == 0 ? GuiDebuggerTheme.HexDimZero : GuiDebuggerTheme.Value;
            DrawRun($"{val:X2} ", byteColor);

            if (val >= 0x20 && val <= 0x7E)
            {
                ascii.Append((char)val);
            }
            else
            {
                ascii.Append('.');
            }
        }

        DrawSeparator("| ");
        DrawAsciiRun(ascii.ToString());
        ImGui.NewLine();
    }

    static void DrawSeparator(string text) => DrawRun(text, GuiDebuggerTheme.PanelBorder);

    static void DrawRun(string text, Color color)
    {
        ImGui.TextColored(C(color), text);
        ImGui.SameLine(0, 0);
    }

    static void DrawAsciiRun(string run)
    {
        for (int i = 0; i < run.Length; i++)
        {
            char c = run[i];
            bool dim = c == '.';
            Color color = dim ? GuiDebuggerTheme.HexDimZero : GuiDebuggerTheme.Value;
            DrawRun(c.ToString(), color);
        }
    }
}
