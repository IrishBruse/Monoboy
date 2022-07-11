namespace Monoboy;

public enum GameboyButton : byte
{
    Right = Bit0,
    Left = Bit1,
    Up = Bit2,
    Down = Bit3,

    // Shifted right 4
    A = Bit4,
    B = Bit5,
    Select = Bit6,
    Start = Bit7,
}
