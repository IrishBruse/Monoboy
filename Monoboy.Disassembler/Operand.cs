namespace Monoboy.Disassembler;

public enum Operand
{
    Bit0 = 0,
    Bit1 = 1,
    Bit2 = 2,
    Bit3 = 3,
    Bit4 = 4,
    Bit5 = 5,
    Bit6 = 6,
    Bit7 = 7,

    // 8-bit Regs
    A,
    B, C,
    D, E,
    H, L,

    // 16-bit Regs
    AF,
    BC,
    HL,
    SP,
    DE,


    a16,
    n16,
    n8,
    e8,
    a8,

    // Jump
    NZ,
    NC,
    Z,

    // RST
    RST00,
    RST08,
    RST10,
    RST18,
    RST20,
    RST28,
    RST30,
    RST38,
}
