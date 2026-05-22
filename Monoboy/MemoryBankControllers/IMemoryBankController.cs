namespace Monoboy.MemoryBankControllers;

public interface IMemoryBankController
{
    /// <summary>ROM bank selected for the switchable 16 KiB window at $4000-$7FFF.</summary>
    byte RomBank { get; }

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
