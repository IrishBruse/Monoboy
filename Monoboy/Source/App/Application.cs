using System.Diagnostics;
using System.IO;
using Monoboy.Core;
using Monoboy.Core.Utility;
using Monoboy.Frontend;
using Newtonsoft.Json.Linq;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Window = Monoboy.Frontend.Window;

namespace Monoboy
{
    public class Application
    {
        Window mainWindow;
        Window debugWindow;

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
        Keybind speedupButton;

        JObject opcodes;

        bool paused = false;

        public Application()
        {
            opcodes = JObject.Parse(File.ReadAllText("Data/opcodes.json"));

            debugWindow = new Window("Monoboy - Debug", Emulator.WindowWidth * 4, Emulator.WindowHeight * 4);
            debugWindow.Position += new Vector2i(340, 0);
            mainWindow = new Window("Monoboy", Emulator.WindowWidth * 4, Emulator.WindowHeight * 4);
            mainWindow.Position += new Vector2i(-340, 0);

            emulator = new Emulator();
            //emulator.LoadRom("Dr. Mario.gb");
            emulator.LoadRom("Tetris.gb");
            //emulator.bus.SkipBootRom();
            //emulator.LoadRom("05-op rp.gb");

            screen = new Texture(Emulator.WindowWidth, Emulator.WindowHeight);

            mainWindow.SetTitle("Monoboy - " + emulator.bus.cartridge.Title);

            screenSprite = new Sprite(screen);
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
            speedupButton = new Keybind(Action.Held, Button.V);

            debugWindow.Draw += DebugDraw;

            Resize(new Vector2u(640, 480));

            //while(emulator.bus.register.PC != 0x100)
            //{
            //    emulator.Step();
            //}

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

            int overclock = 1;

            if(speedupButton.IsActive() == true)
            {
                overclock = 8;
            }

            if(paused == false)
            {
                for(int i = 0; i < overclock; i++)
                {
                    int cycles = 0;
                    while(cycles < CyclesPerFrame)
                    {
                        cycles += emulator.Step();
                    }
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
            surface.DrawString("BC: " + emulator.bus.register.BC.ToHex(), new Vector2f(0, spacing * 1));
            surface.DrawString("DE: " + emulator.bus.register.DE.ToHex(), new Vector2f(0, spacing * 2));
            surface.DrawString("HL: " + emulator.bus.register.HL.ToHex(), new Vector2f(0, spacing * 3));
            surface.DrawString("SP: " + emulator.bus.register.SP.ToHex(), new Vector2f(0, spacing * 4));

            string hexcode = emulator.bus.Read(emulator.bus.register.PC).ToHex();

            string opcode = "OP: " + (string)opcodes.SelectToken("unprefixed." + hexcode + ".mnemonic");

            byte length = (byte)opcodes.SelectToken("unprefixed." + hexcode + ".length");

            for(int i = 1; i < length; i++)
            {
                opcode += " " + emulator.bus.Read((ushort)(emulator.bus.register.PC + i)).ToString("X2");
            }

            surface.DrawString("PC: " + emulator.bus.register.PC.ToHex() + " (" + hexcode + ") ", new Vector2f(0, spacing * 5));
            surface.DrawString(opcode, new Vector2f(0, spacing * 6));

        }

        private void Resize(Vector2u windowSize)
        {
            int scale = (int)(mainWindow.Height / Emulator.WindowHeight);
            screenSprite.Scale = new Vector2f(scale, scale);

            screenSprite.Position = new Vector2f((windowSize.X / 2) - ((screenSprite.Scale.X * Emulator.WindowWidth) / 2), 0);
        }
    }
}