namespace Monoboy;

using System;

using Monoboy.Constants;
using Monoboy.Utility;

public class Ppu
{
    public const byte LCDEnabled = 0b10000000;
    public const byte WindowTilemap = 0b01000000;
    public const byte WindowEnabled = 0b00100000;
    public const byte Tileset = 0b00010000;
    public const byte Tilemap = 0b00001000;
    public const byte SpritesSize = 0b00000100;
    public const byte SpritesEnabled = 0b00000010;
    public const byte BackgroundEnabled = 0b00000001;

    public const byte VBlank = 0b00000001;
    public const byte LCDStat = 0b00000010;
    public const byte Timer = 0b00000100;
    public const byte Serial = 0b00001000;
    public const byte Joypad = 0b00010000;

    private const int IO = 0xFF80;

    public byte LCDC { get => memory[IO + 0x0040]; set => memory[IO + 0x0040] = value; }
    public byte Stat { get => memory[IO + 0x0041]; set => memory[IO + 0x0041] = value; }
    public byte StatMode { get => memory[IO + 0x0041].GetBits(0b00000011); set => memory[IO + 0x0041] = memory[IO + 0x0041].SetBits(0b00000011, value); }
    public byte SCY { get => memory[IO + 0x0042]; set => memory[IO + 0x0042] = value; }
    public byte SCX { get => memory[IO + 0x0043]; set => memory[IO + 0x0043] = value; }
    public byte WX { get => (byte)(memory[IO + 0x004B] - 7); set => memory[IO + 0x004B] = value; }
    public byte WY { get => memory[IO + 0x004A]; set => memory[IO + 0x004A] = value; }
    public byte LY { get => memory[IO + 0x0044]; set => memory[IO + 0x0044] = value; }
    public byte LYC { get => memory[IO + 0x0045]; set => memory[IO + 0x0045] = value; }
    public byte BGP { get => memory[IO + 0x0047]; set => memory[IO + 0x0047] = value; }
    public byte OBP0 { get => memory[IO + 0x0048]; set => memory[IO + 0x0048] = value; }
    public byte OBP1 { get => memory[IO + 0x0049]; set => memory[IO + 0x0049] = value; }

    private readonly byte[] memory;
    private readonly Emulator emulator;
    private readonly Cpu cpu;

    private const int ScanLineCycles = 114;
    private const int HBlankCycles = 51;
    private const int OamCycles = 20;
    private const int VRamCycles = 43;
    private int cycles;

    private byte[,] framebuffer;

    public Ppu(byte[] memory, Emulator emulator, Cpu cpu)
    {
        this.memory = memory;
        this.emulator = emulator;
        this.cpu = cpu;
        framebuffer = new byte[Emulator.WindowWidth, Emulator.WindowHeight];
    }

    public void Step(int ticks)
    {
        cycles += ticks;

        if (LCDC.GetBit(LCDEnabled) == false)
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
                    cpu.RequestInterrupt(VBlank);
                    // DrawFrame?.Invoke();
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
            _ = Stat.SetBit(0b00000100, true);
            if (Stat.GetBit(0b01000000))
            {
                cpu.RequestInterrupt(LCDStat);
            }
        }
        else
        {
            _ = Stat.SetBit(0b00000100, false);
        }
    }

    internal void Reset()
    {
        cycles = 0;
    }

    private void HandleModeChange(byte newMode)
    {
        StatMode = newMode;

        if (newMode == 2 && Stat.GetBit(0b00100000))
        {
            cpu.RequestInterrupt(LCDStat);
        }
        else if (newMode == 0 && Stat.GetBit(0b00001000))
        {
            cpu.RequestInterrupt(LCDStat);
        }
        else if (newMode == 1 && Stat.GetBit(0b00010000))
        {
            cpu.RequestInterrupt(LCDStat);
        }
    }

    private void DrawScanline()
    {
        if (LCDC.GetBit(BackgroundEnabled))
        {
            DrawBackground();
        }

        if (LCDC.GetBit(WindowEnabled))
        {
            DrawWindow();
        }

        if (LCDC.GetBit(SpritesEnabled))
        {
            DrawSprites();
        }
    }

    private void DrawBackground()
    {
        bool tileset = LCDC.GetBit(Tileset);
        int tilesetAddress = tileset ? 0x0000 : 0x1000;
        int tilemapAddress = LCDC.GetBit(Tilemap) ? 0x1C00 : 0x1800;

        byte y = (byte)(LY + SCY);
        int row = y / 8;

        for (byte i = 0; i < Emulator.WindowWidth; i++)
        {
            byte x = (byte)(i + SCX);
            int colum = x / 8;
            byte rawTile = memory[8000 + tilemapAddress + (row * 32) + colum];

            int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

            int line = (byte)(y % 8) * 2;
            byte data1 = memory[8000 + vramAddress + line];
            byte data2 = memory[8000 + vramAddress + line + 1];

            byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
            byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
            byte colorIndex = (byte)((BGP >> (palletIndex * 2)) & 0b11);

            if (emulator.BackgroundEnabled)
            {
                framebuffer[i, LY] = colorIndex;
            }
        }
    }

    private void DrawWindow()
    {
        bool tileset = LCDC.GetBit(Tileset);

        int tilesetAddress = tileset ? 0x0000 : 0x1000;
        int tilemapAddress = LCDC.GetBit(WindowTilemap) ? 0x1C00 : 0x1800;

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
            byte rawTile = memory[8000 + tilemapAddress + (row * 32) + colum];

            int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

            int line = (byte)(y % 8) * 2;
            byte data1 = memory[8000 + vramAddress + line];
            byte data2 = memory[8000 + vramAddress + line + 1];

            byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
            byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
            byte colorIndex = (byte)((BGP >> (palletIndex * 2)) & 0b11);

            if (!emulator.WindowEnabled)
            {
                continue;
            }

            framebuffer[i, LY] = colorIndex;
        }
    }

    private void DrawSprites()
    {
        int spriteSize = LCDC.GetBit(SpritesSize) ? 16 : 8;

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

                    if (!emulator.SpritesEnabled)
                    {
                        continue;
                    }

                    if (x + r is >= 0 and < Emulator.WindowWidth)
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
}
