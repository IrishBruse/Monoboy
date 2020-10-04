using Monoboy.Constants;

namespace Monoboy
{
    public class Timer
    {
        public byte Div { get => bus.memory.io[0x04]; set => bus.memory.io[0x04] = value; }
        public byte Tima { get => bus.memory.io[0x05]; set => bus.memory.io[0x05] = value; }
        public byte Tma { get => bus.memory.io[0x06]; set => bus.memory.io[0x06] = value; }
        public bool TacEnabled => (bus.memory.io[0x07] & 0b100) != 0;
        public byte TacFrequancy => (byte)(bus.memory.io[0x07] & 0b011);

        private Bus bus;

        private int dividerCounter;
        private int timerCounter;
        private readonly int[] TimerFrequancy = { 1024, 16, 64, 256 };

        public Timer(Bus bus)
        {
            this.bus = bus;
        }

        public void Step(int ticks)
        {
            dividerCounter += ticks;
            if (dividerCounter >= 256)
            {
                Div++;
                dividerCounter -= 256;
            }

            if (TacEnabled == true)
            {
                timerCounter += ticks;

                while (timerCounter >= TimerFrequancy[TacFrequancy])
                {
                    Tima++;
                    timerCounter -= TimerFrequancy[TacFrequancy];
                }

                if (Tima == 0xFF)
                {
                    Tima = Tma;
                    bus.interrupt.InterruptRequest(InterruptFlag.Timer);
                }
            }
        }
    }
}