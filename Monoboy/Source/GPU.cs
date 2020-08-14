using Monoboy.Utility;
using SFML.Graphics;

namespace Monoboy
{
    public class GPU
    {
        public int cycles;
        readonly Bus bus;

        public byte[] VideoRam { get => bus.memory.vram; set => bus.memory.vram = value; }

        readonly Color[] pallet = new Color[] { new Color(0x332C50FF), new Color(0x46878FFF), new Color(0x94E344FF), new Color(0xe2F3E4FF) };
        public Image Framebuffer { get; }
        public System.Action DrawFrame;

        public GPU(Bus bus)
        {
            this.bus = bus;
            Framebuffer = new Image(Emulator.WindowWidth, Emulator.WindowHeight);
        }

        public void Step(int ticks)
        {
            cycles += ticks;

            switch(bus.memory.StatMode)
            {
                case Mode.Hblank:
                if(cycles >= 204)
                {
                    DrawScanline();
                    DrawFrame.Invoke();//TODO Only for debugging

                    bus.memory.LY++;
                    cycles -= 204;


                    if(bus.memory.LY == 144)
                    {
                        bus.memory.StatMode = Mode.Vblank;
                        bus.interrupt.InterruptRequest(Interrupt.InterruptFlag.VBlank);
                    }
                    else
                    {
                        bus.memory.StatMode = Mode.OAM;
                    }
                }
                break;

                case Mode.Vblank:
                if(cycles >= 456)
                {
                    bus.memory.LY++;
                    cycles -= 456;

                    if(bus.memory.LY == 154)
                    {
                        DrawFrame.Invoke();
                        bus.memory.LY = 0;
                        bus.memory.StatMode = Mode.OAM;
                    }
                }
                break;

                case Mode.OAM:
                if(cycles >= 80)
                {
                    cycles -= 80;
                    bus.memory.StatMode = Mode.VRAM;
                }
                break;

                case Mode.VRAM:
                if(cycles >= 172)
                {
                    cycles -= 172;

                    if(bus.memory.LCDC.GetBit((Bit)LCDCBit.Tilemap) == true)
                    {
                        bus.interrupt.InterruptRequest(Interrupt.InterruptFlag.LCDStat);
                    }

                    bool lycInterrupt = bus.memory.Stat.GetBit((Bit)StatBit.CoincidenceInterrupt);
                    bool lyc = bus.memory.LYC == bus.memory.LY;

                    if(lycInterrupt && lyc)
                    {
                        bus.interrupt.InterruptRequest(Interrupt.InterruptFlag.LCDStat);
                    }
                    bus.memory.Stat.SetBit((Bit)StatBit.CoincidenceFlag, lyc);

                    bus.memory.StatMode = Mode.Hblank;
                }
                break;
            }
        }

        #region Drawing
        void DrawScanline()
        {
            if(bus.memory.LCDC.GetBit((Bit)LCDCBit.LCDEnabled) == false)
            {
                return;
            }

            if(bus.memory.LCDC.GetBit(Bit.Bit0) == true)
            {
                DrawBackground();
            }

            if(bus.memory.LCDC.GetBit(Bit.Bit5) == true)
            {
                DrawWindow();
            }

            if(bus.memory.LCDC.GetBit(Bit.Bit1) == true)
            {
                DrawSprites();
            }
        }

        void DrawBackground()
        {
            bool tileset = bus.memory.LCDC.GetBit((Bit)LCDCBit.Tileset);
            bool tilemap = bus.memory.LCDC.GetBit((Bit)LCDCBit.Tilemap);

            ushort tilesetAddress = (ushort)(tileset ? 0x0000 : 0x0800);
            ushort tilemapAddress = (ushort)(tilemap ? 0x1C00 : 0x1800);

            ushort y = (ushort)(bus.memory.LY + bus.memory.SCY);
            ushort row = (ushort)(y / 8);

            for(byte i = 0; i < Emulator.WindowWidth; i++)
            {
                byte x = (byte)(i + bus.memory.SCX);
                ushort colum = (ushort)(x / 8);
                byte rawTile = bus.gpu.VideoRam[tilemapAddress + ((row * 32) + colum)];

                ushort vramAddress;

                bool useUnsignedAdressing = bus.memory.LCDC.GetBit((Bit)LCDCBit.Tileset);

                if(useUnsignedAdressing == true)
                {
                    vramAddress = (ushort)((rawTile * 16) + tilesetAddress);
                }
                else
                {
                    ushort signedAddress = (ushort)((sbyte)rawTile * 16);
                    vramAddress = (ushort)(((short)tilesetAddress) + signedAddress);
                }

                byte line = (byte)((byte)(y % 8) * 2);
                byte data1 = VideoRam[vramAddress + line];
                byte data2 = VideoRam[vramAddress + line + 1];

                Bit bit = (Bit)(0b00000001 << (((x % 8) - 7) * 0xff));
                byte color_value = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));

                Framebuffer.SetPixel(i, bus.memory.LY, pallet[color_value]);
            }
        }

        void DrawWindow()
        {

        }

        void DrawSprites()
        {

        }
        #endregion
    }
}