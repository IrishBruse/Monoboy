namespace Monoboy;

public class Memory(int size)
{
    byte[] data = new byte[size];

    public byte this[int i]
    {
        get => data[i];
        set => data[i] = value;
    }

    public static implicit operator byte[](Memory rhs)
    {
        return rhs.data;
    }

    public byte[] Range(int begin, int end)
    {
        return data[begin..end];
    }

    public void Reset()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0;
        }
    }
}
