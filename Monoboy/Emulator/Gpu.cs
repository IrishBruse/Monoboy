using Monoboy.Constants;
using static Monoboy.Constants.Constant;
using Monoboy.Utility;
using SFML.Graphics;

namespace Monoboy
{
    public class Gpu
    {
        public int cycles;
        private readonly Bus bus;

        public byte[] VideoRam { get => bus.memory.vram; set => bus.memory.vram = value; }

        public byte LCDC { get => bus.memory.io[0x0040]; set => bus.memory.io[0x0040] = value; }
        public byte Stat { get => bus.memory.io[0x0041]; set => bus.memory.io[0x0041] = value; }
        public byte StatMode { get => bus.memory.io[0x0041].GetBits(0b00000011); set => bus.memory.io[0x0041] = bus.memory.io[0x0041].SetBits(0b00000011, value); }
        public byte SCY { get => bus.memory.io[0x0042]; set => bus.memory.io[0x0042] = value; }
        public byte SCX { get => bus.memory.io[0x0043]; set => bus.memory.io[0x0043] = value; }
        public byte WX { get => (byte)(bus.memory.io[0x004B] - 7); set => bus.memory.io[0x0043] = value; }
        public byte WY { get => bus.memory.io[0x004A]; set => bus.memory.io[0x0043] = value; }
        public byte LY { get => bus.memory.io[0x0044]; set => bus.memory.io[0x0044] = value; }
        public byte LYC { get => bus.memory.io[0x0045]; set => bus.memory.io[0x0045] = value; }
        public byte BGP { get => bus.memory.io[0x0047]; set => bus.memory.io[0x0047] = value; }

        bool[] backgroundPriority;

        private readonly Color[] pallet = new Color[] { new Color(0xD0D058FF), new Color(0xA0A840FF), new Color(0x708028FF), new Color(0x405010FF) };
        //private readonly Color[] pallet = new Color[] { new Color(0x332C50FF), new Color(0x46878FFF), new Color(0x94E344FF), new Color(0xe2F3E4FF) };

        public Image Framebuffer { get; }
        public System.Action DrawFrame;

        public Gpu(Bus bus)
        {
            this.bus = bus;
            Framebuffer = new Image(WindowWidth, WindowHeight);
        }

        public void Step(int ticks)
        {
            if(LCDC.GetBit(LCDCBit.LCDEnabled) == false)
            {
                return;
            }

            cycles += ticks;

            switch(StatMode)
            {
                case Mode.Hblank:
                if(cycles >= 204)
                {
                    DrawScanline();
                    DrawFrame?.Invoke();//TODO Only for debugging

                    LY++;
                    cycles -= 204;


                    if(LY == 144)
                    {
                        StatMode = Mode.Vblank;
                        bus.interrupt.InterruptRequest(InterruptFlag.VBlank);
                    }
                    else
                    {
                        StatMode = Mode.OAM;
                    }
                }
                break;

                case Mode.Vblank:
                if(cycles >= 456)
                {
                    LY++;
                    cycles -= 456;

                    if(LY == 154)
                    {
                        DrawFrame?.Invoke();
                        LY = 0;
                        StatMode = Mode.OAM;
                    }
                }
                break;

                case Mode.OAM:
                if(cycles >= 80)
                {
                    cycles -= 80;
                    StatMode = Mode.VRAM;
                }
                break;

                case Mode.VRAM:
                if(cycles >= 172)
                {
                    cycles -= 172;

                    if(LCDC.GetBit(LCDCBit.Tilemap) == true)
                    {
                        bus.interrupt.InterruptRequest(InterruptFlag.LCDStat);
                    }

                    bool lycInterrupt = Stat.GetBit(StatBit.CoincidenceInterrupt);
                    bool lyc = LYC == LY;

                    if(lycInterrupt && lyc)
                    {
                        bus.interrupt.InterruptRequest(InterruptFlag.LCDStat);
                    }
                    Stat = Stat.SetBit(StatBit.CoincidenceFlag, lyc);

                    StatMode = Mode.Hblank;
                }
                break;
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
            int tilesetAddress = LCDC.GetBit(LCDCBit.Tileset) ? 0x0000 : 0x1000;
            int tilemapAddress = LCDC.GetBit(LCDCBit.Tilemap) ? 0x1C00 : 0x1800;

            byte y = (byte)(LY + SCY);
            int row = y / 8;

            for(byte i = 0; i < WindowWidth; i++)
            {
                byte x = (byte)(i + SCX);
                int colum = x / 8;
                byte rawTile = bus.gpu.VideoRam[tilemapAddress + (row * 32) + colum];

                int vramAddress = LCDC.GetBit(LCDCBit.Tileset) ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

                int line = (byte)(y % 8) * 2;
                byte data1 = VideoRam[vramAddress + line];
                byte data2 = VideoRam[vramAddress + line + 1];

                byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
                byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                byte colorIndex = (byte)((BGP >> palletIndex * 2) & 0b11);
                backgroundPriority[i] = colorIndex != 0;
                Framebuffer.SetPixel(i, LY, pallet[colorIndex]);
            }
        }

        private void DrawWindow()
        {
            if(LY < WY) return;

            int tilesetAddress = LCDC.GetBit(LCDCBit.Tileset) ? 0x0000 : 0x1000;
            int tilemapAddress = LCDC.GetBit(LCDCBit.WindowTilemap) ? 0x1C00 : 0x1800;

            byte y = (byte)(LY % 8);
            int row = y / 8;

            for(byte i = WX; i < WindowWidth; i++)
            {
                byte x = (byte)(i + WX);
                if(x >= WX)
                {
                    x = (byte)(i - WX);
                }
                int colum = i / 8;
                byte rawTile = bus.gpu.VideoRam[tilemapAddress + (row * 32) + colum];

                int vramAddress = LCDC.GetBit(LCDCBit.Tileset) ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

                int line = (byte)(y % 8) * 2;
                byte data1 = VideoRam[vramAddress + line];
                byte data2 = VideoRam[vramAddress + line + 1];

                byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
                byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                byte colorIndex = (byte)((BGP >> palletIndex * 2) & 0b11);
                backgroundPriority[i] = colorIndex != 0;
                Framebuffer.SetPixel(i, LY, pallet[colorIndex]);
            }
        }

        private void DrawSprites()
        {
            int spriteSize = LCDC.GetBit(LCDCBit.SpritesSize) ? 16 : 8;


        }
        #endregion
    }
}