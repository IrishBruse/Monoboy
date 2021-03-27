using Monoboy.Constants;

namespace Monoboy
{
    public class Timer
    {
        public byte DIV { get => (byte)(SystemInternalClock >> 8); set => SystemInternalClock = 0; }
        public byte TIMA { get => emulator.memory.io[0x05]; set => emulator.memory.io[0x05] = value; }//Timer counter
        public byte TMA { get => emulator.memory.io[0x06]; set => emulator.memory.io[0x06] = value; }
        public bool TacEnabled => (emulator.memory.io[0x07] & 0b100) != 0;
        public byte TacFrequancy => (byte)(emulator.memory.io[0x07] & 0b011);

        private readonly Emulator emulator;

        private readonly ushort[] TimerFrequancy = { 1024, 16, 64, 256 };

        public ushort SystemInternalClock { get; private set; }

        private ushort timerCounter;

        public Timer(Emulator emulator)
        {
            this.emulator = emulator;
        }

        public void Step(int ticks)
        {
            SystemInternalClock += (ushort)ticks;

            if (TacEnabled == true)
            {
                timerCounter += (ushort)ticks;

                while (timerCounter >= TimerFrequancy[TacFrequancy])
                {
                    TIMA++;
                    timerCounter -= TimerFrequancy[TacFrequancy];
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
}