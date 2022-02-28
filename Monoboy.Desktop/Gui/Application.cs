namespace Monoboy.Desktop.Gui;

using System;
using System.IO;
using System.Numerics;

using ImGuiNET;
using ImGuiNET.OpenTK;

using Monoboy;
using Monoboy.Constants;
using Monoboy.Desktop.Utility;
using Monoboy.Emulator;
using Monoboy.Utility;

using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Application : Window
{
    Texture framebufferTexture;
    readonly Framebuffer backgroundBuffer;
    readonly Texture backgroundTexture;
    readonly Framebuffer tilemapBuffer;
    readonly Texture tilemapTexture;
    readonly Emulator emulator;

    bool backgroundWindow;
    bool tilemapWindow;
    string openRom;
    readonly IniFile configFile;

    public Application(GameWindowSettings gameWindow, NativeWindowSettings nativeWindow, string[] args) : base(gameWindow, nativeWindow)
    {
        configFile = new IniFile();

        configFile.Load();

        emulator = new Emulator();

        if (args.Length == 1 && File.Exists(args[0]))
        {
            emulator.Open(args[0]);
        }

        framebufferTexture = new Texture("FrameBuffer", Emulator.WindowWidth, Emulator.WindowHeight, emulator.ppu.framebuffer.Pixels);
        framebufferTexture.SetMagFilter(TextureMagFilter.Nearest);

        backgroundBuffer = new Framebuffer(256, 256);
        backgroundTexture = new Texture("Background", 256, 256, backgroundBuffer.Pixels);
        backgroundTexture.SetMagFilter(TextureMagFilter.Nearest);

        tilemapBuffer = new Framebuffer(128, 192);
        tilemapTexture = new Texture("Tilemap", 128, 192, tilemapBuffer.Pixels);
        tilemapTexture.SetMagFilter(TextureMagFilter.Nearest);

        emulator.ppu.DrawFrame += () => DrawFrame();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        UpdateJoypad();

        if (emulator.paused == false)
        {
            int cycles = 0;
            while (cycles < Emulator.CyclesPerFrame)
            {
                cycles += emulator.Step();
            }
        }
    }

    void UpdateJoypad()
    {
        JoystickState joystick = JoystickStates[0];
        if (joystick != null)
        {
            emulator.joypad.SetButton(Joypad.Button.A, KeyboardState.IsKeyDown(Keys.Space) || joystick.IsButtonDown(0));
            emulator.joypad.SetButton(Joypad.Button.B, KeyboardState.IsKeyDown(Keys.LeftShift) || joystick.IsButtonDown(1));
            emulator.joypad.SetButton(Joypad.Button.Select, KeyboardState.IsKeyDown(Keys.Escape) || joystick.IsButtonDown(6));
            emulator.joypad.SetButton(Joypad.Button.Start, KeyboardState.IsKeyDown(Keys.Enter) || joystick.IsButtonDown(7));

            float yAxis = joystick.GetAxis(1);
            emulator.joypad.SetButton(Joypad.Button.Up, KeyboardState.IsKeyDown(Keys.W) || joystick.IsButtonDown(10) || yAxis < -0.25f);
            emulator.joypad.SetButton(Joypad.Button.Down, KeyboardState.IsKeyDown(Keys.S) || joystick.IsButtonDown(12) || yAxis > 0.25f);

            float xAxis = joystick.GetAxis(0);
            emulator.joypad.SetButton(Joypad.Button.Left, KeyboardState.IsKeyDown(Keys.A) || joystick.IsButtonDown(13) || xAxis < -0.25f);
            emulator.joypad.SetButton(Joypad.Button.Right, KeyboardState.IsKeyDown(Keys.D) || joystick.IsButtonDown(11) || xAxis > 0.25f);
        }
        else
        {
            emulator.joypad.SetButton(Joypad.Button.A, KeyboardState.IsKeyDown(Keys.Space));
            emulator.joypad.SetButton(Joypad.Button.B, KeyboardState.IsKeyDown(Keys.LeftShift));
            emulator.joypad.SetButton(Joypad.Button.Select, KeyboardState.IsKeyDown(Keys.Escape));
            emulator.joypad.SetButton(Joypad.Button.Start, KeyboardState.IsKeyDown(Keys.Enter));
            emulator.joypad.SetButton(Joypad.Button.Up, KeyboardState.IsKeyDown(Keys.W));
            emulator.joypad.SetButton(Joypad.Button.Down, KeyboardState.IsKeyDown(Keys.S));
            emulator.joypad.SetButton(Joypad.Button.Left, KeyboardState.IsKeyDown(Keys.A));
            emulator.joypad.SetButton(Joypad.Button.Right, KeyboardState.IsKeyDown(Keys.D));
        }
    }

    protected override void OnJoystickConnected(JoystickEventArgs e)
    {
        base.OnJoystickConnected(e);
    }

    void DrawFrame()
    {
        framebufferTexture.Update(emulator.ppu.framebuffer.Pixels);
    }

    public override void DrawImGui()
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

        if (ImGui.Begin("Monoboy", windowFlags) == false)
        {
            ImGui.End();
            return;
        }

        // Menubar
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open", "Ctrl+O"))
                {
                    openRom = TinyFileDialog.OpenFileDialog("Open Rom", "./", new string[] { "*.gb", "*.gbc" }, "Rom (.gb,.gbc)", false);

                    if (string.IsNullOrEmpty(openRom) == false)
                    {
                        emulator.Open(openRom);
                    }
                }
                if (ImGui.MenuItem("Quit", "Alt+F4"))
                {
                    Close();
                }
                ImGui.EndMenu();
            }


            if (ImGui.BeginMenu("Config"))
            {
                ImGui.MenuItem("Use boot rom", null, ref emulator.UseBootRom);
                if (ImGui.MenuItem("Widescreen", null, ref emulator.WidescreenEnabled))
                {
                    emulator.ToggleWidescreen();
                    framebufferTexture = new Texture("FrameBuffer", Emulator.WindowWidth, Emulator.WindowHeight, emulator.ppu.framebuffer.Pixels);
                    framebufferTexture.SetMagFilter(TextureMagFilter.Nearest);
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Debug"))
            {
                ImGui.MenuItem("Background Toggle", null, ref emulator.BackgroundEnabled);
                ImGui.MenuItem("Window Toggle", null, ref emulator.WindowEnabled);
                ImGui.MenuItem("Sprites Toggle", null, ref emulator.SpritesEnabled);
                //if(ImGui.MenuItem("Screenshot", null))
                //{
                //    var builder = PngBuilder.Create(256, 256, false);

                //    for(int x = 0; x < 256; x++)
                //    {
                //        for(int y = 0; y < 256; y++)
                //        {
                //            uint pixel = backgroundBuffer.GetPixel(x, y);
                //            builder.SetPixel((byte)((pixel >> 16) & 0xFF), (byte)((pixel >> 8) & 0xFF), (byte)((pixel >> 0) & 0xFF), x, y);
                //        }
                //    }

                //    builder.Save(File.Create("Screenshot.png"));
                //}
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Windows"))
            {
                if (ImGui.MenuItem("Background", null, ref backgroundWindow))
                { }
                if (ImGui.MenuItem("Tilemap", null, ref tilemapWindow))
                { }
                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }

        // Render the Emulator
        ImGui.BeginChild("Emulation Renderer");
        {
            Vector2 size = ImGui.GetWindowSize();
            int scale = (int)(size.Y / Emulator.WindowHeight);
            scale = scale < 1 ? 1 : scale;
            Vector2 center = (size - (framebufferTexture.Size * scale)) * 0.5f;
            center = new Vector2((int)center.X, (int)center.Y);
            ImGui.SetCursorPos(center);
            ImGui.Image((IntPtr)framebufferTexture.GLTexture, scale * framebufferTexture.Size);
        }
        ImGui.EndChild();

        BackgroundWindow();
        TilemapWindow();

        ImGui.End();
    }

    void BackgroundWindow()
    {
        if (backgroundWindow)
        {
            uint[] palette = { 0xD0D058, 0xA0A840, 0x708028, 0x405010 };

            bool tileset = emulator.ppu.LCDC.GetBit(LCDCBit.Tileset);
            bool tilemap = emulator.ppu.LCDC.GetBit(LCDCBit.Tilemap);

            ushort tilesetAddress = (ushort)(tileset ? 0x0000 : 0x1000);
            ushort tilemapAddress = (ushort)(tilemap ? 0x1C00 : 0x1800);

            for (int y = 0; y < 256; y++)
            {
                ushort row = (ushort)(y / 8);

                for (int x = 0; x < 256; x++)
                {
                    ushort colum = (ushort)(x / 8);

                    byte rawTile = emulator.ppu.VideoRam[tilemapAddress + ((row * 32) + colum)];

                    int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : (short)tilesetAddress + ((sbyte)rawTile * 16);

                    int line = (byte)(y % 8) * 2;
                    byte data1 = emulator.ppu.VideoRam[vramAddress + line];
                    byte data2 = emulator.ppu.VideoRam[vramAddress + line + 1];

                    byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
                    byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                    byte colorIndex = (byte)((emulator.ppu.BGP >> (palletIndex * 2)) & 0b11);
                    backgroundBuffer.SetPixel(x, y, palette[colorIndex]);
                }
            }

            backgroundTexture.Update(backgroundBuffer.Pixels);

            ImGui.Begin("Background Window", ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar);
            {
                ImGui.SetWindowSize(backgroundBuffer.Size);
                ImGui.SetCursorPos(Vector2.Zero);
                ImGui.Image((IntPtr)backgroundTexture.GLTexture, backgroundBuffer.Size);
            }
            ImGui.End();
        }
    }


    void TilemapWindow()
    {
        if (tilemapWindow)
        {
            uint[] palette = { 0xD0D058, 0xA0A840, 0x708028, 0x405010 };

            for (int y = 0; y < 192; y++)
            {
                ushort row = (ushort)(y / 8);

                for (int x = 0; x < 128; x++)
                {
                    ushort colum = (ushort)(x / 8);

                    ushort rawTile = (ushort)((row * 16) + colum);

                    ushort tileGraphicAddress = (ushort)(rawTile * 16);

                    byte line = (byte)((byte)(y % 8) * 2);
                    byte data1 = emulator.ppu.VideoRam[tileGraphicAddress + line];
                    byte data2 = emulator.ppu.VideoRam[tileGraphicAddress + line + 1];

                    byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
                    byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                    byte colorIndex = (byte)((emulator.ppu.BGP >> (palletIndex * 2)) & 0b11);
                    tilemapBuffer.SetPixel(x, y, palette[colorIndex]);
                }
            }

            tilemapTexture.Update(tilemapBuffer.Pixels);

            _ = ImGui.Begin("Tilemap Window", ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar);
            {
                ImGui.SetWindowSize(tilemapBuffer.Size);
                ImGui.SetCursorPos(Vector2.Zero);
                ImGui.Image((IntPtr)tilemapTexture.GLTexture, tilemapBuffer.Size);
            }
            ImGui.End();
        }
    }
}
