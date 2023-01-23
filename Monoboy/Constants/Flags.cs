namespace Monoboy.Constants;

public class Flags
{
    public const byte LCDEnabled = Bit.Bit7;
    public const byte WindowTilemap = Bit.Bit6;
    public const byte WindowEnabled = Bit.Bit5;
    public const byte Tileset = Bit.Bit4;
    public const byte Tilemap = Bit.Bit3;
    public const byte SpritesSize = Bit.Bit2;
    public const byte SpritesEnabled = Bit.Bit1;
    public const byte BackgroundEnabled = Bit.Bit0;
    public const byte VBlank = Bit.Bit0;
    public const byte LCDStat = Bit.Bit1;
    public const byte Timer = Bit.Bit2;
    public const byte Serial = Bit.Bit3;
    public const byte Joypad = Bit.Bit4;
}
