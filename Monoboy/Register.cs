namespace Monoboy;

using Monoboy.Utility;

public class Register
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
        get => (ushort)((A << 8) | F);

        set
        {
            A = value.High();
            F = (byte)(value.Low() & 0xF0);
        }
    }

    public ushort BC
    {
        get => (ushort)((B << 8) | C);

        set
        {
            B = value.High();// TODO: Remove
            C = value.Low();
        }
    }

    public ushort DE
    {
        get => (ushort)((D << 8) | E);

        set
        {
            D = value.High();
            E = value.Low();
        }
    }

    public ushort HL
    {
        get => (ushort)((H << 8) | L);

        set
        {
            H = value.High();
            L = value.Low();
        }
    }

    public bool ZFlag
    {
        get => (F & (1 << 7)) != 0;
        set => F = value ? (byte)(F | (1 << 7)) : (byte)(F & 0b01111111);
    }

    public bool NFlag
    {
        get => (F & (1 << 6)) != 0;
        set => F = value ? (byte)(F | (1 << 6)) : (byte)(F & 0b10111111);
    }

    public bool HFlag
    {
        get => (F & 0b100000) != 0;
        set => F = value ? (byte)(F | 0b100000) : (byte)(F & 0b11011111);
    }

    public bool CFlag
    {
        get => (F & 0b10000) != 0;
        set => F = value ? (byte)(F | 0b10000) : (byte)(F & 0b11101111);
    }

    internal void Reset()
    {
        A = 0;
        F = 0;
        B = 0;
        C = 0;
        D = 0;
        E = 0;
        H = 0;
        L = 0;

        SP = 0;
        PC = 0;
    }
}
