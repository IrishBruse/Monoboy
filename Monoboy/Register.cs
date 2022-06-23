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
        get
        {
            return (ushort)((A << 8) | F);
        }

        set
        {
            A = value.High();
            F = (byte)(value.Low() & 0xF0);
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

    public void SetFlag(byte flag, bool condition)
    {
        if (condition)
        {
            F |= flag;
        }
        else
        {
            F &= (byte)~flag;
        }
    }

    public bool GetFlag(byte flag)
    {
        return (F & flag) != 0;
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