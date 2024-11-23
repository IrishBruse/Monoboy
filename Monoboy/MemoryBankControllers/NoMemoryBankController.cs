namespace Monoboy.MemoryBankControllers;

using System;

// https://gbdev.io/pandocs/nombc.html#no-mbc
public class NoMemoryBankController : IMemoryBankController
{
    byte[] rom;

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

    public void Save() { }

    public void Load(byte[] data)
    {
        rom = data;
    }

    public byte[] GetRam()
    {
        return Array.Empty<byte>();
    }

    public void SetRam(byte[] ram)
    {

    }
}
