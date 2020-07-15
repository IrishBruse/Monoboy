using System;
using System.Collections.Generic;
using SFML.Graphics;

namespace Monoboy.Frontend
{
    public class DrawingSurface
    {
        private readonly RenderWindow window;

        public DrawingSurface(RenderWindow window)
        {
            this.window = window;
        }

        public void Clear(Color color)
        {
            window.Clear(new Color(color.R, color.G, color.B, color.A));
        }

        public void Draw(Sprite sprite)
        {
            window.Draw(sprite);
        }
    }
}
