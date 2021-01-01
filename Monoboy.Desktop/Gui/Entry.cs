using System;
using System.IO;
using System.Reflection;

using BmpSharp;

using OpenTK.Windowing.Desktop;

namespace Monoboy.Gui
{
    public static class Entry
    {
        public static void Main()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Monoboy.Desktop.Icon.bmp";

            using var s = assembly.GetManifestResourceStream(resourceName);
            using var r = new BinaryReader(s);
            using var fs = new FileStream("Icon.bmp", FileMode.Create, FileAccess.ReadWrite);
            using BinaryWriter w = new BinaryWriter(fs);
            w.Write(r.ReadBytes((int)s.Length));

            fs.Close();
            w.Close();

            Bitmap bmp = BitmapFileHelper.ReadFileAsBitmap("Icon.bmp", true);
            OpenTK.Windowing.Common.Input.Image test = new OpenTK.Windowing.Common.Input.Image(32, 32, bmp.PixelDataFliped);

            File.Delete("Icon.bmp");

            NativeWindowSettings settings = new NativeWindowSettings
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(3, 3),
                Size = new OpenTK.Mathematics.Vector2i(1280, 720),
                Icon = new OpenTK.Windowing.Common.Input.WindowIcon(test)
            };

            new Application(GameWindowSettings.Default, settings).Run();
        }
    }
}