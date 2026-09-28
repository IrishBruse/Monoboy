namespace Monoboy.Desktop.Debugger;

using Monoboy;
using Monoboy.Disassembler;

/// <summary>Instruction sizing and branch targets for the GUI disassembly views.</summary>
static class DisassemblyFormatter
{
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

    /// <summary>Instruction length in bytes.</summary>
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

    static ushort ReadU16(Emulator emulator, ushort addr) =>
        (ushort)(emulator.Read(addr) | (emulator.Read((ushort)(addr + 1)) << 8));
}
