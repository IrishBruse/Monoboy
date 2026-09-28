#nullable enable

namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.Numerics;

using ImGuiNET;

using Monoboy;
using Monoboy.Debug;

using Raylib_cs;

/// <summary>GBL source for the sequence point at the program counter.</summary>
public static class GuiSourceView
{
    static string? _loadedKey;
    static string[] _lines = [];
    static int _scrollFile = -1;
    static int _scrollLine = -1;

    public static void Draw(Emulator emulator)
    {
        Vector2 avail = ImGui.GetContentRegionAvail();
        if (avail.X <= 0 || avail.Y <= 0)
        {
            return;
        }

        GblDebugMap? map = GblDebugMap.ForRom(emulator.RomPath);
        if (map == null || map.PointCount == 0)
        {
            ImGui.TextDisabled("No .gbldbg beside this ROM.");
            return;
        }

        GblSourceBreakpoints.Bind(emulator.RomPath);

        DebugState state = emulator.GetDebugState();
        int? index = map.ActiveIndex(emulator.RomBank, state.PC);
        if (index == null || !map.TryGetPoint(index.Value, out GblSequencePoint point))
        {
            ImGui.TextDisabled("No source for this address.");
            return;
        }

        if (!map.TryGetSource(point.FileId, out string name, out string text))
        {
            ImGui.TextDisabled("Source file is missing.");
            return;
        }

        EnsureLines(point.FileId, text);
        ImGui.TextDisabled(name);

        bool scroll = _scrollFile != point.FileId || _scrollLine != point.Line;
        if (scroll)
        {
            _scrollFile = point.FileId;
            _scrollLine = point.Line;
        }

        float lineH = ImGui.GetTextLineHeightWithSpacing();
        ImGui.BeginChild("GblSource", new Vector2(Math.Max(1f, avail.X), Math.Max(1f, avail.Y - lineH)), ImGuiChildFlags.None);
        if (scroll)
        {
            float target = Math.Max(0f, (point.Line - 1 - 3) * lineH);
            ImGui.SetScrollY(target);
        }

        ImGuiListClipperPtr clipper = GuiImGuiClipper.Create();
        try
        {
            clipper.Begin(_lines.Length, lineH);
            while (clipper.Step())
            {
                for (int row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
                {
                    DrawSourceLine(map, point.FileId, row + 1, _lines[row], row + 1 == point.Line);
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

    static void EnsureLines(int fileId, string text)
    {
        string key = fileId + "\n" + text.Length;
        if (key == _loadedKey)
        {
            return;
        }

        _loadedKey = key;
        string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        _lines = normalized.Split('\n');
        if (_lines.Length > 0 && _lines[^1].Length == 0)
        {
            Array.Resize(ref _lines, _lines.Length - 1);
        }
    }

    static void DrawSourceLine(GblDebugMap map, int fileId, int lineNumber, string text, bool current)
    {
        float lineH = ImGui.GetTextLineHeightWithSpacing();
        float width = Math.Max(1f, ImGui.GetContentRegionAvail().X);
        Vector2 pos = ImGui.GetCursorScreenPos();
        bool canBreak = map.LineHasPoint(fileId, lineNumber);

        ImGui.InvisibleButton($"gbl-src-{fileId}-{lineNumber}", new Vector2(width, lineH));
        bool hovered = ImGui.IsItemHovered();
        if (hovered && canBreak)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && canBreak)
        {
            GblSourceBreakpoints.Toggle(map, fileId, lineNumber);
        }

        bool breakpoint = GblSourceBreakpoints.IsSet(fileId, lineNumber);
        ImDrawListPtr draw = ImGui.GetWindowDrawList();
        if (current || breakpoint || (hovered && canBreak))
        {
            Color fill = current ? GuiDebuggerTheme.ProgramCounterRow : GuiDebuggerTheme.PanelBackground;
            if (hovered && canBreak && !current)
            {
                fill = GuiDebuggerTheme.MenuHover;
            }

            draw.AddRectFilled(pos, new Vector2(pos.X + width, pos.Y + lineH), ToU32(fill));
        }

        if (current)
        {
            draw.AddRectFilled(pos, new Vector2(pos.X + 3f, pos.Y + lineH), ToU32(GuiDebuggerTheme.DebugToolbarAction));
        }

        if (breakpoint || (hovered && canBreak))
        {
            float radius = breakpoint ? 4.5f : 3.5f;
            Color dot = breakpoint ? GuiDebuggerTheme.CloseHover : GuiDebuggerTheme.Label;
            draw.AddCircleFilled(new Vector2(pos.X + 9f, pos.Y + (lineH * 0.5f)), radius, ToU32(dot));
        }

        ImGui.SetCursorScreenPos(new Vector2(pos.X + 16f, pos.Y));
        ImGui.TextColored(ToVec(GuiDebuggerTheme.Address), $"{lineNumber,4}  ");
        ImGui.SameLine(0, 0);
        ImGui.TextUnformatted(text);
        ImGui.SetCursorScreenPos(new Vector2(pos.X, pos.Y + lineH));
    }

    static uint ToU32(Color color) =>
        ImGui.ColorConvertFloat4ToU32(new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f));

    static Vector4 ToVec(Color color) =>
        new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
}
