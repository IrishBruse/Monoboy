using System;

namespace Monoboy.Utility
{
    static class Debug
    {
        static bool enabled = true;
        static bool oldEnabled = true;
        static bool disableNext = false;

        public static void Log<T>(T log, ConsoleColor color = ConsoleColor.White)
        {
            if(disableNext == true)
            {
                disableNext = false;
                return;
            }

            if(enabled == true)
            {
                Console.ForegroundColor = color;
                System.Diagnostics.Debug.WriteLine(log);
                Console.ResetColor();
            }
        }

        public static void LogHex(byte hex, ConsoleColor color = ConsoleColor.White)
        {
            Log(hex.ToHex(), color);
        }

        public static void LogHex(ushort hex, ConsoleColor color = ConsoleColor.White)
        {
            Log(hex.ToHex(), color);
        }

        public static void Enable()
        {
            enabled = true;
        }

        public static void Disable()
        {
            oldEnabled = enabled;
            enabled = false;
        }

        public static void DisableNext()
        {
            disableNext = true;
        }

        public static void Restore()
        {
            enabled = oldEnabled;
        }
    }
}
