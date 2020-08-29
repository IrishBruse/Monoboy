namespace Monoboy.Utility
{
    public static class Helper
    {
        /// <summary>
        /// Combines the bytes into a ushort
        /// </summary>
        /// <param name="low">The lower byte to combine</param>
        /// <param name="high">The higher byte to combine</param>
        /// <returns></returns>
        public static ushort Combine(byte low, byte high)
        {
            return (ushort)(high << 8 | low);
        }
    }
}