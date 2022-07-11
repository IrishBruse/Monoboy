namespace Monoboy;

using System;

public class Debug
{
    public static bool Enabled { get; set; }
    public static void Log(string message)
    {
        if (!Enabled)
        {
            return;
        }

        Console.WriteLine(message);
    }
}
