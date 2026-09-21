namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.Collections.Generic;
using System.Numerics;

using ImGuiNET;

using Monoboy;
using Monoboy.Desktop.TuiDebugger;

using Raylib_cs;

/// <summary>Scrollable branch/call target disassembly beside the main list.</summary>
static class GuiAsmBranchPreviewView
{
    const int MaxInstructions = 512;

    static ushort _scrollTarget;

    static Vector4 C(Color color) =>
        new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    public static void Draw(Emulator emulator, ushort target, SymSymbolMap? symbols)
    {
        if (target != _scrollTarget)
        {
            _scrollTarget = target;
            ImGui.SetScrollY(0);
        }

        var drawList = ImGui.GetWindowDrawList();
        Vector2 childMin = ImGui.GetWindowPos();
        drawList.AddLine(
            childMin,
            new Vector2(childMin.X, childMin.Y + ImGui.GetWindowHeight()),
            ImGui.ColorConvertFloat4ToU32(C(GuiDebuggerTheme.PanelBorder)));

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 4);

        List<GuiDisassemblySyntax.Line> lines = CollectLines(emulator, target, symbols, MaxInstructions);
        float lineH = ImGui.GetTextLineHeightWithSpacing();

        ImGuiListClipperPtr clipper = GuiImGuiClipper.Create();
        try
        {
            clipper.Begin(lines.Count, lineH);
            while (clipper.Step())
            {
                for (int idx = clipper.DisplayStart; idx < clipper.DisplayEnd; idx++)
                {
                    GuiDisassemblySyntax.Line line = lines[idx];
                    if (line.IsLabel)
                    {
                        GuiDisassemblySyntax.DrawLabel(line.Label);
                    }
                    else
                    {
                        GuiDisassemblySyntax.DrawLine(emulator, line.Address, isPcRow: false, symbols);
                    }
                }
            }

            clipper.End();
        }
        finally
        {
            clipper.Destroy();
        }
    }

    static List<GuiDisassemblySyntax.Line> CollectLines(Emulator emulator, ushort start, SymSymbolMap? symbols, int maxCount)
    {
        var list = new List<GuiDisassemblySyntax.Line>(Math.Min(maxCount, 64));
        ushort cursor = start;
        for (int i = 0; i < maxCount; i++)
        {
            GuiDisassemblySyntax.AppendBlock(list, emulator, cursor, symbols);
            ushort size = TuiDisassemblyFormatter.GetInstructionByteSize(emulator, cursor);
            if (size == 0)
            {
                break;
            }

            int next = cursor + size;
            if (next > 0xFFFF)
            {
                break;
            }

            cursor = (ushort)next;
        }

        return list;
    }
}
