using System.IO;
using Monoboy.Emulator.Utility;

namespace Monoboy.Emulator
{
    public class Memory
    {
        bool booted = true;

        byte[] boot;
        byte[] cartBank0;
        byte[] cartBankN;
        byte[] videoRAM;
        byte[] cartRAM;
        byte[] workRAM;
        byte[] spriteRAM;
        byte[] io;
        byte[] zp;

        public Memory()
        {
            boot = File.ReadAllBytes("Roms/Boot.bin");
            videoRAM = new byte[8192];
            cartRAM = new byte[8192];
            workRAM = new byte[8192];
            spriteRAM = new byte[160];
            io = new byte[128];
            zp = new byte[128];

            LoadRom(@"Roms\cpu test.gb");
        }

        public void LoadRom(string path)
        {
            cartBank0 = new byte[16384];
            cartBankN = new byte[16384];

            using(BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                for(int i = 0; i < 16384; i++)
                {
                    cartBank0[i] = reader.ReadByte();
                }

                for(int i = 0; i < 16384; i++)
                {
                    cartBankN[i] = reader.ReadByte();
                }
            }
        }

        public void Dump()
        {
            Debug.Disable();
            string dumpFile = @"C:\Users\Econn\Desktop\Dump.bin";

            using(BinaryWriter writer = new BinaryWriter(File.Create(dumpFile)))
            {
                for(int i = 0; i <= 65535; i++)
                {
                    writer.Write(Read((ushort)i));
                }
            }
            Debug.Enable();
        }

        public byte Read(ushort address)
        {
            byte data;
            string location;
            if(address >= 0x0000 && address <= 0x3FFF)// 16KB ROM Bank 00     (in cartridge, fixed at bank 00)
            {

                if((address >= 0x0000 && address <= 0x00FF) && booted == false)
                {
                    data = boot[address];
                    location = "boot";
                }
                else
                {
                    data = cartBank0[address];
                    location = "cartBank0";
                }
            }
            else if(address >= 0x4000 && address <= 0x7FFF)// 16KB ROM Bank 01..NN (in cartridge, switchable bank number)
            {
                data = cartBankN[address - 0x4000];
                location = "cartBankN";
            }
            else if(address >= 0x8000 && address <= 0x9FFF)// 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            {
                data = videoRAM[address - 0x8000];
                location = "videoRAM";
            }
            else if(address >= 0xA000 && address <= 0xBFFF)// 8KB External RAM     (in cartridge, switchable bank, if any)
            {
                data = cartRAM[address - 0xA000];
                location = "cartRAM";
            }
            else if(address >= 0xC000 && address <= 0xDFFF)// 4KB Work RAM Bank 0 (WRAM)
            {
                data = workRAM[address - 0xC000];
                location = "workRAM";
            }
            else if(address >= 0xD000 && address <= 0xDFFF)// 4KB Work RAM Bank 1 (WRAM)  (switchable bank 1-7 in CGB Mode)
            {
                data = workRAM[address - 0xC000];
                location = "workRAM";
            }
            else if(address >= 0xE000 && address <= 0xFDFF)// Same as C000-DDFF (ECHO)    (typically not used)
            {
                data = workRAM[address - 0xE000];
                location = "workRAM";
            }
            else if(address >= 0xFE00 && address <= 0xFE9F)// Sprite Attribute Table (OAM)
            {
                data = spriteRAM[address - 0xFE00];
                location = "spriteRAM";
            }
            else if(address >= 0xFEA0 && address <= 0xFEFF)// Not Usable
            {
                data = 0;
                location = "Not Usable";
            }
            else if(address >= 0xFF00 && address <= 0xFF7F)// I/O Ports
            {
                data = io[address - 0xFF00];
                location = "io";
            }
            else if(address >= 0xFF80 && address <= 0xFFFF)// Zero Page RAM
            {
                data = zp[address - 0xFF80];
                location = "zp";
            }
            else
            {
                data = 0;
                location = "Error";
            }

            Debug.Log("R " + address.ToHex() + "=" + data.ToHex() + " " + location);

            return data;
        }

        public void Write(ushort address, byte data)
        {
            string location = "";

            // cant write to rom skipped
            if(address >= 0x8000 && address <= 0x9FFF)// 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            {
                videoRAM[address - 0x8000] = data;
                location = "videoRAM";
            }
            else if(address >= 0xA000 && address <= 0xBFFF)// 8KB External RAM (in cartridge, switchable bank, if any)
            {
                cartRAM[address - 0xA000] = data;
                location = "cartRAM";
            }
            else if(address >= 0xC000 && address <= 0xCFFF)// 4KB Work RAM Bank 0 (WRAM)
            {
                workRAM[address - 0xC000] = data;
                location = "workRAM";
            }
            else if(address >= 0xD000 && address <= 0xDFFF)// 4KB Work RAM Bank 1 (WRAM) (switchable bank 1-7 in CGB Mode)
            {
                workRAM[address - 0xC000] = data;
                location = "workRAM";
            }
            else if(address >= 0xE000 && address <= 0xFDFF)// Same as C000-DDFF (ECHO) (typically not used)
            {
                workRAM[address - 0xC000 - 0x0200] = data;
                location = "workRAM";
            }
            else if(address >= 0xFE00 && address <= 0xFE9F)// Sprite Attribute Table (OAM)
            {
                spriteRAM[address - 0xFE00] = data;
                location = "spriteRAM";

            }
            else if(address >= 0xFEA0 && address <= 0xFEFF)// Not Usable
            {
                Debug.Log("Not Usable");
            }
            else if(address >= 0xFF00 && address <= 0xFF7F)// I/O Ports
            {
                if(address == 0xFF02 && data == 0x81)
                {
                    Debug.Log((char)Read(0xFF01));
                }
                io[address - 0xFF00] = data;
                location = "io";

            }
            else if(address >= 0xFF80 && address <= 0xFFFF)// Zero Page RAM
            {
                zp[address - 0xFF80] = data;
                location = "zp";
            }

            Debug.Log("W " + address.ToHex() + "=" + data.ToHex() + " " + location);
        }
    }
}