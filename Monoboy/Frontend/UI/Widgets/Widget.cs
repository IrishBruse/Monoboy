using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace Monoboy.Frontend.UI
{
    public abstract class Widget : Drawable
    {
        public float X
        {
            get => rectangle.X;
            set
            {
                rectangle.X = value;
            }
        }
        public float Y
        {
            get => rectangle.Y;
            set
            {
                rectangle.Y = value;
            }
        }
        public float Width
        {
            get => rectangle.Width;
            set
            {
                rectangle.Width = value;
            }
        }
        public float Height
        {
            get => rectangle.Height;
            set
            {
                rectangle.Height = value;
            }
        }

        public float TextX
        {
            get => text.Position.X;
            set
            {
                text.Position = new Vector2f(rectangle.X + value, text.Position.Y);
            }
        }

        public float TextY
        {
            get => text.Position.Y;
            set
            {
                text.Position = new Vector2f(text.Position.X, rectangle.CenterY + value);
            }
        }

        public float OutlineSize
        {
            get => rectangle.OutlineThickness;
            set
            {
                rectangle.OutlineThickness = value;
            }
        }

        public ColorPair OnExitColor;
        public ColorPair OnHoverColor;
        public ColorPair OnPressedColor;
        public ColorPair OnReleasedColor;

        public Text text = new Text("Error", new Font("Data/Arial.ttf"), 12);
        public Rectangle rectangle;
        public System.Action Clicked;

        public abstract void Draw(RenderTarget target, RenderStates states);
        public abstract bool ContainsMouse(int x, int y);
        public virtual Widget[] GetChildren() { return null; }
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnPressed() { }
        public virtual void OnReleased() { }
        public virtual void OnFocused() { }
        public virtual void OnUnfocused() { }

        public class ColorPair
        {
            public Color FillColor { get; set; }
            public Color OutlineColor { get; set; }

            public ColorPair(Color outlineColor, Color fillColor)
            {
                OutlineColor = outlineColor;
                FillColor = fillColor;
            }

            public ColorPair(uint outlineColor, uint fillColor)
            {
                OutlineColor = new Color(outlineColor);
                FillColor = new Color(fillColor);
            }

            public ColorPair(uint color)
            {
                OutlineColor = new Color(color);
                FillColor = new Color(color);
            }
        }
    }
}