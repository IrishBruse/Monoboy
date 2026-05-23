namespace Monoboy.Desktop.Debugger;

using System;
using System.Collections.Generic;
using System.Linq;

using Monoboy;
using Monoboy.Disassembler;

using Spectre.Console;

/// <summary>Disassembly column: instruction-sized scrolling and syntax-highlighted lines.</summary>
static class TuiDisassemblyFormatter
{
    /// <summary>Two visible columns before address (matches <c>&gt; </c> marker width).</summary>
    internal const string LinePrefix = "  ";

    internal const string PcMarkerPrefix = "[bold yellow]>[/] ";

    internal const string BreakpointPrefix = "[red]#[/] ";

    const int AddrFieldVisibleWidth = 6;
    const int BytesFieldVisibleWidth = 8;
    const int MnemonicGapVisibleWidth = 3;

    /// <summary>Visible column where mnemonics start (after prefix, addr, bytes, gap).</summary>
    internal static int MnemonicColumn =>
        LinePrefix.Length + AddrFieldVisibleWidth + BytesFieldVisibleWidth + MnemonicGapVisibleWidth;

    /// <summary>Visible column where standalone symbol labels start (aligned with addresses).</summary>
    internal static int LabelColumn => LinePrefix.Length;

    /// <summary>Build lines with syntax highlighting; <paramref name="lineSkip"/> scrolls the viewport in display lines from the PC-anchored position.</summary>
    /// <param name="instructionsAbovePc">How many prior instructions to include above PC.</param>
    internal static List<string> BuildLines(
        Emulator emulator,
        ushort pc,
        int lineSkip,
        int needLines,
        int instructionsAbovePc = 0,
        bool showSymbols = true,
        SymSymbolMap? symbols = null,
        DisasmAlignmentCache? alignment = null,
        DebuggerBreakpoints? breakpoints = null)
    {
        ushort anchor = pc;
        var lines = new List<string>();
        ushort rangeStart = anchor;
        const int historyMargin = 2;
        int historyTarget = instructionsAbovePc > 0 ? instructionsAbovePc + historyMargin : 0;
        if (historyTarget > 0)
        {
            lines.AddRange(CollectLinesAboveMarker(
                emulator, anchor, pc, historyTarget, showSymbols, symbols, alignment, breakpoints, out ushort historyHead));
            rangeStart = historyHead;
        }

        int targetLines = Math.Max(needLines + 4, 40);
        ushort cursor = anchor;
        while (lines.Count < targetLines)
        {
            bool atPc = cursor == pc;
            AppendInstructionBlock(lines, emulator, cursor, pc, showSymbols, symbols, breakpoints, atPc, out ushort size);
            cursor += size;
        }

        EnsureInstructionsAbovePc(emulator, pc, lines, instructionsAbovePc, rangeStart, showSymbols, symbols, alignment, breakpoints);
        EnsureViewportLines(
            lines,
            emulator,
            pc,
            instructionsAbovePc,
            lineSkip,
            needLines,
            ref rangeStart,
            cursor,
            showSymbols,
            symbols,
            alignment,
            breakpoints);

        return lines;
    }

    /// <summary>First visible line index for the disassembly pane (after <paramref name="lineSkip"/>).</summary>
    internal static int GetViewStartIndex(List<string> lines, int pcLinesFromTop, int lineSkip)
    {
        int pcLineIdx = FindPcLineIndex(lines);
        if (pcLineIdx < 0)
        {
            return Math.Max(0, lineSkip);
        }

        if (pcLineIdx < pcLinesFromTop)
        {
            return Math.Max(0, lineSkip);
        }

        return Math.Max(0, pcLineIdx - pcLinesFromTop + lineSkip);
    }

    static void EnsureViewportLines(
        List<string> lines,
        Emulator emulator,
        ushort pc,
        int pcLinesFromTop,
        int lineSkip,
        int visibleLineCount,
        ref ushort rangeStart,
        ushort rangeEnd,
        bool showSymbols,
        SymSymbolMap? symbols,
        DisasmAlignmentCache? alignment,
        DebuggerBreakpoints? breakpoints)
    {
        byte romBank = emulator.RomBank;
        while (GetViewStartIndex(lines, pcLinesFromTop, lineSkip) < 0)
        {
            if (!TryGetPreviousInstructionStart(emulator, romBank, rangeStart, symbols, alignment, out ushort prev))
            {
                break;
            }

            PrependInstructionBlock(lines, emulator, prev, pc, showSymbols, symbols, breakpoints);
            rangeStart = prev;
        }

        ushort cursor = rangeEnd;
        while (GetViewStartIndex(lines, pcLinesFromTop, lineSkip) + visibleLineCount > lines.Count)
        {
            AppendInstructionBlock(lines, emulator, cursor, pc, showSymbols, symbols, breakpoints, markPc: false, out ushort size);
            cursor += size;
        }
    }

    /// <summary>Pad history so the PC marker can sit <paramref name="needInstructions"/> lines below the top of the buffer.</summary>
    static void EnsureInstructionsAbovePc(
        Emulator emulator,
        ushort pc,
        List<string> lines,
        int needInstructions,
        ushort historyHead,
        bool showSymbols,
        SymSymbolMap? symbols,
        DisasmAlignmentCache? alignment,
        DebuggerBreakpoints? breakpoints)
    {
        if (needInstructions <= 0)
        {
            return;
        }

        byte romBank = emulator.RomBank;
        ushort head = historyHead;
        while (true)
        {
            int pcIdx = FindPcLineIndex(lines);
            if (pcIdx < 0 || pcIdx >= needInstructions)
            {
                return;
            }

            if (!TryGetPreviousInstructionStart(emulator, romBank, head, symbols, alignment, out ushort prev))
            {
                return;
            }

            PrependInstructionBlock(lines, emulator, prev, pc, showSymbols, symbols, breakpoints);
            head = prev;
        }
    }

    /// <summary>Prior instructions strictly before <paramref name="addr"/>.</summary>
    static List<string> CollectLinesAboveMarker(
        Emulator emulator,
        ushort addr,
        ushort focusPc,
        int needInstructions,
        bool showSymbols,
        SymSymbolMap? symbols,
        DisasmAlignmentCache? alignment,
        DebuggerBreakpoints? breakpoints,
        out ushort historyHead)
    {
        var above = new List<string>();
        ushort walk = addr;
        int collected = 0;
        byte romBank = emulator.RomBank;

        while (collected < needInstructions)
        {
            if (!TryGetPreviousInstructionStart(emulator, romBank, walk, symbols, alignment, out ushort prev))
            {
                break;
            }

            walk = prev;
            collected++;
            PrependInstructionBlock(above, emulator, prev, focusPc, showSymbols, symbols, breakpoints);
        }

        historyHead = walk;
        return above;
    }

    static string FormatInstructionLine(
        Emulator emulator,
        ushort lineAddr,
        ushort focusPc,
        bool showSymbols,
        SymSymbolMap? symbols) =>
        LinePrefix + FormatLineMarkup(emulator, lineAddr, focusPc, showSymbols, symbols, out _);

    static void AppendInstructionBlock(
        List<string> lines,
        Emulator emulator,
        ushort lineAddr,
        ushort focusPc,
        bool showSymbols,
        SymSymbolMap? symbols,
        DebuggerBreakpoints? breakpoints,
        bool markPc,
        out ushort size)
    {
        AddLabelLines(lines, symbols, lineAddr, emulator.RomBank);
        string body = FormatLineMarkup(emulator, lineAddr, focusPc, showSymbols, symbols, out size);
        string prefix = markPc
            ? PcMarkerPrefix
            : breakpoints != null && breakpoints.Contains(lineAddr)
                ? BreakpointPrefix
                : LinePrefix;
        lines.Add(prefix + body);
    }

    static void PrependInstructionBlock(
        List<string> lines,
        Emulator emulator,
        ushort lineAddr,
        ushort focusPc,
        bool showSymbols,
        SymSymbolMap? symbols,
        DebuggerBreakpoints? breakpoints)
    {
        var block = new List<string>();
        AppendInstructionBlock(block, emulator, lineAddr, focusPc, showSymbols, symbols, breakpoints, markPc: false, out _);
        for (int i = block.Count - 1; i >= 0; i--)
        {
            lines.Insert(0, block[i]);
        }
    }

    static void AddLabelLines(
        List<string> lines,
        SymSymbolMap? symbols,
        ushort addr,
        byte romBank)
    {
        if (symbols == null || !symbols.TryGetLabels(addr, romBank, out IReadOnlyList<string> names))
        {
            return;
        }

        lines.Add(LinePrefix + FormatLabelMarkup(names));
    }

    static string FormatLabelMarkup(IReadOnlyList<string> names) =>
        $"[bold white]{Markup.Escape(string.Join(", ", names))}[/]";

    internal static bool IsLabelMarkupLine(string markup) =>
        markup.Contains("[bold white]", StringComparison.Ordinal)
        && !markup.Contains("[grey]", StringComparison.Ordinal)
        && !markup.Contains("[green]", StringComparison.Ordinal);

    /// <summary>Parse the instruction address from a disassembly markup line (not label lines).</summary>
    internal static bool TryParseInstructionAddress(string markupLine, out ushort address)
    {
        address = 0;
        const string marker = "[grey]";
        int i = markupLine.IndexOf(marker, StringComparison.Ordinal);
        if (i < 0)
        {
            return false;
        }

        int start = i + marker.Length;
        int end = markupLine.IndexOf("[/]", start, StringComparison.Ordinal);
        if (end <= start)
        {
            return false;
        }

        string hex = markupLine[start..end];
        return ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out address);
    }

    /// <summary>Static branch/call destination for JP, JR, CALL, and RST (not JP HL).</summary>
    internal static bool TryGetBranchTarget(Emulator emulator, ushort addr, out ushort target)
    {
        target = 0;
        byte op = emulator.Read(addr);
        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            return false;
        }

        if (instruction.Mnemonic == Mnemonic.RST && instruction.Operands.Length > 0)
        {
            target = RstOperandToAddress(instruction.Operands[0]);
            return true;
        }

        if (instruction.Mnemonic is not (Mnemonic.JP or Mnemonic.CALL or Mnemonic.JR))
        {
            return false;
        }

        if (instruction.Operands.Length == 1 && instruction.Operands[0] == Operand.HL)
        {
            return false;
        }

        foreach (Operand operand in instruction.Operands)
        {
            if (TryResolveTargetOperand(operand, emulator, addr, out target))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Disassemble up to <paramref name="maxInstructions"/> from <paramref name="target"/>.</summary>
    internal static List<string> BuildPreviewLines(
        Emulator emulator,
        ushort target,
        int maxInstructions,
        bool showSymbols,
        SymSymbolMap? symbols)
    {
        var lines = new List<string>();
        ushort cursor = target;
        const ushort noFocusPc = ushort.MaxValue;
        for (int i = 0; i < maxInstructions; i++)
        {
            AppendInstructionBlock(
                lines, emulator, cursor, noFocusPc, showSymbols, symbols, breakpoints: null, markPc: false, out ushort size);
            cursor += size;
        }

        return lines;
    }

    static ushort RstOperandToAddress(Operand operand) =>
        operand switch
        {
            Operand.RST00 => 0x00,
            Operand.RST08 => 0x08,
            Operand.RST10 => 0x10,
            Operand.RST18 => 0x18,
            Operand.RST20 => 0x20,
            Operand.RST28 => 0x28,
            Operand.RST30 => 0x30,
            Operand.RST38 => 0x38,
            _ => 0,
        };

    internal static int FindPcLineIndex(List<string> lines)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith(PcMarkerPrefix, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Instruction length in bytes, matching <see cref="FormatLineMarkup"/>.</summary>
    internal static ushort GetInstructionByteSize(Emulator emulator, ushort addr)
    {
        byte op = emulator.Read(addr);
        if (op == 0xCB)
        {
            byte cbOp = emulator.Read((ushort)(addr + 1));
            if (Ops.CBprefixed.TryGetValue(cbOp, out var cbInsn))
            {
                return cbInsn.Bytes;
            }

            return 2;
        }

        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            return 1;
        }

        return instruction.Bytes;
    }

    internal static bool TryGetPreviousInstructionStart(
        Emulator emulator,
        byte romBank,
        ushort addr,
        SymSymbolMap? symbols,
        DisasmAlignmentCache? alignment,
        out ushort prevStart)
    {
        if (alignment != null && alignment.TryGetPreviousInstructionStart(emulator, romBank, addr, symbols, out prevStart))
        {
            return true;
        }

        return TryGetPreviousInstructionStartHeuristic(emulator, addr, out prevStart);
    }

    internal static bool TryGetPreviousInstructionStartHeuristic(Emulator emulator, ushort addr, out ushort prevStart)
    {
        const int maxLookback = 32;
        ushort bestStart = 0;
        ushort bestSize = ushort.MaxValue;

        for (int delta = 1; delta <= maxLookback; delta++)
        {
            int candidate = addr - delta;
            if (candidate < 0)
            {
                break;
            }

            ushort c = (ushort)candidate;
            ushort size = GetInstructionByteSize(emulator, c);
            if ((int)c + size != addr)
            {
                continue;
            }

            // Prefer the shortest match: backward decode is ambiguous, and the
            // last executed instruction is usually the nearest (smallest) one.
            if (size < bestSize)
            {
                bestStart = c;
                bestSize = size;
            }
        }

        if (bestSize == ushort.MaxValue)
        {
            prevStart = 0;
            return false;
        }

        prevStart = bestStart;
        return true;
    }

    static string FormatLineMarkup(
        Emulator emulator,
        ushort lineAddr,
        ushort focusPc,
        bool showSymbols,
        SymSymbolMap? symbols,
        out ushort size)
    {
        byte op = emulator.Read(lineAddr);
        byte romBank = emulator.RomBank;
        string addrMk = $"[grey]{lineAddr:X4}[/]";

        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            size = 1;
            string byteMk = $"[cyan]{op:X2}[/]";
            string bytePad = new string(' ', BytesFieldVisibleWidth - 2);
            string gap = new string(' ', MnemonicGapVisibleWidth);
            return $"{addrMk}: {byteMk}{bytePad}{gap}[bold red]DB[/] [yellow]${op:X2}[/]";
        }

        size = instruction.Bytes;
        string bytesPlain = string.Join(' ', Enumerable.Range(0, size).Select(i =>
            emulator.Read((ushort)(lineAddr + i)).ToString("X2")));
        string padBytes = bytesPlain.Length < 8 ? bytesPlain + new string(' ', 8 - bytesPlain.Length) : bytesPlain;
        string bytesMk = $"[cyan]{padBytes}[/]";

        bool focus = lineAddr == focusPc;
        string mnStyle = focus ? "[bold green]" : "[green]";
        string mnMk = $"{mnStyle}{Markup.Escape(instruction.Mnemonic.ToString())}[/]";

        var ops = instruction.Operands.Select(o => OperandToMarkup(o, emulator, lineAddr, showSymbols, symbols, romBank));
        string opsStr = string.Join(' ', ops);
        string tail = opsStr.Length > 0 ? $" {opsStr}" : "";

        string mnemonicGap = new string(' ', MnemonicGapVisibleWidth);
        return $"{addrMk}: {bytesMk}{mnemonicGap}{mnMk}{tail}";
    }

    static string OperandToMarkup(
        Operand operand,
        Emulator emulator,
        ushort atPc,
        bool showSymbols,
        SymSymbolMap? symbols,
        byte romBank)
    {
        if (TryResolveTargetOperand(operand, emulator, atPc, out ushort target))
        {
            string addrText = FormatTargetAddressText(operand, target);
            string escAddr = Markup.Escape(addrText);
            if (showSymbols)
            {
                string? label = FormatTargetName(symbols, target, romBank);
                if (label != null)
                {
                    return $"[bold white]{Markup.Escape(label)}[/]";
                }
            }

            return $"[yellow]{escAddr}[/]";
        }

        string s = FormatOperandPretty(operand, emulator, atPc);
        string esc = Markup.Escape(s);
        return operand switch
        {
            Operand.n8 or Operand.n16 or Operand.a16 or Operand.a8 or Operand.e8 => $"[yellow]{esc}[/]",
            Operand.NZ or Operand.NC or Operand.Z => $"[darkorange]{esc}[/]",
            Operand.RST00 or Operand.RST08 or Operand.RST10 or Operand.RST18
                or Operand.RST20 or Operand.RST28 or Operand.RST30 or Operand.RST38 => $"[magenta]{esc}[/]",
            Operand.Bit0 or Operand.Bit1 or Operand.Bit2 or Operand.Bit3
                or Operand.Bit4 or Operand.Bit5 or Operand.Bit6 or Operand.Bit7 => $"[orchid]{esc}[/]",
            _ => $"[silver]{esc}[/]",
        };
    }

    static bool TryResolveTargetOperand(Operand operand, Emulator emulator, ushort pc, out ushort target)
    {
        switch (operand)
        {
            case Operand.a16:
            target = ReadU16(emulator, (ushort)(pc + 1));
            return true;
            case Operand.a8:
            target = (ushort)(0xFF00 | emulator.Read((ushort)(pc + 1)));
            return true;
            case Operand.e8:
            target = (ushort)(pc + 2 + (sbyte)emulator.Read((ushort)(pc + 1)));
            return true;
            default:
            target = 0;
            return false;
        }
    }

    static string FormatTargetAddressText(Operand operand, ushort target) =>
        operand switch
        {
            Operand.a8 => $"FF{target & 0xFF:X2}",
            Operand.e8 or Operand.a16 => $"{target:X4}",
            _ => $"{target:X4}",
        };

    static string FormatOperandPretty(Operand operand, Emulator emulator, ushort pc)
    {
        return operand switch
        {
            Operand.n8 => $"${emulator.Read((ushort)(pc + 1)):X2}",
            Operand.n16 or Operand.a16 => $"{ReadU16(emulator, (ushort)(pc + 1)):X4}",
            Operand.e8 => $"{(sbyte)emulator.Read((ushort)(pc + 1))}",
            Operand.a8 => $"FF{emulator.Read((ushort)(pc + 1)):X2}",
            _ => operand.ToString()
        };
    }

    static ushort ReadU16(Emulator emulator, ushort addr) =>
        (ushort)(emulator.Read(addr) | (emulator.Read((ushort)(addr + 1)) << 8));

    static string? FormatTargetName(SymSymbolMap? symbols, ushort target, byte romBank)
    {
        if (symbols == null || !symbols.TryGetLabels(target, romBank, out IReadOnlyList<string> names) || names.Count == 0)
        {
            return null;
        }

        return names[0];
    }
}
