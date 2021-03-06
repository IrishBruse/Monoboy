﻿
using Monoboy.Constants;
using Monoboy.Utility;

namespace Monoboy
{
    public class Joypad
    {
        private bool readPad;

        public byte buttonState;
        public byte padState;
        private readonly Emulator emulator;

        private byte joyp = 0b110000;
        public byte JOYP
        {
            get
            {
                byte result = (byte)(joyp & 0b110000);
                if (readPad == true)
                {
                    return (byte)(result | (~padState & 0b1111));
                }
                else
                {
                    return (byte)(result | (~buttonState & 0b1111));
                }
            }
            set
            {
                joyp = value;
                if (value.GetBit(Bit.Bit4) == false)
                {
                    readPad = true;
                }
                if (value.GetBit(Bit.Bit5) == false)
                {
                    readPad = false;
                }
            }
        }

        public Joypad(Emulator emulator)
        {
            this.emulator = emulator;
        }

        public void SetButton(Button button, bool state)
        {
            byte key = (byte)button;

            if (key > 0b1000)
            {
                key = (byte)(key >> 4);

                if (state == true)
                {
                    buttonState |= key;
                    emulator.interrupt.RequestInterrupt(InterruptFlag.Joypad);
                }
                else
                {
                    buttonState &= (byte)~key;
                }
            }
            else
            {
                if (state == true)
                {
                    padState |= key;
                    emulator.interrupt.RequestInterrupt(InterruptFlag.Joypad);
                }
                else
                {
                    padState &= (byte)~key;
                }
            }
        }

        public enum Button : byte
        {
            Right = 0b00000001,
            Left = 0b00000010,
            Up = 0b00000100,
            Down = 0b00001000,

            // Shifted right 4
            A = 0b00010000,
            B = 0b00100000,
            Select = 0b01000000,
            Start = 0b10000000,
        }

        internal void Reset()
        {
            readPad = false;
            joyp = 0b110000;
            buttonState = 0;
            padState = 0;
        }
    }
}