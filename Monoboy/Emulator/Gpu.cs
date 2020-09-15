using Monoboy.Constants;
using Monoboy.Utility;
using OpenTK.Graphics.GL;
using static Monoboy.Constants.Constant;

namespace Monoboy
{
    public class Gpu
    {
        public byte[] VideoRam { get => bus.memory.vram; set => bus.memory.vram = value; }

        public byte LCDC { get => bus.memory.io[0x0040]; set => bus.memory.io[0x0040] = value; }
        public byte Stat { get => bus.memory.io[0x0041]; set => bus.memory.io[0x0041] = value; }
        public byte StatMode { get => bus.memory.io[0x0041].GetBits(0b00000011); set => bus.memory.io[0x0041] = bus.memory.io[0x0041].SetBits(0b00000011, value); }
        public byte SCY { get => bus.memory.io[0x0042]; set => bus.memory.io[0x0042] = value; }
        public byte SCX { get => bus.memory.io[0x0043]; set => bus.memory.io[0x0043] = value; }
        public byte WX { get => (byte)(bus.memory.io[0x004B] - 7); set => bus.memory.io[0x004B] = value; }
        public byte WY { get => bus.memory.io[0x004A]; set => bus.memory.io[0x004A] = value; }
        public byte LY { get => bus.memory.io[0x0044]; set => bus.memory.io[0x0044] = value; }
        public byte LYC { get => bus.memory.io[0x0045]; set => bus.memory.io[0x0045] = value; }
        public byte BGP { get => bus.memory.io[0x0047]; set => bus.memory.io[0x0047] = value; }
        public byte OBP0 { get => bus.memory.io[0x0048]; set => bus.memory.io[0x0048] = value; }
        public byte OBP1 { get => bus.memory.io[0x0049]; set => bus.memory.io[0x0049] = value; }

        public int cycles;

        private Bus bus;
        private bool[] backgroundPriority;

        public Framebuffer framebuffer;

        public System.Action DrawFrame;

        public Gpu(Bus bus)
        {
            this.bus = bus;
            framebuffer = new Framebuffer(WindowWidth, WindowHeight);
        }

        public void Step(int ticks)
        {
            if(LCDC.GetBit(LCDCBit.LCDEnabled) == false)
            {
                LY = 0;
                cycles = 0;
                StatMode = 0;
                return;
            }

            cycles += ticks;

            switch(StatMode)
            {
                case Mode.Hblank:
                if(cycles >= 204)
                {
                    cycles -= 204;
                    LY++;

                    if(LY == 144)
                    {
                        HandleModeChange(Mode.Vblank);
                        bus.interrupt.InterruptRequest(InterruptFlag.VBlank);
                        DrawFrame?.Invoke();
                    }
                    else
                    {
                        HandleModeChange(Mode.OAM);
                    }
                }
                break;

                case Mode.Vblank:
                if(cycles >= 456)
                {
                    cycles -= 456;
                    LY++;

                    if(LY == 154)
                    {
                        HandleModeChange(Mode.OAM);
                        LY = 0;
                    }
                }
                break;

                case Mode.OAM:
                if(cycles >= 80)
                {
                    cycles -= 80;
                    HandleModeChange(Mode.VRAM);
                }
                break;

                case Mode.VRAM:
                if(cycles >= 172)
                {
                    cycles -= 172;
                    DrawScanline();
                    HandleModeChange(Mode.Hblank);
                }
                break;
            }

            if(LY == LYC)
            {
                Stat.SetBit(Bit.Bit2, true);
                if(Stat.GetBit(Bit.Bit6) == true)
                {
                    bus.interrupt.InterruptRequest(InterruptFlag.LCDStat);
                }
            }
            else
            {
                Stat.SetBit(Bit.Bit2, false);
            }
        }

        void HandleModeChange(byte newMode)
        {
            StatMode = newMode;

            if(newMode == 2 && Stat.GetBit(Bit.Bit5) == true)
            {
                bus.interrupt.InterruptRequest(InterruptFlag.LCDStat);
            }
            else if(newMode == 0 && Stat.GetBit(Bit.Bit3))
            {
                bus.interrupt.InterruptRequest(InterruptFlag.LCDStat);
            }
            else if(newMode == 1 && Stat.GetBit(Bit.Bit4))
            {
                bus.interrupt.InterruptRequest(InterruptFlag.LCDStat);
            }
        }

        #region Drawing
        private void DrawScanline()
        {
            backgroundPriority = new bool[WindowWidth];

            if(LCDC.GetBit(LCDCBit.BackgroundEnabled) == true)
            {
                DrawBackground();
            }

            if(LCDC.GetBit(LCDCBit.WindowEnabled) == true)
            {
                DrawWindow();
            }

            if(LCDC.GetBit(LCDCBit.SpritesEnabled) == true)
            {
                DrawSprites();
            }
        }

        private void DrawBackground()
        {
            bool tileset = LCDC.GetBit(LCDCBit.Tileset);
            int tilesetAddress = tileset ? 0x0000 : 0x1000;
            int tilemapAddress = LCDC.GetBit(LCDCBit.Tilemap) ? 0x1C00 : 0x1800;

            byte y = (byte)(LY + SCY);
            int row = y / 8;

            for(byte i = 0; i < WindowWidth; i++)
            {
                byte x = (byte)(i + SCX);
                int colum = x / 8;
                byte rawTile = bus.gpu.VideoRam[tilemapAddress + (row * 32) + colum];

                int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

                int line = (byte)(y % 8) * 2;
                byte data1 = VideoRam[vramAddress + line];
                byte data2 = VideoRam[vramAddress + line + 1];

                byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
                byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                byte colorIndex = (byte)((BGP >> palletIndex * 2) & 0b11);
                backgroundPriority[i] = colorIndex != 0;

                if(Application.Application.BackgroundEnabled == true)
                {
                    framebuffer.SetPixel(i, LY, Pallet.GetColor(colorIndex));
                }
            }
        }

        private void DrawWindow()
        {
            bool tileset = LCDC.GetBit(LCDCBit.Tileset);

            int tilesetAddress = tileset ? 0x0000 : 0x1000;
            int tilemapAddress = LCDC.GetBit(LCDCBit.WindowTilemap) ? 0x1C00 : 0x1800;

            byte y = (byte)(LY - WY);
            int row = y / 8;

            for(byte i = WX; i < WindowWidth; i++)
            {
                byte x = (byte)(i + SCX);
                if(x >= WX)
                {
                    x = (byte)(i - WX);
                }
                int colum = x / 8;
                byte rawTile = bus.gpu.VideoRam[tilemapAddress + (row * 32) + colum];

                int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

                int line = (byte)(y % 8) * 2;
                byte data1 = VideoRam[vramAddress + line];
                byte data2 = VideoRam[vramAddress + line + 1];

                byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
                byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                byte colorIndex = (byte)((BGP >> palletIndex * 2) & 0b11);
                backgroundPriority[i] = colorIndex != 0;

                if(Application.Application.WindowEnabled == true)
                {
                    framebuffer.SetPixel(i, LY, Pallet.GetColor(colorIndex));
                }
            }
        }

        private void DrawSprites()
        {
            int spriteSize = LCDC.GetBit(LCDCBit.SpritesSize) ? 16 : 8;

            for(int i = 64; i >= 0; i--)
            {
                ushort offset = (ushort)(0xFE00 + (i * 4));

                int y = bus.Read((ushort)(offset + 0)) - 16;
                int x = bus.Read((ushort)(offset + 1)) - 8;
                byte tileID = bus.Read((ushort)(offset + 2));
                byte flags = bus.Read((ushort)(offset + 3));
                byte obp = flags.GetBit(Bit.Bit4) ? OBP1 : OBP0;

                bool mirrorX = flags.GetBit(Bit.Bit5);
                bool mirrorY = flags.GetBit(Bit.Bit6);
                bool aboveBG = flags.GetBit(Bit.Bit7);

                if(LY >= y && LY < y + spriteSize)
                {
                    int row = mirrorY ? spriteSize - 1 - (LY - y) : LY - y;

                    int vramAddress = 0x8000 + (tileID * 16) + (row * 2);
                    byte data1 = bus.Read((ushort)(vramAddress + 0));
                    byte data2 = bus.Read((ushort)(vramAddress + 1));

                    for(int r = 0; r < 8; r++)
                    {
                        int pixelBit = mirrorX ? r : 7 - r;

                        int hi = (data2 >> pixelBit) & 1;
                        int lo = (data1 >> pixelBit) & 1;
                        byte palletIndex = (byte)(hi << 1 | lo);
                        byte palletColor = (byte)((obp >> palletIndex * 2) & 0b11);

                        if(x + r >= 0 && x + r < WindowWidth)
                        {
                            if(palletIndex != 0 && (aboveBG == false || framebuffer.GetPixel(x - r, LY) == Pallet.GetColor((byte)(BGP & 0b11))))
                            {
                                framebuffer.SetPixel(x + r, LY, Pallet.GetColor(palletColor));
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}