namespace Monoboy.Cartridge;

using System;

public class MemoryBankControllerDummy : IMemoryBankController
{
    public byte[] Rom { get; set; }

    public byte ReadBank00(ushort address)
    {
        return 0;
    }

    public byte ReadBankNN(ushort address)
    {
        return 0;
    }

    public byte ReadRam(ushort address)
    {
        return 0;
    }

    public void WriteRam(ushort address, byte data)
    {
    }

    public void WriteBank(ushort address, byte data)
    {
    }

    public byte[] GetRam()
    {
        return Array.Empty<byte>();
    }

    public void SetRam(byte[] ram)
    {
    }
}
