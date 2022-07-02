namespace Monoboy;

using Monoboy.Constants;

public class Timer
{
    byte TacFrequancy => (byte)(memory[0xFF07] & 0b11);

    private readonly ushort[] timerFrequancy = { 1024, 16, 64, 256 };
    private Memory memory;
    private readonly Cpu cpu;

    public ushort SystemInternalClock { get; set; }

    private ushort timerCounter;

    public Timer(Memory memory, Cpu cpu)
    {
        this.memory = memory;
        this.cpu = cpu;
    }

    public void Step(int ticks)
    {
        SystemInternalClock += (ushort)ticks;

        if ((memory[0xFF07] & 0b100) != 0)
        {
            timerCounter += (ushort)ticks;

            while (timerCounter >= timerFrequancy[TacFrequancy])
            {
                memory[0xFF05]++;
                timerCounter -= timerFrequancy[TacFrequancy];
            }

            // Overflow occured
            if (memory[0xFF05] == 0xFF)
            {
                cpu.RequestInterrupt(InterruptFlag.Timer);
                memory[0xFF05] = memory[0xFF06];
            }
        }
    }

    internal void Reset()
    {
        SystemInternalClock = 0;
        timerCounter = 0;
    }
}
