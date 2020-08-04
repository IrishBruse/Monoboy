using System.IO;

namespace Monoboy
{
    public class MemoryBankController0 : IMemoryBankController
    {
        public byte[] rom;

        public byte Read(ushort address)
        {
            return rom[address];
        }

        public void Write(ushort address, byte data)
        {
            // Do nothing
        }

        public void Load(string path)
        {
            rom = File.ReadAllBytes(path);
        }
    }
}