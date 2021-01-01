using Monoboy.Constants;

namespace Monoboy
{
    public class Timer
    {
        public byte Div { get => emulator.memory.io[0x04]; set => emulator.memory.io[0x04] = value; }
        public byte Tima { get => emulator.memory.io[0x05]; set => emulator.memory.io[0x05] = value; }
        public byte Tma { get => emulator.memory.io[0x06]; set => emulator.memory.io[0x06] = value; }
        public bool TacEnabled => (emulator.memory.io[0x07] & 0b100) != 0;
        public byte TacFrequancy => (byte)(emulator.memory.io[0x07] & 0b011);

        private Emulator emulator;

        private int dividerCounter;
        private int timerCounter;
        private readonly int[] TimerFrequancy = { 1024, 16, 64, 256 };

        public Timer(Emulator emulator)
        {
            this.emulator = emulator;
        }

        public void Step(int ticks)
        {
            dividerCounter += ticks;
            if(dividerCounter >= 256)
            {
                Div++;
                dividerCounter -= 256;
            }

            if(TacEnabled == true)
            {
                timerCounter += ticks;

                while(timerCounter >= TimerFrequancy[TacFrequancy])
                {
                    Tima++;
                    timerCounter -= TimerFrequancy[TacFrequancy];
                }

                if(Tima == 0xFF)
                {
                    Tima = Tma;
                    emulator.interrupt.InterruptRequest(InterruptFlag.Timer);
                }
            }
        }
    }
}