namespace Monoboy.Desktop.Debugger;

using System;
using System.Text;

using Monoboy;

using Spectre.Console;

/// <summary>Three-column layout: disassembly, register grid, memory; plus pinned footer.</summary>
static class TuiDebuggerView
{
    internal static void DrawFrame(
        Emulator emulator,
        int termW,
        int termH,
        int disasmSkip,
        int memRowSkip,
        DebuggerPaneFocus paneFocus)
    {
        var s = emulator.GetDebugState();

        int maxContentLines = Math.Max(8, termH - 1);

        const int registerContentRows = 24;
        int registerRows = Math.Min(maxContentLines, registerContentRows);

        int gap = 1;
        int leftPct = 26;
        int leftInner = termW * leftPct / 100;
        leftInner = Math.Clamp(leftInner, 8, Math.Max(8, termW - 28));
        int rowBudget = Math.Max(1, termW - leftInner - 2 * gap);
        int memoryWidth = (rowBudget * 42 + 50) / 100;
        memoryWidth = Math.Clamp(memoryWidth, 1, Math.Max(1, rowBudget - 1));
        int midInner = rowBudget - memoryWidth;

        int[] subWeights = [19, 19, 19, 19];
        int[] colW = TuiMarkup.DistributeWidths(midInner, gap, subWeights);

        var disasmLines = TuiDisassemblyFormatter.BuildLines(emulator, s.PC, disasmSkip, maxContentLines + 4);

        string gapStr = new(' ', gap);
        int estChars = maxContentLines * (leftInner + midInner + memoryWidth + 64);
        var frame = new StringBuilder(estChars);
        for (int r = 0; r < maxContentLines; r++)
        {
            string left;
            if (r == 0)
            {
                string disTitle = paneFocus == DebuggerPaneFocus.Disassembly
                    ? "[bold yellow]Disassembly[/]"
                    : "[dim]Disassembly[/]";
                left = TuiMarkup.PadMarkup(disTitle, leftInner);
            }
            else
            {
                int idx = r - 1;
                left = idx >= 0 && idx < disasmLines.Count
                    ? TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(disasmLines[idx], leftInner), leftInner)
                    : new string(' ', leftInner);
            }

            string mid = r < registerRows
                ? TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(TuiRegisterGridFormatter.BuildRow(emulator, s, r, colW, gap), midInner), midInner)
                : new string(' ', midInner);

            string mem;
            if (r == 0)
            {
                string memTitle = paneFocus == DebuggerPaneFocus.Memory
                    ? "[bold yellow]Memory[/]"
                    : "[dim]Memory[/]";
                mem = TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(memTitle, memoryWidth), memoryWidth);
            }
            else
            {
                string memLine = TuiMemoryDumpFormatter.BuildLine(emulator, TuiMemoryDumpFormatter.DefaultBaseAddress, memRowSkip + r - 1, memoryWidth);
                mem = TuiMarkup.PadMarkupLeft(TuiMarkup.ClipMarkup(memLine, memoryWidth), memoryWidth);
            }

            string row = left + gapStr + mid + gapStr + mem;
            row = TuiMarkup.ClipMarkupToVisibleWidth(row, termW);
            frame.Append(row);
            frame.Append('\n');
        }

        string frameText = frame.ToString();
        try
        {
            AnsiConsole.Markup(frameText);
        }
        catch (Exception)
        {
            Console.Write(TuiMarkup.StripSpectreTags(frameText));
        }

        DrawPinnedFooter(termW, termH, paneFocus);
    }

    /// <summary>Last screen row: key hints; no trailing newline so it stays pinned.</summary>
    static void DrawPinnedFooter(int termW, int termH, DebuggerPaneFocus paneFocus)
    {
        int row = Math.Min(termH - 1, Math.Max(0, Console.WindowHeight - 1));
        int width = Math.Max(1, Math.Min(termW, Console.WindowWidth));
        Console.SetCursorPosition(0, row);

        string focusStr = paneFocus == DebuggerPaneFocus.Disassembly
            ? "[bold cyan]Disasm[/] [dim grey]│ Mem[/]"
            : "[dim grey]Disasm │[/] [bold cyan]Mem[/]";
        string keysMarkup =
            "  [red]S[/]tep  [red]F[/]rame  [red]R[/]un  [red]Q[/]uit  [red]P[/]review  [grey]↑↓[/]scroll  [red]Pg[/] jump  [red]H[/]ome  [red]Tab[/] "
            + focusStr;

        string plain = Markup.Remove(keysMarkup);
        if (plain.Length >= width)
        {
            Console.Write(plain[..width]);
            return;
        }

        string pad = new(' ', width - plain.Length);
        try
        {
            AnsiConsole.Markup(keysMarkup + pad);
        }
        catch (Exception)
        {
            Console.Write(plain.PadRight(width));
        }
    }
}
