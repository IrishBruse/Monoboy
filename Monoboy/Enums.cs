using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monoboy.Core
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
        DisplayToggle = Bit.Bit7,
        WindowTilemap = Bit.Bit6,
        WindowToggle = Bit.Bit5,
        BackgroundWindowData = Bit.Bit4,
        BackgroundTilemap = Bit.Bit3,
        SpritesSize = Bit.Bit2,
        SpritesToggle = Bit.Bit1,
        BackgroundToggle = Bit.Bit0,
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
        Drawing = 0b11,
        Hblank = 0b00,
        Vblank = 0b01,
    }

    public enum Button : byte
    {
        Right = 0b00000001,
        Left = 0b00000010,
        Up = 0b00000100,
        Down = 0b00001000,
        A = 0b00010000,
        B = 0b00100000,
        Select = 0b01000000,
        Start = 0b10000000,
    }
}
