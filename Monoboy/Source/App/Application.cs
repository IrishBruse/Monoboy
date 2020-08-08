using System.IO;
using Monoboy.Frontend;
using Newtonsoft.Json.Linq;
using SFML.Graphics;
using SFML.System;
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
        Keybind memoryDumpButton;
        Keybind pauseButton;
        Keybind stepButton;
        Keybind speedupButton;
        Keybind runButton;

        JObject opcodes;

        bool paused = true;

        public Application()
        {
            opcodes = JObject.Parse(File.ReadAllText("Data/opcodes.json"));

            debugWindow = new Window("Monoboy - Debug", Emulator.WindowWidth * Emulator.WindowScale, Emulator.WindowHeight * Emulator.WindowScale);
            debugWindow.Position += new Vector2i(340, 0);
            mainWindow = new Window("Monoboy", Emulator.WindowWidth * Emulator.WindowScale, Emulator.WindowHeight * Emulator.WindowScale);
            mainWindow.Position += new Vector2i(-340, 0);

            emulator = new Emulator();
            //emulator.LoadRom("Dr. Mario.gb");
            //emulator.LoadRom("Tetris.gb");
            //emulator.bus.SkipBootRom();
            //emulator.LoadRom("01-special.gb");
            emulator.LoadRom("02-interrupts.gb");
            //emulator.LoadRom("01-special.gb");
            //emulator.LoadRom("01-special.gb");
            //emulator.LoadRom("01-special.gb");
            //emulator.LoadRom("01-special.gb");
            //emulator.LoadRom("01-special.gb");
            //emulator.LoadRom("01-special.gb");
            //emulator.LoadRom("01-special.gb");
            //emulator.LoadRom("01-special.gb");

            screen = new Texture(Emulator.WindowWidth, Emulator.WindowHeight);

            mainWindow.SetTitle("Monoboy - " + emulator.bus.cartridge.Title);

            screenSprite = new Sprite(screen)
            {
                Position = new Vector2f(0, 0),
                Scale = new Vector2f(Emulator.WindowScale, Emulator.WindowScale)
            };

            mainWindow.Update += Update;
            mainWindow.Draw += Draw;
            emulator.bus.gpu.DrawFrame += UpdateFrame;
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
            memoryDumpButton = new Keybind(Action.Pressed, Button.U);
            pauseButton = new Keybind(Action.Pressed, Button.P);
            stepButton = new Keybind(Action.Pressed, Button.Enter);
            speedupButton = new Keybind(Action.Held, Button.V);
            runButton = new Keybind(Action.Held, Button.LeftShift);

            debugWindow.Draw += DebugDraw;

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
                emulator.DumpTilemap();
            }

            if(backgroundDumpButton.IsActive() == true)
            {
                emulator.DumpBackground();
            }

            if(memoryDumpButton.IsActive() == true)
            {
                emulator.DumpMemory();
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

            if(runButton.IsActive() == true)
            {
                for(int i = 0; i < overclock; i++)
                {
                    emulator.Step();
                }
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
            surface.Clear(Color.Magenta);
            surface.Draw(screenSprite);
        }

        private void UpdateFrame()
        {
            screen.Update(emulator.bus.gpu.Framebuffer);
        }

        private void DebugDraw(DrawingSurface surface)
        {
            surface.Clear(Color.White);

            int spacing = 24;

            surface.DrawString("AF: 0x" + emulator.bus.register.AF.ToString("X4"), new Vector2f(0, spacing * 0));
            surface.DrawString("BC: 0x" + emulator.bus.register.BC.ToString("X4"), new Vector2f(0, spacing * 1));
            surface.DrawString("DE: 0x" + emulator.bus.register.DE.ToString("X4"), new Vector2f(0, spacing * 2));
            surface.DrawString("HL: 0x" + emulator.bus.register.HL.ToString("X4"), new Vector2f(0, spacing * 3));
            surface.DrawString("SP: 0x" + emulator.bus.register.SP.ToString("X4"), new Vector2f(0, spacing * 4));

            string hexcode = emulator.bus.Read(emulator.bus.register.PC).ToString("x");

            string opcode = "OP: " + (string)opcodes.SelectToken("unprefixed." + "0x" + hexcode + ".mnemonic") + " ";

            byte length = (byte)opcodes.SelectToken("unprefixed." + "0x" + hexcode + ".length");

            for(int i = 1; i < length; i++)
            {
                opcode += emulator.bus.Read((ushort)(emulator.bus.register.PC + i)).ToString("X2");
            }

            surface.DrawString("PC: " + emulator.bus.register.PC.ToString("X4") + "(" + hexcode + ") ", new Vector2f(0, spacing * 5));
            surface.DrawString(opcode, new Vector2f(0, spacing * 6));
            surface.DrawString("" + emulator.cyclesRan, new Vector2f(0, spacing * 7));

            surface.DrawString("LY: " + emulator.bus.memory.LY.ToString("X2"), new Vector2f(260, spacing * 0));
            surface.DrawString("STAT: " + System.Convert.ToString(emulator.bus.memory.Stat, 2), new Vector2f(260, spacing * 1));
            surface.DrawString("LCDC: " + System.Convert.ToString(emulator.bus.memory.LCDC, 2), new Vector2f(260, spacing * 2));
            surface.DrawString("SCX: " + emulator.bus.memory.SCX.ToString("X2"), new Vector2f(260, spacing * 3));
            surface.DrawString("SCY: " + emulator.bus.memory.SCY.ToString("X2"), new Vector2f(260, spacing * 4));
            surface.DrawString("WindX: " + emulator.bus.memory.WindowX.ToString("X2"), new Vector2f(260, spacing * 5));
            surface.DrawString("WindY: " + emulator.bus.memory.WindowY.ToString("X2"), new Vector2f(260, spacing * 6));
        }

        private void Resize(Vector2u windowSize)
        {
            int scale = (int)(mainWindow.Height / Emulator.WindowHeight);

            screenSprite.Scale = new Vector2f(scale, scale);
            screenSprite.Position = new Vector2f(
                (windowSize.X / 2) - ((screenSprite.Scale.X * Emulator.WindowWidth) / 2),
                (windowSize.Y / 2) - ((screenSprite.Scale.Y * Emulator.WindowHeight) / 2));
        }
    }
}