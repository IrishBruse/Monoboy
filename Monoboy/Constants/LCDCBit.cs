namespace Monoboy.Constants;

public static class LCDCBit
{
    public const byte LCDEnabled = 0b10000000;
    public const byte WindowTilemap = 0b01000000;
    public const byte WindowEnabled = 0b00100000;
    public const byte Tileset = 0b00010000;
    public const byte Tilemap = 0b00001000;
    public const byte SpritesSize = 0b00000100;
    public const byte SpritesEnabled = 0b00000010;
    public const byte BackgroundEnabled = 0b00000001;
}
