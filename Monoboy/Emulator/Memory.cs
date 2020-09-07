namespace Monoboy
{
    public class Memory
    {
        public byte[] boot;             // 0x0000-0x00FF
        public byte[] vram;             // 0x8000-0x9FFF
        public byte[] workram;          // 0xC000-0xFDFF
        public byte[] oam;              // 0xFE00-0xFE9F
        // Unused                       // 0xFEA0-0xFEFF
        public byte[] io;               // 0xFF00-0xFF7F
        public byte[] zp;               // 0xFF80-0xFFFF

        public Memory()
        {
            workram = new byte[8192];
            oam = new byte[160];
            io = new byte[128];
            zp = new byte[128];
            vram = new byte[8196];
        }
    }
}
