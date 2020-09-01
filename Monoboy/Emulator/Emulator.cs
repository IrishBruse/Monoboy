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

        string romPath;
        bool skipBootRom = false;

        public Emulator()
        {
            bus = new Bus();

            //Open("Data/Roms/Mario.gb");
            //Open("Data/Roms/Dr. Mario.gb");
            //Open("Data/Roms/Tetris.gb");

            //Open("Data/Roms/cpu_instrs.gb");
            //Open("Data/Roms/01-special.gb");
            //Open("Data/Roms/02-interrupts.gb");
            //Open("Data/Roms/03-op sp,hl.gb");
            //Open("Data/Roms/04-op r,imm.gb");
            //Open("Data/Roms/05-op rp.gb");
            //Open("Data/Roms/06-ld r,r.gb");
            //Open("Data/Roms/07-jr,jp,call,ret,rst.gb");
            //Open("Data/Roms/08-misc instrs.gb");
            //Open("Data/Roms/09-op r,r.gb");
            //Open("Data/Roms/10-bit ops.gb");
            //Open("Data/Roms/11-op a,(hl).gb");
        }

        public byte Step()
        {
            byte cycles = bus.cpu.Step();

            bus.timer.Step(cycles);
            bus.gpu.Step(cycles);
            bus.interrupt.HandleInterupts();

            cyclesRan += cycles;

            // Disable the bios in the bus
            if(bus.biosEnabled == true && bus.register.PC >= 0x100)
            {
                bus.biosEnabled = false;
            }

            return cycles;
        }

        public void Open()
        {
            Open(romPath);
        }

        public void Open(string rom)
        {
            Reset();

            paused = false;

            byte cartridgeType;
            using(BinaryReader reader = new BinaryReader(new FileStream(rom, FileMode.Open)))
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

            bus.memoryBankController.Load(rom);
        }

        public void Reset()
        {
            bus.memory = new Memory();
            bus.register = new Register();

            if(File.Exists("Data/dmg_boot.bin") == true)
            {
                if(skipBootRom == false)
                {
                    bus.memory.boot = File.ReadAllBytes("Data/dmg_boot.bin");
                    bus.biosEnabled = true;
                }
                else
                {
                    SkipBootRom();
                }
            }
            else
            {
                SkipBootRom();
            }
        }

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

        #region Debug

        public void DumpBackground()
        {
            Color[] palette = new Color[] { new Color(0, 0, 0, 255), new Color(76, 76, 76, 255), new Color(107, 107, 107, 255), new Color(255, 255, 255, 255) };
            Image img = new Image(256, 256);

            bool tileset = bus.gpu.LCDC.GetBit(LCDCBit.Tileset);
            bool tilemap = bus.gpu.LCDC.GetBit(LCDCBit.Tilemap);

            ushort tilesetAddress = (ushort)(tileset ? 0x0000 : 0x1000);
            ushort tilemapAddress = (ushort)(tilemap ? 0x1C00 : 0x1800);

            for(uint y = 0; y < 256; y++)
            {
                ushort row = (ushort)(y / 8);

                for(uint x = 0; x < 256; x++)
                {
                    ushort colum = (ushort)(x / 8);

                    byte rawTile = bus.gpu.VideoRam[tilemapAddress + ((row * 32) + colum)];

                    int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

                    int line = (byte)(y % 8) * 2;
                    byte data1 = bus.gpu.VideoRam[vramAddress + line];
                    byte data2 = bus.gpu.VideoRam[vramAddress + line + 1];

                    byte bit = (byte)(0b00000001 << ((((int)x % 8) - 7) * 0xff));
                    byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                    byte colorIndex = (byte)((bus.gpu.BGP >> palletIndex * 2) & 0b11);
                    img.SetPixel(x, y, palette[colorIndex]);
                }
            }

            img.SaveToFile("Background.png");
        }

        public void DumpTilemap()
        {
            Color[] palette = new Color[] { new Color(0, 0, 0, 255), new Color(76, 76, 76, 255), new Color(107, 107, 107, 255), new Color(255, 255, 255, 255) };
            Image img = new Image(128, 192);

            for(uint y = 0; y < 192; y++)
            {
                ushort row = (ushort)(y / 8);

                for(uint x = 0; x < 128; x++)
                {
                    ushort colum = (ushort)(x / 8);

                    ushort rawTile = (ushort)((row * 16) + colum);

                    ushort tileGraphicAddress = (ushort)(rawTile * 16);

                    byte line = (byte)((byte)(y % 8) * 2);
                    byte data1 = bus.gpu.VideoRam[tileGraphicAddress + line];
                    byte data2 = bus.gpu.VideoRam[tileGraphicAddress + line + 1];

                    byte bit = (byte)(0b00000001 << ((((int)x % 8) - 7) * 0xff));
                    byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                    byte colorIndex = (byte)((bus.gpu.BGP >> palletIndex * 2) & 0b11);
                    img.SetPixel(x, y, palette[colorIndex]);
                }
            }

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

        #endregion
    }
}