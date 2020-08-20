using System;
using System.Drawing;
using System.Threading;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Monoboy.Frontend
{
    public class Window
    {
        public uint Width { get => window.Size.X; }
        public uint Height { get => window.Size.Y; }
        public Vector2i Position { get => window.Position; set => window.Position = value; }
        public bool Open { get => window.IsOpen; }

        public delegate void UpdateEvent();
        public event UpdateEvent Update;

        public delegate void DrawEvent(DrawingSurface surface);
        public event DrawEvent Draw;

        public delegate void ResizeEvent(Vector2u windowSize);
        public event ResizeEvent Resize;

        public bool Focused = true;

        private RenderWindow window;

        Clock clock;
        public DrawingSurface surface;

        public Window(string title, uint width, uint height)
        {
            window = new RenderWindow(new VideoMode(width, height), title);

            // Event handling
            window.Closed += (sender, e) => Quit();

            window.Resized += WindowResize;

            window.LostFocus += (sender, e) => Focused = false;
            window.GainedFocus += (sender, e) => Focused = true;
            window.RequestFocus();

            Bitmap bitmap = new Bitmap("Data/Icon.png");

            byte[] pixels = new byte[bitmap.Width * bitmap.Width * 4];

            for(int x = 0; x < bitmap.Width; x++)
                for(int y = 0; y < bitmap.Height; y++)
                {
                    int index = ((y * bitmap.Width) + x) * 4;
                    pixels[index] = bitmap.GetPixel(x, y).R;
                    pixels[index + 1] = bitmap.GetPixel(x, y).G;
                    pixels[index + 2] = bitmap.GetPixel(x, y).B;
                    pixels[index + 3] = bitmap.GetPixel(x, y).A;
                }

            window.SetIcon((uint)bitmap.Width, (uint)bitmap.Width, pixels);


            clock = new Clock();
            surface = new DrawingSurface(window);
        }

        public void Loop()
        {
            clock.Restart();

            window.DispatchEvents();

            if(Update != null)
            {
                Update.Invoke();
            }

            if(Draw != null)
            {
                Draw.Invoke(surface);
            }

            long elapsed = clock.ElapsedTime.AsMicroseconds();

            if(elapsed < 16000)
            {
                long sleepTime = 16000 - elapsed;

                Thread.Sleep(new TimeSpan(sleepTime));
            }

            window.Display();
        }

        private void WindowResize(object sender, SizeEventArgs e)
        {
            Vector2u size = window.Size;

            int scale = (int)(Height / 144);

            if(size.X < 160 * scale)
            {
                size.X = (uint)(160 * scale);
            }
            if(size.Y < 144)
            {
                size.Y = 144;
            }

            window.Size = size;

            window.SetView(new View(new FloatRect(0, 0, e.Width, e.Height)));
            if(Resize != null)
            {
                Resize.Invoke(new Vector2u(e.Width, e.Height));
            }
        }

        public void SetTitle(string title)
        {
            window.SetTitle(title);
        }

        public void Quit()
        {
            window.Close();
        }
    }
}