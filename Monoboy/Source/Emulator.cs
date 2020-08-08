using System.IO;
using Monoboy.Utility;
using SFML.Graphics;

namespace Monoboy
{
    public class Emulator
    {
        public const byte WindowScale = 4;
        public const byte WindowWidth = 160;
        public const byte WindowHeight = 144;

        public Bus bus;

        public long cyclesRan;

        public Emulator()
        {
            bus = new Bus();
        }

        public byte Step()
        {
            byte cycles = bus.cpu.Step();
            bus.gpu.Step(cycles);
            cyclesRan += cycles;
            return cycles;
        }

        public void LoadRom(string path)
        {
            bus.cartridge.LoadRom("Data/Roms/" + path);
        }

        #region Debug


        public void DumpBackground()
        {
            Color[] pallet = new Color[] { new Color(0, 0, 0, 255), new Color(76, 76, 76, 255), new Color(107, 107, 107, 255), new Color(255, 255, 255, 255) };
            Color[,] pixels = new Color[256, 256];

            ushort tilemapAddress = (ushort)(bus.memory.LCDC.GetBit((Bit)LCDCBit.BackgroundTilemap) ? 0x1C00 : 0x1800);
            bool unsignTilemapAddress = bus.memory.LCDC.GetBit((Bit)LCDCBit.BackgroundWindowData);

            for(int y = 0; y < 256; y++)
            {
                ushort row = (ushort)(y / 8);

                for(int x = 0; x < 256; x++)
                {
                    ushort colum = (ushort)(x / 8);

                    byte raw_tile_num = bus.gpu.VideoRam[tilemapAddress + ((row * 32) + colum)];

                    ushort tileGraphicAddress = (ushort)(
                        (unsignTilemapAddress == true) ?
                        raw_tile_num * 16 :
                        0x0800 + (sbyte)raw_tile_num * 16
                        );

                    byte line = (byte)((byte)(y % 8) * 2);
                    byte data1 = bus.gpu.VideoRam[tileGraphicAddress + line];
                    byte data2 = bus.gpu.VideoRam[tileGraphicAddress + line + 1];

                    Bit bit = (Bit)(0b00000001 << (((x % 8) - 7) * 0xff));
                    byte color_value = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));

                    pixels[x, y] = pallet[color_value];
                }
            }

            Image img = new Image(pixels);
            img.SaveToFile("C:\\Users\\Econn\\Desktop\\Background.png");
        }

        public void DumpTilemap()
        {
            Color[] pallet = new Color[] { new Color(0, 0, 0, 255), new Color(76, 76, 76, 255), new Color(107, 107, 107, 255), new Color(255, 255, 255, 255) };

            Color[,] pixels = new Color[128, 192];

            for(int y = 0; y < 192; y++)
            {
                ushort row = (ushort)(y / 8);

                for(int x = 0; x < 128; x++)
                {
                    ushort colum = (ushort)(x / 8);

                    ushort raw_tile_num = (ushort)((row * 16) + colum);

                    ushort tileGraphicAddress = (ushort)(raw_tile_num * 16);

                    byte line = (byte)((byte)(y % 8) * 2);
                    byte data1 = bus.gpu.VideoRam[tileGraphicAddress + line];
                    byte data2 = bus.gpu.VideoRam[tileGraphicAddress + line + 1];

                    Bit bit = (Bit)(0b00000001 << (((x % 8) - 7) * 0xff));
                    byte color_value = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));

                    pixels[x, y] = pallet[color_value];
                }
            }

            Image img = new Image(pixels);
            img.SaveToFile("C:\\Users\\Econn\\Desktop\\Tilemap.png");
        }

        public void SkipBootRom()
        {
            bus.biosEnabled = false;
            bus.register.AF = 0x01B0;
            bus.register.BC = 0x0013;
            bus.register.DE = 0x00D8;
            bus.register.HL = 0x014D;
            bus.register.SP = 0xFFFE;
            bus.Write(0xFF05, 0x00);
            bus.Write(0xFF06, 0x00);
            bus.Write(0xFF07, 0x00);
            bus.Write(0xFF10, 0x80);
            bus.Write(0xFF11, 0xBF);
            bus.Write(0xFF12, 0xF3);
            bus.Write(0xFF14, 0xBF);
            bus.Write(0xFF16, 0x3F);
            bus.Write(0xFF17, 0x00);
            bus.Write(0xFF19, 0xBF);
            bus.Write(0xFF1A, 0x7F);
            bus.Write(0xFF1B, 0xFF);
            bus.Write(0xFF1C, 0x9F);
            bus.Write(0xFF1E, 0xBF);
            bus.Write(0xFF20, 0xFF);
            bus.Write(0xFF21, 0x00);
            bus.Write(0xFF22, 0x00);
            bus.Write(0xFF23, 0xBF);
            bus.Write(0xFF24, 0x77);
            bus.Write(0xFF25, 0xF3);
            bus.Write(0xFF26, 0xF1);
            bus.Write(0xFF40, 0x91);
            bus.Write(0xFF42, 0x00);
            bus.Write(0xFF43, 0x00);
            bus.Write(0xFF45, 0x00);
            bus.Write(0xFF47, 0xFC);
            bus.Write(0xFF48, 0xFF);
            bus.Write(0xFF49, 0xFF);
            bus.Write(0xFF4A, 0x00);
            bus.Write(0xFF4B, 0x00);
            bus.Write(0xFFFF, 0x00);
        }

        public void DumpMemory()
        {
            string file = "";

            for(int i = 0; i < 0xFFFF; i++)
            {
                if(i % 0xF == 0)
                {
                    file += "\n";
                }

                file += bus.Read((ushort)i);
            }

            File.WriteAllText("C:\\Users\\Econn\\Desktop\\Memory.txt", file);
        }

        #endregion
    }
}
