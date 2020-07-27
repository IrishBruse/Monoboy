using Monoboy.Core.Utility;

namespace Monoboy.Core
{
    public class Memory
    {
        public byte[] boot;             //  0x0000-0x00FF
        // Vram                         //  0x8000-0x9FFF
        public byte[] workRAM;          //  0xC000-0xFDFF
        public byte[] spriteRAM;        //  0xFE00-0xFE9F
        // Unused                       //  0xFEA0-0xFEFF
        public byte[] io;               //  0xFF00-0xFF7F
        public byte[] zp;               //  0xFF80-0xFFFF

        public byte JoypadInput { get => io[0x0000]; set => io[0x0000] = value; }
        public byte LCDC { get => io[0x0040]; set => io[0x0040] = value; }
        public byte Stat { get => io[0x0041]; set => io[0x0041] = value; }
        public Mode StatMode { get => (Mode)io[0x0041].GetBits(0b00000011); set => io[0x0041] = io[0x0041].SetBits(0b00000011, (byte)value); }
        public byte ScrollY { get => io[0x0042]; set => io[0x0042] = value; }
        public byte ScrollX { get => io[0x0043]; set => io[0x0043] = value; }
        public byte Scanline { get => io[0x0044]; set => io[0x0044] = value; }
        public byte BackgroundPallet { get => io[0x0047]; set => io[0x0047] = value; }
        public byte BootRomEnabled { get => io[0x0050]; set => io[0x0050] = value; }

        public Memory()
        {
            workRAM = new byte[8192];
            spriteRAM = new byte[160];
            io = new byte[128];
            zp = new byte[128];
        }
    }
}
