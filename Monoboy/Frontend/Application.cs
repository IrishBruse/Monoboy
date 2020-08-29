using Monoboy.Constants;
using Monoboy.Frontend.UI;
using Monoboy.Frontend.UI.Widgets;
using Monoboy.Utility;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using static Monoboy.Constants.Constant;

namespace Monoboy.Frontend
{
    public class Application
    {
        public static Application Instance { get; internal set; }

        private Window mainWindow;
        private Window debugWindow;

        private Emulator emulator;
        private Texture screen;
        private Sprite screenSprite;

        private Keybind aButton;
        private Keybind bButton;
        private Keybind startButton;
        private Keybind selectButton;
        private Keybind upButton;
        private Keybind downButton;
        private Keybind leftButton;
        private Keybind rightButton;

        private Keybind tilemapDumpButton;
        private Keybind backgroundDumpButton;
        private Keybind memoryDumpButton;
        private Keybind traceDumpButton;
        private Keybind pauseButton;
        private Keybind stepButton;
        private Keybind speedupButton;
        private Keybind runButton;

        public static bool Focused = true;
        private string[] debug = new string[32];

        public Application()
        {
            Instance = this;

            EmulatorInitilization();
            WindowInitilization();
            InputInitilization();

            while(mainWindow.Open && debugWindow.Open)
            {
                Focused = debugWindow.Focused || mainWindow.Focused;
                mainWindow.Loop();
                debugWindow.Loop();
            }
        }

        private void EmulatorInitilization()
        {
            emulator = new Emulator();
            screen = new Texture(WindowWidth, WindowHeight);

            screenSprite = new Sprite(screen)
            {
                Position = new Vector2f(0, 0),
                Scale = new Vector2f(4, 4),
            };
        }

        private void WindowInitilization()
        {
            mainWindow = new Window("Monoboy", WindowWidth * 4, 16 + (WindowHeight * 4));
            mainWindow.Position += new Vector2i(-340, 0);
            debugWindow = new Window("Monoboy - Debug", WindowWidth * 4, (WindowHeight * 4) + 200);
            debugWindow.Position += new Vector2i(340, 0);

            mainWindow.SetTitle("Monoboy");

            mainWindow.Update = Update;
            mainWindow.Draw = Draw;
            emulator.bus.gpu.DrawFrame += UpdateFrame;
            mainWindow.Resize = Resize;
            debugWindow.Draw += DebugDraw;

            CreateGUI();
        }

        private void InputInitilization()
        {
            // Emulator
            aButton = new Keybind(InputAction.Held, Key.Space, Key.JoystickA);
            bButton = new Keybind(InputAction.Held, Key.LeftShift, Key.JoystickB);
            startButton = new Keybind(InputAction.Held, Key.Escape, Key.JoystickStart);
            selectButton = new Keybind(InputAction.Held, Key.Enter, Key.JoystickSelect);
            upButton = new Keybind(InputAction.Held, Key.W, Key.JoystickUp);
            downButton = new Keybind(InputAction.Held, Key.S, Key.JoystickDown);
            leftButton = new Keybind(InputAction.Held, Key.A, Key.JoystickLeft);
            rightButton = new Keybind(InputAction.Held, Key.D, Key.JoystickRight);

            // Debug
            tilemapDumpButton = new Keybind(InputAction.Pressed, Key.Num1);
            backgroundDumpButton = new Keybind(InputAction.Pressed, Key.Num2);
            memoryDumpButton = new Keybind(InputAction.Pressed, Key.Num3);
            traceDumpButton = new Keybind(InputAction.Pressed, Key.Num4);
            pauseButton = new Keybind(InputAction.Pressed, Key.P);
            stepButton = new Keybind(InputAction.Pressed, Key.Enter);
            speedupButton = new Keybind(InputAction.Held, Key.V);
            runButton = new Keybind(InputAction.Held, Key.LeftShift);
        }

        private void CreateGUI()
        {
            Dropdown filemenu = new Dropdown("File");
            {
                Button open = new Button("Open Rom");
                open.Clicked += () => OpenRom();
                filemenu.Add(open);

                Button quit = new Button("Quit");
                quit.Clicked += () => mainWindow.Quit();
                filemenu.Add(quit);
            }
            mainWindow.gui.Add(filemenu);
        }

        private void OpenRom()
        {
            string file = TinyFileDialog.OpenFileDialog("Open Rom", "", new string[] { "*.gb" }, "Gameboy Rom (.gb)", false);

            if(string.IsNullOrEmpty(file) == false)
            {
                emulator.Open(file);
                UpdateTitle();
            }
        }

        private void UpdateTitle()
        {
            string title = "";

            for(ushort i = 0x134; i < 0x144; i++)
            {
                byte val = emulator.bus.memoryBankController.ReadBank00(i);

                if(val == 0)
                {
                    break;
                }
                title += (char)val;
            }

            mainWindow.SetTitle("Monoboy - " + title);
        }

        private void Update()
        {
            Joystick.Update();

            emulator.bus.joypad.SetButton(Joypad.Button.A, aButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.B, bButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.Select, selectButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.Start, startButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.Down, downButton.IsActive());
            emulator.bus.joypad.SetButton(Joypad.Button.Up, upButton.IsActive());
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

            if(traceDumpButton.IsActive() == true)
            {
                emulator.DumpTrace();
            }

            if(pauseButton.IsActive() == true)
            {
                emulator.paused = !emulator.paused;
            }

            if(stepButton.IsActive() == true)
            {
                emulator.Step();
            }

            int overclock = speedupButton.IsActive() ? 8 : 1;

            if(runButton.IsActive() == true)
            {
                for(int i = 0; i < overclock; i++)
                {
                    emulator.Step();
                }
            }

            if(emulator.paused == false)
            {
                int cycles = 0;
                while(cycles < CyclesPerFrame * overclock)
                {
                    cycles += emulator.Step();
                }
            }
        }

        private void Draw(DrawingSurface surface)
        {
            surface.Clear(Color.White);
            surface.Draw(screenSprite);
        }

        private void UpdateFrame()
        {
            screen.Update(emulator.bus.gpu.Framebuffer);
        }

        private void DebugDraw(DrawingSurface surface)
        {
            surface.Clear(Color.White);

            if(emulator.paused == true)
            {
                return;
            }

            string flags =
            "(" +
            (emulator.bus.register.F.GetBit(Flag.C) ? "C" : "-") +
            (emulator.bus.register.F.GetBit(Flag.H) ? "H" : "-") +
            (emulator.bus.register.F.GetBit(Flag.N) ? "N" : "-") +
            (emulator.bus.register.F.GetBit(Flag.Z) ? "Z" : "-") +
            ")";

            debug[00] = "AF: 0x" + emulator.bus.register.AF.ToString("X4") + flags;
            debug[01] = "BC: 0x" + emulator.bus.register.BC.ToString("X4");
            debug[02] = "DE: 0x" + emulator.bus.register.DE.ToString("X4");
            debug[03] = "HL: 0x" + emulator.bus.register.HL.ToString("X4");
            debug[04] = "SP: 0x" + emulator.bus.register.SP.ToString("X4");
            debug[05] = "PC: 0x" + emulator.bus.register.PC.ToString("X4");
            debug[06] = "Ticks: " + emulator.cyclesRan;
            debug[07] = "LCDC:";
            debug[08] = " LCD enabled: " + (emulator.bus.gpu.LCDC.GetBit(LCDCBit.LCDEnabled) ? "On" : "Off");
            debug[09] = " Background and Window: " + (emulator.bus.gpu.LCDC.GetBit(LCDCBit.BackgroundEnabled) ? "On" : "Off");
            debug[10] = " Sprites: " + (emulator.bus.gpu.LCDC.GetBit(LCDCBit.SpritesEnabled) ? "On" : "Off");
            debug[11] = " SpriteSize: " + (emulator.bus.gpu.LCDC.GetBit(LCDCBit.SpritesSize) ? "8x16" : "8x8");
            debug[12] = " Background tilemap: " + (emulator.bus.gpu.LCDC.GetBit(LCDCBit.Tilemap) ? "9C00-9FFF" : "9800-9BFF");
            debug[13] = " Background tileset: " + (emulator.bus.gpu.LCDC.GetBit(LCDCBit.Tileset) ? "8000-8FFF" : "8800-97FF");
            debug[14] = " Window: " + (emulator.bus.gpu.LCDC.GetBit(LCDCBit.WindowEnabled) ? "On" : "Off");
            debug[15] = " Window tilemap: " + (emulator.bus.gpu.LCDC.GetBit(LCDCBit.WindowTilemap) ? "9C00-9FFF" : "9800-9BFF");
            debug[16] = "STAT:";
            debug[17] = " Mode:     " + emulator.bus.gpu.StatMode.ToString();
            debug[18] = " LYC Flag: " + (emulator.bus.gpu.Stat.GetBit(StatBit.CoincidenceFlag) ? "LYC=LY" : "LYC!=LY");
            debug[19] = " H-Blank:  " + (emulator.bus.gpu.Stat.GetBit(StatBit.HBlankInterrupt) ? "1" : "0");
            debug[20] = " V-Blank:  " + (emulator.bus.gpu.Stat.GetBit(StatBit.VBlankInterrupt) ? "1" : "0");
            debug[21] = " OAM:      " + (emulator.bus.gpu.Stat.GetBit(StatBit.OAMInterrupt) ? "1" : "0");
            debug[22] = " LYC:      " + (emulator.bus.gpu.Stat.GetBit(StatBit.CoincidenceInterrupt) ? "1" : "0");
            debug[23] = "LY: " + emulator.bus.gpu.LY;
            debug[24] = "LYC: " + emulator.bus.gpu.LYC;
            debug[25] = "Window Pos: " + emulator.bus.gpu.WX + " , " + emulator.bus.gpu.WY;
            debug[26] = "Scroll Pos: " + emulator.bus.gpu.SCX + " , " + emulator.bus.gpu.SCY;

            for(int i = 0; i < debug.Length; i++)
            {
                surface.DrawString(debug[i], new Vector2f(0, 10 + (20 * i)));
            }
        }

        private void Resize(Vector2u windowSize)
        {
            int scale = (int)(mainWindow.Height / WindowHeight);

            screenSprite.Scale = new Vector2f(scale, scale);
            screenSprite.Position = new Vector2f(
                (windowSize.X / 2) - (screenSprite.Scale.X * WindowWidth / 2),
                (windowSize.Y / 2) - (screenSprite.Scale.Y * WindowHeight / 2) + 8);
        }
    }
}