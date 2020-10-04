using System;
using System.IO;

namespace Monoboy
{
    public class MemoryBankController3 : IMemoryBankController
    {
        public byte[] rom;
        public byte[] ram = new byte[32768];

        public byte romBank = 1;
        public byte ramBank = 0;
        public bool ramEnabled = false;

        // Timer
        byte seconds;   // RTC S
        byte minutes;   // RTC M
        byte hours;     // RTC H
        byte days;      // RTC DL
        byte daysCarry; // RTC DH

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
            if (ramEnabled == true)
            {
                switch (ramBank)
                {
                    case byte _ when address < 0x3:
                        return ram[(0x2000 * ramBank) + (address & 0x1FFF)];

                    // Timer
                    case 0x8:
                        return seconds;

                    case 0x9:
                        return minutes;

                    case 0xA:
                        return hours;

                    case 0xB:
                        return days;

                    case 0xC:
                        return daysCarry;
                }
            }

            return 0xFF;
        }

        public void WriteRam(ushort address, byte data)
        {
            if (ramEnabled == true)
            {
                switch (ramBank)
                {
                    case byte _ when address < 0x3:
                        ram[(0x2000 * ramBank) + (address & 0x1FFF)] = data;
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
                }
            }
        }

        public void WriteBank(ushort address, byte data)
        {
            switch (address)
            {
                case ushort _ when address < 0x2000:
                    {
                        ramEnabled = data == 0x0A;
                    }
                    break;

                case ushort _ when address < 0x4000:
                    {
                        romBank = (byte)(data & 0b01111111);
                        if (romBank == 0x00)
                        {
                            romBank += 1;
                        }
                    }
                    break;

                case ushort _ when address < 0x6000:
                    {
                        if (data >= 0x00 && data <= 0x03 && data >= 0x08 && data <= 0xC0)
                        {
                            ramBank = data;
                        }
                    }
                    break;

                case ushort _ when address < 0x8000:
                    {
                        DateTime now = DateTime.Now;
                        seconds = (byte)now.Second;
                        minutes = (byte)now.Minute;
                        hours = (byte)now.Hour;
                    }
                    break;
            }
        }

        public void Load(string path)
        {
            rom = File.ReadAllBytes(path);

            string save = path.Replace("Roms", "Saves").Replace(".gb", ".sav", true, null);

            if (File.Exists(save) == true)
            {
                ram = File.ReadAllBytes(save);
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
    }
}