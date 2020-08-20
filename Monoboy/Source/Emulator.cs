using System;
using System.IO;
using Monoboy.Constants;
using Monoboy.Utility;
using SFML.Graphics;

namespace Monoboy
{
    public class Emulator
    {
        public Bus bus;
        public long cyclesRan;
        public bool paused = true;

        public Emulator()
        {
            bus = new Bus();

            if(File.Exists("Data/dmg_boot.bin") == true)
            {
                bus.memory.boot = File.ReadAllBytes("Data/dmg_boot.bin");
            }
            else
            {
                SkipBootRom();
            }

            SkipBootRom();

            //LoadRom("Mario.gb");
            //LoadRom("Dr. Mario.gb");
            //LoadRom("Tetris.gb");
            LoadRom("cpu_instrs.gb");
            //LoadRom("01-special.gb");
            //LoadRom("02-interrupts.gb");
            //LoadRom("03-op sp,hl.gb");
            //LoadRom("04-op r,imm.gb");
            //LoadRom("05-op rp.gb");
            //LoadRom("06-ld r,r.gb");
            //LoadRom("07-jr,jp,call,ret,rst.gb");
            //LoadRom("08-misc instrs.gb");
            //LoadRom("09-op r,r.gb");
            //LoadRom("10-bit ops.gb");
            //LoadRom("11-op a,(hl).gb");


            //while(bus.register.PC != 0xc018)
            //{
            //    Step();
            //}
        }

        public byte Step()
        {
            byte cycles = bus.cpu.Step();
            bus.gpu.Step(cycles);
            bus.joypad.Step();
            bus.interrupt.HandleInterupts();
            cyclesRan += cycles;

            //if(bus.Read((ushort)(bus.register.PC + 1)) == 0x01)
            //{
            //    paused = true;
            //}

            // Disable the bios in the bus
            if(bus.biosEnabled == true && bus.register.PC >= 0x100)
            {
                bus.biosEnabled = false;
            }

            return cycles;
        }

        public void LoadRom(string rom)
        {
            string path = "Data/Roms/" + rom;

            byte cartridgeType;
            using(BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                reader.BaseStream.Seek(0x147, SeekOrigin.Begin);
                cartridgeType = reader.ReadByte();
            }

            bus.memoryBankController = cartridgeType switch
            {
                byte _ when(cartridgeType <= 0) => new MemoryBankController0(),
                byte _ when(cartridgeType <= 3) => new MemoryBankController1(),
                _ => throw new NotImplementedException()
            };

            bus.memoryBankController.Load(path);
        }

        #region Debug

        public void SkipBootRom()
        {
            bus.biosEnabled = false;
            bus.register.AF = 0x01B0;
            bus.register.BC = 0x0013;
            bus.register.DE = 0x00D8;
            bus.register.HL = 0x014D;
            bus.register.SP = 0xFFFE;
            bus.register.PC = 0x100;
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

        public void DumpBackground()
        {
            Color[] pallet = new Color[] { new Color(0, 0, 0, 255), new Color(76, 76, 76, 255), new Color(107, 107, 107, 255), new Color(255, 255, 255, 255) };
            Color[,] pixels = new Color[256, 256];

            bool tileset = bus.memory.LCDC.GetBit(LCDCBit.Tileset);
            bool tilemap = bus.memory.LCDC.GetBit(LCDCBit.Tilemap);

            ushort tilesetAddress = (ushort)(tileset ? 0x0000 : 0x0800);
            ushort tilemapAddress = (ushort)(tilemap ? 0x1C00 : 0x1800);

            for(int y = 0; y < 256; y++)
            {
                ushort row = (ushort)(y / 8);

                for(int x = 0; x < 256; x++)
                {
                    ushort colum = (ushort)(x / 8);

                    byte rawTile = bus.gpu.VideoRam[tilemapAddress + ((row * 32) + colum)];

                    ushort vramAddress;

                    if(bus.memory.LCDC.GetBit(LCDCBit.Tileset) == true)
                    {
                        vramAddress = (ushort)((rawTile * 16) + tilesetAddress);
                    }
                    else
                    {
                        ushort signedAddress = (ushort)((sbyte)rawTile * 16);
                        vramAddress = (ushort)(((short)tilesetAddress) + signedAddress);
                    }

                    byte line = (byte)((byte)(y % 8) * 2);
                    byte data1 = bus.gpu.VideoRam[vramAddress + line];
                    byte data2 = bus.gpu.VideoRam[vramAddress + line + 1];

                    byte bit = ((byte)(0b00000001 << (((x % 8) - 7) * 0xff)));
                    byte color_value = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));

                    pixels[x, y] = pallet[color_value];
                }
            }

            Image img = new Image(pixels);
            img.SaveToFile("Background.png");
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

                    byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
                    byte color_value = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));

                    pixels[x, y] = pallet[color_value];
                }
            }

            Image img = new Image(pixels);
            img.SaveToFile("Tilemap.png");
        }

        public void DumpMemory()
        {
            byte[] file = new byte[0xFFFF];

            for(ushort i = 0; i < 0xFFFF; i++)
            {
                file[i] = bus.Read(i);
            }

            File.WriteAllBytes("Memory.bin", file);
        }

        public void DumpTrace()
        {
            File.WriteAllBytes("Trace.bin", bus.trace.ToArray());
        }

        #endregion
    }
}