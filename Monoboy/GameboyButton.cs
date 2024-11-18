namespace Monoboy;

using Monoboy.Constants;

public enum GameboyButton : byte
{
    Right = 0b1,
    Left = 0b10,
    Up = 0b100,
    Down = Bit.Bit3,

    // Shifted right 4
    A = Bit.Bit4,
    B = Bit.Bit5,
    Select = Bit.Bit6,
    Start = Bit.Bit7,
}
