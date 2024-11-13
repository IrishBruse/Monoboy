namespace Monoboy.Desktop;

using System.Diagnostics;
using System.IO;

using Monoboy.Constants;
using Monoboy.Utility;

using NativeFileDialogSharp;

using Raylib_cs;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using static Monoboy.Constants.Bit;

using Color = Raylib_cs.Color;

public class Application
{
    Emulator emulator;
    Stopwatch timer = new();

    int frame;
    bool speedup;
    bool paused;

    public Application()
    {
        emulator = new(Boot.Bootix);
        emulator.SkipBootRom();
    }

    public void Run()
    {
        Raylib.InitWindow(800, 480, "Hello World");

        while (!Raylib.WindowShouldClose())
        {
            Update();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);

            Raylib.DrawText("Hello, world!", 12, 12, 20, Color.Black);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();

        Emulator.Save();
    }

    int milis;

    void Update()
    {
        string title = !string.IsNullOrEmpty(emulator.GameTitle) ? $" - {emulator.GameTitle}" : "";
        string pausedTitle = paused ? " - Paused" : "";

        Raylib.SetWindowTitle("Monoboy" + title + pausedTitle + " - " + milis + "ms");

        if (paused)
        {
            return;
        }

        EmulatorInput();
        KeyDown();

        if (frame >= 3)
        {
            frame = 0;
            milis = (int)(timer.ElapsedMilliseconds / 3);
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

    void EmulatorInput()
    {
        bool right = Raylib.IsKeyPressed(KeyboardKey.D) || Raylib.IsKeyPressed(KeyboardKey.Right);
        bool left = Raylib.IsKeyPressed(KeyboardKey.A) || Raylib.IsKeyPressed(KeyboardKey.Left);
        bool up = Raylib.IsKeyPressed(KeyboardKey.W) || Raylib.IsKeyPressed(KeyboardKey.Up);
        bool down = Raylib.IsKeyPressed(KeyboardKey.S) || Raylib.IsKeyPressed(KeyboardKey.Down);

        bool a = Raylib.IsKeyPressed(KeyboardKey.Space);
        bool b = Raylib.IsKeyPressed(KeyboardKey.LeftShift);
        bool select = Raylib.IsKeyPressed(KeyboardKey.Enter);
        bool start = Raylib.IsKeyPressed(KeyboardKey.Escape);

        if (Raylib.IsGamepadAvailable(0))
        {
            right |= Raylib.IsGamepadButtonPressed(0, GamepadButton.LeftFaceRight) || Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX) > 0.5f;
            left |= Raylib.IsGamepadButtonPressed(0, GamepadButton.LeftFaceLeft) || Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX) < -0.5f;
            up |= Raylib.IsGamepadButtonPressed(0, GamepadButton.LeftFaceUp) || Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY) < -0.5f;
            down |= Raylib.IsGamepadButtonPressed(0, GamepadButton.LeftFaceDown) || Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY) > 0.5f;

            a |= Raylib.IsGamepadButtonPressed(0, GamepadButton.RightFaceDown);
            b |= Raylib.IsGamepadButtonPressed(0, GamepadButton.RightFaceLeft);
            select |= Raylib.IsGamepadButtonPressed(0, GamepadButton.MiddleLeft);
            start |= Raylib.IsGamepadButtonPressed(0, GamepadButton.MiddleRight);
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

    void KeyDown()
    {
        speedup = Raylib.IsKeyDown(KeyboardKey.F);

        if (Raylib.IsKeyPressed(KeyboardKey.F))
        {
            paused = !paused;
        }
        else if (Raylib.IsKeyDown(KeyboardKey.LeftControl) && Raylib.IsKeyPressed(KeyboardKey.O))
        {
            OpenFile();
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F5))
        {
            File.WriteAllBytes("Memory.bin", emulator.Memory);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F6))
        {
            DumpBackground();
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F7))
        {
            DumpTileset();
        }
    }

    void OpenFile()
    {
        DialogResult file = Dialog.FileOpen("gb,gbc");
        if (file.IsOk)
        {
            emulator.Open(file.Path);
        }
    }

    void DumpBackground()
    {
        Image<Rgba32> backgroundImage = new(256, 256, new Rgba32(Pallet.GetColor(0)));

        bool tileset = emulator.Read(0xFF40).GetBit(Flags.Tileset);
        bool tilemap = emulator.Read(0xFF40).GetBit(Flags.Tilemap);

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

                byte bit = (byte)(Bit0 << (((x % 8) - 7) * 0xff));
                byte palletIndex = (byte)((((data2 & (bit)) != 0 ? 1 : 0) << 1) | ((data1 & (bit)) != 0 ? 1 : 0));
                byte colorIndex = (byte)((emulator.Read(0xFF47) >> (palletIndex * 2)) & Bit01);
                backgroundImage[x, y] = new(packed: Pallet.GetColor(colorIndex));
            }
        }

        backgroundImage.SaveAsPng("Background.png");
    }

    void DumpTileset()
    {
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

                byte bit = (byte)(Bit0 << (((x % 8) - 7) * 0xff));
                byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                byte colorIndex = (byte)((emulator.Read(0xFF47) >> (palletIndex * 2)) & Bit01);
                tilesetImage[x, y] = new(Pallet.GetColor(colorIndex));
            }
        }

        tilesetImage.SaveAsPng("Tileset.png");
    }
}
