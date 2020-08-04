using System.Diagnostics;
using System.IO;
using Monoboy.Core.Utility;
using SFML.Graphics;

namespace Monoboy.Core
{
    public class Bus
    {
        public Interrupt interrupt;
        public Register register;
        public Memory memory;
        public Cartridge cartridge;
        public CPU cpu;
        public GPU gpu;
        public Joypad joypad;

        public bool biosEnabled = true;

        public Bus()
        {
            register = new Register();
            memory = new Memory();
            cartridge = new Cartridge();

            cpu = new CPU(this);
            gpu = new GPU(this);
            joypad = new Joypad(this);
            interrupt = new Interrupt(this);

            if(File.Exists("Roms/Boot.gb") == true)
            {
                memory.boot = File.ReadAllBytes("Roms/Boot.gb");
            }
            else
            {
                SkipBootRom();
            }
        }

        public void DumpBackground()
        {
            Color[] pallet = new Color[] { new Color(0, 0, 0, 255), new Color(76, 76, 76, 255), new Color(107, 107, 107, 255), new Color(255, 255, 255, 255) };
            Color[,] pixels = new Color[256, 256];

            ushort tilemapAddress = (ushort)(memory.LCDC.GetBit((Bit)LCDCBit.BackgroundTilemap) ? 0x1C00 : 0x1800);
            bool unsignTilemapAddress = memory.LCDC.GetBit((Bit)LCDCBit.BackgroundWindowData);

            for(int y = 0; y < 256; y++)
            {
                ushort row = (ushort)(y / 8);

                for(int x = 0; x < 256; x++)
                {
                    ushort colum = (ushort)(x / 8);

                    byte raw_tile_num = gpu.vRam[tilemapAddress + ((row * 32) + colum)];

                    ushort tileGraphicAddress = (ushort)(
                        (unsignTilemapAddress == true) ?
                        raw_tile_num * 16 :
                        0x0800 + (sbyte)raw_tile_num * 16
                        );

                    byte line = (byte)((byte)(y % 8) * 2);
                    byte data1 = gpu.vRam[tileGraphicAddress + line];
                    byte data2 = gpu.vRam[tileGraphicAddress + line + 1];

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
                    byte data1 = gpu.vRam[tileGraphicAddress + line];
                    byte data2 = gpu.vRam[tileGraphicAddress + line + 1];

                    Bit bit = (Bit)(0b00000001 << (((x % 8) - 7) * 0xff));
                    byte color_value = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));

                    pixels[x, y] = pallet[color_value];
                }
            }

            Image img = new Image(pixels);
            img.SaveToFile("C:\\Users\\Econn\\Desktop\\Tilemap.png");
        }

        public byte Read(ushort address)
        {
            byte data = 0xFF;

            if(address >= 0x0000 && address <= 0x7FFF)// 16KB ROM Bank 00 (in cartridge, fixed at bank 00)
            {
                if(address >= 0x0000 && address <= 0x00FF && biosEnabled == true)
                {
                    data = memory.boot[address];
                }
                else
                {
                    data = cartridge.Read(address);
                }
            }
            else if(address >= 0x8000 && address <= 0x9FFF)// 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            {
                data = gpu.vRam[address - 0x8000];
            }
            else if(address >= 0xA000 && address <= 0xBFFF)// 8KB External RAM (in cartridge, switchable bank, if any)
            {
                data = cartridge.Read(address);
            }
            else if(address >= 0xC000 && address <= 0xDFFF)// 4KB Work RAM Bank 0 (WRAM)
            {
                data = memory.workRAM[address - 0xC000];
            }
            else if(address >= 0xD000 && address <= 0xDFFF)// 4KB Work RAM Bank 1 (WRAM) (switchable bank 1-7 in CGB Mode)
            {
                data = memory.workRAM[address - 0xC000];
            }
            else if(address >= 0xE000 && address <= 0xFDFF)// Same as C000-DDFF (ECHO) (typically not used)
            {
                data = memory.workRAM[address - 0xE000];
            }
            else if(address >= 0xFE00 && address <= 0xFE9F)// Sprite Attribute Table (OAM)
            {
                data = memory.spriteRAM[address - 0xFE00];
            }
            else if(address >= 0xFEA0 && address <= 0xFEFF)// Not Usable
            {
                data = 0xFF;
            }
            else if(address >= 0xFF00 && address <= 0xFF7F)// I/O Ports
            {
                if(address == 0xFF00)
                {
                    data = joypad.Read();
                }
                else if(address == 0xFF0F)
                {
                    data = interrupt.IF;
                }
                else
                {
                    data = memory.io[address - 0xFF00];
                }
            }
            else if(address >= 0xFF80 && address <= 0xFFFF)// Zero Page RAM
            {
                if(address == 0xFFFF)
                {
                    data = interrupt.IE;
                }
                else
                {
                    data = memory.zp[address - 0xFF80];
                }
            }

            //Debug.Log("R " + address.ToHex() + "=" + data.ToHex() + " " + location);

            return data;
        }

        public void Write(ushort address, byte data)
        {
            if(address >= 0x0000 && address <= 0x7FFF)// 16KB ROM Bank 00 (in cartridge, fixed at bank 00)
            {
                cartridge.Write(address, data);
            }
            else if(address >= 0x8000 && address <= 0x9FFF)// 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            {
                gpu.vRam[address - 0x8000] = data;
            }
            else if(address >= 0xA000 && address <= 0xBFFF)// 8KB External RAM (in cartridge, switchable bank, if any)
            {
                cartridge.Write(address, data);
            }
            else if(address >= 0xC000 && address <= 0xCFFF)// 4KB Work RAM Bank 0 (WRAM)
            {
                memory.workRAM[address - 0xC000] = data;
            }
            else if(address >= 0xD000 && address <= 0xDFFF)// 4KB Work RAM Bank 1 (WRAM) (switchable bank 1-7 in CGB Mode)
            {
                memory.workRAM[address - 0xC000] = data;
            }
            else if(address >= 0xE000 && address <= 0xFDFF)// Same as C000-DDFF (ECHO) (typically not used)
            {
                memory.workRAM[address - 0xC000 - 0x0200] = data;
            }
            else if(address >= 0xFE00 && address <= 0xFE9F)// Sprite Attribute Table (OAM)
            {
                memory.spriteRAM[address - 0xFE00] = data;

            }
            // Not Usable
            else if(address >= 0xFF00 && address <= 0xFF7F)// I/O Ports
            {
                if(address == 0xFF0F)
                {
                    interrupt.IF = data;
                }
                else
                {
                    memory.io[address - 0xFF00] = data;
                }
            }
            else if(address >= 0xFF80 && address <= 0xFFFF)// Zero Page RAM
            {
                if(address == 0xFFFF)
                {
                    interrupt.IE = data;
                }
                else if(address == 0xFF01)
                {
                    if(Read(0xFF02) == 0x81)
                    {
                        Debug.Write(data);
                    }

                    memory.zp[address - 0xFF80] = data;
                }
                else
                {
                    memory.zp[address - 0xFF80] = data;
                }
            }

            //Debug.Log("W " + address.ToHex() + "=" + data.ToHex() + " " + location);
        }

        public void SkipBootRom()
        {
            biosEnabled = false;
            register.AF = 0x01B0;
            register.BC = 0x0013;
            register.DE = 0x00D8;
            register.HL = 0x014D;
            register.SP = 0xFFFE;

            Write(0xFF05, 0x00);
            Write(0xFF06, 0x00);
            Write(0xFF07, 0x00);
            Write(0xFF10, 0x80);
            Write(0xFF11, 0xBF);
            Write(0xFF12, 0xF3);
            Write(0xFF14, 0xBF);
            Write(0xFF16, 0x3F);
            Write(0xFF17, 0x00);
            Write(0xFF19, 0xBF);
            Write(0xFF1A, 0x7F);
            Write(0xFF1B, 0xFF);
            Write(0xFF1C, 0x9F);
            Write(0xFF1E, 0xBF);
            Write(0xFF20, 0xFF);
            Write(0xFF21, 0x00);
            Write(0xFF22, 0x00);
            Write(0xFF23, 0xBF);
            Write(0xFF24, 0x77);
            Write(0xFF25, 0xF3);
            Write(0xFF26, 0xF1);
            Write(0xFF40, 0x91);
            Write(0xFF42, 0x00);
            Write(0xFF43, 0x00);
            Write(0xFF45, 0x00);
            Write(0xFF47, 0xFC);
            Write(0xFF48, 0xFF);
            Write(0xFF49, 0xFF);
            Write(0xFF4A, 0x00);
            Write(0xFF4B, 0x00);
            Write(0xFFFF, 0x00);
        }
    }
}