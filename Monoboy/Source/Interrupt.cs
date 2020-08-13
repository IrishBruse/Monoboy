using System;
using System.Runtime.InteropServices.ComTypes;
using Monoboy.Utility;

namespace Monoboy
{
    public class Interrupt
    {
        Bus bus;

        bool IME = false;// Master interupt enabled

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
            IME = true;
        }

        public void InterruptRequest(InterruptFlag interrupt)
        {
            IF.SetBit((Bit)interrupt, true);
        }

        public void HandleInterupts()
        {
            if(IME == true)
            {
                byte firedInterupts = (byte)(IF & IE);

                if(firedInterupts != 0)
                {
                    return;
                }

                bus.cpu.halted = false;
                bus.cpu.Push(bus.register.PC);

                if(HandleInterupt(InterruptFlag.VBlank, firedInterupts) == true) return;
                if(HandleInterupt(InterruptFlag.LCDStat, firedInterupts) == true) return;
                if(HandleInterupt(InterruptFlag.Timer, firedInterupts) == true) return;
                if(HandleInterupt(InterruptFlag.Serial, firedInterupts) == true) return;
                if(HandleInterupt(InterruptFlag.Joypad, firedInterupts) == true) return;
            }
        }

        private bool HandleInterupt(InterruptFlag interupt, byte firedInterupts)
        {
            if(firedInterupts.GetBit((Bit)interupt) == false) return false;

            IF.SetBit((Bit)interupt, false);
            bus.register.PC = InterruptToJumpVector(interupt);
            IME = false;
            return true;
        }

        ushort InterruptToJumpVector(InterruptFlag flag)
        {
            switch(flag)
            {
                case InterruptFlag.VBlank:
                return 0x0040;
                case InterruptFlag.LCDStat:
                return 0x0048;
                case InterruptFlag.Timer:
                return 0x0050;
                case InterruptFlag.Serial:
                return 0x0058;
                case InterruptFlag.Joypad:
                return 0x0060;
            }

            return 0;
        }

        public enum InterruptFlag
        {
            VBlank = Bit.Bit0,
            LCDStat = Bit.Bit1,
            Timer = Bit.Bit2,
            Serial = Bit.Bit3,
            Joypad = Bit.Bit4,
        }
    }
}