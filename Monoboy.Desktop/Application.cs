namespace Monoboy.Desktop;

using System;
using System.Diagnostics;
using System.IO;

using Monoboy.Constants;
using Monoboy.Desktop.Data;
using Monoboy.Utility;

using NativeFileDialogSharp;

using Silk.NET.Input;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Veldrid;

public class Application
{
    private VeldridWindow Window { get; set; }

    private Emulator emulator;
    private Stopwatch timer = new();

    private int frame;
    private bool speedup = false;

    public Application()
    {
        emulator = new(Boot.DMG);

        Window = new(GraphicsBackend.Vulkan, Icon.Data);

        Window.Framebuffer = emulator.Framebuffer;

        Window.Update += OnUpdate;
        Window.FileDrop += OnFilesDrop;
        Window.Load += OnLoad;
    }

    private void OnLoad()
    {
        Window.Keyboard.KeyDown += KeyDown;
        Window.Keyboard.KeyUp += KeyUp;
        timer.Start();
    }

    public void Run()
    {
        Window.Run();
    }

    public void OnUpdate(double deltaTime)
    {
        EmulatorEmulator();

        if (frame >= 3)
        {
            frame = 0;
            string title = !string.IsNullOrEmpty(emulator.GameTitle) ? $" - {emulator.GameTitle}" : "";
            Window.Title = "Monoboy" + title + " - " + (timer.ElapsedMilliseconds / 3);
            timer.Reset();
        }
        else
        {
            frame++;
        }

        timer.Start();
        emulator.StepFrame();
        timer.Stop();

        if (speedup)
        {
            emulator.StepFrame();
            emulator.StepFrame();
            emulator.StepFrame();
            emulator.StepFrame();
        }
    }

    private void EmulatorEmulator()
    {
        // Keyboard
        var keyboard = Window.Keyboard;

        bool right = keyboard.IsKeyPressed(Key.D) || keyboard.IsKeyPressed(Key.Right);
        bool left = keyboard.IsKeyPressed(Key.A) || keyboard.IsKeyPressed(Key.Left);
        bool up = keyboard.IsKeyPressed(Key.W) || keyboard.IsKeyPressed(Key.Up);
        bool down = keyboard.IsKeyPressed(Key.S) || keyboard.IsKeyPressed(Key.Down);

        bool a = keyboard.IsKeyPressed(Key.Space);
        bool b = keyboard.IsKeyPressed(Key.ShiftLeft);
        bool select = keyboard.IsKeyPressed(Key.Escape);
        bool start = keyboard.IsKeyPressed(Key.Enter);

        // Controller/Gamepad
        var gamepad = Window.Gamepad;

        if (gamepad != null)
        {
            right |= gamepad.DPadRight().Pressed || gamepad.Thumbsticks[0].X > 0.5f;
            left |= gamepad.DPadLeft().Pressed || gamepad.Thumbsticks[0].X < -0.5f;
            up |= gamepad.DPadUp().Pressed || gamepad.Thumbsticks[0].Y < -0.5f;
            down |= gamepad.DPadDown().Pressed || gamepad.Thumbsticks[0].Y > 0.5f;

            a |= gamepad.A().Pressed;
            b |= gamepad.B().Pressed;
            select |= gamepad.Back().Pressed;
            start |= gamepad.Start().Pressed;
        }

        // Set data in emulator
        emulator.SetButtonState(GameboyButton.Right, right);
        emulator.SetButtonState(GameboyButton.Left, left);
        emulator.SetButtonState(GameboyButton.Up, up);
        emulator.SetButtonState(GameboyButton.Down, down);

        emulator.SetButtonState(GameboyButton.A, a);
        emulator.SetButtonState(GameboyButton.B, b);
        emulator.SetButtonState(GameboyButton.Select, select);
        emulator.SetButtonState(GameboyButton.Start, start);
    }

    public void OnFilesDrop(string[] files)
    {
        if (files.Length == 0)
        {
            return;
        }
        emulator.Open(files[0]);
    }

    private void KeyUp(IKeyboard keyboard, Key key, int i)
    {
        switch (key)
        {
            case Key.F:
            speedup = false;
            break;
        }
    }

    private void KeyDown(IKeyboard keyboard, Key key, int i)
    {
        switch (key)
        {
            case Key.F:
            speedup = true;
            break;

            case Key.F2:
            Window.Screenshot();
            break;

            case Key.O:
            if (keyboard.IsKeyPressed(Key.ControlLeft))
            {
                DialogResult file = Dialog.FileOpen("gb,gbc");
                if (file.IsOk)
                {
                    emulator.Open(file.Path);
                }
            }
            break;

            case Key.F5:
            File.WriteAllBytes("Memory.bin", emulator.Memory);
            break;

            case Key.F6:
            Image<Rgba32> backgroundImage = new(256, 256, new Rgba32(Pallet.GetColor(0)));

            bool tileset = emulator.ppu.LCDC.GetBit(Bit.Tileset);
            bool tilemap = emulator.ppu.LCDC.GetBit(Bit.Tilemap);

            ushort tilesetAddress = (ushort)(tileset ? 0x0000 : 0x1000);
            ushort tilemapAddress = (ushort)(tilemap ? 0x1C00 : 0x1800);

            for (int y = 0; y < 256; y++)
            {
                ushort row = (ushort)(y / 8);

                for (int x = 0; x < 256; x++)
                {
                    ushort colum = (ushort)(x / 8);

                    byte rawTile = emulator.Read((ushort)(0x8000 + tilemapAddress + (row * 32) + colum));

                    int vramAddress = tileset ? (rawTile * 16) + tilesetAddress : ((short)tilesetAddress) + ((sbyte)rawTile * 16);

                    int line = (byte)(y % 8) * 2;
                    byte data1 = emulator.Read((ushort)(0x8000 + vramAddress + line));
                    byte data2 = emulator.Read((ushort)(0x8000 + vramAddress + line + 1));

                    byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
                    byte palletIndex = (byte)((((data2 & (bit)) != 0 ? 1 : 0) << 1) | ((data1 & (bit)) != 0 ? 1 : 0));
                    byte colorIndex = (byte)((emulator.ppu.BGP >> (palletIndex * 2)) & 0b11);
                    backgroundImage[x, y] = new(Pallet.GetColor(colorIndex));
                }
            }

            backgroundImage.SaveAsPng("Background.png");
            break;

            case Key.F7:
            Image<Rgba32> tilesetImage = new(128, 192, new Rgba32(Pallet.GetColor(0)));

            for (int y = 0; y < 192; y++)
            {
                ushort row = (ushort)(y / 8);

                for (int x = 0; x < 128; x++)
                {
                    ushort colum = (ushort)(x / 8);

                    ushort rawTile = (ushort)((row * 16) + colum);

                    ushort tileGraphicAddress = (ushort)(rawTile * 16);

                    byte line = (byte)((byte)(y % 8) * 2);
                    byte data1 = emulator.Read((ushort)(0x8000 + tileGraphicAddress + line));
                    byte data2 = emulator.Read((ushort)(0x8000 + tileGraphicAddress + line + 1));

                    byte bit = (byte)(0b00000001 << (((x % 8) - 7) * 0xff));
                    byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                    byte colorIndex = (byte)((emulator.ppu.BGP >> (palletIndex * 2)) & 0b11);
                    tilesetImage[x, y] = new(Pallet.GetColor(colorIndex));
                }
            }

            tilesetImage.SaveAsPng("Tileset.png");
            break;
        }
    }
}
