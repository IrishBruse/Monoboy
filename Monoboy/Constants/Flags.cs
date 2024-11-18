namespace Monoboy.Constants;

public class Flags
{
    public const byte LCDEnabled = Bit.Bit7;
    public const byte WindowTilemap = Bit.Bit6;
    public const byte WindowEnabled = Bit.Bit5;
    public const byte Tileset = Bit.Bit4;
    public const byte Tilemap = Bit.Bit3;
    public const byte SpritesSize = 0b100;
    public const byte SpritesEnabled = 0b10;
    public const byte BackgroundEnabled = 0b1;
    public const byte VBlank = 0b1;
    public const byte LCDStat = 0b10;
    public const byte Timer = 0b100;
    public const byte Serial = Bit.Bit3;
    public const byte Joypad = Bit.Bit4;
}
