namespace Monoboy.Constants;

public static class StatBit
{
    public const byte CoincidenceInterrupt = 0b01000000;
    public const byte OAMInterrupt = 0b00100000;
    public const byte VBlankInterrupt = 0b00010000;
    public const byte HBlankInterrupt = 0b00001000;
    public const byte CoincidenceFlag = 0b00000100;
    public const byte ModeFlag1 = 0b00000010;
    public const byte ModeFlag0 = 0b00000001;
}
