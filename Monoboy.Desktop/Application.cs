namespace Monoboy.Desktop;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Monoboy.Constants;
using Monoboy.Utility;

using NativeFileDialogSharp;

using Raylib_cs;

public class Application
{
    Emulator emulator;
    Stopwatch timer = new();

    bool speedup;
    bool paused;

    public Application()
    {
        // byte[] dmgbootRom = GetEmbeddedFile("Monoboy.Desktop/Data/dmg_boot.bin");
        // byte[] bootRom = GetDataFile("Monoboy.Desktop/Data/bootix_dmg.bin");

        emulator = new();
        emulator.Open("/mnt/data/Emulation/GB/test/mooneye-test-suite/acceptance/intr_timing.gb");
        // emulator.Open("/mnt/data/Emulation/GB/Tetris.gb");
        // emulator.Open("/mnt/data/Emulation/GB/test/blargg/oam_bug/rom_singles/1-lcd_sync.gb");
    }

    public void Run()
    {
        Raylib.SetTraceLogLevel(TraceLogLevel.Error);

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(Emulator.WindowWidth * 4, Emulator.WindowHeight * 4, "Monoboy");
        byte[] icon = GetEmbeddedFile("Monoboy.Desktop/Data/Icon.png");
        Raylib.SetWindowIcon(Raylib.LoadImageFromMemory(".png", icon));
        Raylib.SetWindowMinSize(Emulator.WindowWidth, Emulator.WindowHeight);
        Raylib.InitAudioDevice();

        var framebufferImage = Raylib.GenImageColor(Emulator.WindowWidth, Emulator.WindowHeight, Color.Red);
        var framebuffer = Raylib.LoadTextureFromImage(framebufferImage);

        Raylib.SetExitKey(0);
        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            int width = Raylib.GetScreenWidth();
            int height = Raylib.GetScreenHeight();

            Update();

            if (Raylib.IsFileDropped())
            {
                string[] files = Raylib.GetDroppedFiles();

                if (files.Length == 1)
                {
                    emulator.Open(files[0]);
                }
            }

            Raylib.UpdateTexture(framebuffer, emulator.Framebuffer);

            Raylib.BeginDrawing();
            {
                Raylib.ClearBackground(new Color(0xD0, 0xD0, 0x58, 0xFF));
                int scale = Math.Min(Math.Max(width / Emulator.WindowWidth, 1), Math.Max(height / Emulator.WindowHeight, 1));
                Raylib.DrawTextureEx(framebuffer, new((width - (Emulator.WindowWidth * scale)) * 0.5f, (height - (Emulator.WindowHeight * scale)) * 0.5f), 0, scale, Color.White);

                // var reg = emulator.Registers;
                // string text = "A:" + reg.A.ToString("X2") + " F:" + reg.F.ToString("B8") + "\n\n" +
                // "B:" + reg.B.ToString("X2") + " C:" + reg.C.ToString("X2") + "\n\n" +
                // "D:" + reg.D.ToString("X2") + " E:" + reg.E.ToString("X2") + "\n\n" +
                // "H:" + reg.H.ToString("X2") + " L:" + reg.L.ToString("X2");

                // Raylib.DrawText(text, 4, 0, 25, Color.Black);
            }
            Raylib.EndDrawing();
        }

        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();

        Emulator.Save();
    }

    static byte[] GetEmbeddedFile(string path)
    {
        string embedPath = path.Replace("/", ".");
        BinaryReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream(embedPath));
        return reader.ReadBytes((int)reader.BaseStream.Length);
    }

    Queue<long> frameTimes = new([0]);

    void Update()
    {
        if (paused)
        {
            return;
        }

        // Console.WriteLine(Enumerable.Average(frameTimes));

        EmulatorInput();
        KeyDown();

        timer.Restart();
        emulator.StepFrame();
        timer.Stop();

        frameTimes.Enqueue(timer.ElapsedMilliseconds);


        if (frameTimes.Count > 30)
        {
            _ = frameTimes.Dequeue();
        }

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
        bool right = Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right);
        bool left = Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left);
        bool up = Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up);
        bool down = Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down);

        bool a = Raylib.IsKeyDown(KeyboardKey.Space);
        bool b = Raylib.IsKeyDown(KeyboardKey.LeftShift);
        bool select = Raylib.IsKeyDown(KeyboardKey.Escape);
        bool start = Raylib.IsKeyDown(KeyboardKey.Enter);

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

    void KeyDown()
    {
        speedup = Raylib.IsKeyDown(KeyboardKey.F);

        if (Raylib.IsKeyPressed(KeyboardKey.P))
        {
            paused = !paused;
        }
        else if (Raylib.IsKeyDown(KeyboardKey.LeftControl) && Raylib.IsKeyPressed(KeyboardKey.O))
        {
            OpenFile();
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F5))
        {
            emulator.Dump("F5");
            DumpBackground("dumps/F5/background.png");
            DumpTileset("dumps/F5/tileset.png");
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

    void DumpBackground(string path)
    {
        Image backgroundImage = Raylib.GenImageColor(256, 256, Color.Red);

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

                byte bit = (byte)(1 << (((x % 8) - 7) * 0xff));
                byte palletIndex = (byte)((((data2 & (bit)) != 0 ? 1 : 0) << 1) | ((data1 & (bit)) != 0 ? 1 : 0));
                byte colorIndex = (byte)((emulator.Read(0xFF47) >> (palletIndex * 2)) & 0b11);

                var pal = Pallet.GetColor(colorIndex);
                var col = new Color((byte)(pal & 0xFF), (byte)((pal >> 8) & 0xFF), (byte)((pal >> 16) & 0xFF), (byte)0xFF);
                Raylib.ImageDrawPixel(ref backgroundImage, x, y, col);
            }
        }

        _ = Raylib.ExportImage(backgroundImage, path);
    }

    void DumpTileset(string path)
    {
        Image tilesetImage = Raylib.GenImageColor(128, 192, Color.White);

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

                byte bit = (byte)(1 << (((x % 8) - 7) * 0xff));
                byte palletIndex = (byte)(((data2.GetBit(bit) ? 1 : 0) << 1) | (data1.GetBit(bit) ? 1 : 0));
                byte colorIndex = (byte)((emulator.Read(0xFF47) >> (palletIndex * 2)) & 0b11);

                var pal = Pallet.GetColor(colorIndex);
                var col = new Color((byte)(pal & 0xFF), (byte)((pal >> 8) & 0xFF), (byte)((pal >> 16) & 0xFF), (byte)0xFF);
                Raylib.ImageDrawPixel(ref tilesetImage, x, y, col);
            }
        }

        _ = Raylib.ExportImage(tilesetImage, path);
    }
}
