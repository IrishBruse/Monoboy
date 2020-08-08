using Monoboy.Utility;

namespace Monoboy
{
    public class Joypad
    {
        byte JOYP;// P1
        byte buttonState;
        byte dPadState;

        bool readButtonsNext = false;

        Bus bus;

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
                dPadState |= (byte)(key & (state ? 0x00 : 0xFF));
            }

            if(state == true)
            {
                bus.interrupt.InterruptRequest(Interrupt.InterruptFlag.Joypad);
            }
        }

        public byte Read()
        {
            byte result = (byte)(JOYP & 0b00110000);

            if(readButtonsNext == true)
            {
                result &= buttonState;
            }
            else
            {
                result &= dPadState;
            }

            return result;
        }

        public void Write(byte data)
        {
            JOYP = data;
            if(data.GetBit(Bit.Bit4) == false)
            {
                readButtonsNext = false;
            }
            if(data.GetBit(Bit.Bit5) == false)
            {
                readButtonsNext = true;
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