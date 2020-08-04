namespace Monoboy
{
    public interface IMemoryBankController
    {
        public byte Read(ushort address);
        public void Write(ushort address, byte data);
        public void Load(string path);
    }
}