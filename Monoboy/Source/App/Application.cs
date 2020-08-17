using System.IO;
using Monoboy.Constants;
using Monoboy.Frontend;
using Monoboy.Utility;
using Newtonsoft.Json.Linq;
using SFML.Graphics;
using SFML.System;
using static Monoboy.Constants.Constant;
using Window = Monoboy.Frontend.Window;

namespace Monoboy
{
    public class Application
    {
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
        private JObject opcodes;
        private bool paused = true;

        public static bool Focused = true;
        private string[] debug = new string[32];

        public Application()
        {
            emulator = new Emulator();

            opcodes = JObject.Parse(File.ReadAllText("Data/opcodes.json"));

            debugWindow = new Window("Monoboy - Debug", WindowWidth * 4, WindowHeight * 4);
            debugWindow.Position += new Vector2i(340, 0);
            mainWindow = new Window("Monoboy", WindowWidth * 4, WindowHeight * 4);
            mainWindow.Position += new Vector2i(-340, 0);

            screen = new Texture(WindowWidth, WindowHeight);

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

            screenSprite = new Sprite(screen)
            {
                Position = new Vector2f(0, 0),
                Scale = new Vector2f(4, 4)
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
            tilemapDumpButton = new Keybind(Action.Pressed, Button.Num1);
            backgroundDumpButton = new Keybind(Action.Pressed, Button.Num2);
            memoryDumpButton = new Keybind(Action.Pressed, Button.Num3);
            traceDumpButton = new Keybind(Action.Pressed, Button.Num4);
            pauseButton = new Keybind(Action.Pressed, Button.P);
            stepButton = new Keybind(Action.Pressed, Button.Enter);
            speedupButton = new Keybind(Action.Held, Button.V);
            runButton = new Keybind(Action.Held, Button.LeftShift);

            debugWindow.Draw += DebugDraw;

            while(mainWindow.Open && debugWindow.Open)
            {
                Focused = debugWindow.Focused || mainWindow.Focused;
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

            if(traceDumpButton.IsActive() == true)
            {
                emulator.DumpTrace();
            }

            if(pauseButton.IsActive() == true)
            {
                paused = !paused;
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

            if(paused == false)
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

            string hexcode = emulator.bus.Read(emulator.bus.register.PC).ToString("x");

            string opcode = (string)opcodes.SelectToken("unprefixed." + "0x" + hexcode + ".mnemonic");
            string operand1 = (string)opcodes.SelectToken("unprefixed." + "0x" + hexcode + ".operand1");
            string operand2 = (string)opcodes.SelectToken("unprefixed." + "0x" + hexcode + ".operand2");

            string firstHex = emulator.bus.Read((ushort)(emulator.bus.register.PC + 1)).ToString("X2");
            string secondHex = emulator.bus.Read((ushort)(emulator.bus.register.PC + 2)).ToString("X2");

            operand2 = operand2?.Replace("d16", "0x" + secondHex + firstHex);
            operand2 = operand2?.Replace("a16", "0x" + secondHex + firstHex);
            operand2 = operand2?.Replace("a8", "0x" + firstHex);
            operand2 = operand2?.Replace("r8", "0x" + firstHex);
            operand2 = operand2?.Replace("d8", "0x" + firstHex);

            operand1 = operand1?.Replace("d16", "0x" + secondHex + firstHex);
            operand1 = operand1?.Replace("a16", "0x" + secondHex + firstHex);
            operand1 = operand1?.Replace("a8", "0x" + firstHex);
            operand1 = operand1?.Replace("r8", "0x" + firstHex);
            operand1 = operand1?.Replace("d8", "0x" + firstHex);

            string flags = "(" +
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
            debug[05] = "PC: 0x" + emulator.bus.register.PC.ToString("X4") + "(0x" + hexcode + ") " + opcode + " " + operand1 + ", " + operand2;
            debug[06] = "Ticks: " + emulator.cyclesRan;
            debug[07] = "LCDC:";
            debug[08] = " LCD enabled: " + (emulator.bus.memory.LCDC.GetBit(LCDCBit.LCDEnabled) ? "On" : "Off");
            debug[09] = " Background and Window: " + (emulator.bus.memory.LCDC.GetBit(LCDCBit.BackgroundWindowPriority) ? "On" : "Off");
            debug[10] = " Sprites: " + (emulator.bus.memory.LCDC.GetBit(LCDCBit.SpritesEnabled) ? "On" : "Off");
            debug[11] = " SpriteSize: " + (emulator.bus.memory.LCDC.GetBit(LCDCBit.SpritesSize) ? "8x16" : "8x8");
            debug[12] = " Background tilemap: " + (emulator.bus.memory.LCDC.GetBit(LCDCBit.Tilemap) ? "9C00-9FFF" : "9800-9BFF");
            debug[13] = " Background tileset: " + (emulator.bus.memory.LCDC.GetBit(LCDCBit.Tileset) ? "8000-8FFF" : "8800-97FF");
            debug[14] = " Window: " + (emulator.bus.memory.LCDC.GetBit(LCDCBit.WindowEnabled) ? "On" : "Off");
            debug[15] = " Window tilemap: " + (emulator.bus.memory.LCDC.GetBit(LCDCBit.WindowTilemap) ? "9C00-9FFF" : "9800-9BFF");
            debug[16] = "STAT:";
            debug[17] = " Mode:     " + emulator.bus.memory.StatMode.ToString();
            debug[18] = " LYC Flag: " + (emulator.bus.memory.Stat.GetBit(StatBit.CoincidenceFlag) ? "LYC=LY" : "LYC!=LY");
            debug[19] = " H-Blank:  " + (emulator.bus.memory.Stat.GetBit(StatBit.HBlankInterrupt) ? "1" : "0");
            debug[20] = " V-Blank:  " + (emulator.bus.memory.Stat.GetBit(StatBit.VBlankInterrupt) ? "1" : "0");
            debug[21] = " OAM:      " + (emulator.bus.memory.Stat.GetBit(StatBit.OAMInterrupt) ? "1" : "0");
            debug[22] = " LYC:      " + (emulator.bus.memory.Stat.GetBit(StatBit.CoincidenceInterrupt) ? "1" : "0");
            debug[23] = "LY: " + emulator.bus.memory.LY;
            debug[24] = "LYC: " + emulator.bus.memory.LYC;
            debug[25] = "Window Pos: " + emulator.bus.memory.WindowX + " , " + emulator.bus.memory.WindowY;
            debug[26] = "Scroll Pos: " + emulator.bus.memory.SCX + " , " + emulator.bus.memory.SCY;

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
                (windowSize.X / 2) - ((screenSprite.Scale.X * WindowWidth) / 2),
                (windowSize.Y / 2) - ((screenSprite.Scale.Y * WindowHeight) / 2));
        }
    }
}