using System;
using System.IO;

namespace Monoboy
{
    public class MemoryBankController2 : IMemoryBankController
    {
        public byte[] rom;

        public byte Read(ushort address)
        {
            throw new NotImplementedException();
        }

        public void Write(ushort address, byte data)
        {
            throw new NotImplementedException();
        }

        public void Load(string path)
        {
            rom = File.ReadAllBytes(path);
        }
    }
}
