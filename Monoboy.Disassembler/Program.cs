namespace Monoboy.Disassembler;

public class Program
{
    public static void Main(string[] args)
    {
        var dis = new Dis();
        dis.Decompile("../roms/Tetris.gb");
    }
}
