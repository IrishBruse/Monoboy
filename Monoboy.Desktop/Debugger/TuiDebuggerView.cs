namespace Monoboy.Desktop.Debugger;

using System;
using System.Collections.Generic;
using System.Text;

using Monoboy;

using Spectre.Console;

/// <summary>Four-column layout: disassembly, branch preview, registers, memory; plus pinned footer.</summary>
static class TuiDebuggerView
{
    internal static void DrawFrame(
        Emulator emulator,
        int termW,
        int termH,
        int disasmLineSkip,
        int memRowSkip,
        DebuggerPaneFocus paneFocus,
        RegisterLabelDisplay registerLabelDisplay,
        bool showDisasmSymbols,
        SymSymbolMap? symbols = null)
    {
        var s = emulator.GetDebugState();

        int maxContentLines = Math.Max(8, termH - 1);

        const int registerContentRows = 24;
        int registerRows = Math.Min(maxContentLines, registerContentRows);

        int gap = 1;
        const int disasmTailWidth = 26;
        int disasmWidth = TuiDisassemblyFormatter.MnemonicColumn + disasmTailWidth;
        disasmWidth = Math.Clamp(disasmWidth, 44, 58);
        int rightBudget = Math.Max(1, termW - disasmWidth - 3 * gap);
        int memoryWidth = (rightBudget * 42 + 50) / 100;
        memoryWidth = Math.Clamp(memoryWidth, 1, Math.Max(1, rightBudget - 24));
        int branchWidth = Math.Clamp(34, 20, Math.Max(20, rightBudget - memoryWidth - 16));
        int registerWidth = Math.Max(1, rightBudget - branchWidth - memoryWidth);

        const int registerSubGap = 3;
        int[] subWeights = [48, 52];
        int[] colW = TuiMarkup.DistributeWidths(registerWidth, registerSubGap, subWeights);

        const int pcLinesFromTop = 3;
        int branchPanelStartRow = TuiBranchPreviewFormatter.PanelStartRow(pcLinesFromTop);

        bool showBranchPanel = TuiBranchPreviewFormatter.TryBuildPanel(
            emulator, s.PC, showDisasmSymbols, symbols, out _, out List<string> branchPreviewLines);
        int branchPanelLines = showBranchPanel
            ? TuiBranchPreviewFormatter.PanelLineCount(branchPreviewLines)
            : 0;

        var disasmLines = TuiDisassemblyFormatter.BuildLines(
            emulator, s.PC, disasmLineSkip, maxContentLines + 4, pcLinesFromTop, showDisasmSymbols, symbols);

        int disasmViewStart = TuiDisassemblyFormatter.GetViewStartIndex(disasmLines, pcLinesFromTop, disasmLineSkip);

        string gapStr = new(' ', gap);
        int estChars = maxContentLines * (disasmWidth + branchWidth + registerWidth + memoryWidth + 64);
        var frame = new StringBuilder(estChars);
        for (int r = 0; r < maxContentLines; r++)
        {
            string left;
            if (r == 0)
            {
                string disTitle = paneFocus == DebuggerPaneFocus.Disassembly
                    ? "[bold yellow]Disassembly[/]"
                    : "[dim]Disassembly[/]";
                left = TuiMarkup.PadMarkup(disTitle, disasmWidth);
            }
            else
            {
                int idx = disasmViewStart + (r - 1);
                if (idx >= disasmLines.Count && disasmLineSkip == 0)
                {
                    // Short history at PC: keep marker on a fixed row without trailing blanks.
                    int pcLineIdx = TuiDisassemblyFormatter.FindPcLineIndex(disasmLines);
                    if (pcLineIdx >= 0 && pcLineIdx < pcLinesFromTop)
                    {
                        idx = r - 1;
                    }
                }
                if (idx >= 0 && idx < disasmLines.Count)
                {
                    string line = TuiMarkup.ClipMarkup(disasmLines[idx], disasmWidth);
                    if (TuiDisassemblyFormatter.IsLabelMarkupLine(disasmLines[idx]))
                    {
                        int labelPad = TuiDisassemblyFormatter.LabelColumn - TuiDisassemblyFormatter.LinePrefix.Length;
                        if (labelPad > 0)
                        {
                            line = new string(' ', labelPad) + line;
                        }
                    }

                    left = TuiMarkup.PadMarkup(line, disasmWidth);
                }
                else
                {
                    left = new string(' ', disasmWidth);
                }
            }

            string branch;
            if (showBranchPanel && r >= branchPanelStartRow && r < branchPanelStartRow + branchPanelLines)
            {
                int panelRow = r - branchPanelStartRow;
                branch = TuiBranchPreviewFormatter.BuildPanelRow(branchPreviewLines, panelRow, branchWidth);
            }
            else
            {
                branch = new string(' ', branchWidth);
            }

            string reg;
            if (r < registerRows)
            {
                reg = TuiMarkup.PadMarkupLeft(
                    TuiMarkup.ClipMarkup(TuiRegisterGridFormatter.BuildRow(emulator, s, r, colW, registerSubGap, registerLabelDisplay), registerWidth),
                    registerWidth);
            }
            else
            {
                reg = new string(' ', registerWidth);
            }

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

            string row = left + gapStr + branch + gapStr + reg + gapStr + mem;
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

        DrawPinnedFooter(termW, termH, paneFocus, registerLabelDisplay);
    }

    /// <summary>Last screen row: key hints; no trailing newline so it stays pinned.</summary>
    static void DrawPinnedFooter(
        int termW,
        int termH,
        DebuggerPaneFocus paneFocus,
        RegisterLabelDisplay registerLabelDisplay)
    {
        int row = Math.Min(termH - 1, Math.Max(0, Console.WindowHeight - 1));
        int width = Math.Max(1, Math.Min(termW, Console.WindowWidth));
        Console.SetCursorPosition(0, row);

        string regToggle = FormatKeyToggle(
            "A", "Addr", "Label", registerLabelDisplay == RegisterLabelDisplay.Address);
        string paneToggle = FormatKeyToggle(
            "Tab", "Disasm", "Mem", paneFocus == DebuggerPaneFocus.Disassembly);
        string keysMarkup =
            "  [red]S[/]tep  [red]F[/]rame  [red]V[/]blank  [red]R[/]un  [red]^R[/]eset  [red]Q[/]uit  [red]P[/]review  [red]↑↓[/]scroll  [red]Pg[/] jump  [red]H[/]ome  "
            + regToggle
            + "  "
            + paneToggle;

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

    static string FormatKeyToggle(string key, string left, string right, bool leftSelected) =>
        leftSelected
            ? $"[red]{key}[/]([bold cyan]{left}[/][grey]|[/][dim grey]{right}[/])"
            : $"[red]{key}[/]([dim grey]{left}[/][grey]|[/][bold cyan]{right}[/])";
}
