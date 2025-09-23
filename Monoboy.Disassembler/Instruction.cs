namespace Monoboy.Disassembler;

public class Instruction(Mnemonic mnemonic, ushort bytes, Operand[] operands, byte opcode)
{
    public Mnemonic Mnemonic { get; } = mnemonic;
    public ushort Bytes { get; } = bytes;
    public Operand[] Operands { get; } = operands;
    public byte Opcode { get; } = opcode;

    public override string ToString()
    {
        string output = Mnemonic.ToString();

        foreach (var op in Operands)
        {
            output += " " + op;
        }

        return output;
    }
}
