using Monoboy.Core;
using Monoboy.Frontend;
using SFML.Graphics;
using SFML.System;

namespace Monoboy
{
    public class Application
    {
        readonly Window window;

        const int CyclesPerFrame = 69905;
        Emulator emulator;
        Texture screen;
        Sprite screenSprite;

        public Application()
        {
            window = new Window("Monoboy", 640, 480);

            emulator = new Emulator();
            emulator.LoadRom("Roms/Tetris.gb");

            screen = new Texture(Emulator.WindowWidth, Emulator.WindowHeight);

            screenSprite = new Sprite(screen);
            screenSprite.Scale = new Vector2f(2, 2);
            screenSprite.Position = new Vector2f((window.Width / 2) - ((screenSprite.Scale.X * Emulator.WindowWidth) / 2), 0);

            window.Update += Update;
            window.Draw += Draw;
            window.Resize += Resize;

            window.Loop();
        }

        private void Update()
        {
            int cycles = 0;
            while(cycles < CyclesPerFrame)
            {
                cycles += emulator.Step();
            }
        }

        private void Draw(DrawingSurface surface)
        {
            surface.Clear(Color.White);

            screen.Update(emulator.bus.gpu.Framebuffer);

            surface.Draw(screenSprite);
        }

        private void Resize(Vector2u windowSize)
        {
            int scale = (int)(window.Height / Emulator.WindowHeight);
            screenSprite.Scale = new Vector2f(scale, scale);

            screenSprite.Position = new Vector2f((windowSize.X / 2) - ((screenSprite.Scale.X * Emulator.WindowWidth) / 2), 0);
        }
    }
}