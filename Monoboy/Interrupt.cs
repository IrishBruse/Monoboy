namespace Monoboy;

using Monoboy.Utility;

public class Interrupt
{
    readonly Emulator emulator;
    bool IME;// Master interupt enabled
    bool IMEDelay;// Master interupt enabled delay

    public byte IE
    {
        get
        {
            return emulator.Read(0xFFFF);
        }

        set
        {
            emulator.Write(0xFFFF, value);
        }
    }
    public byte IF
    {
        get
        {
            return emulator.Read(0xFF0F);
        }

        set
        {
            emulator.Write(0xFF0F, value);
        }
    }

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
        if (IME == false)
        {
            if ((IE & IF & 0b11111) == 0)
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

    public void RequestInterrupt(byte bit)
    {
        IF = IF.SetBit(bit, true);
    }

    public void HandleInterupts()
    {
        for (byte i = 0; i < 5; i++)
        {
            if ((((IE & IF) >> i) & 0x1) == 1)
            {
                if (emulator.cpu.halted == true)
                {
                    emulator.register.PC++;
                    emulator.cpu.halted = false;
                }
                if (IME)
                {
                    emulator.cpu.Push(emulator.register.PC);
                    emulator.register.PC = (ushort)(64 + (8 * i));
                    IME = false;
                    IF = IF.SetBit((byte)(0b1 << i), false);
                }
            }
        }

        IME |= IMEDelay;
        IMEDelay = false;
    }

    internal void Reset()
    {
        IME = false;
        IMEDelay = false;
    }
}
