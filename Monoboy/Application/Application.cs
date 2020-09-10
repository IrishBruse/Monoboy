using System;
using System.IO;
using System.Numerics;
using ImGuiNET;
using ImGuiOpenTK;
using Monoboy.Constants;
using Monoboy.Utility;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;

namespace Monoboy.Application
{
    public class Application : Window
    {
        Texture framebufferTexture;

        Framebuffer backgroundBuffer;
        Texture backgroundTexture;

        Framebuffer tilemapBuffer;
        Texture tilemapTexture;

        Emulator emulator;

        public static bool BackgroundEnabled = true;
        public static bool WindowEnabled = true;
        public static bool SpritesEnabled = true;

        bool backgroundWindow = false;
        bool tilemapWindow = false;

        string openRom;

        public Application(GameWindowSettings gameWindow, NativeWindowSettings nativeWindow) : base(gameWindow, nativeWindow)
        {

            emulator = new Emulator();

            // opentk 4.0 bug!
            MakeCurrent();

            framebufferTexture = new Texture("FrameBuffer", Constant.WindowWidth, Constant.WindowHeight, emulator.bus.gpu.framebuffer.Pixels);
            framebufferTexture.SetMagFilter(TextureMagFilter.Nearest);

            backgroundBuffer = new Framebuffer(256, 256);
            backgroundTexture = new Texture("Background", 256, 256, backgroundBuffer.Pixels);
            backgroundTexture.SetMagFilter(TextureMagFilter.Nearest);

            tilemapBuffer = new Framebuffer(128, 192);
            tilemapTexture = new Texture("Background", 128, 192, tilemapBuffer.Pixels);
            tilemapTexture.SetMagFilter(TextureMagFilter.Nearest);

            emulator.bus.gpu.DrawFrame += () => DrawFrame();
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
            framebufferTexture.Update(emulator.bus.gpu.framebuffer.Pixels);
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
                        openRom = TinyFileDialog.OpenFileDialog("Open Rom", "", new string[] { "*.gb", "*.gbc" }, "Rom (.gb,.gbc)", false);

                        if(string.IsNullOrEmpty(openRom) == false)
                        {
                            emulator.Open(openRom);
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

                if(ImGui.BeginMenu("Windows"))
                {
                    if(ImGui.MenuItem("Background", null, ref backgroundWindow)) { }
                    if(ImGui.MenuItem("Tilemap", null, ref tilemapWindow)) { }
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            // Render the Emulator
            ImGui.BeginChild("Emulation Renderer");
            {
                Vector2 size = ImGui.GetWindowSize();
                int scale = (int)(size.Y / Constant.WindowHeight);

                ImGui.SetCursorPos((size - (framebufferTexture.Size * scale)) * 0.5f);
                ImGui.Image((IntPtr)framebufferTexture.GLTexture, scale * framebufferTexture.Size);
            }
            ImGui.EndChild();

            BackgroundWindow();
            TilemapWindow();

            ImGui.End();
        }

        private void BackgroundWindow()
        {
            if(backgroundWindow == true)
            {
                uint[] palette = { 0xD0D058, 0xA0A840, 0x708028, 0x405010 };

                bool tileset = emulator.bus.gpu.LCDC.GetBit(LCDCBit.Tileset);
                bool tilemap = emulator.bus.gpu.LCDC.GetBit(LCDCBit.Tilemap);

                ushort tilesetAddress = (ushort)(tileset ? 0x0000 : 0x1000);
                ushort tilemapAddress = (ushort)(tilemap ? 0x1C00 : 0x1800);

                for(int y = 0; y < 256; y++)
                {
                    ushort row = (ushort)(y / 8);

                    for(int x = 0; x < 256; x++)
                    {
                        ushort colum = (ushort)(x / 8);

                        byte rawTile = emulator.bus.gpu.VideoRam[tilemapAddress + ((row * 32) + colum)];

                        int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

                        int line = (byte)(y % 8) * 2;
                        byte data1 = emulator.bus.gpu.VideoRam[vramAddress + line];
                        byte data2 = emulator.bus.gpu.VideoRam[vramAddress + line + 1];

                        byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
                        byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                        byte colorIndex = (byte)((emulator.bus.gpu.BGP >> palletIndex * 2) & 0b11);
                        backgroundBuffer.SetPixel(x, y, palette[colorIndex]);
                    }
                }

                backgroundTexture.Update(backgroundBuffer.Pixels);

                ImGui.Begin("Background Window");
                {
                    Vector2 size = ImGui.GetWindowSize();
                    int scale = (int)(size.Y / backgroundBuffer.Size.Y);
                    ImGui.SetCursorPos((size - (backgroundBuffer.Size * scale)) * 0.5f);
                    ImGui.Image((IntPtr)backgroundTexture.GLTexture, backgroundBuffer.Size * scale);
                }
                ImGui.End();
            }
        }


        private void TilemapWindow()
        {
            if(tilemapWindow == true)
            {
                uint[] palette = { 0xD0D058, 0xA0A840, 0x708028, 0x405010 };

                for(int y = 0; y < 192; y++)
                {
                    ushort row = (ushort)(y / 8);

                    for(int x = 0; x < 128; x++)
                    {
                        ushort colum = (ushort)(x / 8);

                        ushort rawTile = (ushort)((row * 16) + colum);

                        ushort tileGraphicAddress = (ushort)(rawTile * 16);

                        byte line = (byte)((byte)(y % 8) * 2);
                        byte data1 = emulator.bus.gpu.VideoRam[tileGraphicAddress + line];
                        byte data2 = emulator.bus.gpu.VideoRam[tileGraphicAddress + line + 1];

                        byte bit = (byte)(0b00000001 << ((((int)x % 8) - 7) * 0xff));
                        byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                        byte colorIndex = (byte)((emulator.bus.gpu.BGP >> palletIndex * 2) & 0b11);
                        tilemapBuffer.SetPixel(x, y, palette[colorIndex]);
                    }
                }

                tilemapTexture.Update(tilemapBuffer.Pixels);

                ImGui.Begin("Tilemap Window");
                {
                    Vector2 size = ImGui.GetWindowSize();
                    int scale = (int)(size.Y / tilemapBuffer.Size.Y);
                    ImGui.SetCursorPos((size - (tilemapBuffer.Size * scale)) * 0.5f);
                    ImGui.Image((IntPtr)tilemapTexture.GLTexture, tilemapBuffer.Size * scale);
                }
                ImGui.End();
            }
        }
    }
}