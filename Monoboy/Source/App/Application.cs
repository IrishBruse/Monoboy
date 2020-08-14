using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Monoboy.Frontend;
using Monoboy.Utility;
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
        Keybind traceDumpButton;
        Keybind pauseButton;
        Keybind stepButton;
        Keybind speedupButton;
        Keybind runButton;

        JObject opcodes;

        bool paused = true;

        public static bool Focused = true;

        string[] debug = new string[32];

        public Application()
        {
            opcodes = JObject.Parse(File.ReadAllText("Data/opcodes.json"));

            debugWindow = new Window("Monoboy - Debug", Emulator.WindowWidth * Emulator.WindowScale, Emulator.WindowHeight * Emulator.WindowScale);
            debugWindow.Position += new Vector2i(340, 0);
            mainWindow = new Window("Monoboy", Emulator.WindowWidth * Emulator.WindowScale, Emulator.WindowHeight * Emulator.WindowScale);
            mainWindow.Position += new Vector2i(-340, 0);

            emulator = new Emulator();
            //emulator.bus.SkipBootRom();
            //emulator.LoadRom("Mario.gb");
            //emulator.LoadRom("Dr. Mario.gb");
            //emulator.LoadRom("Tetris.gb");
            //emulator.LoadRom("cpu_instrs.gb");
            //emulator.LoadRom("01-special.gb");
            //emulator.LoadRom("02-interrupts.gb");
            //emulator.LoadRom("03-op sp,hl.gb");
            //emulator.LoadRom("04.gb");
            //emulator.LoadRom("01-special.gb");
            //emulator.LoadRom("01-special.gb");
            emulator.LoadRom("07-jr,jp,call,ret,rst.gb");
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
            tilemapDumpButton = new Keybind(Action.Pressed, Button.Num1);
            backgroundDumpButton = new Keybind(Action.Pressed, Button.Num2);
            memoryDumpButton = new Keybind(Action.Pressed, Button.Num3);
            traceDumpButton = new Keybind(Action.Pressed, Button.Num4);
            pauseButton = new Keybind(Action.Pressed, Button.P);
            stepButton = new Keybind(Action.Pressed, Button.Enter);
            speedupButton = new Keybind(Action.Held, Button.V);
            runButton = new Keybind(Action.Held, Button.LeftShift);

            debugWindow.Draw += DebugDraw;

            //while(emulator.bus.register.PC != 0xc365)// 0xc365
            //{
            //    emulator.Step();
            //}

            //while(emulator.bus.memory.LY == 0)
            //{
            //    emulator.Step();
            //}

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
            (emulator.bus.register.F.GetBit((Bit)Flag.FullCarry) ? "C" : "-") +
            (emulator.bus.register.F.GetBit((Bit)Flag.HalfCarry) ? "H" : "-") +
            (emulator.bus.register.F.GetBit((Bit)Flag.Negative) ? "N" : "-") +
            (emulator.bus.register.F.GetBit((Bit)Flag.Zero) ? "Z" : "-") +
            ")";

            debug[00] = "AF: 0x" + emulator.bus.register.AF.ToString("X4") + flags;
            debug[01] = "BC: 0x" + emulator.bus.register.BC.ToString("X4");
            debug[02] = "DE: 0x" + emulator.bus.register.DE.ToString("X4");
            debug[03] = "HL: 0x" + emulator.bus.register.HL.ToString("X4");
            debug[04] = "SP: 0x" + emulator.bus.register.SP.ToString("X4");
            debug[05] = "PC: 0x" + emulator.bus.register.PC.ToString("X4") + "(0x" + hexcode + ") " + opcode + " " + operand1 + ", " + operand2;
            debug[06] = "Ticks: " + emulator.cyclesRan;
            debug[07] = "LCDC:";
            debug[08] = " LCD enabled: " + (emulator.bus.memory.LCDC.GetBit((Bit)LCDCBit.LCDEnabled) ? "On" : "Off");
            debug[09] = " Background and Window: " + (emulator.bus.memory.LCDC.GetBit((Bit)LCDCBit.BackgroundWindowPriority) ? "On" : "Off");
            debug[10] = " Sprites: " + (emulator.bus.memory.LCDC.GetBit((Bit)LCDCBit.SpritesEnabled) ? "On" : "Off");
            debug[11] = " SpriteSize: " + (emulator.bus.memory.LCDC.GetBit((Bit)LCDCBit.SpritesSize) ? "8x16" : "8x8");
            debug[12] = " Background tilemap: " + (emulator.bus.memory.LCDC.GetBit((Bit)LCDCBit.Tilemap) ? "9C00-9FFF" : "9800-9BFF");
            debug[13] = " Background tileset: " + (emulator.bus.memory.LCDC.GetBit((Bit)LCDCBit.Tileset) ? "8000-8FFF" : "8800-97FF");
            debug[14] = " Window: " + (emulator.bus.memory.LCDC.GetBit((Bit)LCDCBit.WindowEnabled) ? "On" : "Off");
            debug[15] = " Window tilemap: " + (emulator.bus.memory.LCDC.GetBit((Bit)LCDCBit.WindowTilemap) ? "9C00-9FFF" : "9800-9BFF");
            debug[16] = "STAT:";
            debug[17] = " Mode:     " + emulator.bus.memory.StatMode.ToString();
            debug[18] = " LYC Flag: " + (emulator.bus.memory.Stat.GetBit((Bit)StatBit.CoincidenceFlag) ? "LYC=LY" : "LYC!=LY");
            debug[19] = " H-Blank:  " + (emulator.bus.memory.Stat.GetBit((Bit)StatBit.HBlankInterrupt) ? "1" : "0");
            debug[20] = " V-Blank:  " + (emulator.bus.memory.Stat.GetBit((Bit)StatBit.VBlankInterrupt) ? "1" : "0");
            debug[21] = " OAM:      " + (emulator.bus.memory.Stat.GetBit((Bit)StatBit.OAMInterrupt) ? "1" : "0");
            debug[22] = " LYC:      " + (emulator.bus.memory.Stat.GetBit((Bit)StatBit.CoincidenceInterrupt) ? "1" : "0");
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
            int scale = (int)(mainWindow.Height / Emulator.WindowHeight);

            screenSprite.Scale = new Vector2f(scale, scale);
            screenSprite.Position = new Vector2f(
                (windowSize.X / 2) - ((screenSprite.Scale.X * Emulator.WindowWidth) / 2),
                (windowSize.Y / 2) - ((screenSprite.Scale.Y * Emulator.WindowHeight) / 2));
        }
    }
}