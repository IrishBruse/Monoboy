namespace Monoboy.Wasm;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;

public partial class Program
{
    static Stopwatch timer = new();
    static Emulator emulator;

    public static void Main()
    {
        Console.WriteLine("Wasm C# main");
        emulator = new Emulator(Boot.Bootix);
        emulator.StepFrame();
    }

    [JSExport()]
    [return: JSMarshalAs<JSType.MemoryView>()]
    public static Span<byte> GetFramebuffer()
    {
        return emulator.Framebuffer.AsSpan();
    }

    [JSExport()]
    public static void OpenFile(byte[] data)
    {
        emulator.Open(data);
    }

    [JSExport()]
    public static void RunFrame()
    {
        emulator.StepFrame();
    }

    [JSExport()]
    public static void SetButtonState(int button, bool state)
    {
        emulator.SetButtonState((GameboyButton)button, state);
    }
}
