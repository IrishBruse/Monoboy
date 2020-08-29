using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Monoboy.Frontend.UI;
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

        public Action<Vector2u> Resize;
        public Action<DrawingSurface> Draw;
        public Action Update;

        public bool Focused = true;
        public RenderWindow window;
        public DrawingSurface surface;

        public Gui gui;

        private bool firstLoop = true;
        private Clock clock;

        public Window(string title, uint width, uint height)
        {
            window = new RenderWindow(new VideoMode(width, height), title);
            gui = new Gui(window);

            // Event handling
            window.Closed += (sender, e) => Quit();

            window.Resized += (a, e) =>
            {
                Resize.Invoke(new Vector2u(e.Width, e.Height));
                window.SetView(new View(new FloatRect(0, 0, e.Width, e.Height)));
            };

            window.LostFocus += (sender, e) => Focused = false;
            window.GainedFocus += (sender, e) => Focused = true;
            window.RequestFocus();

            Image img = new Image("Data/Icon.png");
            window.SetIcon(img.Size.X, img.Size.Y, img.Pixels);

            clock = new Clock();
            surface = new DrawingSurface(window);
        }

        public void Loop()
        {
            if(firstLoop == true)
            {
                firstLoop = false;
                Resize?.Invoke(new Vector2u(Width, Height));
            }

            clock.Restart();

            window.DispatchEvents();

            Update?.Invoke();
            Draw?.Invoke(surface);

            gui.Draw();

            long elapsed = clock.ElapsedTime.AsMicroseconds();

            if(elapsed < 16000)
            {
                long sleepTime = 16000 - elapsed;

                Thread.Sleep(new TimeSpan(sleepTime));
            }

            window.Display();
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