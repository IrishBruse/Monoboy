using System;
using System.IO;
using Utility;

namespace Monoboy.Emulator
{
    public class Memory
    {
        byte[] cartBank0;
        byte[] cartBankN;
        byte[] videoRAM;
        byte[] cartRAM;
        byte[] workRAM;
        byte[] spriteRAM;
        byte[] io;
        byte[] zp;

        public Memory()
        {
            videoRAM = new byte[8192];
            cartRAM = new byte[8192];
            workRAM = new byte[8192];
            spriteRAM = new byte[160];
            io = new byte[128];
            zp = new byte[128];
        }

        public void LoadRom(string path)
        {
            cartBank0 = new byte[16384];
            cartBankN = new byte[16384];

            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                for (int i = 0; i < 16384; i++)
                {
                    cartBank0[i] = reader.ReadByte();
                }

                for (int i = 0; i < 16384; i++)
                {
                    cartBankN[i] = reader.ReadByte();
                }
            }
        }

        public byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0x3FFF)// 16KB ROM Bank 00     (in cartridge, fixed at bank 00)
            {
                return cartBank0[address - 0x0000];
            }
            else
            if (address >= 0x4000 && address <= 0x7FFF)// 16KB ROM Bank 01..NN (in cartridge, switchable bank number)
            {
                return cartBankN[address - 0x4000];
            }
            else
            if (address >= 0x8000 && address <= 0x9FFF)// 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            {
                return videoRAM[address - 0x8000];
            }
            else
            if (address >= 0xA000 && address <= 0xBFFF)// 8KB External RAM     (in cartridge, switchable bank, if any)
            {
                return cartRAM[address - 0xA000];
            }
            else
            if (address >= 0xC000 && address <= 0xDFFF)// 4KB Work RAM Bank 0 (WRAM)
            {
                return workRAM[address - 0xC000];
            }
            else
            if (address >= 0xD000 && address <= 0xDFFF)// 4KB Work RAM Bank 1 (WRAM)  (switchable bank 1-7 in CGB Mode)
            {
                return workRAM[address - 0xC000];
            }
            else
            if (address >= 0xE000 && address <= 0xFDFF)// Same as C000-DDFF (ECHO)    (typically not used)
            {
                return workRAM[address - 0xC000 - 0x0200];
            }
            else
            if (address >= 0xFE00 && address <= 0xFE9F)// Sprite Attribute Table (OAM)
            {
                return spriteRAM[address - 0xFE00];
            }
            else
            if (address >= 0xFEA0 && address <= 0xFEFF)// Not Usable
            {
                return 0x00;
            }
            else
            if (address >= 0xFF00 && address <= 0xFF7F)// I/O Ports
            {
                return io[address - 0xFF00];
            }
            else
            if (address >= 0xFF80 && address <= 0xFFFF)// Zero Page RAM
            {
                return zp[address - 0xFF80];
            }
            else
            {
                throw new Exception("Should not be here");
            }
        }

        public void Write(ushort address, byte data)
        {
            // cant write to rom skipped
            if (address >= 0x8000 && address <= 0x9FFF)// 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            {
                videoRAM[address - 0x8000] = data;
            }
            else
            if (address >= 0xA000 && address <= 0xBFFF)// 8KB External RAM (in cartridge, switchable bank, if any)
            {
                cartRAM[address - 0xA000] = data;
            }
            else
            if (address >= 0xC000 && address <= 0xCFFF)// 4KB Work RAM Bank 0 (WRAM)
            {
                workRAM[address - 0xC000] = data;
            }
            else
            if (address >= 0xD000 && address <= 0xDFFF)// 4KB Work RAM Bank 1 (WRAM) (switchable bank 1-7 in CGB Mode)
            {
                workRAM[address - 0xC000] = data;
            }
            else
            if (address >= 0xE000 && address <= 0xFDFF)// Same as C000-DDFF (ECHO) (typically not used)
            {
                workRAM[address - 0xC000 - 0x0200] = data;
            }
            else
            if (address >= 0xFE00 && address <= 0xFE9F)// Sprite Attribute Table (OAM)
            {
                spriteRAM[address - 0xFE00] = data;
            }
            else
            if (address >= 0xFEA0 && address <= 0xFEFF)// Not Usable
            {
                throw new Exception("Should not be here");
            }
            else
            if (address >= 0xFF00 && address <= 0xFF7F)// I/O Ports
            {
                io[address - 0xFF00] = data;
            }
            else
            if (address >= 0xFF80 && address <= 0xFFFF)// Zero Page RAM
            {
                zp[address - 0xFF80] = data;
            }
            else
            {
                throw new Exception("Should not be here");
            }
        }

        public void Write(ushort address, ushort data)
        {
            Write(address, data.Low());
            Write((ushort)(address + 1), data.High());
        }
    }
}
