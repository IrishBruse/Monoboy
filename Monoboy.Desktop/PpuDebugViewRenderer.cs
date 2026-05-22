namespace Monoboy.Desktop;

using System;

using Monoboy;
using Monoboy.Constants;
using Monoboy.Utility;

using Raylib_cs;

/// <summary>Builds RGBA buffers for Game Boy PPU debug panels (tilemaps, VRAM, OAM).</summary>
static class PpuDebugViewRenderer
{
    public const int TileMapPixels = 256;
    public const int VramTilesWidth = 256;
    public const int VramTilesHeight = 128;
    public const int OamGridCols = 10;
    public const int OamGridRows = 4;
    public const int OamCellWidth = 8;

    const int VramBase = 0x8000;
    const int OamBase = 0xFE00;

    public static void FillTileMap(byte[] rgba, Emulator emulator, bool windowMap)
    {
        byte lcdc = emulator.Read(0xFF40);
        bool tileset = lcdc.GetBit(Flags.Tileset);
        bool use9C00 = windowMap
            ? lcdc.GetBit(Flags.WindowTilemap)
            : lcdc.GetBit(Flags.Tilemap);

        int tilesetAddress = tileset ? 0x0000 : 0x1000;
        int tilemapAddress = use9C00 ? 0x1C00 : 0x1800;
        byte bgp = emulator.Read(0xFF47);

        for (int y = 0; y < TileMapPixels; y++)
        {
            int row = y / 8;
            for (int x = 0; x < TileMapPixels; x++)
            {
                int col = x / 8;
                byte rawTile = emulator.Read((ushort)(VramBase + tilemapAddress + (row * 32) + col));
                WriteTilePixel(rgba, x, y, emulator, tileset, tilesetAddress, rawTile, bgp);
            }
        }
    }

    public static void FillVramTiles(byte[] rgba, Emulator emulator)
    {
        byte bgp = emulator.Read(0xFF47);
        for (int y = 0; y < VramTilesHeight; y++)
        {
            int row = y / 8;
            for (int x = 0; x < VramTilesWidth; x++)
            {
                int col = x / 8;
                int tileIndex = (row * 32) + col;
                int tileGraphicAddress = tileIndex * 16;
                WriteTileGraphicPixel(rgba, x, y, emulator, tileGraphicAddress, bgp);
            }
        }
    }

    public static void FillOamGrid(byte[] rgba, Emulator emulator)
    {
        Array.Clear(rgba);
        byte lcdc = emulator.Read(0xFF40);
        int spriteHeight = lcdc.GetBit(Flags.SpritesSize) ? 16 : 8;

        for (int slot = 0; slot < 40; slot++)
        {
            int col = slot % OamGridCols;
            int row = slot / OamGridCols;
            int destX = col * OamCellWidth;
            int destY = row * spriteHeight;

            ushort oamOffset = (ushort)(OamBase + (slot * 4));
            byte tileId = emulator.Read((ushort)(oamOffset + 2));
            byte flags = emulator.Read((ushort)(oamOffset + 3));
            byte obp = flags.GetBit(0b10000) ? emulator.Read(0xFF49) : emulator.Read(0xFF48);

            bool mirrorX = flags.GetBit(1 << 5);
            bool mirrorY = flags.GetBit(1 << 6);

            DrawSpriteTile(rgba, destX, destY, emulator, tileId, spriteHeight, obp, mirrorX, mirrorY);
        }
    }

    public static int OamGridHeight(Emulator emulator)
    {
        byte lcdc = emulator.Read(0xFF40);
        int spriteHeight = lcdc.GetBit(Flags.SpritesSize) ? 16 : 8;
        return OamGridRows * spriteHeight;
    }

    public static int OamGridWidth => OamGridCols * OamCellWidth;

    static void WriteTilePixel(
        byte[] rgba,
        int x,
        int y,
        Emulator emulator,
        bool tileset8000,
        int tilesetAddress,
        byte rawTile,
        byte bgp)
    {
        int vramAddress = tileset8000
            ? (rawTile * 16) + tilesetAddress
            : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

        WriteTileGraphicPixel(rgba, x, y, emulator, vramAddress, bgp);
    }

    static void WriteTileGraphicPixel(byte[] rgba, int x, int y, Emulator emulator, int tileGraphicAddress, byte bgp)
    {
        int line = (y % 8) * 2;
        byte data1 = emulator.Read((ushort)(VramBase + tileGraphicAddress + line));
        byte data2 = emulator.Read((ushort)(VramBase + tileGraphicAddress + line + 1));
        byte palletIndex = DecodeTilePaletteIndex(data1, data2, x % 8);
        byte colorIndex = (byte)((bgp >> (palletIndex * 2)) & 0b11);
        WriteColor(rgba, x + (y * TileMapPixels), Pallet.GetColor(colorIndex));
    }

    static void DrawSpriteTile(
        byte[] rgba,
        int destX,
        int destY,
        Emulator emulator,
        byte tileId,
        int spriteHeight,
        byte obp,
        bool mirrorX,
        bool mirrorY)
    {
        int width = OamGridWidth;
        int height = OamGridHeight(emulator);

        for (int ty = 0; ty < spriteHeight; ty++)
        {
            int tileRow = spriteHeight == 16 && ty >= 8 ? ty - 8 : ty;
            int vramTile = spriteHeight == 16 && ty >= 8 ? tileId + 1 : tileId;
            int row = mirrorY ? spriteHeight - 1 - ty : tileRow;
            int line = row * 2;

            byte data1 = emulator.Read((ushort)(VramBase + (vramTile * 16) + line));
            byte data2 = emulator.Read((ushort)(VramBase + (vramTile * 16) + line + 1));

            for (int tx = 0; tx < 8; tx++)
            {
                int pixelBit = mirrorX ? tx : 7 - tx;
                byte palletIndex = (byte)(((data2 >> pixelBit) & 1) << 1 | ((data1 >> pixelBit) & 1));
                if (palletIndex == 0)
                {
                    continue;
                }

                byte colorIndex = (byte)((obp >> (palletIndex * 2)) & 0b11);
                int px = destX + tx;
                int py = destY + ty;
                if (px < 0 || py < 0 || px >= width || py >= height)
                {
                    continue;
                }

                WriteColor(rgba, px + (py * width), Pallet.GetColor(colorIndex));
            }
        }
    }

    static byte DecodeTilePaletteIndex(byte data1, byte data2, int xInTile)
    {
        byte bit = (byte)(1 << (((xInTile % 8) - 7) * 0xff));
        return (byte)(((data2 & bit) != 0 ? 1 : 0) << 1 | ((data1 & bit) != 0 ? 1 : 0));
    }

    static void WriteColor(byte[] rgba, int pixelIndex, uint color)
    {
        int i = pixelIndex * 4;
        rgba[i + 0] = (byte)color;
        rgba[i + 1] = (byte)((color & 0xff00) >> 8);
        rgba[i + 2] = (byte)((color & 0xff0000) >> 16);
        rgba[i + 3] = 255;
    }

    public static Color PpuRegisterBackdrop => new(0xD0, 0xD0, 0x58, 0xFF);
}
