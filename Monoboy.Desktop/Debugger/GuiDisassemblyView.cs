namespace Monoboy.Desktop.Debugger;

using System;
using System.Collections.Generic;
using System.Text;

using Monoboy;
using Monoboy.Disassembler;

using Raylib_cs;

/// <summary>
/// Instruction list for the GUI debugger pane.
/// Theme: <see cref="GuiDebuggerTheme.PanelBackground"/>, <see cref="GuiDebuggerTheme.Value"/>,
/// <see cref="GuiDebuggerTheme.Label"/> (address field), <see cref="GuiDebuggerTheme.ProgramCounterRow"/>,
/// <see cref="GuiDebuggerTheme.HexDimZero"/> (unused bytes pad), <see cref="GuiDebuggerTheme.PanelBorder"/> (scrollbar).
/// </summary>
public static class GuiDisassemblyView
{
    const int FontSize = 13;
    const int LineHeight = 18;
    const int ScrollbarWidth = 8;
    const int PcLinesFromTop = 3;
    const int AddrFieldWidth = 6;
    const int BytesFieldWidth = 8;
    const int MnemonicGapWidth = 3;

    static readonly Color AddressGrey = new(0x8A, 0x8A, 0x9E, 255);

    public static void Draw(Rectangle area, Emulator emulator, ref int scrollRows)
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

        int contentW = Math.Max(1, areaW - ScrollbarWidth - 2);
        int visibleRows = Math.Max(1, areaH / LineHeight);

        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), area))
        {
            float wheel = Raylib.GetMouseWheelMove();
            if (wheel != 0)
            {
                scrollRows -= (int)Math.Sign(wheel);
            }
        }

        ushort pc = emulator.GetDebugState().PC;
        AdjustScrollForPc(emulator, pc, visibleRows, ref scrollRows);

        ushort startAddr = WalkBackInstructions(emulator, pc, PcLinesFromTop + Math.Max(0, scrollRows));
        var rows = new List<(ushort Addr, bool IsPc)>(visibleRows + 4);
        ushort cursor = startAddr;
        for (int i = 0; i < visibleRows && rows.Count < visibleRows; i++)
        {
            rows.Add((cursor, cursor == pc));
            cursor += GetInstructionByteSize(emulator, cursor);
        }

        bool pcVisible = rows.Exists(r => r.IsPc);
        if (!pcVisible)
        {
            scrollRows = 0;
            startAddr = WalkBackInstructions(emulator, pc, PcLinesFromTop);
            rows.Clear();
            cursor = startAddr;
            for (int i = 0; i < visibleRows; i++)
            {
                rows.Add((cursor, cursor == pc));
                cursor += GetInstructionByteSize(emulator, cursor);
            }
        }

        Raylib.BeginScissorMode(areaX, areaY, areaW - ScrollbarWidth, areaH);
        for (int row = 0; row < rows.Count; row++)
        {
            int y = areaY + row * LineHeight;
            var (addr, isPc) = rows[row];
            if (isPc)
            {
                Raylib.DrawRectangle(areaX, y, contentW, LineHeight, GuiDebuggerTheme.ProgramCounterRow);
            }

            DrawInstructionLine(areaX + 2, y, emulator, addr, isPc);
        }

        Raylib.EndScissorMode();

        DrawScrollbar(
            areaX + areaW - ScrollbarWidth,
            areaY,
            ScrollbarWidth,
            areaH,
            scrollRows,
            maxScrollRows: 4096);
    }

    static void AdjustScrollForPc(Emulator emulator, ushort pc, int visibleRows, ref int scrollRows)
    {
        if (scrollRows == 0)
        {
            return;
        }

        ushort start = WalkBackInstructions(emulator, pc, PcLinesFromTop + scrollRows);
        ushort end = start;
        for (int i = 0; i < visibleRows; i++)
        {
            end += GetInstructionByteSize(emulator, end);
        }

        if (pc >= start && pc < end)
        {
            return;
        }

        scrollRows = 0;
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

    static void DrawInstructionLine(int x, int y, Emulator emulator, ushort lineAddr, bool isPcRow)
    {
        FormatPlainLine(emulator, lineAddr, out string addrText, out string bytesText, out string mnemonicText);

        Color addrColor = isPcRow ? GuiDebuggerTheme.Value : AddressGrey;
        Color textColor = GuiDebuggerTheme.Value;

        GuiDebuggerFont.Draw(addrText, x, y + 2, FontSize, addrColor);
        int addrW = GuiDebuggerFont.Measure(addrText, FontSize);
        GuiDebuggerFont.Draw(bytesText, x + addrW, y + 2, FontSize, textColor);

        int bytesW = GuiDebuggerFont.Measure(bytesText, FontSize);
        string gap = new(' ', MnemonicGapWidth);
        GuiDebuggerFont.Draw(gap, x + addrW + bytesW, y + 2, FontSize, textColor);
        int gapW = GuiDebuggerFont.Measure(gap, FontSize);
        GuiDebuggerFont.Draw(mnemonicText, x + addrW + bytesW + gapW, y + 2, FontSize, textColor);
    }

    static void FormatPlainLine(
        Emulator emulator,
        ushort lineAddr,
        out string addrText,
        out string bytesText,
        out string mnemonicText)
    {
        addrText = $"{lineAddr:X4}:".PadRight(AddrFieldWidth);

        byte op = emulator.Read(lineAddr);
        if (op == 0xCB)
        {
            byte cbOp = emulator.Read((ushort)(lineAddr + 1));
            if (Ops.CBprefixed.TryGetValue(cbOp, out Instruction? cbInsn))
            {
                bytesText = FormatBytesField(emulator, lineAddr, cbInsn.Bytes);
                mnemonicText = FormatMnemonicPlain(emulator, lineAddr, cbInsn);
                return;
            }

            bytesText = FormatBytesField(emulator, lineAddr, 2);
            mnemonicText = "DB CB";
            return;
        }

        if (!Ops.Unprefixed.TryGetValue(op, out Instruction? instruction))
        {
            bytesText = $"{op:X2}".PadRight(BytesFieldWidth);
            mnemonicText = $"DB ${op:X2}";
            return;
        }

        bytesText = FormatBytesField(emulator, lineAddr, instruction.Bytes);
        mnemonicText = FormatMnemonicPlain(emulator, lineAddr, instruction);
    }

    static string FormatBytesField(Emulator emulator, ushort lineAddr, int size)
    {
        var parts = new StringBuilder();
        for (int i = 0; i < size; i++)
        {
            if (i > 0)
            {
                parts.Append(' ');
            }

            parts.Append(emulator.Read((ushort)(lineAddr + i)).ToString("X2"));
        }

        string plain = parts.ToString();
        return plain.Length < BytesFieldWidth ? plain.PadRight(BytesFieldWidth) : plain;
    }

    static string FormatMnemonicPlain(Emulator emulator, ushort lineAddr, Instruction instruction)
    {
        var sb = new StringBuilder(instruction.Mnemonic.ToString());
        foreach (Operand operand in instruction.Operands)
        {
            sb.Append(' ');
            sb.Append(FormatOperandPlain(operand, emulator, lineAddr));
        }

        return sb.ToString();
    }

    static string FormatOperandPlain(Operand operand, Emulator emulator, ushort atPc)
    {
        if (TryResolveTargetOperand(operand, emulator, atPc, out ushort target))
        {
            return FormatTargetAddressText(operand, target);
        }

        return FormatOperandPretty(operand, emulator, atPc);
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

    static string FormatOperandPretty(Operand operand, Emulator emulator, ushort pc) =>
        operand switch
        {
            Operand.n8 => $"${emulator.Read((ushort)(pc + 1)):X2}",
            Operand.n16 or Operand.a16 => $"{ReadU16(emulator, (ushort)(pc + 1)):X4}",
            Operand.e8 => $"{(sbyte)emulator.Read((ushort)(pc + 1))}",
            Operand.a8 => $"FF{emulator.Read((ushort)(pc + 1)):X2}",
            _ => operand.ToString() ?? string.Empty,
        };

    static ushort ReadU16(Emulator emulator, ushort addr) =>
        (ushort)(emulator.Read(addr) | (emulator.Read((ushort)(addr + 1)) << 8));

    /// <summary>Matches TUI instruction sizing without Spectre markup.</summary>
    static ushort GetInstructionByteSize(Emulator emulator, ushort addr)
    {
        byte op = emulator.Read(addr);
        if (op == 0xCB)
        {
            byte cbOp = emulator.Read((ushort)(addr + 1));
            if (Ops.CBprefixed.TryGetValue(cbOp, out Instruction? cbInsn))
            {
                return cbInsn.Bytes;
            }

            return 2;
        }

        if (!Ops.Unprefixed.TryGetValue(op, out Instruction? instruction))
        {
            return 1;
        }

        return instruction.Bytes;
    }

    static void DrawScrollbar(int x, int y, int width, int height, int scrollRows, int maxScrollRows)
    {
        Raylib.DrawRectangle(x, y, width, height, GuiDebuggerTheme.ScrollbarTrack);
        Raylib.DrawLine(x, y, x, y + height, GuiDebuggerTheme.PanelBorder);

        if (height <= 4)
        {
            return;
        }

        int thumbH = Math.Max(8, height / 16);
        float t = maxScrollRows <= 0 ? 0 : Math.Clamp(scrollRows / (float)maxScrollRows, 0f, 1f);
        int thumbY = y + (int)((height - thumbH) * t);
        Raylib.DrawRectangle(x + 1, thumbY, Math.Max(1, width - 2), thumbH, GuiDebuggerTheme.ScrollbarThumb);
    }
}
