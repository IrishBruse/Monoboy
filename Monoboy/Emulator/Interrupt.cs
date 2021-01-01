using Monoboy.Utility;

namespace Monoboy
{
    public class Interrupt
    {
        private Emulator emulator;
        private bool IME;// Master interupt enabled
        private bool IMEDelay;// Master interupt enabled delay

        public byte IE { get => emulator.Read(0xFFFF); set => emulator.Write(0xFFFF, value); }
        public byte IF { get => emulator.Read(0xFF0F); set => emulator.Write(0xFF0F, value); }

        public Interrupt(Emulator emulator)
        {
            this.emulator = emulator;
        }

        public void Disable()
        {
            IME = false;
        }

        public void Enable()
        {
            IMEDelay = true;
        }

        public void Halt()
        {
            if(IME == false)
            {
                if((IE & IF & 0b11111) == 0)
                {
                    emulator.cpu.halted = true;
                    emulator.register.PC--;
                }
                else
                {
                    emulator.cpu.haltBug = true;
                }
            }
        }

        public void InterruptRequest(byte interrupt)
        {
            IF = IF.SetBit(interrupt, true);
        }

        public void HandleInterupts()
        {
            byte _ie = IE;
            byte _if = IF;

            for(byte i = 0; i < 5; i++)
            {
                if((((_ie & _if) >> i) & 0b1) == 1)
                {
                    if(emulator.cpu.halted == true)
                    {
                        emulator.register.PC++;
                        emulator.cpu.halted = false;
                    }
                    if(IME == true)
                    {
                        emulator.cpu.Push(emulator.register.PC);
                        emulator.register.PC = (ushort)(0b1000000 + (8 * i));
                        IME = false;
                        IF = IF.SetBit((byte)(1 << i), false);
                    }
                }
            }

            IME |= IMEDelay;
            IMEDelay = false;
        }
    }
}