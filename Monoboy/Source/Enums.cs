﻿namespace Monoboy
{
    public enum Bit : byte
    {
        Bit7 = 0b10000000,
        Bit6 = 0b01000000,
        Bit5 = 0b00100000,
        Bit4 = 0b00010000,
        Bit3 = 0b00001000,
        Bit2 = 0b00000100,
        Bit1 = 0b00000010,
        Bit0 = 0b00000001,
    }

    public enum LCDCBit : byte
    {
        LCDEnabled = Bit.Bit7,
        WindowTilemap = Bit.Bit6,
        WindowEnabled = Bit.Bit5,
        Tileset = Bit.Bit4,
        Tilemap = Bit.Bit3,
        SpritesSize = Bit.Bit2,
        SpritesEnabled = Bit.Bit1,
        BackgroundWindowPriority = Bit.Bit0,
    }

    public enum StatBit : byte
    {
        CoincidenceInterrupt = Bit.Bit6,
        OAMInterrupt = Bit.Bit5,
        VBlankInterrupt = Bit.Bit4,
        HBlankInterrupt = Bit.Bit3,
        CoincidenceFlag = Bit.Bit2,
        ModeFlag1 = Bit.Bit1,
        ModeFlag0 = Bit.Bit0,
    }

    public enum Flag : byte
    {
        Zero = Bit.Bit7,
        Negative = Bit.Bit6,
        HalfCarry = Bit.Bit5,
        FullCarry = Bit.Bit4
    }

    public enum Mode
    {
        OAM = 0b10,
        VRAM = 0b11,
        Hblank = 0b00,
        Vblank = 0b01,
    }
}
