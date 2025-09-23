namespace Monoboy.Disassembler;

public class Program
{
    public static void Main()
    {
        var dis = new Dis();
        dis.Decompile("../roms/Tetris.gb");
    }
}
