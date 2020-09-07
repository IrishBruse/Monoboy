using System;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using ImGuiOpenTK;
using Monoboy.Constants;
using Monoboy.Utility;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;

namespace Monoboy.Application
{
    public class Application : Window
    {
        Texture framebufferTexture;

        Emulator emulator;

        public static bool BackgroundEnabled = true;
        public static bool WindowEnabled = true;
        public static bool SpritesEnabled = true;

        public Application()
        {
            emulator = new Emulator();

            byte[] testArray = new byte[Constant.WindowWidth * Constant.WindowHeight * 4];
            Random rng = new Random();

            for(int i = 0; i < testArray.Length; i++)
            {
                testArray[i] = (byte)rng.Next(0, 255);
            }

            IntPtr dataPRT = GCHandle.Alloc(testArray, GCHandleType.Pinned).AddrOfPinnedObject();

            framebufferTexture = new Texture("FrameBuffer", Constant.WindowWidth, Constant.WindowHeight, dataPRT);// emulator.bus.gpu.framebuffer.pixels
            //framebufferTexture.SetMagFilter(TextureMagFilter.Nearest);

            //emulator.bus.gpu.DrawFrame += () => DrawFrame();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            UpdateJoypad();

            if(emulator.paused == false)
            {
                int cycles = 0;
                while(cycles < Constant.CyclesPerFrame)
                {
                    cycles += emulator.Step();
                }
            }
        }

        void UpdateJoypad()
        {
            emulator.bus.joypad.SetButton(Joypad.Button.A, KeyboardState.IsKeyDown(Key.Space));
            emulator.bus.joypad.SetButton(Joypad.Button.B, KeyboardState.IsKeyDown(Key.ShiftLeft));
            emulator.bus.joypad.SetButton(Joypad.Button.Select, KeyboardState.IsKeyDown(Key.Escape));
            emulator.bus.joypad.SetButton(Joypad.Button.Start, KeyboardState.IsKeyDown(Key.Enter));
            emulator.bus.joypad.SetButton(Joypad.Button.Up, KeyboardState.IsKeyDown(Key.W));
            emulator.bus.joypad.SetButton(Joypad.Button.Down, KeyboardState.IsKeyDown(Key.S));
            emulator.bus.joypad.SetButton(Joypad.Button.Left, KeyboardState.IsKeyDown(Key.A));
            emulator.bus.joypad.SetButton(Joypad.Button.Right, KeyboardState.IsKeyDown(Key.D));
        }

        void DrawFrame()
        {
            //framebufferTexture.Update(emulator.bus.gpu.framebuffer.pixels);
        }

        public override void ImGuiRender()
        {
            ImGui.SetNextWindowPos(new Vector2(-1, 0), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(Size.X + 2, Size.Y), ImGuiCond.Always);

            //ImGui.ShowDemoWindow();

            ImGuiStylePtr style = ImGui.GetStyle();
            style.WindowRounding = 0;
            style.FrameRounding = 0;

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.MenuBar;
            windowFlags |= ImGuiWindowFlags.NoTitleBar;
            windowFlags |= ImGuiWindowFlags.NoResize;
            windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus;
            windowFlags |= ImGuiWindowFlags.AlwaysAutoResize;

            if(ImGui.Begin("Monoboy", windowFlags) == false)
            {
                ImGui.End();
                return;
            }

            // Menubar
            if(ImGui.BeginMenuBar())
            {
                if(ImGui.BeginMenu("File"))
                {
                    if(ImGui.MenuItem("Open", "Ctrl+O"))
                    {
                        string rom = TinyFileDialog.OpenFileDialog("Open Rom", "", new string[] { "*.gb", "*.gbc" }, "Rom (.gb,.gbc)", false);

                        if(string.IsNullOrEmpty(rom) == false)
                        {
                            emulator.Open(rom);
                        }
                    }
                    if(ImGui.MenuItem("Quit", "Alt+F4"))
                    {
                        Close();
                    }
                    ImGui.EndMenu();
                }

                if(ImGui.BeginMenu("Debug"))
                {
                    if(ImGui.MenuItem("Background Toggle", null, ref BackgroundEnabled)) { }
                    if(ImGui.MenuItem("Window Toggle", null, ref WindowEnabled)) { }
                    if(ImGui.MenuItem("Sprites Toggle", null, ref SpritesEnabled)) { }
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            // Render the Emulator
            ImGui.BeginChild("Emulation Renderer");
            {
                Vector2 size = ImGui.GetWindowSize();
                int scale = (int)(size.Y / Constant.WindowHeight);

                //ImGui.SetCursorPos((size - (framebufferTexture.Size * scale)) * 0.5f);
                ImGui.Image((IntPtr)framebufferTexture.GLTexture, new Vector2(Constant.WindowWidth, Constant.WindowHeight));
            }
            ImGui.EndChild();

            ImGui.End();
        }
    }
}