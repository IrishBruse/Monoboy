namespace Monoboy.Emulator
{
    public struct Register
    {
        public byte A { get; set; }

        public byte F { get; set; }
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }
        public ushort SP { get; set; }
        public ushort PC { get; set; }

        public ushort AF
        {
            get
            {
                return (ushort)((A << 8) | F);
            }
            set
            {
                A = (byte)((value & 0xFF00) >> 8);
                F = (byte)(value & 0x00FF);
            }
        }

        public ushort BC
        {
            get
            {
                return (ushort)((B << 8) | C);
            }
            set
            {
                B = (byte)((value & 0xFF00) >> 8);
                C = (byte)(value & 0x00FF);
            }
        }

        public ushort DE
        {
            get
            {
                return (ushort)((D << 8) | E);
            }
            set
            {
                D = (byte)((value & 0xFF00) >> 8);
                E = (byte)(value & 0x00FF);
            }
        }

        public ushort HL
        {
            get
            {
                return (ushort)((H << 8) | L);
            }
            set
            {
                H = (byte)((value & 0xFF00) >> 8);
                L = (byte)(value & 0x00FF);
            }
        }

        public void SetFlag(Flag flag, bool condition)
        {
            if (condition == true)
            {
                F |= (byte)flag;
            }
            else
            {
                F &= (byte)(~flag);
            }
        }
    }

    public enum Flag
    {
        Zero = 0b10000000,
        Subt = 0b01000000,
        Half = 0b00100000,
        Full = 0b00010000
    }
}