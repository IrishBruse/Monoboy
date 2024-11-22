namespace Monoboy;
public enum GameboyButton : byte
{
    Right = 0b1,
    Left = 0b10,
    Up = 0b100,
    Down = 0b1000,

    // Shifted right 4
    A = 0b10000,
    B = 0b100000,
    Select = 1 << 6,
    Start = 0b10000000,
}
