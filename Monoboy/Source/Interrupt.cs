using Monoboy.Utility;

namespace Monoboy
{
    public class Interrupt
    {
        private Bus bus;
        private bool IME = false;// Master interupt enabled
        private bool IMEDelay = false;// Master interupt enabled delay

        public byte IE { get; set; }
        public byte IF { get; set; }

        public Interrupt(Bus bus)
        {
            this.bus = bus;
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
                    bus.cpu.halted = true;
                    bus.register.PC--;
                }
                else
                {
                    bus.cpu.haltBug = true;
                }
            }
        }

        public void InterruptRequest(byte interrupt)
        {
            IF.SetBit(interrupt, true);
        }

        public void HandleInterupts()
        {
            for(byte i = 0; i < 5; i++)
            {
                if((((IE & IF) >> i) & 0b1) == 1)
                {
                    if(bus.cpu.halted == true)
                    {
                        bus.register.PC++;
                        bus.cpu.halted = false;
                    }
                    if(IME == true)
                    {
                        bus.cpu.Push(bus.register.PC);
                        bus.register.PC = (ushort)(0b10000000 + (8 * i));
                        IME = false;
                        IF.SetBit(i, false);
                    }
                }
            }

            IME |= IMEDelay;
            IMEDelay = false;
        }
    }
}