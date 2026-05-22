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

    const int AddrFieldVisibleWidth = 6;
    const int BytesFieldVisibleWidth = 8;
    const int MnemonicGapVisibleWidth = 3;

    /// <summary>Visible column where mnemonics start (after prefix, addr, bytes, gap).</summary>
    internal static int MnemonicColumn =>
        LinePrefix.Length + AddrFieldVisibleWidth + BytesFieldVisibleWidth + MnemonicGapVisibleWidth;

    /// <summary>Build lines with syntax highlighting; <paramref name="skip"/> moves the window in instruction steps from PC.</summary>
    /// <param name="instructionsAbovePc">How many prior instructions to include above PC.</param>
    internal static List<string> BuildLines(
        Emulator emulator,
        ushort pc,
        int skip,
        int needLines,
        int instructionsAbovePc = 0,
        SymSymbolMap? symbols = null)
    {
        ushort anchor = pc;
        if (skip > 0)
        {
            int consumed = 0;
            while (consumed < skip)
            {
                ushort sz = GetInstructionByteSize(emulator, anchor);
                anchor += sz;
                consumed++;
            }
        }
        else if (skip < 0)
        {
            for (int i = 0; i < -skip; i++)
            {
                if (!TryGetPreviousInstructionStart(emulator, anchor, out ushort prev))
                {
                    break;
                }

                anchor = prev;
            }
        }

        var lines = new List<string>();
        ushort historyHead = anchor;
        const int historyMargin = 2;
        int historyTarget = instructionsAbovePc > 0 ? instructionsAbovePc + historyMargin : 0;
        if (historyTarget > 0)
        {
            lines.AddRange(CollectLinesAboveMarker(emulator, anchor, pc, historyTarget, symbols, out historyHead));
        }

        int targetLines = Math.Max(needLines + 4, 40);
        ushort cursor = anchor;
        while (lines.Count < targetLines)
        {
            bool atPc = cursor == pc;
            string body = FormatLineMarkup(emulator, cursor, pc, symbols, out ushort size);
            string prefix = atPc ? PcMarkerPrefix : LinePrefix;
            lines.Add(prefix + body);
            cursor += size;
        }

        EnsureInstructionsAbovePc(emulator, pc, lines, instructionsAbovePc, historyHead, symbols);

        return lines;
    }

    /// <summary>Pad history so the PC marker can sit <paramref name="needInstructions"/> lines below the top of the buffer.</summary>
    static void EnsureInstructionsAbovePc(
        Emulator emulator,
        ushort pc,
        List<string> lines,
        int needInstructions,
        ushort historyHead,
        SymSymbolMap? symbols)
    {
        if (needInstructions <= 0)
        {
            return;
        }

        ushort head = historyHead;
        while (true)
        {
            int pcIdx = FindPcLineIndex(lines);
            if (pcIdx < 0 || pcIdx >= needInstructions)
            {
                return;
            }

            if (!TryGetPreviousInstructionStart(emulator, head, out ushort prev))
            {
                return;
            }

            lines.Insert(0, FormatInstructionLine(emulator, prev, pc, symbols));
            head = prev;
        }
    }

    /// <summary>Prior instructions strictly before <paramref name="addr"/>.</summary>
    static List<string> CollectLinesAboveMarker(
        Emulator emulator,
        ushort addr,
        ushort focusPc,
        int needInstructions,
        SymSymbolMap? symbols,
        out ushort historyHead)
    {
        var above = new List<string>();
        ushort walk = addr;
        int collected = 0;

        while (collected < needInstructions)
        {
            if (!TryGetPreviousInstructionStart(emulator, walk, out ushort prev))
            {
                break;
            }

            walk = prev;
            collected++;
            above.Insert(0, FormatInstructionLine(emulator, prev, focusPc, symbols));
        }

        historyHead = walk;
        return above;
    }

    static string FormatInstructionLine(Emulator emulator, ushort lineAddr, ushort focusPc, SymSymbolMap? symbols) =>
        LinePrefix + FormatLineMarkup(emulator, lineAddr, focusPc, symbols, out _);

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

    internal static bool TryGetPreviousInstructionStart(Emulator emulator, ushort addr, out ushort prevStart)
    {
        const int maxLookback = 32;
        ushort bestStart = 0;
        ushort bestSize = 0;

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

            if (size > bestSize)
            {
                bestStart = c;
                bestSize = size;
            }
        }

        if (bestSize == 0)
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
        SymSymbolMap? symbols,
        out ushort size)
    {
        byte op = emulator.Read(lineAddr);
        byte romBank = emulator.RomBank;
        string addrMk = $"[grey]{lineAddr:X4}[/]";
        string labelMk = FormatAddressLabels(symbols, lineAddr, romBank);

        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            size = 1;
            string byteMk = $"[cyan]{op:X2}[/]";
            string bytePad = new string(' ', BytesFieldVisibleWidth - 2);
            string gap = new string(' ', MnemonicGapVisibleWidth);
            return $"{addrMk}{labelMk}: {byteMk}{bytePad}{gap}[bold red]DB[/] [yellow]${op:X2}[/]";
        }

        size = instruction.Bytes;
        string bytesPlain = string.Join(' ', Enumerable.Range(0, size).Select(i =>
            emulator.Read((ushort)(lineAddr + i)).ToString("X2")));
        string padBytes = bytesPlain.Length < 8 ? bytesPlain + new string(' ', 8 - bytesPlain.Length) : bytesPlain;
        string bytesMk = $"[cyan]{padBytes}[/]";

        bool focus = lineAddr == focusPc;
        string mnStyle = focus ? "[bold green]" : "[green]";
        string mnMk = $"{mnStyle}{Markup.Escape(instruction.Mnemonic.ToString())}[/]";

        var ops = instruction.Operands.Select(o => OperandToMarkup(o, emulator, lineAddr, symbols, romBank));
        string opsStr = string.Join(' ', ops);
        string tail = opsStr.Length > 0 ? $" {opsStr}" : "";

        string mnemonicGap = new string(' ', MnemonicGapVisibleWidth);
        return $"{addrMk}{labelMk}: {bytesMk}{mnemonicGap}{mnMk}{tail}";
    }

    static string FormatAddressLabels(SymSymbolMap? symbols, ushort addr, byte romBank)
    {
        if (symbols == null || !symbols.TryGetLabels(addr, romBank, out IReadOnlyList<string> names))
        {
            return string.Empty;
        }

        return $" [bold white]{Markup.Escape(string.Join(", ", names))}[/]";
    }

    static string OperandToMarkup(
        Operand operand,
        Emulator emulator,
        ushort atPc,
        SymSymbolMap? symbols,
        byte romBank)
    {
        if (TryFormatSymbolicOperand(operand, emulator, atPc, symbols, romBank, out string symbolic))
        {
            return $"[bold white]{Markup.Escape(symbolic)}[/]";
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

    static bool TryFormatSymbolicOperand(
        Operand operand,
        Emulator emulator,
        ushort pc,
        SymSymbolMap? symbols,
        byte romBank,
        out string name)
    {
        name = string.Empty;
        if (symbols == null)
        {
            return false;
        }

        ushort? target = operand switch
        {
            Operand.a16 => ReadU16(emulator, (ushort)(pc + 1)),
            Operand.a8 => (ushort)(0xFF00 | emulator.Read((ushort)(pc + 1))),
            Operand.e8 => (ushort)(pc + 2 + (sbyte)emulator.Read((ushort)(pc + 1))),
            _ => null
        };

        if (target is not ushort addr)
        {
            return false;
        }

        string? label = FormatTargetName(symbols, addr, romBank);
        if (label == null)
        {
            return false;
        }

        name = label;
        return true;
    }

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
