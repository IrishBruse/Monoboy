using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Monoboy.Core
{
    public class Emulator
    {
        public const byte WindowWidth = 160;
        public const byte WindowHeight = 144;

        public Bus bus;

        public Emulator()
        {
            bus = new Bus();
        }

        public byte Step()
        {
            byte cycles = bus.cpu.Step();
            bus.gpu.Step(cycles);
            return cycles;
        }

        public void LoadRom(string path)
        {
            using BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));

            for(int i = 0; i < 16384; i++)
            {
                bus.cartridge.cartBank0[i] = reader.ReadByte();
            }

            for(int i = 0; i < 16384; i++)
            {
                bus.cartridge.cartBankN[i] = reader.ReadByte();
            }
        }
    }
}
