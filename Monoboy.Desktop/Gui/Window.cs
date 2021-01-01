using ImGuiNET.OpenTK;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Monoboy.Gui
{
    public class Window : GameWindow
    {
        private ImGuiController imGuiController;

        public Window(GameWindowSettings gameWindow, NativeWindowSettings nativeWindow) : base(gameWindow, nativeWindow) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            Title = "Monoboy";

            imGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            Vector2i windowSize = ClientSize;

            if(windowSize.X < Emulator.WindowWidth)
            {
                windowSize.X = Emulator.WindowWidth;
            }

            if(windowSize.Y < Emulator.WindowHeight)
            {
                windowSize.Y = Emulator.WindowHeight;
            }

            // Update the opengl viewport
            GL.Viewport(0, 0, windowSize.X, windowSize.Y);

            // Tell ImGui of the new size
            imGuiController.WindowResized(windowSize.X, windowSize.Y);

            Size = windowSize;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            imGuiController.Update(this, (float)e.Time);

            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            DrawImGui();

            imGuiController.Render();

            Util.CheckGLError("End of frame");

            SwapBuffers();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            imGuiController.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            imGuiController.MouseScroll(e.Offset);
        }
        public virtual void DrawImGui()
        {

        }
    }
}
