using Microsoft.Xna.Framework.Graphics;
using Monoboy.Emulator.Utility;

namespace Monoboy.Emulator
{
    public class MonoboyEmulator
    {
        public int steps;
        public string CurrentOpcode { get => memory.Read(register.PC).ToHex(); }
        public ushort CurrentAddress { get => register.PC; }

        Interrupt interrupt;
        Memory memory;
       public Register register;
        CPU cpu;
        GPU gpu;

        int cycles;

        public MonoboyEmulator()
        {
            interrupt = new Interrupt();
            memory = new Memory();
            register = new Register();

            cpu = new CPU
            {
                memory = memory,
                register = register,
                interrupt = interrupt
            };

            gpu = new GPU
            {
                memory = memory,
                interrupt = interrupt
            };
        }

        public void Step()
        {
            steps++;
            cycles = cpu.Step();
            gpu.Step(cycles);
        }

        public void LinkTexture(Texture2D texture)
        {
            gpu.screen = texture;
        }

        public void SkipBootRom()
        {
            memory.booted = true;
            register.AF = 0x01B0;
            register.BC = 0x0013;
            register.DE = 0x00D8;
            register.HL = 0x014D;
            register.SP = 0xFFFE;

            memory.Write(0xFF05, 0x00);
            memory.Write(0xFF06, 0x00);
            memory.Write(0xFF07, 0x00);
            memory.Write(0xFF10, 0x80);
            memory.Write(0xFF11, 0xBF);
            memory.Write(0xFF12, 0xF3);
            memory.Write(0xFF14, 0xBF);
            memory.Write(0xFF16, 0x3F);
            memory.Write(0xFF17, 0x00);
            memory.Write(0xFF19, 0xBF);
            memory.Write(0xFF1A, 0x7F);
            memory.Write(0xFF1B, 0xFF);
            memory.Write(0xFF1C, 0x9F);
            memory.Write(0xFF1E, 0xBF);
            memory.Write(0xFF20, 0xFF);
            memory.Write(0xFF21, 0x00);
            memory.Write(0xFF22, 0x00);
            memory.Write(0xFF23, 0xBF);
            memory.Write(0xFF24, 0x77);
            memory.Write(0xFF25, 0xF3);
            memory.Write(0xFF26, 0xF1);
            memory.Write(0xFF40, 0x91);
            memory.Write(0xFF42, 0x00);
            memory.Write(0xFF43, 0x00);
            memory.Write(0xFF45, 0x00);
            memory.Write(0xFF47, 0xFC);
            memory.Write(0xFF48, 0xFF);
            memory.Write(0xFF49, 0xFF);
            memory.Write(0xFF4A, 0x00);
            memory.Write(0xFF4B, 0x00);
            memory.Write(0xFFFF, 0x00);
        }

        public void LoadRom(string path)
        {
            memory.LoadRom(path);
        }

        public void Dump()
        {
            memory.Dump();
        }

        public void DumpAsImage(GraphicsDevice graphics)
        {
            memory.DumpAsImage(graphics);
        }

        public string[] DebugInfo()
        {
            return new string[]
            {
                "F : " + (cpu.register.GetFlag(Flag.Zero) ? "Z" : "-") +
                        (cpu.register.GetFlag(Flag.Negative) ? "N" : "-").ToString() +
                        (cpu.register.GetFlag(Flag.HalfCarry) ? "H" : "-").ToString() +
                        (cpu.register.GetFlag(Flag.FullCarry) ? "F" : "-").ToString(),
                "AF: " + cpu.register.AF.ToHex(),
                "BC: " + cpu.register.BC.ToHex(),
                "DE: " + cpu.register.DE.ToHex(),
                "HL: " + cpu.register.HL.ToHex(),
                "SP: " + cpu.register.SP.ToHex(),
                "PC: " + cpu.register.PC.ToHex() + " = " + CurrentOpcode,
                "Step: " + steps,
                "",
                "Gpu clock:" + gpu.clock
            };
        }
    }
}