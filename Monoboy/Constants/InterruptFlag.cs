namespace Monoboy.Constants;

public static class InterruptFlag
{
    public const byte VBlank = 0b1;
    public const byte LCDStat = 0b10;
    public const byte Timer = 0b100;
    public const byte Serial = 0b1000;
    public const byte Joypad = 0b10000;
}
