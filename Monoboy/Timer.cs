namespace Monoboy;

using Monoboy.Constants;

public class Timer
{
    public byte TIMA { get => memory[0xFF05]; set => memory[0xFF05] = value; }
    public byte TMA { get => memory[0xFF06]; set => memory[0xFF06] = value; }

    public bool TacEnabled => (memory[0xFF07] & 0b100) != 0;
    public byte TacFrequancy => (byte)(memory[0xFF07] & 0b011);

    private readonly ushort[] timerFrequancy = { 1024, 16, 64, 256 };
    private readonly byte[] memory;
    private readonly Cpu cpu;

    public ushort SystemInternalClock { get; set; }

    private ushort timerCounter;

    public Timer(byte[] memory, Cpu cpu)
    {
        this.memory = memory;
        this.cpu = cpu;
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
                cpu.RequestInterrupt(InterruptFlag.Timer);
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
