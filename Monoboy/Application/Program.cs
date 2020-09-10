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
            if(File.Exists("Config.ini") == false)
            {
                using StreamWriter stream = File.CreateText("Config.ini");
                stream.WriteLine("# WIP ");
                stream.WriteLine("# Keybinds");
                stream.WriteLine("A:      Space");
                stream.WriteLine("B:      ShiftLeft");
                stream.WriteLine("Select: Escape");
                stream.WriteLine("Start:  Enter");
                stream.WriteLine("Up:     W");
                stream.WriteLine("Down:   A");
                stream.WriteLine("Left:   S");
                stream.WriteLine("Right:  D");
                stream.WriteLine("# Gameboy PalletSwaps");
                stream.WriteLine("0xD0D058 0xA0A840 0x708028 0x405010 : Default");
                stream.WriteLine("0xD0D058 0xA0A840 0x708028 0x405010 : Test");
                stream.Close();
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