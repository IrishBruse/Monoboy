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
    }
}
