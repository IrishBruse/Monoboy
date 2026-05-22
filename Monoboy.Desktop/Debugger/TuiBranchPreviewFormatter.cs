namespace Monoboy.Desktop.Debugger;

using System;
using System.Collections.Generic;

using Monoboy;

using Spectre.Console;

/// <summary>Compact disassembly preview of a branch/call target below the register grid.</summary>
static class TuiBranchPreviewFormatter
{
    /// <summary>First screen row for the preview block (below the 24-row register panel).</summary>
    internal const int PanelStartRow = 24;

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

    internal static string BuildTitleMarkup(ushort target, bool showSymbols, SymSymbolMap? symbols, byte romBank)
    {
        string addr = $"{target:X4}";
        string label = addr;
        if (showSymbols
            && symbols != null
            && symbols.TryGetLabels(target, romBank, out var names)
            && names.Count > 0)
        {
            label = $"{addr} {names[0]}";
        }

        return $"[dim cyan]>> {Markup.Escape(label)}[/]";
    }

    /// <summary>One row of the preview panel; <paramref name="panelRow"/> is 0 = title, 1+ = instruction lines.</summary>
    internal static string BuildPanelRow(List<string> instructionLines, string titleMarkup, int panelRow, int width)
    {
        if (panelRow == 0)
        {
            return TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(titleMarkup, width), width);
        }

        int idx = panelRow - 1;
        if (idx < 0 || idx >= instructionLines.Count)
        {
            return new string(' ', width);
        }

        string line = TuiMarkup.ClipMarkup(instructionLines[idx], width);
        return TuiMarkup.PadMarkup(line, width);
    }

    internal static int PanelLineCount(List<string> instructionLines) => 1 + instructionLines.Count;
}
