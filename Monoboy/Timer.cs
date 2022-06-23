namespace Monoboy;

using Monoboy.Constants;

public class Timer
{
    public byte DIV
    {
        get
        {
            return (byte)(SystemInternalClock >> 8);
        }

        set
        {
            SystemInternalClock = 0;
        }
    }

    public byte TIMA
    {
        get
        {
            return emulator.memory.io[0x05];
        }

        set
        {
            emulator.memory.io[0x05] = value;
        }
    }

    //Timer counter
    public byte TMA
    {
        get
        {
            return emulator.memory.io[0x06];
        }

        set
        {
            emulator.memory.io[0x06] = value;
        }
    }
    public bool TacEnabled => (emulator.memory.io[0x07] & 0b100) != 0;
    public byte TacFrequancy => (byte)(emulator.memory.io[0x07] & 0b011);

    readonly Emulator emulator;

    readonly ushort[] timerFrequancy = { 1024, 16, 64, 256 };

    public ushort SystemInternalClock { get; private set; }

    ushort timerCounter;

    public Timer(Emulator emulator)
    {
        this.emulator = emulator;
    }

    public void Step(int ticks)
    {
        SystemInternalClock += (ushort)ticks;

        if (TacEnabled)
        {
            timerCounter += (ushort)ticks;

            while (timerCounter >= timerFrequancy[TacFrequancy])
            {
                TIMA++;
                timerCounter -= timerFrequancy[TacFrequancy];
            }

            // Overflow occured
            if (TIMA == 0xFF)
            {
                emulator.interrupt.RequestInterrupt(InterruptFlag.Timer);
                TIMA = TMA;
            }
        }
    }

    internal void Reset()
    {
        SystemInternalClock = 0;
        timerCounter = 0;
    }
}