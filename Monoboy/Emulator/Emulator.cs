using System;
using System.IO;

namespace Monoboy
{
    public class Emulator
    {
        public Bus bus;
        public long cyclesRan;
        public bool paused = true;
        private string openRom;
        private bool skipBootRom = false;


        public Emulator()
        {
            bus = new Bus();
        }

        public byte Step()
        {
            byte cycles = bus.cpu.Step();

            bus.timer.Step(cycles);
            bus.gpu.Step(cycles);
            bus.interrupt.HandleInterupts();

            cyclesRan += cycles;

            // Disable the bios in the bus
            if (bus.biosEnabled == true && bus.register.PC >= 0x100)
            {
                bus.biosEnabled = false;
            }

            return cycles;
        }

        public void Open()
        {
            Open(openRom);
        }

        public void Open(string rom)
        {
            openRom = rom;

            Reset();

            paused = false;

            byte cartridgeType;
            using (BinaryReader reader = new BinaryReader(new FileStream(openRom, FileMode.Open)))
            {
                reader.BaseStream.Seek(0x147, SeekOrigin.Begin);
                cartridgeType = reader.ReadByte();
            }

            bus.memoryBankController = cartridgeType switch
            {
                byte _ when (cartridgeType <= 00) => new MemoryBankController0(),
                byte _ when (cartridgeType <= 03) => new MemoryBankController1(),
                byte _ when (cartridgeType <= 06) => new MemoryBankController2(),
                byte _ when (cartridgeType <= 19) => new MemoryBankController3(),
                byte _ when (cartridgeType <= 27) => new MemoryBankController5(),
                _ => throw new NotImplementedException()
            };

            bus.memoryBankController.Load(openRom);

            string saveFile = openRom.Replace("Roms", "Saves").Replace(".gb", ".sav");
            if (File.Exists(saveFile) == true)
            {
                bus.memoryBankController.SetRam(File.ReadAllBytes(saveFile));
            }
        }

        public void Reset()
        {
            bus.memory = new Memory();
            bus.register = new Register();

            if (File.Exists("Data/dmg_boot.bin") == true)
            {
                if (skipBootRom == false)
                {
                    bus.memory.boot = File.ReadAllBytes("Data/dmg_boot.bin");
                    bus.biosEnabled = true;
                }
                else
                {
                    SkipBootRom();
                }
            }
            else
            {
                SkipBootRom();
            }
        }

        public void SkipBootRom()
        {
            bus.biosEnabled = false;
            bus.register.AF = 0x01B0;
            bus.register.BC = 0x0013;
            bus.register.DE = 0x00D8;
            bus.register.HL = 0x014D;
            bus.register.SP = 0xFFFE;
            bus.register.PC = 0x100;
            bus.Write(0xFF05, 0x00);
            bus.Write(0xFF06, 0x00);
            bus.Write(0xFF07, 0x00);
            bus.Write(0xFF10, 0x80);
            bus.Write(0xFF11, 0xBF);
            bus.Write(0xFF12, 0xF3);
            bus.Write(0xFF14, 0xBF);
            bus.Write(0xFF16, 0x3F);
            bus.Write(0xFF17, 0x00);
            bus.Write(0xFF19, 0xBF);
            bus.Write(0xFF1A, 0x7F);
            bus.Write(0xFF1B, 0xFF);
            bus.Write(0xFF1C, 0x9F);
            bus.Write(0xFF1E, 0xBF);
            bus.Write(0xFF20, 0xFF);
            bus.Write(0xFF21, 0x00);
            bus.Write(0xFF22, 0x00);
            bus.Write(0xFF23, 0xBF);
            bus.Write(0xFF24, 0x77);
            bus.Write(0xFF25, 0xF3);
            bus.Write(0xFF26, 0xF1);
            bus.Write(0xFF40, 0x91);
            bus.Write(0xFF42, 0x00);
            bus.Write(0xFF43, 0x00);
            bus.Write(0xFF45, 0x00);
            bus.Write(0xFF47, 0xFC);
            bus.Write(0xFF48, 0xFF);
            bus.Write(0xFF49, 0xFF);
            bus.Write(0xFF4A, 0x00);
            bus.Write(0xFF4B, 0x00);
            bus.Write(0xFFFF, 0x00);
        }
    }
}