namespace Monoboy.MemoryBankControllers;

public interface IMemoryBankController
{
    byte ReadBank00(ushort address);
    byte ReadBankNN(ushort address);
    byte ReadRam(ushort address);
    void WriteBank(ushort address, byte data);
    void WriteRam(ushort address, byte data);
    byte[] GetRam();
    void SetRam(byte[] ram);
    void Save();
    void Load(byte[] data);
}
