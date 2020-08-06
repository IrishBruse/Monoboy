using Monoboy.Core.Utility;
using SFML.Graphics;

namespace Monoboy.Core
{
    public class GPU
    {
        public int clock;
        readonly Bus bus;

        public byte[] VideoRam { get => bus.memory.vram; set => bus.memory.vram = value; }

        readonly Color[] pallet = new Color[] { new Color(0x332C50FF), new Color(0x46878FFF), new Color(0x94E344FF), new Color(0xe2F3E4FF) };
        public Image Framebuffer { get; }
        public System.Action DrawFrame;

        byte LCDCStatusInterrupt;
        byte VBlankLine;

        public GPU(Bus bus)
        {
            this.bus = bus;
            Framebuffer = new Image(Emulator.WindowWidth, Emulator.WindowHeight);
        }

        public void Step(int cycles)
        {
            if(bus.memory.LCDC.GetBit((Bit)LCDCBit.DisplayToggle) == false)
            {
                return;
            }

            bool requestInterrupt = false;

            clock += cycles;

            switch(bus.memory.StatMode)
            {
                case Mode.Hblank:
                if(clock >= 204)
                {
                    bus.memory.LY++;
                    clock -= 204;
                    bus.memory.StatMode = Mode.OAM;

                    CompareLYToLYC();

                    if(bus.memory.LY == 144)
                    {
                        bus.memory.StatMode = Mode.Vblank;
                        VBlankLine = 0;
                        bus.interrupt.InterruptRequest(Interrupt.InterruptFlag.VBlank);
                        DrawFrame.Invoke();
                    }
                }
                break;

                case Mode.Vblank:
                if(clock >= 456)
                {
                    bus.memory.LY++;
                    clock -= 456;

                    if(bus.memory.LY > 153)
                    {
                        bus.memory.LY = 0;
                        bus.memory.StatMode = Mode.OAM;
                    }
                }
                break;

                case Mode.OAM:
                if(clock >= 80)
                {
                    clock -= 80;
                    bus.memory.StatMode = Mode.Transfering;
                }
                break;

                case Mode.Transfering:
                if(clock >= 172)
                {
                    clock -= 172;
                    bus.memory.StatMode = Mode.Hblank;
                    DrawScanline();
                }
                break;
            }

            if(requestInterrupt == true)
            {
                bus.interrupt.InterruptRequest(Interrupt.InterruptFlag.LCDStat);
            }
        }

        void DrawScanline()
        {
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

        #region Drawing
        void DrawBackground()
        {
            ushort tilemapAddress = (ushort)(bus.memory.LCDC.GetBit((Bit)LCDCBit.BackgroundTilemap) ? 0x1C00 : 0x1800);
            bool unsignTilemapAddress = bus.memory.LCDC.GetBit((Bit)LCDCBit.BackgroundWindowData);

            ushort y = (ushort)(bus.memory.LY + bus.memory.SCY);
            ushort row = (ushort)(y / 8);

            for(byte i = 0; i < Emulator.WindowWidth; i++)
            {
                byte x = (byte)(i + bus.memory.SCX);
                ushort colum = (ushort)(x / 8);
                byte rawTile = bus.gpu.VideoRam[tilemapAddress + ((row * 32) + colum)];

                ushort tileGraphicAddress;

                if(unsignTilemapAddress == true)
                {
                    tileGraphicAddress = rawTile;
                }
                else
                {
                    tileGraphicAddress = (ushort)(sbyte)(rawTile + 256);
                }

                tileGraphicAddress *= 16;

                byte line = (byte)((y % 8) * 2);
                byte data1 = bus.gpu.VideoRam[tileGraphicAddress + line];
                byte data2 = bus.gpu.VideoRam[tileGraphicAddress + line + 1];

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

        void CompareLYToLYC()
        {
            if(bus.memory.LCDC.GetBit((Bit)LCDCBit.DisplayToggle) == false)
            {
                return;
            }

            if(bus.memory.LYC == clock)
            {
                bus.memory.Stat.SetBit(Bit.Bit2, true);

                if(bus.memory.Stat.GetBit(Bit.Bit6) == true)
                {
                    if(LCDCStatusInterrupt == 0)
                    {
                        bus.interrupt.InterruptRequest(Interrupt.InterruptFlag.LCDStat);
                    }
                    LCDCStatusInterrupt.SetBit(Bit.Bit3, true);
                }
            }
            else
            {
                bus.memory.Stat.SetBit(Bit.Bit2, false);
                LCDCStatusInterrupt.SetBit(Bit.Bit3, false);
            }
        }
    }
}