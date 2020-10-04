using OpenTK.Windowing.Desktop;

using System;
using System.IO;

namespace Monoboy.Application
{
    public static class Program
    {
        public static void Main()
        {
            if (Directory.Exists("Roms") == false)
            {
                Directory.CreateDirectory("Roms");
            }
            if (Directory.Exists("Saves") == false)
            {
                Directory.CreateDirectory("Saves");
            }

            NativeWindowSettings settings = new NativeWindowSettings
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(3, 3),
                Size = new OpenTK.Mathematics.Vector2i(1280, 720)
            };

            new Application(GameWindowSettings.Default, settings).Run();
        }
    }
}