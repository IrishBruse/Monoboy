﻿namespace Monoboy.MemoryBankControllers;

public class MemoryBankController2 : IMemoryBankController
{
    byte[] rom;

    byte[] ram = new byte[0x200];

    byte romBank = 1;
    bool ramEnabled;

    public byte ReadBank00(ushort address)
    {
        return rom[address];
    }

    public byte ReadBankNN(ushort address)
    {
        int offset = (0x4000 * romBank) + (address & 0x3FFF);
        return rom[offset];
    }

    public byte ReadRam(ushort address)
    {
        return ramEnabled ? ram[address & 0x1FFF] : (byte)0xff;
    }

    public void WriteBank(ushort address, byte data)
    {
        switch (address)
        {
            case ushort when address < 0x2000:
            {
                ramEnabled = (data & 0b1) == 0;
            }
            break;
            case ushort when address < 0x4000:
            {
                romBank = (byte)(data & 0xF);
            }
            break;
            default:
            break;
        }
    }

    public void WriteRam(ushort address, byte data)
    {
        if (ramEnabled)
        {
            ram[address & 0x1FFF] = data;
        }
    }

    public void Save() { }

    public void Load(byte[] data)
    {
        rom = data;

        // string save = romPath.Replace("Roms", "Saves").Replace(".gb", ".sav", true, null);

        // if (File.Exists(save))
        // {
        //     ram = File.ReadAllBytes(save);
        // }
    }

    public byte[] GetRam()
    {
        return ram;
    }

    public void SetRam(byte[] ram)
    {
        this.ram = ram;
    }
}
