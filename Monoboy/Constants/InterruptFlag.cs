namespace Monoboy.Constants;

public static class InterruptFlag
{
    public const byte VBlank = Bit.Bit0;
    public const byte LCDStat = Bit.Bit1;
    public const byte Timer = Bit.Bit2;
    public const byte Serial = Bit.Bit3;
    public const byte Joypad = Bit.Bit4;
}
