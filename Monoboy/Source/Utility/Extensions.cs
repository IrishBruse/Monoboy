using System;

namespace Monoboy.Utility
{
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

        /// <summary>
        /// Add and return bool if carry
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static (byte result, bool halfCarry, bool fullCarry) AddOverflow(this byte data, int value, int carry = 0)
        {
            int result = data + value;
            bool halfCarry = (data & 0xF) + (value & 0xF) + carry > 0xF || (data & 0xF) + (value & 0xF) + carry < 0;
            bool fullCarry = (data & 0xFF) + (value & 0xFF) + carry > 0xFF || (data & 0xFF) + (value & 0xFF) + carry < 0;
            return ((byte)result, halfCarry, fullCarry);
        }

        /// <summary>
        /// Add and return bool if carry
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static (ushort result, bool halfCarry, bool fullCarry) AddOverflow(this ushort data, int value, int carry = 0)
        {
            int result = data + value;
            bool halfCarry = (data & 0xFFF) + (value & 0xFFF) + carry > 0xFFF || (data & 0xFFF) + (value & 0xFFF) + carry < 0;
            bool fullCarry = (data & 0xFFFF) + (value & 0xFFFF) + carry > 0xFFFF || (data & 0xFFFF) + (value & 0xFFFF) + carry < 0;
            return ((ushort)result, halfCarry, fullCarry);
        }

    }
}