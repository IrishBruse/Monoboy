namespace Monoboy.Utility;

public static class Extensions
{
    /// <summary>
    /// Returns the higher byte
    /// </summary>
    public static byte High(this ushort data)
    {
        return (byte)(data >> 8);
    }

    /// <summary>
    /// Returns the lower byte
    /// </summary>
    public static byte Low(this ushort data)
    {
        return (byte)(data & 0xFF);
    }

    /// <summary>
    /// Set bit
    /// </summary>
    /// <param name="bit">The bit to set</param>
    /// <returns></returns>
    public static byte SetBit(this byte data, byte bit, bool condition)
    {
        if (condition)
        {
            return data |= bit;
        }
        else
        {
            return data &= (byte)~bit;
        }
    }

    /// <summary>
    /// Set bit
    /// </summary>
    /// <param name="bit">The bit to set</param>
    /// <returns></returns>
    public static byte SetBits(this byte data, byte mask, byte value)
    {
        byte untouched = (byte)(data & ~mask);
        return (byte)(untouched | (mask & value));
    }

    /// <summary>
    /// Get bit Value as bool
    /// </summary>
    /// <param name="bit">The bit returned</param>
    /// <returns></returns>
    public static bool GetBit(this byte data, byte bit)
    {
        return (data & bit) != 0;
    }

    /// <summary>
    /// Get bit Value as bool
    /// </summary>
    /// <param name="bit">The bit returned</param>
    /// <returns></returns>
    public static byte GetBits(this byte data, byte bits)
    {
        return (byte)(data & bits);
    }

    /// <summary>
    /// Combines the bytes into a ushort
    /// </summary>
    /// <param name="low">The lower byte to combine</param>
    /// <param name="high">The higher byte to combine</param>
    /// <returns></returns>
    public static ushort Combine(this byte low, byte high)
    {
        return (ushort)((high << 8) | low);
    }
}