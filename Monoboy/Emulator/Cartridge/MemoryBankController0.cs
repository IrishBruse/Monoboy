using System.IO;

namespace Monoboy
{
    public class MemoryBankController0 : IMemoryBankController
    {
        public byte[] rom;

        public byte ReadBank00(ushort address)
        {
            return rom[address];
        }

        public byte ReadBankNN(ushort address)
        {
            return rom[address];
        }

        public byte ReadRam(ushort address)
        {
            return 0xFF;
        }

        public void WriteRam(ushort address, byte data)
        {
            // Ignore
        }

        public void WriteBank(ushort address, byte data)
        {
            // Ignore
        }

        public void Load(string path)
        {
            rom = File.ReadAllBytes(path);
        }

        public byte[] GetRam()
        {
            return null;
        }

        public void SetRam(byte[] ram)
        {
        }
    }
}