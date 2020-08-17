using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Monoboy
{
    public class Bus
    {
        public Interrupt interrupt;
        public Register register;
        public Memory memory;
        public IMemoryBankController memoryBankController;
        public Cpu cpu;
        public Gpu gpu;
        public Joypad joypad;

        public bool biosEnabled = true;
        public List<byte> trace = new List<byte>(100000);

        public Bus()
        {
            register = new Register();
            memory = new Memory();

            cpu = new Cpu(this);
            gpu = new Gpu(this);
            joypad = new Joypad(this);
            interrupt = new Interrupt(this);
        }

        public byte Read(ushort address)
        {
            return address switch
            {
                ushort _ when(address <= 0x00FF) && biosEnabled == true => memory.boot[address], // Boot rom only enabled while pc has not reached 0x100 yet
                ushort _ when(address <= 0x3FFF) => memoryBankController.ReadBank00(address),    // 16KB ROM Bank = 00 (in cartridge, fixed at bank 00)
                ushort _ when(address <= 0x7FFF) => memoryBankController.ReadBankNN(address),    // 16KB ROM Bank > 00 (in cartridge, every other bank)
                ushort _ when(address <= 0x9FFF) => memory.vram[address - 0x8000],               // 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
                ushort _ when(address <= 0xBFFF) => memoryBankController.ReadRam(address),       // 8KB External RAM (in cartridge, switchable bank, if any)
                ushort _ when(address <= 0xDFFF) => memory.workram[address - 0xC000],            // 4KB Work RAM Bank 0 (WRAM) (switchable bank 1-7 in CGB Mode)
                ushort _ when(address <= 0xFDFF) => memory.workram[address - 0xE000],            // Same as C000-DDFF (ECHO) (typically not used)
                ushort _ when(address <= 0xFE9F) => memory.oam[address - 0xFE00],                // Sprite Attribute Table (OAM)
                ushort _ when(address <= 0xFEFF) => 0x00,                                        // Not Usable
                ushort _ when(address <= 0xFF7F) => memory.io[address - 0xFF00],                 // I/O Ports
                ushort _ when(address <= 0xFFFF) => memory.zp[address - 0xFF80],                 // Zero Page RAM
                _ => 0xFF,
            };
        }

        public ushort ReadShort(ushort address)
        {
            return (ushort)(Read((ushort)(address + 1)) << 8 | Read(address));
        }

        public void Write(ushort address, byte data)
        {
            switch(address)
            {
                case ushort _ when(address <= 0x7FFF): memoryBankController.WriteBank(address, data); break;     // 16KB ROM Bank > 00 (in cartridge, every other bank)
                case ushort _ when(address <= 0x9FFF): memory.vram[address - 0x8000] = data; break;              // 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
                case ushort _ when(address <= 0xBFFF): memoryBankController.WriteRam(address, data); break;      // 8KB External RAM (in cartridge, switchable bank, if any)
                case ushort _ when(address <= 0xDFFF): memory.workram[address - 0xC000] = data; break;           // 4KB Work RAM Bank 0 (WRAM) (switchable bank 1-7 in CGB Mode)
                case ushort _ when(address <= 0xFDFF): memory.workram[address - 0xE000] = data; break;           // Same as C000-DDFF (ECHO) (typically not used)
                case ushort _ when(address <= 0xFE9F): memory.oam[address - 0xFE00] = data; break;               // Sprite Attribute Table (OAM)
                case ushort _ when(address <= 0xFEFF): break;                                                    // Not Usable
                case ushort _ when(address <= 0xFF7F): WriteIO(address, data); break;                            // I/O Ports
                case ushort _ when(address <= 0xFFFF): WriteZP(address, data); break;                            // Zero Page RAM
            }
        }

        public void WriteWord(ushort address, ushort data)
        {
            Write((ushort)(address + 1), (byte)(data >> 8));
            Write(address, (byte)data);
        }

        private void WriteIO(ushort address, byte data)
        {
            switch(address)
            {
                case 0xFF04:// DIV
                {
                    data = 0;
                }
                break;

                case 0xFF0F:// Mask IF first 3 bits to 1
                {
                    data |= 0xE0;
                }
                break;

                case 0xFF44:// LY
                {
                    data = 0;
                }
                break;

                case 0xFF46:
                {
                    ushort offset = (ushort)(data << 8);
                    for(byte i = 0; i < memory.oam.Length; i++)
                    {
                        memory.oam[i] = Read((ushort)(offset + i));
                    }
                }
                break;

                case 0xFF02:
                {
                    if(data == 0x81)
                    {
                        Debug.Write(Convert.ToChar(Read(0xFF01)));
                    }
                }
                break;
            }

            memory.io[address - 0xFF00] = data;
        }

        private void WriteZP(ushort address, byte data)
        {
            switch(address)
            {
                case 0xFFFF:// IE
                {
                    interrupt.IE = data;
                }
                break;
            }

            memory.zp[address - 0xFF80] = data;
        }
    }
}