namespace Monoboy.Terminal;

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

public static class Program
{
    static Emulator emulator = new();
    static short[] lut = { 15, 7, 8, 0 };
    static SafeFileHandle h;
    static CharInfo[] buf;
    static SmallRect rect;

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
            Input();
            emulator.RunFrame();
        }
    }

    static void Input()
    {
        bool buttonA = false;
        bool buttonB = false;
        bool buttonSelect = false;
        bool buttonStart = false;
        bool buttonUp = false;
        bool buttonDown = false;
        bool buttonLeft = false;
        bool buttonRight = false;

        while (Console.KeyAvailable)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);

            buttonA |= key.Key == ConsoleKey.Spacebar;
            buttonB |= key.Modifiers == ConsoleModifiers.Shift;
            buttonSelect |= key.Key == ConsoleKey.Escape;
            buttonStart |= key.Key == ConsoleKey.Enter;
            buttonUp |= key.Key == ConsoleKey.W;
            buttonDown |= key.Key == ConsoleKey.S;
            buttonLeft |= key.Key == ConsoleKey.A;
            buttonRight |= key.Key == ConsoleKey.D;
        }

        emulator.joypad.SetButton(Joypad.Button.A, buttonA);
        emulator.joypad.SetButton(Joypad.Button.B, buttonB);
        emulator.joypad.SetButton(Joypad.Button.Select, buttonSelect);
        emulator.joypad.SetButton(Joypad.Button.Start, buttonStart);
        emulator.joypad.SetButton(Joypad.Button.Up, buttonUp);
        emulator.joypad.SetButton(Joypad.Button.Down, buttonDown);
        emulator.joypad.SetButton(Joypad.Button.Left, buttonLeft);
        emulator.joypad.SetButton(Joypad.Button.Right, buttonRight);
    }

    static void Render()
    {
        for (int x = 0; x < Emulator.WindowWidth; x++)
        {
            for (int y = 0; y < Emulator.WindowHeight; y++)
            {
                byte colorID = emulator.ppu.framebuffer[x, y];
                int num = (y * Emulator.WindowWidth * 2) + (x * 2);
                buf[num + 1].Attributes = buf[num].Attributes = (short)(16 * lut[colorID]);
                buf[num + 1].Char.AsciiChar = buf[num].Char.AsciiChar = (byte)' ';
            }
        }

        ResizeWindow();

        _ = ConsoleHelper.WriteConsoleOutputW(h, buf, new Coord() { X = Emulator.WindowWidth * 2, Y = Emulator.WindowHeight }, new Coord() { X = 0, Y = 0 }, ref rect);
    }

#pragma warning disable
    static void ConfigConsole()
    {
        h = ConsoleHelper.CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

        buf = new CharInfo[Emulator.WindowWidth * 2 * Emulator.WindowHeight];
        rect = new() { Left = 0, Top = 0, Right = Emulator.WindowWidth * 2, Bottom = Emulator.WindowHeight };

        Console.CursorVisible = false;
        // Instantiating CONSOLE_FONT_INFO_EX and setting its size (the function will fail otherwise)
        CONSOLE_FONT_INFO_EX ConsoleFontInfo = new();
        ConsoleFontInfo.cbSize = (uint)Marshal.SizeOf(ConsoleFontInfo);

        // Optional, implementing this will keep the fontweight and fontsize from changing
        // See notes
        // GetCurrentConsoleFontEx(GetStdHandle(StdHandle.OutputHandle), false, ref ConsoleFontInfo);
        ConsoleFontInfo.dwFontSize.X = 2;
        ConsoleFontInfo.dwFontSize.Y = 4;

        ResizeWindow();

        ConsoleHelper.SetCurrentConsoleFontEx(ConsoleHelper.GetStdHandle(StdHandle.OutputHandle), false, ref ConsoleFontInfo);
    }

    private static void ResizeWindow()
    {
        try
        {
            Console.SetWindowSize(Emulator.WindowWidth * 2, Emulator.WindowHeight);
            Console.SetBufferSize(Emulator.WindowWidth * 2, Emulator.WindowHeight);
        }
        catch (System.Exception) { }
    }
#pragma warning restore
}
