namespace Monoboy.Cartridge;

using System.IO;

public class MemoryBankController0 : IMemoryBankController
{
    public byte[] Rom { get; set; }

    public byte ReadBank00(ushort address)
    {
        return Rom[address];
    }

    public byte ReadBankNN(ushort address)
    {
        return Rom[address];
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
        Rom = File.ReadAllBytes(path);
    }

    public byte[] GetRam()
    {
        return null;
    }

    public void SetRam(byte[] ram)
    {
    }
}
