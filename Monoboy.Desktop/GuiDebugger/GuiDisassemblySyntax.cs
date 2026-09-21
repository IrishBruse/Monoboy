namespace Monoboy.Desktop.GuiDebugger;

using System.Collections.Generic;
using System.Numerics;
using System.Text;

using ImGuiNET;

using Monoboy;
using Monoboy.Desktop.TuiDebugger;
using Monoboy.Disassembler;

using Raylib_cs;

/// <summary>Syntax-highlighted disassembly line drawing (TUI color semantics, ImGui output).</summary>
static class GuiDisassemblySyntax
{
    const int AddrFieldWidth = 6;
    const int BytesFieldWidth = 8;
    const int MnemonicGapWidth = 3;

    static Vector4 C(Color color) =>
        new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    public readonly struct Line
    {
        public bool IsLabel { get; init; }

        public ushort Address { get; init; }

        public string Label { get; init; }

        public static Line Instruction(ushort address) => new() { Address = address };

        public static Line Symbol(ushort address, string label) =>
            new() { IsLabel = true, Address = address, Label = label };
    }

    public static void AppendBlock(List<Line> lines, Emulator emulator, ushort addr, SymSymbolMap? symbols)
    {
        if (symbols != null
            && symbols.TryGetLabels(addr, emulator.RomBank, out IReadOnlyList<string> names)
            && names.Count > 0)
        {
            lines.Add(Line.Symbol(addr, string.Join(", ", names)));
        }

        lines.Add(Line.Instruction(addr));
    }

    public static void DrawLabel(string label)
    {
        ImGui.PushTextWrapPos(-1f);
        ImGui.TextColored(C(GuiDebuggerTheme.DisasmSymbol), label);
        ImGui.PopTextWrapPos();
    }

    public static void DrawLine(Emulator emulator, ushort lineAddr, bool isPcRow, SymSymbolMap? symbols = null)
    {
        Color addrColor = isPcRow ? GuiDebuggerTheme.Value : GuiDebuggerTheme.Address;
        DrawRun($"{lineAddr:X4}:".PadRight(AddrFieldWidth), addrColor);

        byte op = emulator.Read(lineAddr);
        if (op == 0xCB)
        {
            byte cbOp = emulator.Read((ushort)(lineAddr + 1));
            if (Ops.CBprefixed.TryGetValue(cbOp, out Instruction? cbInsn))
            {
                DrawBytesField(emulator, lineAddr, cbInsn.Bytes);
                DrawGap();
                DrawInstruction(emulator, lineAddr, cbInsn, symbols);
                ImGui.NewLine();
                return;
            }

            DrawBytesField(emulator, lineAddr, 2);
            DrawGap();
            DrawRun("DB CB", GuiDebuggerTheme.DisasmUnknown);
            ImGui.NewLine();
            return;
        }

        if (!Ops.Unprefixed.TryGetValue(op, out Instruction? instruction))
        {
            DrawBytesField(emulator, lineAddr, 1);
            DrawGap();
            DrawRun("DB", GuiDebuggerTheme.DisasmUnknown);
            DrawRun($" ${op:X2}", GuiDebuggerTheme.DisasmImmediate);
            ImGui.NewLine();
            return;
        }

        DrawBytesField(emulator, lineAddr, instruction.Bytes);
        DrawGap();
        DrawInstruction(emulator, lineAddr, instruction, symbols);
        ImGui.NewLine();
    }

    static void DrawInstruction(Emulator emulator, ushort lineAddr, Instruction instruction, SymSymbolMap? symbols)
    {
        DrawRun(instruction.Mnemonic.ToString(), GuiDebuggerTheme.DisasmMnemonic);
        foreach (Operand operand in instruction.Operands)
        {
            DrawRun(" ", GuiDebuggerTheme.DisasmOperand);
            string text = FormatOperandText(operand, emulator, lineAddr, symbols);
            DrawRun(text, OperandColor(operand, emulator, lineAddr, symbols));
        }
    }

    static void DrawBytesField(Emulator emulator, ushort lineAddr, int size)
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
        DrawRun(padded, GuiDebuggerTheme.DisasmBytes);
    }

    static void DrawGap() => DrawRun(new string(' ', MnemonicGapWidth), GuiDebuggerTheme.Value);

    static void DrawRun(string text, Color color)
    {
        ImGui.PushTextWrapPos(-1f);
        ImGui.TextColored(C(color), text);
        ImGui.PopTextWrapPos();
        ImGui.SameLine(0, 0);
    }

    static Color OperandColor(Operand operand, Emulator emulator, ushort atPc, SymSymbolMap? symbols)
    {
        if (TryResolveTargetOperand(operand, emulator, atPc, out ushort target)
            && TryGetSymbolName(symbols, emulator, target, out _))
        {
            return GuiDebuggerTheme.DisasmSymbol;
        }

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

    static string FormatOperandText(Operand operand, Emulator emulator, ushort atPc, SymSymbolMap? symbols)
    {
        if (TryResolveTargetOperand(operand, emulator, atPc, out ushort target))
        {
            if (TryGetSymbolName(symbols, emulator, target, out string name))
            {
                return name;
            }

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

    static bool TryGetSymbolName(SymSymbolMap? symbols, Emulator emulator, ushort target, out string name)
    {
        name = string.Empty;
        if (symbols == null
            || !symbols.TryGetLabels(target, emulator.RomBank, out IReadOnlyList<string> names)
            || names.Count == 0)
        {
            return false;
        }

        name = names[0];
        return true;
    }
}
