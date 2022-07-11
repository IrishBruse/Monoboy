namespace Monoboy;

using Monoboy.Constants;
using Monoboy.Utility;

public class Joypad
{
    private bool readPad;
    private byte buttonState;
    private byte padState;

    private byte joyp;
    private readonly Cpu cpu;

    public byte JOYP
    {
        get
        {
            byte result = (byte)(joyp & 0b11110000);
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

    public void SetButton(GameboyButton button, bool state)
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

    internal void Reset()
    {
        readPad = false;
        joyp = 0b11110000;
        buttonState = 0;
        padState = 0;
    }
}
