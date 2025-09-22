namespace Monoboy;

using System.IO;

public class Program
{
    public static void Main(string[] args)
    {
        byte[] data = File.ReadAllBytes("../roms/Tetris.gb");

        foreach (var b in data)
        {
            Console.WriteLine(b);
        }
    }
}
