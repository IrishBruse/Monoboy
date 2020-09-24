using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using Image = OpenTK.Windowing.Common.Input.Image;

namespace Monoboy.Application
{
    public static class Program
    {
        public static void Main()
        {
            if(Directory.Exists("Roms") == false)
            {
                Directory.CreateDirectory("Roms");
            }
            if(Directory.Exists("Saves") == false)
            {
                Directory.CreateDirectory("Saves");
            }

            NativeWindowSettings settings = new NativeWindowSettings
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(4, 5),
                Size = new OpenTK.Mathematics.Vector2i(1280, 720)
            };

            new Application(GameWindowSettings.Default, settings).Run();
        }
    }
}