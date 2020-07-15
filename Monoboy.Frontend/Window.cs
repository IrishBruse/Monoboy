using System;
using System.Threading;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Monoboy.Frontend
{
    public class Window
    {
        static float FrameTime = 1f / 60f;

        public uint Width { get => window.Size.X; }
        public uint Height { get => window.Size.Y; }

        public delegate void UpdateEvent();
        public event UpdateEvent Update;

        public delegate void DrawEvent(DrawingSurface surface);
        public event DrawEvent Draw;

        public delegate void ResizeEvent(Vector2u windowSize);
        public event ResizeEvent Resize;

        private readonly RenderWindow window;

        public Window(string title, uint width, uint height)
        {
            window = new RenderWindow(new VideoMode(width, height), title);

            // Event handling
            window.Closed += (e, y) => Quit();
            window.KeyPressed += KeyPressed;
            window.KeyReleased += KeyReleased;
            window.Resized += WindowResize;
        }

        public void Loop()
        {
            Clock clock = new Clock();
            DrawingSurface surface = new DrawingSurface(window);

            while(window.IsOpen)
            {
                clock.Restart();

                window.DispatchEvents();

                Update.Invoke();
                Draw.Invoke(surface);

                long elapsed = clock.ElapsedTime.AsMicroseconds();

                if(elapsed < 16000)
                {
                    long sleepTime = 16000 - elapsed;

                    Thread.Sleep(new TimeSpan(sleepTime));
                }

                window.Display();
            }
        }

        #region Events

        private void KeyPressed(object sender, KeyEventArgs e)
        {

        }

        private void KeyReleased(object sender, KeyEventArgs e)
        {

        }

        private void WindowResize(object sender, SizeEventArgs e)
        {
            Vector2u size = window.Size;

            if(size.X < 160 * 2)
            {
                size.X = 160 * 2;
            }
            if(size.Y < 144 * 2)
            {
                size.Y = 144 * 2;
            }

            window.Size = size;

            window.SetView(new View(new FloatRect(0, 0, e.Width, e.Height)));
            Resize.Invoke(new Vector2u(e.Width, e.Height));
        }

        #endregion

        public void Quit()
        {
            window.Close();
        }

        public void SetWindowFPS(uint fps)
        {
            FrameTime = 1f / fps;
        }

        public void SetTitle(string title)
        {
            window.SetTitle(title);
        }
    }
}