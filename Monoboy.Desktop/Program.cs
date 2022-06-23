namespace Monoboy.Desktop;

using System;

using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Windowing;

public class Program
{
    private static IWindow? window;
    private static Emulator emulator = new();

    public static void Main()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Monoboy";
        options.VSync = true;

        window = Window.Create(options);

        // Events
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;

        window.Run();
    }

    private static void OnLoad()
    {
        RawImage icon = new(32, 32, Icon.Data);
        window!.SetWindowIcon(ref icon);

        emulator.Open("./Roms/Tetris.gb");
    }

    private static void OnUpdate(double deltaTime)
    {
        Console.WriteLine(1 / deltaTime);

        emulator.Update();
    }

    private static void OnRender(double deltaTime)
    {

    }
}
