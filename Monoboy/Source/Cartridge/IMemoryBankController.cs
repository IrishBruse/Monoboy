namespace Monoboy
{
    public interface IMemoryBankController
    {
        public byte ReadBank00(ushort address);
        public byte ReadBankNN(ushort address);
        public byte ReadRam(ushort address);
        public void WriteBank(ushort address, byte data);
        public void WriteRam(ushort address, byte data);
        public void Load(string path);
    }
}