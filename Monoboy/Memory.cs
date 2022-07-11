namespace Monoboy;

using System;

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

    public static implicit operator byte[](Memory rhs)
    {
        return rhs.data;
    }

    public void Reset()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0;
        }
    }
}
