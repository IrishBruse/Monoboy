namespace Monoboy.Constants;

public static class InterruptFlag
{
    public const byte VBlank = Bit0;
    public const byte LCDStat = Bit1;
    public const byte Timer = Bit2;
    public const byte Serial = Bit3;
    public const byte Joypad = Bit4;
}
