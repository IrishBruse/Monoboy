using System;
using System.IO;
using System.Reflection;

using Monoboy.Gui;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;

namespace Monoboy
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string resourceName = "Monoboy.Desktop.Icon.bmp";
            var icon = LoadIcon(resourceName);

            NativeWindowSettings settings = new NativeWindowSettings
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(3, 3),
                Size = new Vector2i(1280, 720),
                Icon = new WindowIcon(icon)
            };

            new Application(GameWindowSettings.Default, settings, args).Run();
        }

        static Image LoadIcon(string path)
        {
            using BinaryReader br = new(Assembly.GetEntryAssembly().GetManifestResourceStream(path));
            byte[] data = br.ReadBytes((int)br.BaseStream.Length);

            var width = BitConverter.ToInt32(data, 0x12);
            var height = BitConverter.ToInt32(data, 0x16);

            byte[] fixedPixels = new byte[width * height * 4];

            int pixelDataOffset = BitConverter.ToInt32(data, 0x0A);

            // Flip image and fix channel order
            for (int i = 0; i < fixedPixels.Length / 4; i++)
            {
                int x = i % width;
                int y = i / width;

                int flippedIndex = x + width * (height - 1 - y);

                fixedPixels[4 * flippedIndex + 0] = data[pixelDataOffset + (4 * i) + 2];
                fixedPixels[4 * flippedIndex + 1] = data[pixelDataOffset + (4 * i) + 1];
                fixedPixels[4 * flippedIndex + 2] = data[pixelDataOffset + (4 * i) + 0];
                fixedPixels[4 * flippedIndex + 3] = data[pixelDataOffset + (4 * i) + 3];
            }

            return new Image(width, height, fixedPixels);
        }
    }
}