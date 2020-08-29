using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Monoboy.Frontend.UI
{
    public class Gui
    {
        private RenderWindow window;
        private List<Widget> widgets = new List<Widget>();

        Widget focusedWidget;

        Widget oldWidget;
        bool oldButtonPressed;

        public Gui(RenderWindow window)
        {
            this.window = window;
        }

        public void Add(Widget widget)
        {
            widgets.Add(widget);
        }

        public void Draw()
        {
            Vector2i mousePos = Mouse.GetPosition(window);
            bool buttonPressed = Mouse.IsButtonPressed(Mouse.Button.Left);

            Widget widget = GetWidgetUnderMouse(mousePos.X, mousePos.Y);

            if(widget != oldWidget)
            {
                widget?.OnEnter();
                oldWidget?.OnExit();
            }

            if(buttonPressed == true && oldButtonPressed == false)
            {
                widget?.OnPressed();
                widget?.OnFocused();
                if(widget == null)
                {
                    focusedWidget?.OnUnfocused();
                }
                focusedWidget = widget;
            }
            if(buttonPressed == false && oldButtonPressed == true)
            {
                widget?.OnReleased();
            }

            for(int i = 0; i < widgets.Count; i++)
            {
                window.Draw(widgets[i]);
            }

            oldButtonPressed = buttonPressed;
            oldWidget = widget;
        }

        Widget GetWidgetUnderMouse(int x, int y)
        {
            foreach(Widget widget in widgets)
            {
                foreach(Widget child in widget.GetChildren())
                {
                    if(child.ContainsMouse(x, y) == true)
                    {
                        return child;
                    }
                }

                if(widget.ContainsMouse(x, y) == true)
                {
                    return widget;
                }
            }

            return null;
        }
    }
}
