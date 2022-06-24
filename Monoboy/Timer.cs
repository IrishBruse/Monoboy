namespace Monoboy;

using Monoboy.Constants;

public class Timer
{
    public byte DIV
    {
        get => (byte)(SystemInternalClock >> 8);

        set => SystemInternalClock = 0;
    }

    public byte TIMA
    {
        get => memory.io[0x05];

        set => memory.io[0x05] = value;
    }

    //Timer counter
    public byte TMA
    {
        get => memory.io[0x06];

        set => memory.io[0x06] = value;
    }
    public bool TacEnabled => (memory.io[0x07] & 0b100) != 0;
    public byte TacFrequancy => (byte)(memory.io[0x07] & 0b011);

    private readonly ushort[] timerFrequancy = { 1024, 16, 64, 256 };
    private readonly Memory memory;
    private readonly Interrupt interrupt;

    public ushort SystemInternalClock { get; private set; }

    private ushort timerCounter;

    public Timer(Memory memory, Interrupt interrupt)
    {
        this.memory = memory;
        this.interrupt = interrupt;
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
                interrupt.RequestInterrupt(InterruptFlag.Timer);
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
