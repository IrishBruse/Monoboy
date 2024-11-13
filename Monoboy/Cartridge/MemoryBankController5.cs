﻿namespace Monoboy.Cartridge;

using Monoboy.Constants;

public class MemoryBankController5 : IMemoryBankController
{
    public byte[] Rom { get; set; }

    byte[] ram = new byte[0x8000];
    byte romBank = 1;
    byte ramBank;
    bool ramEnabled;
    BankingMode bankingMode = BankingMode.Rom;

    public byte ReadBank00(ushort address)
    {
        return Rom[address];
    }

    public byte ReadBankNN(ushort address)
    {
        int offset = (0x4000 * romBank) + (address & 0x3FFF);
        return Rom[offset];
    }

    public byte ReadRam(ushort address)
    {
        return ramEnabled ? ram[(0x2000 * ramBank) + (address & 0x1FFF)] : (byte)0xff;
    }

    public void WriteRam(ushort address, byte data)
    {
        if (ramEnabled)
        {
            ram[(0x2000 * ramBank) + (address & 0x1FFF)] = data;
        }
    }

    public void WriteBank(ushort address, byte data)
    {
        switch (address)
        {
            case ushort when address < 0x2000:
            {
                ramEnabled = data == 0x0A;
            }
            break;

            case ushort when address < 0x4000:
            {
                byte bank = (byte)(data & 0b00011111);
                romBank = (byte)((romBank & 0b11100000) | bank);
                if (romBank is 0x00 or 0x20 or 0x40 or 0x60)
                {
                    romBank += 1;
                }
            }
            break;

            case ushort when address < 0x6000:
            {
                byte bank = (byte)(data & Bit.Bit01);

                if (bankingMode == BankingMode.Rom)
                {
                    romBank = (byte)(romBank | (bank << 5));
                    if (romBank is 0x00 or 0x20 or 0x40 or 0x60)
                    {
                        romBank++;
                    }
                }
                else
                {
                    ramBank = bank;
                }
            }
            break;

            case ushort when address < 0x8000:
            {
                if ((data & 1) == 0)
                {
                    bankingMode = BankingMode.Rom;
                }
                else
                {
                    bankingMode = BankingMode.Ram;
                }
            }
            break;
            default:
            break;
        }
    }

    public byte[] GetRam()
    {
        return ram;
    }

    public void SetRam(byte[] ram)
    {
        this.ram = ram;
    }

    public void Save(byte[] data)
    {
        // string save = romPath.Replace(".gb", ".sav", true, null);
        // File.WriteAllBytes(save, ram);
    }

    public void Load(byte[] data)
    {
        Rom = data;

        // string save = romPath.Replace("Roms", "Saves").Replace(".gb", ".sav", true, null);

        // if (File.Exists(save))
        // {
        //     ram = File.ReadAllBytes(save);
        // }
    }

    enum BankingMode
    {
        Rom,
        Ram,
    }
}
