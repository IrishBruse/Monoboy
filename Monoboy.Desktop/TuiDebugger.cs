namespace Monoboy.Desktop;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Monoboy.Disassembler;

using Terminal.Gui;

using TuiApp = Terminal.Gui.Application;

public static class TuiDebugger
{
    static readonly ColorScheme NativeScheme = new()
    {
        Normal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
        Focus = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
        HotNormal = new Terminal.Gui.Attribute(Color.White, Color.Black),
        HotFocus = new Terminal.Gui.Attribute(Color.White, Color.Black),
        Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
    };

    sealed class UiRefs
    {
        public required Label Header { get; init; }
        public required TextView Disassembly { get; init; }
        public required TextView Memory { get; init; }
        public required TextView Cpu { get; init; }
        public required TextView Lcd { get; init; }
        public required TextView LcdInternal { get; init; }
        public required TextView VramDma { get; init; }
        public required TextView Interrupts { get; init; }
        public required TextView Serial { get; init; }
        public required TextView Timer { get; init; }
        public required TextView Ch1 { get; init; }
        public required TextView Ch2 { get; init; }
        public required TextView Ch3 { get; init; }
        public required TextView Ch4 { get; init; }
        public required TextView Wave { get; init; }
        public required TextView Sound { get; init; }
    }

    public static void Run(string[] args)
    {
        bool logHeader = args.Contains("--log-header");
        var emulator = new Emulator
        {
            LogCartridgeHeader = logHeader
        };

        string romPath = args.FirstOrDefault(x => !x.StartsWith("--", StringComparison.Ordinal)) ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(romPath) && File.Exists(romPath))
        {
            emulator.Open(romPath);
        }
        else
        {
            emulator.Open(new byte[0x10000]);
        }

        TuiApp.Init();
        var top = TuiApp.Top;
        top.ColorScheme = NativeScheme;
        var window = new Window("Monoboy Debugger")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        window.ColorScheme = NativeScheme;
        top.Add(window);

        UiRefs ui = BuildUi(window);

        window.KeyPress += e =>
        {
            switch (e.KeyEvent.Key)
            {
                case Key.S:
                emulator.Step();
                break;
                case Key.F:
                emulator.StepFrame();
                break;
                case Key.R:
                for (int i = 0; i < 500; i++)
                {
                    emulator.Step();
                }
                break;
                case Key.Q:
                case Key.Esc:
                TuiApp.RequestStop();
                break;
                default:
                return;
            }

            Render(emulator, romPath, ui);
            e.Handled = true;
        };

        int lastWidth = Console.WindowWidth;
        int lastHeight = Console.WindowHeight;
        TuiApp.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(120), _ =>
        {
            if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
            {
                lastWidth = Console.WindowWidth;
                lastHeight = Console.WindowHeight;
                window.SetNeedsDisplay();
            }
            Render(emulator, romPath, ui);
            return true;
        });

        Render(emulator, romPath, ui);
        ui.Disassembly.SetFocus();
        TuiApp.Run();
        TuiApp.Shutdown();
    }

    static UiRefs BuildUi(Window window)
    {
        var header = new Label(string.Empty)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1
        };
        header.ColorScheme = NativeScheme;
        window.Add(header);

        var top = new View
        {
            X = 0,
            Y = Pos.Bottom(header),
            Width = Dim.Fill(),
            Height = Dim.Percent(62)
        };
        top.ColorScheme = NativeScheme;
        var memoryFrame = new FrameView("Memory")
        {
            X = 0,
            Y = Pos.Bottom(top),
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        memoryFrame.ColorScheme = NativeScheme;
        window.Add(top, memoryFrame);

        var left = new View { X = 0, Y = 0, Width = Dim.Percent(34), Height = Dim.Fill() };
        var col2 = new View { X = Pos.Right(left), Y = 0, Width = Dim.Percent(20), Height = Dim.Fill() };
        var col3 = new View { X = Pos.Right(col2), Y = 0, Width = Dim.Percent(16), Height = Dim.Fill() };
        var col4 = new View { X = Pos.Right(col3), Y = 0, Width = Dim.Percent(15), Height = Dim.Fill() };
        var col5 = new View { X = Pos.Right(col4), Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
        left.ColorScheme = NativeScheme;
        col2.ColorScheme = NativeScheme;
        col3.ColorScheme = NativeScheme;
        col4.ColorScheme = NativeScheme;
        col5.ColorScheme = NativeScheme;
        top.Add(left, col2, col3, col4, col5);

        var ppu = Panel(left, "PPU", 0, 52);
        var disassembly = Panel(left, "Disassembly", 52, 100);
        var lcd = Panel(col2, "LCD", 0, 52);
        var lcdInternal = Panel(col2, "LCD (internal)", 52, 76);
        var vramDma = Panel(col2, "vRAM DMA", 76, 100);
        var cpu = Panel(col3, "CPU", 0, 37);
        var interrupts = Panel(col3, "Interrupts", 37, 62);
        var serial = Panel(col3, "Serial Port", 62, 81);
        var timer = Panel(col3, "Timer", 81, 100);
        var ch1 = Panel(col4, "Ch1 (Square)", 0, 30);
        var ch2 = Panel(col4, "Ch2 (Square)", 30, 56);
        var wave = Panel(col4, "Wave RAM", 56, 100);
        var ch3 = Panel(col5, "Ch3 (Wave)", 0, 30);
        var ch4 = Panel(col5, "Ch4 (Noise)", 30, 56);
        var sound = Panel(col5, "Sound Ctrl", 56, 100);
        var memory = CreateTextView(memoryFrame);

        ppu.Text = "PPU preview in TUI not implemented.";

        return new UiRefs
        {
            Header = header,
            Disassembly = disassembly,
            Memory = memory,
            Cpu = cpu,
            Lcd = lcd,
            LcdInternal = lcdInternal,
            VramDma = vramDma,
            Interrupts = interrupts,
            Serial = serial,
            Timer = timer,
            Ch1 = ch1,
            Ch2 = ch2,
            Ch3 = ch3,
            Ch4 = ch4,
            Wave = wave,
            Sound = sound
        };
    }

    static TextView Panel(View parent, string title, int yStartPercent, int yEndPercent)
    {
        var frame = new FrameView(title)
        {
            X = 0,
            Y = Pos.Percent(yStartPercent),
            Width = Dim.Fill(),
            Height = Dim.Percent(Math.Max(8, yEndPercent - yStartPercent))
        };
        frame.ColorScheme = NativeScheme;
        parent.Add(frame);
        return CreateTextView(frame);
    }

    static TextView CreateTextView(View parent)
    {
        var tv = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            WordWrap = false
        };
        tv.ColorScheme = NativeScheme;
        tv.CanFocus = true;
        tv.AllowsTab = false;
        tv.BottomOffset = 0;
        tv.RightOffset = 0;
        parent.Add(tv);
        return tv;
    }

    static void Render(Emulator emulator, string romPath, UiRefs ui)
    {
        var s = emulator.GetDebugState();

        ui.Header.Text = $"Run  PPU | ROM: {(string.IsNullOrWhiteSpace(romPath) ? "<empty>" : romPath)} | [S]tep [F]rame [R]un(500) [Q]/[Esc] Quit";

        var disassembly = new List<string>();
        ushort cursor = s.PC;
        for (int i = 0; i < 20; i++)
        {
            string line = DecodeLine(emulator, cursor, out ushort size);
            disassembly.Add(i == 0 ? $"> {line}" : $"  {line}");
            cursor += size;
        }
        ui.Disassembly.Text = string.Join('\n', disassembly);

        ui.Cpu.Text = string.Join('\n', new[]
        {
            $"AF: {s.AF:X4}",
            $"BC: {s.BC:X4}",
            $"DE: {s.DE:X4}",
            $"HL: {s.HL:X4}",
            $"PC: {s.PC:X4}",
            $"SP: {s.SP:X4}",
        });

        ui.Lcd.Text = string.Join('\n', new[]
        {
            $"FF40 LCDC: {emulator.Read(0xFF40):X2}",
            $"FF41 STAT: {emulator.Read(0xFF41):X2}",
            $"FF42 SCY:  {emulator.Read(0xFF42):X2}",
            $"FF43 SCX:  {emulator.Read(0xFF43):X2}",
            $"FF44 LY:   {emulator.Read(0xFF44):X2}",
            $"FF45 LYC:  {emulator.Read(0xFF45):X2}",
            $"FF46 DMA:  {emulator.Read(0xFF46):X2}",
            $"FF47 BGP:  {emulator.Read(0xFF47):X2}",
            $"FF48 OBP0: {emulator.Read(0xFF48):X2}",
            $"FF49 OBP1: {emulator.Read(0xFF49):X2}",
            $"FF4A WY:   {emulator.Read(0xFF4A):X2}",
            $"FF4B WX:   {emulator.Read(0xFF4B):X2}",
        });

        ui.LcdInternal.Text = string.Join('\n', new[]
        {
            $"VBlank: {(emulator.Read(0xFF44) >= 0x90 ? "yes" : "no")}",
            $"Dots:   {(s.TotalCycles % 456)}",
            $"Mode:   {(emulator.Read(0xFF41) & 0x3)}",
            $"Next:   {(456 - (s.TotalCycles % 456))}",
        });

        ui.VramDma.Text = string.Join('\n', new[]
        {
            $"FF51-52 Src:  {emulator.Read(0xFF51):X2}{emulator.Read(0xFF52):X2}",
            $"FF53-54 Dest: {emulator.Read(0xFF53):X2}{emulator.Read(0xFF54):X2}",
            $"FF55   Len:   {emulator.Read(0xFF55):X2}",
        });

        ui.Interrupts.Text = string.Join('\n', new[]
        {
            $"FF0F IF: {s.IF:X2}",
            $"FF4D KEY1: {emulator.Read(0xFF4D):X2}",
            $"FFFF IE: {s.IE:X2}",
            $"IME: {(s.Ime ? "on" : "off")}",
        });

        ui.Serial.Text = $"FF01 SB: {emulator.Read(0xFF01):X2}\nFF02 SC: {emulator.Read(0xFF02):X2}";
        ui.Timer.Text = string.Join('\n', new[]
        {
            $"FF04 DIV: {emulator.Read(0xFF04):X2}",
            $"FF05 TIMA: {emulator.Read(0xFF05):X2}",
            $"FF06 TMA: {emulator.Read(0xFF06):X2}",
            $"FF07 TAC: {emulator.Read(0xFF07):X2}",
        });

        ui.Ch1.Text = string.Join('\n', ReadRange(emulator, 0xFF10, 5));
        ui.Ch2.Text = string.Join('\n', ReadRange(emulator, 0xFF16, 4));
        ui.Ch3.Text = string.Join('\n', ReadRange(emulator, 0xFF1A, 5));
        ui.Ch4.Text = string.Join('\n', ReadRange(emulator, 0xFF20, 4));

        ui.Wave.Text = string.Join('\n', new[]
        {
            $"{emulator.Read(0xFF30):X2} {emulator.Read(0xFF31):X2} {emulator.Read(0xFF32):X2} {emulator.Read(0xFF33):X2}",
            $"{emulator.Read(0xFF34):X2} {emulator.Read(0xFF35):X2} {emulator.Read(0xFF36):X2} {emulator.Read(0xFF37):X2}",
            $"{emulator.Read(0xFF38):X2} {emulator.Read(0xFF39):X2} {emulator.Read(0xFF3A):X2} {emulator.Read(0xFF3B):X2}",
            $"{emulator.Read(0xFF3C):X2} {emulator.Read(0xFF3D):X2} {emulator.Read(0xFF3E):X2} {emulator.Read(0xFF3F):X2}",
        });

        ui.Sound.Text = string.Join('\n', new[]
        {
            $"FF24 NR50: {emulator.Read(0xFF24):X2}",
            $"FF25 NR51: {emulator.Read(0xFF25):X2}",
            $"FF26 NR52: {emulator.Read(0xFF26):X2}",
        });

        ui.Memory.Text = BuildMemoryBlock(emulator, s.PC, 12);
    }

    static string[] ReadRange(Emulator emulator, ushort start, int length)
    {
        var list = new List<string>(length);
        for (int i = 0; i < length; i++)
        {
            ushort addr = (ushort)(start + i);
            list.Add($"{addr:X4}: {emulator.Read(addr):X2}");
        }
        return list.ToArray();
    }

    static string BuildMemoryBlock(Emulator emulator, ushort pc, int rows)
    {
        ushort start = (ushort)(pc & 0xFF00);
        var memRows = new List<string>(rows);
        for (int row = 0; row < rows; row++)
        {
            ushort addr = (ushort)(start + (row * 16));
            var hex = new StringBuilder();
            var ascii = new StringBuilder();
            for (int col = 0; col < 16; col++)
            {
                byte val = emulator.Read((ushort)(addr + col));
                if (col == 8)
                {
                    hex.Append("| ");
                }
                hex.Append($"{val:X2} ");
                ascii.Append(val >= 32 && val < 127 ? (char)val : '.');
            }
            memRows.Add($"{addr:X4}  {hex}| {ascii}");
        }
        return string.Join('\n', memRows);
    }

    static string DecodeLine(Emulator emulator, ushort pc, out ushort size)
    {
        byte op = emulator.Read(pc);
        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            size = 1;
            return $"{pc:X4}: {op:X2}  DB ${op:X2}";
        }

        size = instruction.Bytes;
        string bytes = string.Join(' ', Enumerable.Range(0, instruction.Bytes).Select(x => emulator.Read((ushort)(pc + x)).ToString("X2")));
        string mnemonic = instruction.Mnemonic.ToString();
        string operands = string.Join(", ", instruction.Operands.Select(o => FormatOperand(o, emulator, pc)));
        return $"{pc:X4}: {bytes,-8} {mnemonic} {operands}".TrimEnd();
    }

    static string FormatOperand(Operand operand, Emulator emulator, ushort pc)
    {
        return operand switch
        {
            Operand.n8 => $"${emulator.Read((ushort)(pc + 1)):X2}",
            Operand.n16 or Operand.a16 => $"${emulator.Read((ushort)(pc + 2)):X2}{emulator.Read((ushort)(pc + 1)):X2}",
            Operand.e8 => ((sbyte)emulator.Read((ushort)(pc + 1))).ToString(),
            Operand.a8 => $"$FF{emulator.Read((ushort)(pc + 1)):X2}",
            _ => operand.ToString()
        };
    }
}
