using System;
using System.IO;

namespace Monoboy
{
    public class MemoryBankController1 : IMemoryBankController
    {
        public byte[] rom;
        public byte[] ram = new byte[32768];

        public byte romBank = 1;
        public byte ramBank = 0;
        public bool ramEnabled = false;

        private BankingMode bankingMode = BankingMode.Rom;

        public byte Read(ushort address)
        {
            if(address >= 0x0000 && address <= 0x3FFF)
            {
                return rom[address];
            }
            else if(address >= 0x4000 && address <= 0x7FFF)
            {
                int offset = (0x4000 * romBank) + (address - 0x4000);
                return rom[offset];
            }
            else if(address >= 0xA000 && address <= 0xBFFF)
            {
                if(ramEnabled == false)
                {
                    return 0xff;
                }
                int offset = (0x2000 * romBank) + (address - 0xA000);
                return ram[offset];
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void Write(ushort address, byte data)
        {
            if(address >= 0x0000 && address <= 0x1FFF)
            {
                byte value = (byte)(data & 0xF);
                ramEnabled = value == 0x0A;
            }
            else if(address >= 0x2000 && address <= 0x3FFF)
            {
                byte bank = (byte)(data & 0b00011111);
                romBank = (byte)((romBank & 0b11100000) | bank);
                if(romBank == 0x00 || romBank == 0x20 || romBank == 0x40 || romBank == 0x60)
                {
                    romBank += 1;
                }
            }
            else if(address >= 0x4000 && address <= 0x5FFF)
            {
                byte bank = (byte)(data & 0b11);

                switch(bankingMode)
                {
                    case BankingMode.Rom:
                    romBank = (byte)(romBank | (bank << 5));
                    if(romBank == 0x00 || romBank == 0x20 || romBank == 0x40 || romBank == 0x60)
                    {
                        romBank += 1;
                    }
                    break;

                    case BankingMode.Ram:
                    ramBank = bank;
                    break;
                }
            }
            else if(address >= 0x6000 && address <= 0x7FFF)
            {
                if((data & 1) == 0)
                {
                    bankingMode = BankingMode.Rom;
                }
                else
                {
                    bankingMode = BankingMode.Ram;
                }
            }
            else if(address >= 0xA000 && address <= 0xBFFF)
            {
                if(ramEnabled == false)
                {
                    return;
                }
                int offset = (0x2000 * romBank) + (address - 0xA000);
                ram[offset] = data;
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void Load(string path)
        {
            rom = File.ReadAllBytes(path);
        }

        enum BankingMode
        {
            Rom,
            Ram,
        }
    }
}