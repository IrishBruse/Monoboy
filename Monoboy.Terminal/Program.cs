namespace Monoboy.Terminal;

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

public static class Program
{
    static Emulator emulator = new();
    static short[] lut = { 0, 8, 7, 15 };

    public static void Main(string[] args)
    {
        ConfigConsole();

        if (args.Length == 1 && File.Exists(args[0]))
        {
            emulator.Open(args[0]);
        }

        emulator.Open("C:\\Users\\Econn\\Desktop\\Tetris.gb");

        emulator.ppu.DrawFrame += () => Render();

        while (true)
        {
            int cycles = 0;
            while (cycles < Emulator.CyclesPerFrame)
            {
                cycles += emulator.Step();
            }
        }
    }

    static void Render()
    {
        while (true)
        {
            for (int x = 0; x < Emulator.WindowWidth; x++)
            {
                for (int y = 0; y < Emulator.WindowHeight; y++)
                {
                    int color = emulator.ppu.framebuffer[x, y];
                    int num = (y * Emulator.WindowWidth) + x;
                    buf[num].Attributes = (short)(16 * lut[color]);
                    buf[num].Char.AsciiChar = (byte)' ';
                }
            }

            _ = ConsoleHelper.WriteConsoleOutputW(h, buf, new Coord() { X = Emulator.WindowWidth, Y = Emulator.WindowHeight }, new Coord() { X = 0, Y = 0 }, ref rect);
        }
    }
    static SafeFileHandle h;
    static CharInfo[] buf;
    static SmallRect rect;
    static void ConfigConsole()
    {
#pragma warning disable
        h = ConsoleHelper.CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);


        buf = new CharInfo[Emulator.WindowWidth * Emulator.WindowHeight];
        rect = new() { Left = 0, Top = 0, Right = Emulator.WindowWidth, Bottom = Emulator.WindowHeight };

        Console.CursorVisible = false;
        // Instantiating CONSOLE_FONT_INFO_EX and setting its size (the function will fail otherwise)
        CONSOLE_FONT_INFO_EX ConsoleFontInfo = new();
        ConsoleFontInfo.cbSize = (uint)Marshal.SizeOf(ConsoleFontInfo);

        // Optional, implementing this will keep the fontweight and fontsize from changing
        // See notes
        // GetCurrentConsoleFontEx(GetStdHandle(StdHandle.OutputHandle), false, ref ConsoleFontInfo);
        // ConsoleFontInfo.FaceName = "Terminal";
        ConsoleFontInfo.dwFontSize.X = 4;
        ConsoleFontInfo.dwFontSize.Y = 6;
        try
        {
            Console.SetBufferSize(Emulator.WindowWidth, Emulator.WindowHeight);
            Console.SetWindowSize(Emulator.WindowWidth, Emulator.WindowHeight);
        }
        catch (System.Exception)
        {

        }

        ConsoleHelper.SetCurrentConsoleFontEx(ConsoleHelper.GetStdHandle(StdHandle.OutputHandle), false, ref ConsoleFontInfo);
#pragma warning restore
    }

}
