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
        Text text;

        public DrawingSurface(RenderWindow window)
        {
            this.window = window;
            font = new Font("Data/Consolas.ttf");
            text = new Text
            {
                FillColor = Color.Black,
                Font = font
            };
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
            this.text.DisplayedString = text;
            this.text.Position = position + new Vector2f(4, -8);
            window.Draw(this.text);
        }

        public void DrawString(string text)
        {
            DrawString(text, new Vector2f(0, 0));
        }
    }
}
