using System.IO;

namespace Monoboy
{
    public class MemoryBankController2 : IMemoryBankController
    {
        public byte[] rom;
        public byte[] ram = new byte[512];

        public byte romBank = 1;
        public bool ramEnabled = false;

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
                return ram[address & 0x1FFF];
            }
            else
            {
                return 0xff;
            }
        }

        public void WriteBank(ushort address, byte data)
        {
            switch(address)
            {
                case ushort _ when address < 0x2000:
                {
                    ramEnabled = (data & 0b1) == 0;
                }
                break;
                case ushort _ when address < 0x4000:
                {
                    romBank = (byte)(data & 0xF);
                }
                break;
            }
        }

        public void WriteRam(ushort address, byte data)
        {
            if(ramEnabled == true)
            {
                ram[address & 0x1FFF] = data;
            }
        }

        public void Load(string path)
        {
            rom = File.ReadAllBytes(path);

            string save = path.Replace("Roms", "Saves").Replace(".gb", ".sav", true, null);

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
    }
}