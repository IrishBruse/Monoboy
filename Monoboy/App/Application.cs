using System.Diagnostics;
using Monoboy.Core;
using Monoboy.Core.Utility;
using Monoboy.Frontend;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Window = Monoboy.Frontend.Window;

namespace Monoboy
{
    public class Application
    {
        readonly Window mainWindow;
        readonly Window debugWindow;

        const int CyclesPerFrame = 69905;
        Emulator emulator;
        Texture screen;
        Sprite screenSprite;

        Keybind aButton;
        Keybind bButton;
        Keybind startButton;
        Keybind selectButton;
        Keybind upButton;
        Keybind downButton;
        Keybind leftButton;
        Keybind rightButton;

        Keybind tilemapDumpButton;
        Keybind backgroundDumpButton;
        Keybind pauseButton;
        Keybind stepButton;

        bool paused = false;

        public Application()
        {
            debugWindow = new Window("Monoboy - Debug", 640, 480);
            debugWindow.Position += new Vector2i(340, 0);
            mainWindow = new Window("Monoboy", 640, 480);
            mainWindow.Position += new Vector2i(-340, 0);

            emulator = new Emulator();
            emulator.LoadRom("Roms/Dr. Mario.gb");
            //emulator.LoadRom("Roms/Tetris.gb");
            //emulator.bus.SkipBootRom();
            //emulator.LoadRom("Tests/Cpu/01-special.gb");

            screen = new Texture(Emulator.WindowWidth, Emulator.WindowHeight);

            screenSprite = new Sprite(screen);
            screenSprite.Scale = new Vector2f(2, 2);
            screenSprite.Position = new Vector2f((mainWindow.Width / 2) - ((screenSprite.Scale.X * Emulator.WindowWidth) / 2), 0);

            mainWindow.Update += Update;
            mainWindow.Draw += Draw;
            mainWindow.Resize += Resize;

            // Input
            aButton = new Keybind(Action.Held, Button.Space, Button.JoystickA);
            bButton = new Keybind(Action.Held, Button.LeftShift, Button.JoystickB);
            startButton = new Keybind(Action.Held, Button.Escape, Button.JoystickStart);
            selectButton = new Keybind(Action.Held, Button.Enter, Button.JoystickSelect);
            upButton = new Keybind(Action.Held, Button.W, Button.JoystickUp);
            downButton = new Keybind(Action.Held, Button.S, Button.JoystickDown);
            leftButton = new Keybind(Action.Held, Button.A, Button.JoystickLeft);
            rightButton = new Keybind(Action.Held, Button.D, Button.JoystickRight);

            // Debug
            tilemapDumpButton = new Keybind(Action.Pressed, Button.T);
            backgroundDumpButton = new Keybind(Action.Pressed, Button.Y);
            pauseButton = new Keybind(Action.Pressed, Button.P);
            stepButton = new Keybind(Action.Pressed, Button.Enter);

            debugWindow.Draw += DebugDraw;

            Resize(new Vector2u(640, 480));

            while(mainWindow.Open && debugWindow.Open)
            {
                mainWindow.Loop();
                debugWindow.Loop();
            }
        }

        private void Update()
        {
            emulator.bus.joypad.SetButton(Joypad.Button.A, aButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.B, bButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.Start, startButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.Select, selectButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.Up, upButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.Down, downButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.Left, leftButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.Right, rightButton.IsActive());

            if(tilemapDumpButton.IsActive() == true)
            {
                emulator.bus.DumpTilemap();
            }

            if(backgroundDumpButton.IsActive() == true)
            {
                emulator.bus.DumpBackground();
            }

            if(pauseButton.IsActive() == true)
            {
                paused = !paused;
            }

            if(stepButton.IsActive() == true)
            {
                emulator.Step();
            }

            if(paused == true)
            {
                int cycles = 0;
                while(cycles < CyclesPerFrame)
                {
                    cycles += emulator.Step();
                }
            }
        }

        private void Draw(DrawingSurface surface)
        {
            surface.Clear(Color.White);

            screen.Update(emulator.bus.gpu.Framebuffer);

            surface.Draw(screenSprite);
        }

        private void DebugDraw(DrawingSurface surface)
        {
            surface.Clear(Color.White);

            int spacing = 24;

            surface.DrawString("AF: " + emulator.bus.register.AF.ToHex(), new Vector2f(0, spacing * 0));
            surface.DrawString("PC: " + emulator.bus.register.BC.ToHex(), new Vector2f(0, spacing * 1));
            surface.DrawString("SP: " + emulator.bus.register.DE.ToHex(), new Vector2f(0, spacing * 2));
            surface.DrawString("HL: " + emulator.bus.register.HL.ToHex(), new Vector2f(0, spacing * 3));
            surface.DrawString("PC: " + emulator.bus.register.PC.ToHex(), new Vector2f(0, spacing * 4));
            surface.DrawString("SP: " + emulator.bus.register.SP.ToHex(), new Vector2f(0, spacing * 5));
        }

        private void Resize(Vector2u windowSize)
        {
            int scale = (int)(mainWindow.Height / Emulator.WindowHeight);
            screenSprite.Scale = new Vector2f(scale, scale);

            screenSprite.Position = new Vector2f((windowSize.X / 2) - ((screenSprite.Scale.X * Emulator.WindowWidth) / 2), 0);
        }
    }
}