namespace Monoboy;

using Monoboy.Constants;

public enum GameboyButton : byte
{
    Right = Bit.Bit0,
    Left = Bit.Bit1,
    Up = Bit.Bit2,
    Down = Bit.Bit3,

    // Shifted right 4
    A = Bit.Bit4,
    B = Bit.Bit5,
    Select = Bit.Bit6,
    Start = Bit.Bit7,
}
