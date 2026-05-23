namespace Monoboy.Desktop.Debugger;

using System;
using System.Collections.Generic;

using Monoboy;

/// <summary>Compact disassembly preview of a branch/call target beside the PC row.</summary>
static class TuiBranchPreviewFormatter
{
    /// <summary>First screen row for the preview block (same row as the PC marker in disassembly).</summary>
    internal static int PanelStartRow(int pcLinesFromTop) => 1 + pcLinesFromTop;

    internal const int MaxInstructionLines = 7;

    internal static bool TryBuildPanel(
        Emulator emulator,
        ushort pc,
        bool showSymbols,
        SymSymbolMap? symbols,
        out ushort target,
        out List<string> lines)
    {
        lines = new List<string>();
        target = 0;

        if (!TuiDisassemblyFormatter.TryGetBranchTarget(emulator, pc, out target))
        {
            return false;
        }

        lines = TuiDisassemblyFormatter.BuildPreviewLines(
            emulator, target, MaxInstructionLines, showSymbols, symbols);
        return true;
    }

    /// <summary>One row of the preview panel.</summary>
    internal static string BuildPanelRow(List<string> instructionLines, int panelRow, int width)
    {
        if (panelRow < 0 || panelRow >= instructionLines.Count)
        {
            return new string(' ', width);
        }

        string line = TuiMarkup.ClipMarkup(instructionLines[panelRow], width);
        return TuiMarkup.PadMarkup(line, width);
    }

    internal static int PanelLineCount(List<string> instructionLines) => instructionLines.Count;
}
