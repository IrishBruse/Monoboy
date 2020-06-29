using System;

namespace Monoboy.Emulator.Utility
{
    public static class Extensions
    {
        #region Debugging

        public static string ToHex(this byte data)
        {
            return "0x" + data.ToString("X2").PadLeft(2, '0').ToUpper();
        }

        public static string ToHex(this ushort data)
        {
            return "0x" + data.ToString("X4").PadLeft(4, '0').ToUpper();
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
            return (byte)((data & 0x0F) << 4 | (data & 0xF0) >> 4);
        }

        /// <summary>
        /// Combines the bytes into a ushort
        /// </summary>
        /// <param name="high">The higher byte to combine</param>
        /// <returns></returns>
        public static ushort ToShort(this byte low, byte high)
        {

            return (ushort)((((ushort)high) << 8) | (ushort)low);
        }
    }
}
