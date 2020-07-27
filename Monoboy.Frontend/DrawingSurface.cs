using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace Monoboy.Frontend
{
    public class DrawingSurface
    {
        private readonly RenderWindow window;

        Font font;

        public DrawingSurface(RenderWindow window)
        {
            this.window = window;
            font = new Font("Data/OpenSans.ttf");
        }

        public void Clear(Color color)
        {
            window.Clear(new Color(color.R, color.G, color.B, color.A));
        }

        public void Draw(Sprite sprite)
        {
            window.Draw(sprite);
        }

        public void DrawString(string text, Vector2f position)
        {
            Text textDrawing = new Text(text, font)
            {
                Position = position + new Vector2f(4, -8),
                FillColor = Color.Black,
            };

            window.Draw(textDrawing);
        }

        public void DrawString(string text)
        {
            DrawString(text, new Vector2f(0, 0));
        }
    }
}
