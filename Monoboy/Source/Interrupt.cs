using Monoboy.Utility;

namespace Monoboy
{
    public class Interrupt
    {
        private Bus bus;
        private bool IME;// Master interupt enabled
        private bool IMEDelay;// Master interupt enabled delay

        public byte IE { get { return bus.Read(0xFFFF); } set { bus.Write(0xFFFF, value); } }
        public byte IF { get { return bus.Read(0xFF0F); } set { bus.Write(0xFF0F, value); } }

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
                    if(bus.cpu.halted == true)
                    {
                        bus.register.PC++;
                        bus.cpu.halted = false;
                    }
                    if(IME == true)
                    {
                        bus.cpu.Push(bus.register.PC);
                        bus.register.PC = (ushort)(0b1000000 + (8 * i));
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