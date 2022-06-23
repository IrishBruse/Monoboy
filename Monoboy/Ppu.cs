﻿namespace Monoboy;

using System;

using Monoboy.Constants;
using Monoboy.Utility;

public class Ppu
{
    public byte[] VideoRam
    {
        get
        {
            return emulator.memory.vram;
        }

        set
        {
            emulator.memory.vram = value;
        }
    }

    public byte LCDC
    {
        get
        {
            return emulator.memory.io[0x0040];
        }

        set
        {
            emulator.memory.io[0x0040] = value;
        }
    }
    public byte Stat
    {
        get
        {
            return emulator.memory.io[0x0041];
        }

        set
        {
            emulator.memory.io[0x0041] = value;
        }
    }
    public byte StatMode
    {
        get
        {
            return emulator.memory.io[0x0041].GetBits(0b00000011);
        }

        set
        {
            emulator.memory.io[0x0041] = emulator.memory.io[0x0041].SetBits(0b00000011, value);
        }
    }
    public byte SCY
    {
        get
        {
            return emulator.memory.io[0x0042];
        }

        set
        {
            emulator.memory.io[0x0042] = value;
        }
    }
    public byte SCX
    {
        get
        {
            return emulator.memory.io[0x0043];
        }

        set
        {
            emulator.memory.io[0x0043] = value;
        }
    }
    public byte WX
    {
        get
        {
            return (byte)(emulator.memory.io[0x004B] - 7);
        }

        set
        {
            emulator.memory.io[0x004B] = value;
        }
    }
    public byte WY
    {
        get
        {
            return emulator.memory.io[0x004A];
        }

        set
        {
            emulator.memory.io[0x004A] = value;
        }
    }
    public byte LY
    {
        get
        {
            return emulator.memory.io[0x0044];
        }

        set
        {
            emulator.memory.io[0x0044] = value;
        }
    }
    public byte LYC
    {
        get
        {
            return emulator.memory.io[0x0045];
        }

        set
        {
            emulator.memory.io[0x0045] = value;
        }
    }
    public byte BGP
    {
        get
        {
            return emulator.memory.io[0x0047];
        }

        set
        {
            emulator.memory.io[0x0047] = value;
        }
    }
    public byte OBP0
    {
        get
        {
            return emulator.memory.io[0x0048];
        }

        set
        {
            emulator.memory.io[0x0048] = value;
        }
    }
    public byte OBP1
    {
        get
        {
            return emulator.memory.io[0x0049];
        }

        set
        {
            emulator.memory.io[0x0049] = value;
        }
    }

    const int ScanLineCycles = 456 / 4;
    const int HBlankCycles = 204 / 4;
    const int OamCycles = 80 / 4;
    const int VRamCycles = 172 / 4;

    public int cycles;

    readonly Emulator emulator;

    public byte[,] framebuffer;

    public Action DrawFrame;

    public Ppu(Emulator emulator)
    {
        this.emulator = emulator;
        framebuffer = new byte[Emulator.WindowWidth, Emulator.WindowHeight];
    }

    public void Step(int ticks)
    {
        cycles += ticks;

        if (LCDC.GetBit(LCDCBit.LCDEnabled) == false)
        {
            LY = 0;
            cycles = 0;
            StatMode = 0;
            return;
        }

        switch (StatMode)
        {
            case Mode.Hblank:
            if (cycles >= HBlankCycles)
            {
                cycles -= HBlankCycles;
                LY++;

                if (LY == Emulator.WindowHeight)
                {
                    HandleModeChange(Mode.Vblank);
                    emulator.interrupt.RequestInterrupt(InterruptFlag.VBlank);
                    DrawFrame?.Invoke();
                }
                else
                {
                    HandleModeChange(Mode.OAM);
                }
            }
            break;

            case Mode.Vblank:
            if (cycles >= ScanLineCycles)
            {
                cycles -= ScanLineCycles;
                LY++;

                if (LY == 154)
                {
                    HandleModeChange(Mode.OAM);
                    LY = 0;
                }
            }
            break;

            case Mode.OAM:
            if (cycles >= OamCycles)
            {
                cycles -= OamCycles;
                HandleModeChange(Mode.VRAM);
            }
            break;

            case Mode.VRAM:
            if (cycles >= VRamCycles)
            {
                cycles -= VRamCycles;
                DrawScanline();
                HandleModeChange(Mode.Hblank);
            }
            break;
            default:
            break;
        }

        if (LY == LYC)
        {
            Stat.SetBit(0b00000100, true);
            if (Stat.GetBit(0b01000000))
            {
                emulator.interrupt.RequestInterrupt(InterruptFlag.LCDStat);
            }
        }
        else
        {
            Stat.SetBit(0b00000100, false);
        }
    }

    internal void Reset()
    {
        cycles = 0;
    }

    void HandleModeChange(byte newMode)
    {
        StatMode = newMode;

        if (newMode == 2 && Stat.GetBit(0b00100000))
        {
            emulator.interrupt.RequestInterrupt(InterruptFlag.LCDStat);
        }
        else if (newMode == 0 && Stat.GetBit(0b00001000))
        {
            emulator.interrupt.RequestInterrupt(InterruptFlag.LCDStat);
        }
        else if (newMode == 1 && Stat.GetBit(0b00010000))
        {
            emulator.interrupt.RequestInterrupt(InterruptFlag.LCDStat);
        }
    }

    #region Drawing
    void DrawScanline()
    {
        if (LCDC.GetBit(LCDCBit.BackgroundEnabled))
        {
            DrawBackground();
        }

        if (LCDC.GetBit(LCDCBit.WindowEnabled))
        {
            DrawWindow();
        }

        if (LCDC.GetBit(LCDCBit.SpritesEnabled))
        {
            DrawSprites();
        }
    }

    void DrawBackground()
    {
        bool tileset = LCDC.GetBit(LCDCBit.Tileset);
        int tilesetAddress = tileset ? 0x0000 : 0x1000;
        int tilemapAddress = LCDC.GetBit(LCDCBit.Tilemap) ? 0x1C00 : 0x1800;

        byte y = (byte)(LY + SCY);
        int row = y / 8;

        for (byte i = 0; i < Emulator.WindowWidth; i++)
        {
            byte x = (byte)(i + SCX);
            int colum = x / 8;
            byte rawTile = emulator.ppu.VideoRam[tilemapAddress + (row * 32) + colum];

            int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

            int line = (byte)(y % 8) * 2;
            byte data1 = VideoRam[vramAddress + line];
            byte data2 = VideoRam[vramAddress + line + 1];

            byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
            byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
            byte colorIndex = (byte)((BGP >> (palletIndex * 2)) & 0b11);

            if (emulator.BackgroundEnabled)
            {
                framebuffer[i, LY] = colorIndex;
            }
        }
    }

    void DrawWindow()
    {
        bool tileset = LCDC.GetBit(LCDCBit.Tileset);

        int tilesetAddress = tileset ? 0x0000 : 0x1000;
        int tilemapAddress = LCDC.GetBit(LCDCBit.WindowTilemap) ? 0x1C00 : 0x1800;

        byte y = (byte)(LY - WY);
        int row = y / 8;

        for (byte i = WX; i < Emulator.WindowWidth; i++)
        {
            byte x = (byte)(i + SCX);
            if (x >= WX)
            {
                x = (byte)(i - WX);
            }
            int colum = x / 8;
            byte rawTile = emulator.ppu.VideoRam[tilemapAddress + (row * 32) + colum];

            int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

            int line = (byte)(y % 8) * 2;
            byte data1 = VideoRam[vramAddress + line];
            byte data2 = VideoRam[vramAddress + line + 1];

            byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
            byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
            byte colorIndex = (byte)((BGP >> (palletIndex * 2)) & 0b11);

            if (emulator.WindowEnabled)
            {
                framebuffer[i, LY] = colorIndex;
            }
        }
    }

    void DrawSprites()
    {
        int spriteSize = LCDC.GetBit(LCDCBit.SpritesSize) ? 16 : 8;

        for (int i = 64; i >= 0; i--)
        {
            ushort offset = (ushort)(0xFE00 + (i * 4));

            int y = emulator.Read((ushort)(offset + 0)) - 16;
            int x = emulator.Read((ushort)(offset + 1)) - 8;
            byte tileID = emulator.Read((ushort)(offset + 2));
            byte flags = emulator.Read((ushort)(offset + 3));
            byte obp = flags.GetBit(0b00010000) ? OBP1 : OBP0;

            bool mirrorX = flags.GetBit(0b00100000);
            bool mirrorY = flags.GetBit(0b01000000);
            bool aboveBG = flags.GetBit(0b10000000);

            if (LY >= y && LY < y + spriteSize)
            {
                int row = mirrorY ? spriteSize - 1 - (LY - y) : LY - y;

                int vramAddress = 0x8000 + (tileID * 16) + (row * 2);
                byte data1 = emulator.Read((ushort)(vramAddress + 0));
                byte data2 = emulator.Read((ushort)(vramAddress + 1));

                for (int r = 0; r < 8; r++)
                {
                    int pixelBit = mirrorX ? r : 7 - r;

                    int hi = (data2 >> pixelBit) & 1;
                    int lo = (data1 >> pixelBit) & 1;
                    byte palletIndex = (byte)((hi << 1) | lo);
                    byte palletColor = (byte)((obp >> (palletIndex * 2)) & 0b11);

                    if (x + r >= 0 && x + r < Emulator.WindowWidth)
                    {
                        if (palletIndex != 0 && (aboveBG == false || framebuffer[x - r, LY] == ((byte)(BGP & 0b11))))
                        {
                            framebuffer[x + r, LY] = palletColor;
                        }
                    }
                }
            }
        }
    }
    #endregion
}
