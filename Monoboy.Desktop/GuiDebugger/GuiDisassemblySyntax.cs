namespace Monoboy.Desktop.GuiDebugger;

using System.Text;

using Monoboy;
using Monoboy.Disassembler;

using Raylib_cs;

/// <summary>Syntax-highlighted disassembly line drawing (TUI color semantics, Raylib output).</summary>
static class GuiDisassemblySyntax
{
    public const int FontSize = 13;
    const int AddrFieldWidth = 6;
    const int BytesFieldWidth = 8;
    const int MnemonicGapWidth = 3;

    public static void DrawLine(int x, int y, Emulator emulator, ushort lineAddr, bool isPcRow)
    {
        int textY = y + 2;
        Color addrColor = isPcRow ? GuiDebuggerTheme.Value : GuiDebuggerTheme.Address;
        int cx = DrawRun(x, textY, $"{lineAddr:X4}:".PadRight(AddrFieldWidth), addrColor);

        byte op = emulator.Read(lineAddr);
        if (op == 0xCB)
        {
            byte cbOp = emulator.Read((ushort)(lineAddr + 1));
            if (Ops.CBprefixed.TryGetValue(cbOp, out Instruction? cbInsn))
            {
                cx = DrawBytesField(cx, textY, emulator, lineAddr, cbInsn.Bytes);
                cx = DrawGap(cx, textY);
                DrawInstruction(cx, textY, emulator, lineAddr, cbInsn);
                return;
            }

            cx = DrawBytesField(cx, textY, emulator, lineAddr, 2);
            cx = DrawGap(cx, textY);
            DrawRun(cx, textY, "DB CB", GuiDebuggerTheme.DisasmUnknown);
            return;
        }

        if (!Ops.Unprefixed.TryGetValue(op, out Instruction? instruction))
        {
            cx = DrawBytesField(cx, textY, emulator, lineAddr, 1);
            cx = DrawGap(cx, textY);
            cx = DrawRun(cx, textY, "DB", GuiDebuggerTheme.DisasmUnknown);
            DrawRun(cx, textY, $" ${op:X2}", GuiDebuggerTheme.DisasmImmediate);
            return;
        }

        cx = DrawBytesField(cx, textY, emulator, lineAddr, instruction.Bytes);
        cx = DrawGap(cx, textY);
        DrawInstruction(cx, textY, emulator, lineAddr, instruction);
    }

    static void DrawInstruction(int x, int y, Emulator emulator, ushort lineAddr, Instruction instruction)
    {
        int cx = DrawRun(x, y, instruction.Mnemonic.ToString(), GuiDebuggerTheme.DisasmMnemonic);
        foreach (Operand operand in instruction.Operands)
        {
            cx = DrawRun(cx, y, " ", GuiDebuggerTheme.DisasmOperand);
            string text = FormatOperandText(operand, emulator, lineAddr);
            cx = DrawRun(cx, y, text, OperandColor(operand, emulator, lineAddr));
        }
    }

    static int DrawBytesField(int x, int y, Emulator emulator, ushort lineAddr, int size)
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
        string padded = plain.Length < BytesFieldWidth ? plain.PadRight(BytesFieldWidth) : plain;
        return DrawRun(x, y, padded, GuiDebuggerTheme.DisasmBytes);
    }

    static int DrawGap(int x, int y) => DrawRun(x, y, new string(' ', MnemonicGapWidth), GuiDebuggerTheme.Value);

    static int DrawRun(int x, int y, string text, Color color)
    {
        GuiDebuggerFont.Draw(text, x, y, FontSize, color);
        return x + GuiDebuggerFont.Measure(text, FontSize);
    }

    static Color OperandColor(Operand operand, Emulator emulator, ushort atPc)
    {
        if (TryResolveTargetOperand(operand, emulator, atPc, out _))
        {
            return GuiDebuggerTheme.DisasmImmediate;
        }

        return operand switch
        {
            Operand.n8 or Operand.n16 or Operand.a16 or Operand.a8 or Operand.e8 => GuiDebuggerTheme.DisasmImmediate,
            Operand.NZ or Operand.NC or Operand.Z => GuiDebuggerTheme.DisasmCondition,
            Operand.RST00 or Operand.RST08 or Operand.RST10 or Operand.RST18
                or Operand.RST20 or Operand.RST28 or Operand.RST30 or Operand.RST38 => GuiDebuggerTheme.DisasmRst,
            Operand.Bit0 or Operand.Bit1 or Operand.Bit2 or Operand.Bit3
                or Operand.Bit4 or Operand.Bit5 or Operand.Bit6 or Operand.Bit7 => GuiDebuggerTheme.DisasmBit,
            _ => GuiDebuggerTheme.DisasmOperand,
        };
    }

    static string FormatOperandText(Operand operand, Emulator emulator, ushort atPc)
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
}
