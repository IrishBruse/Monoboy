using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace Monoboy.Frontend.UI
{
    public class Button : Widget
    {
        public Button(string name)
        {
            text.DisplayedString = name;
            text.FillColor = Color.Black;

            FloatRect textRect = text.GetLocalBounds();
            rectangle = new Rectangle(0, 0, textRect.Width + 8, textRect.Height + 8);

            text.Origin = new Vector2f(textRect.Left + (textRect.Width / 2.0f), textRect.Top + (textRect.Height / 2.0f));
            text.Position = new Vector2f(rectangle.CenterX, rectangle.CenterY);

            OnHoverColor = new ColorPair(0xE5F3FFFF, 0xC2DEF5FF);
            OnExitColor = new ColorPair(0xFFFFFFFF, 0xFFFFFFFF);
            OnPressedColor = new ColorPair(0xCCE8FFFF, 0x99D1FFFF);
            OnReleasedColor = new ColorPair(0xE5F3FFFF, 0xC2DEF5FF);
        }

        public override bool ContainsMouse(int x, int y)
        {
            return rectangle.Contains(x, y);
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(rectangle);
            target.Draw(text);
        }

        public override void OnEnter()
        {
            rectangle.FillColor = new Color(OnHoverColor.FillColor);
            rectangle.OutlineColor = new Color(OnHoverColor.OutlineColor);
        }

        public override void OnExit()
        {
            rectangle.FillColor = new Color(OnExitColor.FillColor);
            rectangle.OutlineColor = new Color(OnExitColor.OutlineColor);
        }

        public override void OnPressed()
        {
            Clicked?.Invoke();
            rectangle.FillColor = new Color(OnPressedColor.FillColor);
            rectangle.OutlineColor = new Color(OnPressedColor.OutlineColor);
        }

        public override void OnReleased()
        {
            rectangle.FillColor = new Color(OnReleasedColor.FillColor);
            rectangle.OutlineColor = new Color(OnReleasedColor.OutlineColor);
        }
    }
}