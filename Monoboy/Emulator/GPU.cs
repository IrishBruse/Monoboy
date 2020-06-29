namespace Monoboy.Emulator
{
    public class GPU
    {
        readonly Memory memory;

        public GPU(Memory memory)
        {
            this.memory = memory;
        }

        public void Call()
        {
            memory.Read(0x0203);
        }
    }
}