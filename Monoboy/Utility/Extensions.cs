﻿namespace Monoboy.Utility;

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
            return (byte)(data | bit);
        }
        else
        {
            return (byte)(data & (byte)~bit);
        }
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

}
