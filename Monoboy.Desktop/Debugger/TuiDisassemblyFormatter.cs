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
    /// <summary>Build lines with syntax highlighting; <paramref name="skip"/> moves the window in instruction steps from PC.</summary>
    internal static List<string> BuildLines(Emulator emulator, ushort pc, int skip, int needLines)
    {
        var lines = new List<string>();
        ushort cursor = pc;
        if (skip > 0)
        {
            int consumed = 0;
            while (consumed < skip)
            {
                ushort sz = GetInstructionByteSize(emulator, cursor);
                cursor += sz;
                consumed++;
            }
        }
        else if (skip < 0)
        {
            for (int i = 0; i < -skip; i++)
            {
                if (!TryGetPreviousInstructionStart(emulator, cursor, out ushort prev))
                {
                    break;
                }

                cursor = prev;
            }
        }

        int count = Math.Max(needLines + 4, 40);
        for (int i = 0; i < count; i++)
        {
            bool atPc = cursor == pc;
            string body = FormatLineMarkup(emulator, cursor, pc, out ushort size);
            string prefix = atPc ? "[bold yellow]>[/] " : "  ";
            lines.Add(prefix + body);
            cursor += size;
        }
        return lines;
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

    static string FormatLineMarkup(Emulator emulator, ushort lineAddr, ushort focusPc, out ushort size)
    {
        byte op = emulator.Read(lineAddr);
        string addrMk = $"[grey]{lineAddr:X4}[/]";

        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            size = 1;
            string byteMk = $"[cyan]{op:X2}[/]";
            return $"{addrMk}: {byteMk}{new string(' ', 8)} [bold red]DB[/] [yellow]${op:X2}[/]";
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

        return $"{addrMk}: {bytesMk} {mnMk}{tail}";
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
