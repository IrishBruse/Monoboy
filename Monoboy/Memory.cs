namespace Monoboy;

public class Memory
{
    public bool Test = false;

    public Memory(int size)
    {
        data = new byte[size];
    }

    byte[] data;

    public byte this[int i]
    {
        get { return data[i]; }
        set { data[i] = value; }
    }
}
