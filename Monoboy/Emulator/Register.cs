using Monoboy.Emulator.Utility;

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
                A = value.High();
                F = value.Low();
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
                B = value.High();
                C = value.Low();
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
                D = value.High();
                E = value.Low();
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
                H = value.High();
                L = value.Low();
            }
        }

        public void SetFlag(Flag flag, bool condition)
        {
            if(condition == true)
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
        Negative = 0b01000000,
        HalfCarry = 0b00100000,
        FullCarry = 0b00010000
    }
}