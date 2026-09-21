namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.Collections.Generic;

using Monoboy;
using Monoboy.Desktop.TuiDebugger;

using Raylib_cs;

/// <summary>
/// Instruction list for the GUI debugger pane, with optional branch-target asm preview.
/// </summary>
public static class GuiDisassemblyView
{
    const int LineHeight = 18;
    const int PcLinesFromTop = 3;
    const int PreviewMinWidth = 140;

    public static void Draw(
        Rectangle area,
        Emulator emulator,
        float wheel,
        ref int scrollRows,
        ref int previewScrollRows,
        ref ushort previewScrollTarget)
    {
        int areaX = (int)area.X;
        int areaY = (int)area.Y;
        int areaW = Math.Max(0, (int)area.Width);
        int areaH = Math.Max(0, (int)area.Height);
        if (areaW <= 0 || areaH <= 0)
        {
            return;
        }

        ushort pc = emulator.GetDebugState().PC;
        bool showPreview = TuiDisassemblyFormatter.TryGetBranchTarget(emulator, pc, out ushort branchTarget);
        int previewW = 0;
        if (showPreview)
        {
            previewW = Math.Clamp((int)(areaW * 0.38f), PreviewMinWidth, Math.Max(PreviewMinWidth, areaW / 2));
            if (previewW + PreviewMinWidth > areaW)
            {
                previewW = Math.Max(0, areaW - PreviewMinWidth);
            }
        }
        else
        {
            previewScrollRows = 0;
        }

        int mainW = areaW - previewW;
        var mainArea = new Rectangle(areaX, areaY, mainW, areaH);
        DrawMainList(mainArea, emulator, wheel, ref scrollRows);

        if (showPreview && previewW > 0)
        {
            var previewArea = new Rectangle(areaX + mainW, areaY, previewW, areaH);
            GuiAsmBranchPreviewView.Draw(
                previewArea, emulator, branchTarget, wheel, ref previewScrollRows, ref previewScrollTarget);
        }
    }

    static void DrawMainList(Rectangle area, Emulator emulator, float wheel, ref int scrollRows)
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

        int contentW = Math.Max(1, areaW - GuiScroll.Width - 2);
        int visibleRows = Math.Max(1, areaH / LineHeight);
        const int maxScroll = 4096;
        GuiScroll.ApplyWheel(area, wheel, ref scrollRows, maxScroll);

        ushort pc = emulator.GetDebugState().PC;
        ushort startAddr = scrollRows == 0
            ? WalkBackInstructions(emulator, pc, PcLinesFromTop)
            : WalkBackInstructions(emulator, pc, PcLinesFromTop + scrollRows);

        var rows = new List<(ushort Addr, bool IsPc)>(visibleRows + 4);
        ushort cursor = startAddr;
        for (int i = 0; i < visibleRows; i++)
        {
            rows.Add((cursor, cursor == pc));
            cursor += TuiDisassemblyFormatter.GetInstructionByteSize(emulator, cursor);
        }

        Raylib.BeginScissorMode(areaX, areaY, areaW - GuiScroll.Width, areaH);
        for (int row = 0; row < rows.Count; row++)
        {
            int y = areaY + row * LineHeight;
            var (addr, isPc) = rows[row];
            if (isPc)
            {
                Raylib.DrawRectangle(areaX, y, contentW, LineHeight, GuiDebuggerTheme.ProgramCounterRow);
            }

            GuiDisassemblySyntax.DrawLine(areaX + 2, y, emulator, addr, isPc);
        }

        Raylib.EndScissorMode();

        var bar = new Rectangle(areaX + areaW - GuiScroll.Width, areaY, GuiScroll.Width, areaH);
        GuiScroll.Draw(1, bar, ref scrollRows, maxScroll);
    }

    static ushort WalkBackInstructions(Emulator emulator, ushort fromPc, int count)
    {
        ushort cursor = fromPc;
        for (int i = 0; i < count; i++)
        {
            if (!TuiDisassemblyFormatter.TryGetPreviousInstructionStartHeuristic(emulator, cursor, out ushort prev))
            {
                break;
            }

            cursor = prev;
        }

        return cursor;
    }
}
