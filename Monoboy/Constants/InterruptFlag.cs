namespace Monoboy.Constants;

public static class InterruptFlag
{
    public const byte VBlank = 0b00000001;
    public const byte LCDStat = 0b00000010;
    public const byte Timer = 0b00000100;
    public const byte Serial = 0b00001000;
    public const byte Joypad = 0b00010000;
}
