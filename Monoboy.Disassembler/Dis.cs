namespace Monoboy.Disassembler;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Dis
{
    ushort pc;
    byte[] data = [];

    Dictionary<ushort, int> jumps = new();

    List<ushort> labels = new();
    SortedDictionary<ushort, string> instructions = new();
    StringBuilder line = new();

    public void Decompile(string file)
    {
        data = File.ReadAllBytes(file);
        pc = 0x100;
        labels.Add(0x100);
        try
        {
            Disassemble();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }

        StringBuilder output = new();
        foreach (var (pc, intr) in instructions)
        {
            if (labels.Contains(pc))
            {
                output.AppendLine();
                output.AppendLine($"label_{pc:X4}:");
            }
            output.AppendLine("    " + intr);
        }
        File.WriteAllText("decomp.gbasm", output.ToString());
    }

    public void Disassemble()
    {
        int iterations = 0;
        while (iterations < 10_000)
        {
            Instruction instr = Decode();
            WriteInstruction(instr);
            ushort newPC = Execute(instr);

            pc = newPC;

            iterations += 1;
        }
    }

    Instruction Decode()
    {
        byte opcode = data[pc];

        if (opcode == 0xCB)
        {
            throw new NotImplementedException();
        }

        Instruction instr = Ops.Unprefixed[opcode];
        return instr;
    }

    void WriteInstruction(Instruction instr)
    {
        Append(instr.Mnemonic);
        Append(' ');
        for (int i = 0; i < instr.Operands.Length; i++)
        {
            Operand op = instr.Operands[i];

            switch (op)
            {
                case Operand.a16:
                Append($"${data[pc + 2]:X2}{data[pc + 1]:X2}");
                labels.Add(pc);
                break;

                case Operand.a8:
                Append($"${data[pc + 1]:X2}");
                labels.Add((ushort)(pc + (sbyte)data[pc]));
                break;

                case Operand.n16:
                Append($"${data[pc + 2]:X2}{data[pc + 1]:X2}");
                break;

                case Operand.n8:
                Append($"${data[pc + 1]:X2}");
                break;

                case Operand.e8:
                Append($"{(sbyte)data[pc + 1]}");
                break;

                default:
                Append(op);
                break;
            }

            if (i < instr.Operands.Length - 1)
            {
                Append(", ");
            }
        }

        Comment($"{pc:X4}: {PeekRom(instr.Bytes)}");
    }

    ushort Execute(Instruction instr)
    {
        switch (instr.Mnemonic)
        {
            case Mnemonic.JR:
            case Mnemonic.CALL:
            case Mnemonic.JP:
            foreach (var operand in instr.Operands)
            {
                if (operand == Operand.a16)
                {
                    return U16(pc + 1);
                }
                else if (operand == Operand.e8)
                {
                    byte e8 = data[pc + 1];

                    if (instructions.ContainsKey(pc))
                    {
                        return (ushort)(pc + instr.Bytes);// Fallthrough
                    }
                    else
                    {
                        return (ushort)(pc + e8);
                    }
                }
                else if (operand == Operand.Z || (operand == Operand.NZ))
                {
                    //
                }
                else
                {
                    throw new NotImplementedException("" + instr.Operands[0] + " " + instr.Operands[1]);
                }
            }
            break;

            default: return (ushort)(pc + instr.Bytes);
        }

        return 0;
    }

    ushort U16(int pc)
    {
        return BitConverter.ToUInt16(data, pc);
    }

    string PeekRom(int bytes)
    {
        int offset = pc;
        StringBuilder sb = new();
        for (int i = 0; i < bytes; i++)
        {
            sb.Append($"{data[offset + i]:X2}");

            if (i < bytes - 1)
            {
                sb.Append(' ');
            }
        }
        return sb.ToString();
        // return $"{data[offset]:X2} {data[offset + 1]:X2} {data[offset + 2]:X2} {data[offset + 3]:X2}";
    }

    public static void Assert(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new Exception("Assertion failed: " + message);
        }
    }

    public void Append<T>(T val)
    {
        string? text = val?.ToString();
        line.Append(text);
    }

    public void NewLine()
    {
        instructions.TryAdd(pc, line.ToString());
        line.Clear();
    }

    public void Comment(string comment)
    {
        string padding = new(' ', 20 - line.Length);
        Append(padding + " ; " + comment);
        NewLine();
    }
}
