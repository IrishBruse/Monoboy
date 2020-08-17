using Monoboy.Constants;
using Monoboy.Utility;

namespace Monoboy
{
    public class Joypad
    {
        private byte buttonState = 0xF;
        private byte padState = 0xF;
        private Bus bus;

        public Joypad(Bus bus)
        {
            this.bus = bus;
        }

        public void SetButton(Button button, bool state)
        {
            byte key = (byte)button;

            if(key > 7)
            {
                buttonState |= (byte)(key >> 4 & (state ? 0x00 : 0xFF));
            }
            else
            {
                padState |= (byte)(key & (state ? 0x00 : 0xFF));
            }
        }

        public void Step()
        {
            if(bus.memory.JOYP.GetBit(Bit.Bit4) == false)
            {
                bus.memory.JOYP = (byte)((bus.memory.JOYP & 0xF0) | padState);
                if(padState != 0xF)
                {
                    bus.interrupt.InterruptRequest(InterruptFlag.Joypad);
                }
            }

            if(bus.memory.JOYP.GetBit(Bit.Bit5) == false)
            {
                bus.memory.JOYP = (byte)((bus.memory.JOYP & 0xF0) | buttonState);
                if(padState != 0xF)
                {
                    bus.interrupt.InterruptRequest(InterruptFlag.Joypad);
                }
            }

            if((bus.memory.JOYP & 0b00110000) == 0b00110000)
            {
                bus.memory.JOYP = 0xFF;
            }
        }

        public enum Button : byte
        {
            Right = 0b00000001,
            Left = 0b00000010,
            Up = 0b00000100,
            Down = 0b00001000,
            A = 0b00010000,
            B = 0b00100000,
            Select = 0b01000000,
            Start = 0b10000000,
        }
    }
}