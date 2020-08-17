using System;

namespace Monoboy.Utility
{
    public static class Extensions
    {
        #region Debugging

        public static string ToHex(this byte data)
        {
            return "0x" + data.ToString("x");
        }

        public static string ToHex(this ushort data)
        {
            return "0x" + data.ToString("x");
        }

        public static string ToBin(this byte data)
        {
            return "0b" + Convert.ToString(data, 2).PadLeft(8, '0');
        }

        public static string ToBin(this ushort data)
        {
            return "0b" + Convert.ToString(data, 2).PadLeft(16, '0');
        }

        #endregion

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
        /// Returns the swapped byte
        /// </summary>
        public static byte Swap(this byte data)
        {
            byte high = (byte)(data >> 4);
            byte low = (byte)(data << 4);
            return (byte)(low | high);
        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="bit">The bit to set</param>
        /// <returns></returns>
        public static byte SetBit(this byte data, byte bit, bool condition)
        {
            if(condition == true)
            {
                return data |= (byte)(1 << bit);
            }
            else
            {
                return data &= (byte)~(1 << bit);
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
    }
}
