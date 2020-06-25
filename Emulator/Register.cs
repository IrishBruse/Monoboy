namespace Monoboy.Emulator
{
    public struct Register
    {
        public byte A;
        public byte F;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;
        public ushort SP;
        public ushort PC;

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

        public bool GetFlag(Flag flag)
        {
            return (F & (byte)flag) != 0;
        }
    }

    public enum Flag
    {
        Zero = 0b10000000,
        Subt = 0b01000000,
        /// <summary>
        /// Byte Carry
        /// </summary>
        Half = 0b00100000,
        /// <summary>
        /// Short Carry
        /// </summary>
        Full = 0b00010000
    }
}