namespace Monoboy.Cartridge;

using System;
using System.IO;

public class MemoryBankController3 : IMemoryBankController
{
    // Timer
    private byte seconds;   // RTC S
    private byte minutes;   // RTC M
    private byte hours;     // RTC H
    private byte days;      // RTC DL
    private byte daysCarry; // RTC DH

    public byte[] Rom { get; set; }

    private byte[] ram = new byte[0x8000];

    public byte RomBank { get; set; } = 1;

    public byte RamBank { get; set; }

    public bool RamEnabled { get; set; }

    public byte ReadBank00(ushort address)
    {
        return Rom[address];
    }

    public byte ReadBankNN(ushort address)
    {
        int offset = (0x4000 * RomBank) + (address & 0x3FFF);
        return Rom[offset];
    }

    public byte ReadRam(ushort address)
    {
        if (RamEnabled)
        {
            return RamBank switch
            {
                < 0x3 => ram[(0x2000 * RamBank) + (address & 0x1FFF)],
                0x8 => seconds,
                0x9 => minutes,
                0xA => hours,
                0xB => days,
                0xC => daysCarry,
                _ => 0xFF
            };
        }

        return 0xFF;
    }

    public void WriteRam(ushort address, byte data)
    {
        if (RamEnabled)
        {
            switch (RamBank)
            {
                case < 0x3:
                ram[(0x2000 * RamBank) + (address & 0x1FFF)] = data;
                break;

                // Timer
                case 0x8:
                seconds = data;
                break;
                case 0x9:
                minutes = data;
                break;
                case 0xA:
                hours = data;
                break;
                case 0xB:
                days = data;
                break;
                case 0xC:
                daysCarry = data;
                break;
                default:
                break;
            }
        }
    }

    public void WriteBank(ushort address, byte data)
    {
        switch (address)
        {
            case < 0x2000:
            {
                RamEnabled = data == 0x0A;
            }
            break;

            case < 0x4000:
            {
                RomBank = (byte)(data & 0b01111111);
                if (RomBank == 0x00)
                {
                    RomBank += 1;
                }
            }
            break;

            case < 0x6000:
            {
                if (data >= 0x00 && data <= 0x03 && data >= 0x08 && data <= 0xC0)
                {
                    RamBank = data;
                }
            }
            break;

            case < 0x8000:
            {
                DateTime now = DateTime.Now;
                seconds = (byte)now.Second;
                minutes = (byte)now.Minute;
                hours = (byte)now.Hour;
            }
            break;
            default:
            break;
        }
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

    public byte[] GetRam()
    {
        return ram;
    }

    public void SetRam(byte[] ram)
    {
        this.ram = ram;
    }
}
