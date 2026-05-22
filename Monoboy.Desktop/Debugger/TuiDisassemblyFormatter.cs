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
    /// <param name="instructionsAbovePc">How many prior instructions to include above PC (each may add label lines).</param>
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
        if (instructionsAbovePc > 0)
        {
            lines.AddRange(CollectLinesAboveMarker(emulator, anchor, instructionsAbovePc, symbols));
        }

        int targetLines = Math.Max(needLines + 4, 40);
        ushort cursor = anchor;
        while (lines.Count < targetLines)
        {
            AddLabelLines(lines, symbols, cursor, emulator.RomBank);

            bool atPc = cursor == pc;
            string body = FormatLineMarkup(emulator, cursor, pc, out ushort size);
            string prefix = atPc ? PcMarkerPrefix : LinePrefix;
            lines.Add(prefix + body);
            cursor += size;
        }

        return lines;
    }

    /// <summary>Prior instructions strictly before <paramref name="addr"/> (labels at addr come from the forward pass).</summary>
    static List<string> CollectLinesAboveMarker(
        Emulator emulator,
        ushort addr,
        int needInstructions,
        SymSymbolMap? symbols)
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

            var block = new List<string>();
            AddLabelLines(block, symbols, prev, emulator.RomBank);
            block.Add(FormatInstructionLine(emulator, prev, addr));
            for (int i = block.Count - 1; i >= 0; i--)
            {
                above.Insert(0, block[i]);
            }
        }

        return above;
    }

    static string FormatInstructionLine(Emulator emulator, ushort lineAddr, ushort focusPc) =>
        LinePrefix + FormatLineMarkup(emulator, lineAddr, focusPc, out _);

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
        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            return 1;
        }

        return instruction.Bytes;
    }

    internal static bool TryGetPreviousInstructionStart(Emulator emulator, ushort addr, out ushort prevStart)
    {
        const int maxLookback = 4;
        int a = addr;
        for (int delta = 1; delta <= maxLookback; delta++)
        {
            int candidate = a - delta;
            if (candidate < 0)
            {
                break;
            }

            ushort c = (ushort)candidate;
            ushort size = GetInstructionByteSize(emulator, c);
            if ((int)c + size == a)
            {
                prevStart = c;
                return true;
            }
        }

        prevStart = 0;
        return false;
    }

    static void AddLabelLines(List<string> lines, SymSymbolMap? symbols, ushort addr, byte romBank)
    {
        if (symbols == null || !symbols.TryGetLabels(addr, romBank, out IReadOnlyList<string> names))
        {
            return;
        }

        foreach (string label in names)
        {
            lines.Add(FormatLabelLine(label));
        }
    }

    static string FormatLabelLine(string label) =>
        $"[bold white]{Markup.Escape(label)}[/]";

    internal static bool IsLabelMarkupLine(string markup) =>
        markup.Contains("[bold white]", StringComparison.Ordinal)
        && !markup.Contains("[grey]", StringComparison.Ordinal);

    static string FormatLineMarkup(Emulator emulator, ushort lineAddr, ushort focusPc, out ushort size)
    {
        byte op = emulator.Read(lineAddr);
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

        var ops = instruction.Operands.Select(o => OperandToMarkup(o, emulator, lineAddr));
        string opsStr = string.Join(' ', ops);
        string tail = opsStr.Length > 0 ? $" {opsStr}" : "";

        string mnemonicGap = new string(' ', MnemonicGapVisibleWidth);
        return $"{addrMk}: {bytesMk}{mnemonicGap}{mnMk}{tail}";
    }

    static string OperandToMarkup(Operand operand, Emulator emulator, ushort atPc)
    {
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

    static string FormatOperandPretty(Operand operand, Emulator emulator, ushort pc)
    {
        return operand switch
        {
            Operand.n8 => $"${emulator.Read((ushort)(pc + 1)):X2}",
            Operand.n16 or Operand.a16 => $"{emulator.Read((ushort)(pc + 2)):X2}{emulator.Read((ushort)(pc + 1)):X2}",
            Operand.e8 => $"{(sbyte)emulator.Read((ushort)(pc + 1))}",
            Operand.a8 => $"FF{emulator.Read((ushort)(pc + 1)):X2}",
            _ => operand.ToString()
        };
    }
}
