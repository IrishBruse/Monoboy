using SFML.Graphics;
using SFML.System;

namespace Monoboy.Frontend.UI
{
    public class Rectangle : Drawable
    {
        private FloatRect bounds;
        private RectangleShape shape = new RectangleShape();

        public float X
        {
            get => bounds.Left;
            set
            {
                bounds.Left = value;
                shape.Position = new Vector2f(value + shape.OutlineThickness, shape.Position.Y);
            }
        }
        public float Y
        {
            get => bounds.Top;
            set
            {
                bounds.Top = value;
                shape.Position = new Vector2f(shape.Position.X, value + shape.OutlineThickness);
            }
        }
        public float Width
        {
            get => bounds.Width;
            set
            {
                bounds.Width = value;
                shape.Size = new Vector2f(value - (shape.OutlineThickness * 2), shape.Size.Y);
            }
        }
        public float Height
        {
            get => bounds.Height;
            set
            {
                bounds.Height = value;
                shape.Size = new Vector2f(shape.Size.X, value - (shape.OutlineThickness * 2));
            }
        }

        public float OutlineThickness
        {
            get => shape.OutlineThickness;
            set
            {
                shape.OutlineThickness = value;
                shape.Position += new Vector2f(value, value);
                shape.Size = new Vector2f(shape.Size.X - (value * 2), shape.Size.Y - (value * 2));
            }
        }

        public float CenterX
        {
            get => bounds.Left + (bounds.Width / 2);
        }
        public float CenterY
        {
            get => bounds.Top + (bounds.Height / 2);
        }

        public Color FillColor { get => shape.FillColor; set => shape.FillColor = value; }
        public Color OutlineColor { get => shape.OutlineColor; set => shape.OutlineColor = value; }

        public Rectangle(float x, float y, float width, float height, float outline = 0)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            OutlineThickness = outline;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(shape);
        }

        public bool Contains(int x, int y)
        {
            return bounds.Contains(x, y);
        }
    }
}
