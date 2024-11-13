namespace Monoboy;

public class Memory
{
    public Memory(int size)
    {
        data = new byte[size];
    }

    byte[] data;

    public byte this[int i]
    {
        get => data[i];
        set => data[i] = value;
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
