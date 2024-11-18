namespace Monoboy.Constants;

public static class InterruptFlag
{
    public const byte VBlank = 0b1;
    public const byte LCDStat = 0b10;
    public const byte Timer = 0b100;
    public const byte Serial = Bit.Bit3;
    public const byte Joypad = Bit.Bit4;
}
