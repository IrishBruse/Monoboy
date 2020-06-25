using System;

namespace Utility
{
    public static class Extensions
    {
        public static string ToHex(this byte data)
        {
            return "0x" + data.ToString("X2").ToUpper();
        }

        public static string ToHex(this ushort data)
        {
            return "0x" + data.ToString("X4").ToUpper();
        }

        public static string ToBin(this byte data)
        {
            if (data == 0)
            {
                return "0b_00000000";
            }
            return "0b_" + Convert.ToString(data, 2).ToUpper();
        }

        public static string ToBin(this ushort data)
        {
            if (data == 0)
            {
                return "0b_0000000000000000";
            }
            return "0b_" + Convert.ToString(data, 2).ToUpper();
        }

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
        /// Combines the bytes into a ushort
        /// </summary>
        /// <param name="high">The higher byte to combine</param>
        /// <returns></returns>
        public static ushort ToShort(this byte low, byte high)
        {
            return (ushort)((high << 8) | low);
        }
    }
}
