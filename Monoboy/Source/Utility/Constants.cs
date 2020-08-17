using System.Runtime.CompilerServices;

namespace Monoboy.Constants
{
    public static class Constant
    {
        public const int CyclesPerFrame = 69905;
        public const byte WindowWidth = 160;
        public const byte WindowHeight = 144;
    }

    public static class Bit
    {
        public const byte Bit7 = 0b10000000;
        public const byte Bit6 = 0b01000000;
        public const byte Bit5 = 0b00100000;
        public const byte Bit4 = 0b00010000;
        public const byte Bit3 = 0b00001000;
        public const byte Bit2 = 0b00000100;
        public const byte Bit1 = 0b00000010;
        public const byte Bit0 = 0b00000001;
    }

    public static class Flag
    {
        public const byte Z = Bit.Bit7;
        public const byte N = Bit.Bit6;
        public const byte H = Bit.Bit5;
        public const byte C = Bit.Bit4;
    }

    public static class LCDCBit
    {
        public const byte LCDEnabled = Bit.Bit7;
        public const byte WindowTilemap = Bit.Bit6;
        public const byte WindowEnabled = Bit.Bit5;
        public const byte Tileset = Bit.Bit4;
        public const byte Tilemap = Bit.Bit3;
        public const byte SpritesSize = Bit.Bit2;
        public const byte SpritesEnabled = Bit.Bit1;
        public const byte BackgroundWindowPriority = Bit.Bit0;
    }

    public static class StatBit
    {
        public const byte CoincidenceInterrupt = Bit.Bit6;
        public const byte OAMInterrupt = Bit.Bit5;
        public const byte VBlankInterrupt = Bit.Bit4;
        public const byte HBlankInterrupt = Bit.Bit3;
        public const byte CoincidenceFlag = Bit.Bit2;
        public const byte ModeFlag1 = Bit.Bit1;
        public const byte ModeFlag0 = Bit.Bit0;
    }

    public static class Mode
    {
        public const byte Hblank = 0;
        public const byte Vblank = 1;
        public const byte OAM = 2;
        public const byte VRAM = 3;
    }

    public static class InterruptFlag
    {
        public const byte VBlank = Bit.Bit0;
        public const byte LCDStat = Bit.Bit1;
        public const byte Timer = Bit.Bit2;
        public const byte Serial = Bit.Bit3;
        public const byte Joypad = Bit.Bit4;
    }
}