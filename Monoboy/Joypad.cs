﻿namespace Monoboy;

using Monoboy.Constants;
using Monoboy.Utility;

public class Joypad
{
    private bool readPad;
    private byte buttonState;
    private byte padState;

    private byte joyp = 0b110000;
    private readonly Cpu cpu;

    public byte JOYP
    {
        get
        {
            byte result = (byte)(joyp & 0b110000);
            return readPad ? (byte)(result | (~padState & 0b1111)) : (byte)(result | (~buttonState & 0b1111));
        }

        set
        {
            joyp = value;
            if (value.GetBit(0b00010000) == false)
            {
                readPad = true;
            }
            if (value.GetBit(0b00100000) == false)
            {
                readPad = false;
            }
        }
    }

    public Joypad(Cpu cpu)
    {
        this.cpu = cpu;
    }

    public void SetButton(Button button, bool state)
    {
        byte key = (byte)button;

        if (key > 0b1000)
        {
            key = (byte)(key >> 4);

            if (state)
            {
                buttonState |= key;
                cpu.RequestInterrupt(InterruptFlag.Joypad);
            }
            else
            {
                buttonState &= (byte)~key;
            }
        }
        else
        {
            if (state)
            {
                padState |= key;
                cpu.RequestInterrupt(InterruptFlag.Joypad);
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
