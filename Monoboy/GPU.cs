using Monoboy.Core.Utility;
using Monoboy.Frontend;
using SFML.Graphics;

namespace Monoboy.Core
{
    public class GPU
    {
        public int clock;
        readonly Bus bus;

        public byte[] vRam;// 0x8000-0x9FFF

        //readonly Color[] pallet = new Color[] { new Color(0xD0D058), new Color(0xa0a840), new Color(0x708028), new Color(0x405010) };
        readonly Color[] pallet = new Color[] { new Color(0x332C50FF), new Color(0x46878FFF), new Color(0x94E344FF), new Color(0xe2F3E4FF) };
        public Image Framebuffer { get; }

        public GPU(Bus bus)
        {
            this.bus = bus;
            vRam = new byte[8196];
            Framebuffer = new Image(Emulator.WindowWidth, Emulator.WindowHeight);
        }

        public void Step(int cycles)
        {
            clock += cycles;

            switch(bus.memory.StatMode)
            {
                case Mode.OAM:
                if(clock >= 80)
                {
                    clock = 0;
                    bus.memory.StatMode = Mode.Drawing;
                }
                break;

                case Mode.Drawing:
                if(clock >= 172)
                {
                    clock = 0;
                    bus.memory.StatMode = Mode.Hblank;

                    DrawScanline();
                }
                break;

                case Mode.Hblank:
                if(clock >= 204)
                {
                    clock = 0;
                    bus.memory.Scanline++;

                    if(bus.memory.Scanline == 144)
                    {
                        bus.memory.StatMode = Mode.Vblank;

                    }
                    else
                    {
                        bus.memory.StatMode = Mode.OAM;
                    }
                }
                break;

                case Mode.Vblank:
                if(clock >= 456)
                {
                    clock = 0;
                    bus.memory.Scanline++;

                    if(bus.memory.Scanline > 153)
                    {
                        bus.memory.Scanline = 0;
                        bus.memory.StatMode = Mode.OAM;
                    }
                }
                break;
            }
        }

        void DrawScanline()
        {
            ushort tilemapAddress = (ushort)(bus.memory.LCDC.GetBit((Bit)LCDCBit.BackgroundTilemap) ? 0x1C00 : 0x1800);
            bool unsignTilemapAddress = bus.memory.LCDC.GetBit((Bit)LCDCBit.BackgroundWindowData);

            ushort y = (ushort)(bus.memory.Scanline + bus.memory.ScrollY);
            ushort row = (ushort)(y / 8);

            for(byte i = 0; i < Emulator.WindowWidth; i++)
            {
                ushort x = (ushort)(i + bus.memory.ScrollX);
                ushort colum = (ushort)(x / 8);

                ushort raw_tile_num = bus.gpu.vRam[tilemapAddress + (row * 32 + colum)];

                ushort tileGraphicAddress = (ushort)(
                    (unsignTilemapAddress == true) ?
                    raw_tile_num * 16 :
                    0x0800 + (short)raw_tile_num * 16
                    );

                byte line = (byte)((byte)(y % 8) * 2);
                byte data1 = bus.gpu.vRam[tileGraphicAddress + line];
                byte data2 = bus.gpu.vRam[tileGraphicAddress + line + 1];

                Bit bit = (Bit)(0b00000001 << (((x % 8) - 7) * 0xff));
                byte color_value = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));

                Framebuffer.SetPixel(i, bus.memory.Scanline, pallet[color_value]);
            }
        }
    }
}