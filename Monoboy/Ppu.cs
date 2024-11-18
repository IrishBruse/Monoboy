namespace Monoboy;

using Monoboy.Constants;
using Monoboy.Utility;

using static Monoboy.Constants.Bit;

public class Ppu(Memory memory, Emulator emulator, Cpu cpu, byte[] framebuffer)
{
    public byte LCDC { get => memory[0xFF40]; set => memory[0xFF40] = value; }
    public byte Stat { get => memory[0xFF41]; set => memory[0xFF41] = value; }
    public byte StatMode { get => (byte)(memory[0xFF41] & 0b00000011); set => memory[0xFF41] = memory[0xFF41] = (byte)(0b00000011 & value); }
    public byte SCY { get => memory[0xFF42]; set => memory[0xFF42] = value; }
    public byte SCX { get => memory[0xFF43]; set => memory[0xFF43] = value; }
    public byte WX { get => (byte)(memory[0xFF4B] - 6); set => memory[0xFF4B] = value; }
    public byte WY { get => memory[0xFF4A]; set => memory[0xFF4A] = value; }
    public byte LY { get => memory[0xFF44]; set => memory[0xFF44] = value; }
    public byte LYC { get => memory[0xFF45]; set => memory[0xFF45] = value; }
    public byte BGP { get => memory[0xFF47]; set => memory[0xFF47] = value; }
    public byte OBP0 { get => memory[0xFF48]; set => memory[0xFF48] = value; }
    public byte OBP1 { get => memory[0xFF49]; set => memory[0xFF49] = value; }

    readonly Memory memory = memory;
    readonly Emulator emulator = emulator;
    readonly Cpu cpu = cpu;
    readonly byte[] framebuffer = framebuffer;
    const int ScanLineCycles = 114;
    const int HBlankCycles = 51;
    const int OamCycles = 20;
    const int VRamCycles = 43;
    const int VramAddress = 0x8000;
    int cycles;

    public void Step(int ticks)
    {
        if (LCDC.GetBit(Flags.LCDEnabled) == false)
        {
            LY = 0;
            cycles = 0;
            StatMode = 0;
            return;
        }

        cycles += ticks;

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
                    cpu.RequestInterrupt(Flags.VBlank);
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
            Stat = Stat.SetBit(0b100, true);
            if (Stat.GetBit(Bit6))
            {
                cpu.RequestInterrupt(Flags.LCDStat);
            }
        }
        else
        {
            Stat = Stat.SetBit(0b100, false);
        }
    }

    internal void Reset()
    {
        cycles = 0;
    }

    void HandleModeChange(byte newMode)
    {
        StatMode = newMode;

        if (newMode == 2 && Stat.GetBit(Bit5))
        {
            cpu.RequestInterrupt(Flags.LCDStat);
        }
        else if (newMode == 0 && Stat.GetBit(Bit3))
        {
            cpu.RequestInterrupt(Flags.LCDStat);
        }
        else if (newMode == 1 && Stat.GetBit(Bit4))
        {
            cpu.RequestInterrupt(Flags.LCDStat);
        }
    }

    void DrawScanline()
    {
        if (LCDC.GetBit(Flags.BackgroundEnabled))
        {
            DrawBackground();
        }

        if (LCDC.GetBit(Flags.WindowEnabled) && WY <= LY)
        {
            DrawWindow();
        }

        if (LCDC.GetBit(Flags.SpritesEnabled))
        {
            DrawSprites();
        }
    }

    void DrawBackground()
    {
        bool tileset = LCDC.GetBit(Flags.Tileset);

        int tilesetAddress = tileset ? 0x0000 : 0x1000;
        int tilemapAddress = LCDC.GetBit(Flags.Tilemap) ? 0x1C00 : 0x1800;

        byte y = (byte)(LY + SCY);
        int row = y / 8;

        for (byte i = 0; i < Emulator.WindowWidth; i++)
        {
            byte x = (byte)(i + SCX);
            int colum = x / 8;
            byte rawTile = memory[VramAddress + tilemapAddress + (row * 32) + colum];

            int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

            int line = (byte)(y % 8) * 2;
            byte data1 = memory[VramAddress + vramAddress + line];
            byte data2 = memory[VramAddress + vramAddress + line + 1];

            byte bit = (byte)(1 << (((x % 8) - 7) * 0xff));
            byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
            byte colorIndex = (byte)((BGP >> (palletIndex * 2)) & 0b11);

            uint color = Pallet.GetColor(colorIndex);
            int index = i + (Emulator.WindowWidth * LY);
            framebuffer[(index * 4) + 0] = (byte)color;
            framebuffer[(index * 4) + 1] = (byte)((color & 0xff00) >> 8);
            framebuffer[(index * 4) + 2] = (byte)((color & 0xff0000) >> 16);
            framebuffer[(index * 4) + 3] = 255;
        }
    }

    void DrawWindow()
    {
        bool tileset = LCDC.GetBit(Flags.Tileset);

        int tilesetAddress = tileset ? 0x0000 : 0x1000;
        int tilemapAddress = LCDC.GetBit(Flags.WindowTilemap) ? 0x1C00 : 0x1800;

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
            byte rawTile = memory[VramAddress + tilemapAddress + (row * 32) + colum];

            int offset = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

            int line = (byte)(y % 8) * 2;
            byte data1 = memory[VramAddress + offset + line];
            byte data2 = memory[VramAddress + offset + line + 1];

            byte bit = (byte)(1 << (((x % 8) - 7) * 0xff));
            byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
            byte colorIndex = (byte)((BGP >> (palletIndex * 2)) & 0b11);

            // framebuffer[i + (Emulator.WindowWidth * LY)] = Pallet.GetColor(colorIndex);

            uint color = Pallet.GetColor(colorIndex);
            int index = i + (Emulator.WindowWidth * LY);
            framebuffer[(index * 4) + 0] = (byte)color;
            framebuffer[(index * 4) + 1] = (byte)((color & 0xff00) >> 8);
            framebuffer[(index * 4) + 2] = (byte)((color & 0xff0000) >> 16);
            framebuffer[(index * 4) + 3] = 255;

        }
    }

    void DrawSprites()
    {
        int spriteSize = LCDC.GetBit(Flags.SpritesSize) ? 16 : 8;

        for (int i = 64; i >= 0; i--)
        {
            ushort offset = (ushort)(0xFE00 + (i * 4));

            int y = memory[(ushort)(offset + 0)] - 16;
            int x = memory[(ushort)(offset + 1)] - 8;
            byte tileID = memory[(ushort)(offset + 2)];
            byte flags = memory[(ushort)(offset + 3)];
            byte obp = flags.GetBit(Bit4) ? OBP1 : OBP0;

            bool mirrorX = flags.GetBit(Bit5);
            bool mirrorY = flags.GetBit(Bit6);
            bool aboveBG = flags.GetBit(Bit7);

            if (LY >= y && LY < y + spriteSize)
            {
                int row = mirrorY ? spriteSize - 1 - (LY - y) : LY - y;

                int vramAddress = VramAddress + (tileID * 16) + (row * 2);
                byte data1 = emulator.Read((ushort)(vramAddress + 0));
                byte data2 = emulator.Read((ushort)(vramAddress + 1));

                for (int r = 0; r < 8; r++)
                {
                    int pixelBit = mirrorX ? r : 7 - r;

                    int hi = (data2 >> pixelBit) & 1;
                    int lo = (data1 >> pixelBit) & 1;
                    byte palletIndex = (byte)((hi << 1) | lo);
                    byte colorIndex = (byte)((obp >> (palletIndex * 2)) & 0b11);

                    if (x + r is >= 0 and < Emulator.WindowWidth)
                    {
                        uint color = Pallet.GetColor((byte)(BGP & 0b11));
                        int index = x + r + (Emulator.WindowWidth * LY);
                        bool conditional =
                            framebuffer[(index * 4) + 0] == (byte)color &&
                            framebuffer[(index * 4) + 1] == (byte)((color & 0xff00) >> 8) &&
                            framebuffer[(index * 4) + 2] == (byte)((color & 0xff0000) >> 16);

                        if (palletIndex != 0 && (aboveBG == false || conditional))
                        {
                            color = Pallet.GetColor(colorIndex);
                            framebuffer[(index * 4) + 0] = (byte)color;
                            framebuffer[(index * 4) + 1] = (byte)((color & 0xff00) >> 8);
                            framebuffer[(index * 4) + 2] = (byte)((color & 0xff0000) >> 16);
                            framebuffer[(index * 4) + 3] = 255;
                        }
                    }
                }
            }
        }
    }
}
