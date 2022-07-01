namespace Monoboy;

public enum GameboyButton : byte
{
    Right = 0b00000001,
    Left = 0b00000010,
    Up = 0b00000100,
    Down = 0b00001000,

    // Shifted right 4
    A = 0b00010000,
    B = 0b00100000,
    Select = 0b01000000,
    Start = 0b10000000,
}
