namespace Monoboy;

using Monoboy.Constants;

public class Timer
{
    public byte Div { get => memory[0xFF04]; set => memory[0xFF04] = value; }
    public byte Tima { get => memory[0xFF05]; set => memory[0xFF05] = value; }
    public byte Tma { get => memory[0xFF06]; set => memory[0xFF06] = value; }
    public bool TacEnabled => (memory[0xFF07] & Bit.Bit2) != 0;
    public byte TacFrequancy => (byte)(memory[0xFF07] & 0b011);

    static readonly int[] TimerFrequancy = { 1024 / 4, 16 / 4, 64 / 4, 256 / 4 };

    int timerCounter;

    Memory memory;
    readonly Cpu cpu;

    public int DivCounter { get; set; }

    public Timer(Memory memory, Cpu cpu)
    {
        this.memory = memory;
        this.cpu = cpu;
    }

    public void Step(int mCycles)
    {
        DivCounter += mCycles;
        if (DivCounter >= 256 / 4)
        {
            Div++;
            DivCounter -= 256 / 4;
        }

        if (TacEnabled)
        {
            timerCounter += mCycles;

            while (timerCounter >= TimerFrequancy[TacFrequancy])
            {
                Tima++;
                timerCounter -= TimerFrequancy[TacFrequancy];
            }

            if (Tima == 0xFF)
            {
                Tima = Tma;
                cpu.RequestInterrupt(InterruptFlag.Timer);
            }
        }
    }

    internal void Reset()
    {
        DivCounter = 0;
        timerCounter = 0;
    }
}
