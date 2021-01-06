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

        public byte ReadBank00(ushort address)
        {
            return rom[address];
        }

        public byte ReadBankNN(ushort address)
        {
            int offset = (0x4000 * romBank) + (address & 0x3FFF);
            return rom[offset];
        }

        public byte ReadRam(ushort address)
        {
            if(ramEnabled == true)
            {
                return ram[(0x2000 * ramBank) + (address & 0x1FFF)];
            }
            else
            {
                return 0xff;
            }
        }

        public void WriteRam(ushort address, byte data)
        {
            if(ramEnabled == true)
            {
                ram[(0x2000 * ramBank) + (address & 0x1FFF)] = data;
            }
        }

        public void WriteBank(ushort address, byte data)
        {
            switch(address)
            {
                case ushort _ when address < 0x2000:
                {
                    ramEnabled = data == 0x0A;
                }
                break;

                case ushort _ when address < 0x4000:
                {
                    byte bank = (byte)(data & 0b00011111);
                    romBank = (byte)((romBank & 0b11100000) | bank);
                    if(romBank == 0x00 || romBank == 0x20 || romBank == 0x40 || romBank == 0x60)
                    {
                        romBank += 1;
                    }
                }
                break;

                case ushort _ when address < 0x6000:
                {
                    byte bank = (byte)(data & 0b11);

                    if(bankingMode == BankingMode.Rom)
                    {
                        romBank = (byte)(romBank | (bank << 5));
                        if(romBank == 0x00 || romBank == 0x20 || romBank == 0x40 || romBank == 0x60)
                        {
                            romBank++;
                        }
                    }
                    else
                    {
                        ramBank = bank;
                    }
                }
                break;

                case ushort _ when address < 0x8000:
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
                break;
            }
        }

        public void Load(string path)
        {
            rom = File.ReadAllBytes(path);

            string save = path.Replace(".gb", ".sav", true, null);

            if(File.Exists(save) == true)
            {
                ram = File.ReadAllBytes(save);
            }
        }

        public byte[] GetRam()
        {
            return ram;
        }

        public void SetRam(byte[] ram)
        {
            this.ram = ram;
        }

        private enum BankingMode
        {
            Rom,
            Ram,
        }
    }
}