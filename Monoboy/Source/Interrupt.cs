using System;

namespace Monoboy.Core
{
    public class Interrupt
    {
        Bus bus;

        bool IME;// Master interupt enabled

        byte ie;
        public byte IE
        {
            get
            {
                if(IME == true)
                {
                    return 0;
                }
                else
                {
                    return ie;
                }
            }
            set
            {
                ie = value;
            }
        }

        public byte IF
        {
            get;
            set;
        }

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
            IF |= (byte)interrupt;
        }

        public void HandleInterupts()
        {
            if(IME == true)
            {
                for(int i = 0; i < 5; i++)
                {
                    byte bit = (byte)(1 << i);
                    if((ie & bit) > 0)
                    {
                        if((IF & bit) > 0)
                        {
                            IF = (byte)(IF & ~bit);

                            JumpVector jump = bit switch
                            {
                                0b00000001 => JumpVector.VBlank,
                                0b00000010 => JumpVector.LCDStat,
                                0b00000100 => JumpVector.Timer,
                                0b00001000 => JumpVector.Serial,
                                0b00010000 => JumpVector.Joypad,
                                _ => throw new Exception("Impossible!"),
                            };

                            bus.cpu.CALL((ushort)jump);
                        }
                    }
                }
            }

            bus.cpu.halted = false;
        }

        public enum InterruptFlag
        {
            VBlank = Bit.Bit0,
            LCDStat = Bit.Bit1,
            Timer = Bit.Bit2,
            Serial = Bit.Bit3,
            Joypad = Bit.Bit4,
        }

        public enum JumpVector
        {
            VBlank = 0x0040,
            LCDStat = 0x0048,
            Timer = 0x0050,
            Serial = 0x0058,
            Joypad = 0x0060,
        }
    }
}