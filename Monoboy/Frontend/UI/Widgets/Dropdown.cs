using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace Monoboy.Frontend.UI.Widgets
{
    public class Dropdown : Button
    {
        List<Widget> children = new List<Widget>();

        public Rectangle dropdown;

        bool opened;

        public Dropdown(string name) : base(name)
        {
            dropdown = new Rectangle(rectangle.X, rectangle.Y + rectangle.Height, 200, 0, 1);

            OutlineSize = 1;

            dropdown.FillColor = new Color(0xF2F2F2FF);
            dropdown.OutlineColor = new Color(0xCCCCCCFF);

            OnHoverColor = new ColorPair(0xE5F3FFFF, 0xC2DEF5FF);
            OnExitColor = new ColorPair(0xFFFFFFFF, 0xFFFFFFFF);
            OnPressedColor = new ColorPair(0xCCE8FFFF, 0x99D1FFFF);
            OnReleasedColor = new ColorPair(0xE5F3FFFF, 0xC2DEF5FF);
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if(opened == true)
            {
                target.Draw(dropdown);

                for(int i = 0; i < children.Count; i++)
                {
                    target.Draw(children[i], states);
                }
            }

            base.Draw(target, states);
        }

        public void Add(Widget widget)
        {
            // Calculate y position based on already added widgets
            widget.X = dropdown.X + 2;
            widget.Y = dropdown.Y + 2 + (children.Count > 0 ? children[^1].Y + 4 : 0);
            widget.Width = dropdown.Width - 4;

            widget.TextX = 32;
            widget.TextY = 0;

            widget.OnHoverColor = new ColorPair(0x91C9F7FF);
            widget.OnExitColor = new ColorPair(0xF2F2F2FF);
            widget.OnPressedColor = new ColorPair(0x91C9F7FF);
            widget.OnReleasedColor = new ColorPair(0x91C9F7FF);

            widget.Clicked += () => opened = false;

            FloatRect textRect = widget.text.GetLocalBounds();

            widget.text.Origin = new Vector2f(textRect.Left, textRect.Top + (textRect.Height / 2.0f));

            dropdown.Height += widget.Height + 4;

            children.Add(widget);
        }

        public override void OnPressed()
        {
            opened = !opened;

            base.OnPressed();
        }

        public override void OnUnfocused()
        {
            opened = false;

            base.OnUnfocused();
        }

        public override bool ContainsMouse(int x, int y)
        {
            return base.ContainsMouse(x, y);
        }

        public override Widget[] GetChildren()
        {
            return children.ToArray();
        }
    }
}