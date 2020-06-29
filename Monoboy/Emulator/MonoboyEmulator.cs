namespace Monoboy.Emulator
{
    public class MonoboyEmulator
    {
        public int steps;
        public string CurrentOpcode { get => cpu.currentOpcode; }


        public Register register;
        readonly Memory memory;
        readonly CPU cpu;
        readonly GPU gpu;


        public MonoboyEmulator()
        {
            register = new Register();
            memory = new Memory();
            cpu = new CPU(memory, register);
            gpu = new GPU(memory);
            gpu.Call();
        }

        public void Step()
        {
            steps++;
            cpu.Step();
        }

        public void Boot()
        {
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
            memory.Write(0xFF26, 0xF1);//GB
            //memory.Write(0xFF26, 0xF0);// SGB
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
    }
}
