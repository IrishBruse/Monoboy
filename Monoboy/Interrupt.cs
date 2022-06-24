namespace Monoboy;

using Monoboy.Utility;

public class Interrupt
{
    private readonly Emulator emulator;
    private readonly Register register;
    public byte IE { get => emulator.Read(0xFFFF); set => emulator.Write(0xFFFF, value); }
    public byte IF { get => emulator.Read(0xFF0F); set => emulator.Write(0xFF0F, value); }

    public Interrupt(Emulator emulator, Register register)
    {
        this.emulator = emulator;
        this.register = register;
    }

    public void Disable()
    {
        emulator.Ime = false;
    }

    public void Enable()
    {
        emulator.ImeDelay = true;
    }

    public void Halt()
    {
        if (emulator.Ime == false)
        {
            if ((emulator.Read(NamedMemory.IE) & emulator.Read(NamedMemory.IF) & 0b11111) == 0)
            {
                emulator.Halted = true;
                register.PC--;
            }
            else
            {
                emulator.HaltBug = true;
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
                if (emulator.Halted)
                {
                    register.PC++;
                    emulator.Halted = false;
                }
                if (emulator.Ime)
                {
                    emulator.Push(register.PC);
                    register.PC = (ushort)(64 + (8 * i));
                    emulator.Ime = false;
                    IF = IF.SetBit((byte)(0b1 << i), false);
                }
            }
        }

        emulator.Ime |= emulator.ImeDelay;
        emulator.ImeDelay = false;
    }

    internal void Reset()
    {
        emulator.Ime = false;
        emulator.ImeDelay = false;
    }
}
