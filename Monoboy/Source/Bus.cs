using System.Diagnostics;
using System.IO;

namespace Monoboy
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

            if(File.Exists("Data/Roms/Boot.gb") == true)
            {
                memory.boot = File.ReadAllBytes("Data/Roms/Boot.gb");
            }
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
                data = gpu.VideoRam[address - 0x8000];
            }
            else if(address >= 0xA000 && address <= 0xBFFF)// 8KB External RAM (in cartridge, switchable bank, if any)
            {
                data = cartridge.Read(address);
            }
            else if(address >= 0xC000 && address <= 0xDFFF)// 4KB Work RAM Bank 0 (WRAM)
            {
                data = memory.workram[address - 0xC000];
            }
            else if(address >= 0xD000 && address <= 0xDFFF)// 4KB Work RAM Bank 1 (WRAM) (switchable bank 1-7 in CGB Mode)
            {
                data = memory.workram[address - 0xC000];
            }
            else if(address >= 0xE000 && address <= 0xFDFF)// Same as C000-DDFF (ECHO) (typically not used)
            {
                data = memory.workram[address - 0xE000];
            }
            else if(address >= 0xFE00 && address <= 0xFE9F)// Sprite Attribute Table (OAM)
            {
                data = memory.spriteram[address - 0xFE00];
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
                gpu.VideoRam[address - 0x8000] = data;
            }
            else if(address >= 0xA000 && address <= 0xBFFF)// 8KB External RAM (in cartridge, switchable bank, if any)
            {
                cartridge.Write(address, data);
            }
            else if(address >= 0xC000 && address <= 0xCFFF)// 4KB Work RAM Bank 0 (WRAM)
            {
                memory.workram[address - 0xC000] = data;
            }
            else if(address >= 0xD000 && address <= 0xDFFF)// 4KB Work RAM Bank 1 (WRAM) (switchable bank 1-7 in CGB Mode)
            {
                memory.workram[address - 0xC000] = data;
            }
            else if(address >= 0xE000 && address <= 0xFDFF)// Same as C000-DDFF (ECHO) (typically not used)
            {
                memory.workram[address - 0xC000 - 0x0200] = data;
            }
            else if(address >= 0xFE00 && address <= 0xFE9F)// Sprite Attribute Table (OAM)
            {
                memory.spriteram[address - 0xFE00] = data;

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
                        Write(0xFF02, 0);
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
    }
}