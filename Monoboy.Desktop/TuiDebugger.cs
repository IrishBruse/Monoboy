namespace Monoboy.Desktop;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Monoboy;
using Monoboy.Disassembler;

using Spectre.Console;

/// <summary>
/// Debugger TUI: left = PPU + disassembly (full height); center = register grid; right = memory dump
/// (full height, flush right). Resize uses Console size each frame.
/// </summary>
public static class TuiDebugger
{
    const ushort MemoryBase = 0x0000;

    static readonly Regex SpectreTag = new(@"\[[^\]]*\]", RegexOptions.Compiled);

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

        int disasmSkip = 0;
        int memRowSkip = 0;
        bool quit = false;

        while (!quit)
        {
            int w = Math.Max(Console.WindowWidth, 40);
            int h = Math.Max(Console.WindowHeight, 12);

            Console.Clear();
            DrawFrame(emulator, romPath, w, h, disasmSkip, memRowSkip);

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                switch (key.Key)
                {
                    case ConsoleKey.S:
                    emulator.Step();
                    disasmSkip = 0;
                    break;
                    case ConsoleKey.F:
                    emulator.StepFrame();
                    disasmSkip = 0;
                    break;
                    case ConsoleKey.R:
                    for (int i = 0; i < 500; i++)
                    {
                        emulator.Step();
                    }
                    disasmSkip = 0;
                    break;
                    case ConsoleKey.Q:
                    case ConsoleKey.Escape:
                    quit = true;
                    break;
                    case ConsoleKey.PageDown:
                    disasmSkip += 3;
                    break;
                    case ConsoleKey.PageUp:
                    disasmSkip = Math.Max(0, disasmSkip - 3);
                    break;
                    case ConsoleKey.Oem4:
                    memRowSkip = Math.Max(0, memRowSkip - 4);
                    break;
                    case ConsoleKey.Oem6:
                    memRowSkip += 4;
                    break;
                    case ConsoleKey.OemComma:
                    memRowSkip = Math.Max(0, memRowSkip - 4);
                    break;
                    case ConsoleKey.OemPeriod:
                    memRowSkip += 4;
                    break;
                    case ConsoleKey.Home:
                    disasmSkip = 0;
                    memRowSkip = 0;
                    break;
                    default:
                    break;
                }
            }
            else
            {
                Thread.Sleep(40);
            }
        }

        Console.Clear();
    }

    static void DrawFrame(Emulator emulator, string romPath, int termW, int termH, int disasmSkip, int memRowSkip)
    {
        var s = emulator.GetDebugState();
        string rom = string.IsNullOrWhiteSpace(romPath) ? "<empty>" : romPath;

        // Last row is pinned footer; everything else is main content (no top header).
        int maxContentLines = Math.Max(8, termH - 1);

        // Keep register rows close to real content depth to avoid huge empty area.
        const int registerContentRows = 19;
        int registerRows = Math.Min(maxContentLines, registerContentRows + 2);

        int gap = 1;
        int leftPct = 26;
        int leftInner = termW * leftPct / 100;
        leftInner = Math.Clamp(leftInner, 16, Math.Max(16, termW - 20));
        int rowBudget = Math.Max(12, termW - leftInner - 2 * gap - 2);
        // Center = register grid; right = memory (far-right column, full height).
        int memoryWidth = Math.Clamp(rowBudget * 42 / 100, 28, 72);
        int midInner = rowBudget - memoryWidth;
        if (midInner < 12)
        {
            midInner = 12;
            memoryWidth = Math.Max(16, rowBudget - midInner);
        }

        int[] subWeights = [19, 19, 19, 19];
        int[] colW = DistributeWidths(midInner, gap, subWeights);

        var disasmLines = BuildDisassemblyLines(emulator, s.PC, disasmSkip, maxContentLines + 4);

        int ppuInnerWidth = Math.Max(8, leftInner - 2);
        // Terminal cells are taller than they are wide, so use ~2.22 cols per row for GB aspect.
        int ppuPreviewRows = Math.Clamp((int)Math.Round(ppuInnerWidth / 2.22), 6, Math.Max(6, maxContentLines - 6));
        int ppuRows = Math.Min(maxContentLines - 2, ppuPreviewRows + 1);

        for (int r = 0; r < maxContentLines; r++)
        {
            string left;
            if (r < ppuRows)
            {
                if (r == 0)
                {
                    left = PadMarkup("[green]PPU[/]", leftInner);
                }
                else
                {
                    string bar = new string('█', ppuInnerWidth);
                    left = PadMarkup("[green]" + bar + "[/]", leftInner);
                }
            }
            else if (r == ppuRows)
            {
                left = PadMarkup("[yellow]── Disassembly ──[/]", leftInner);
            }
            else
            {
                int idx = r - ppuRows - 1;
                left = idx >= 0 && idx < disasmLines.Count
                    ? PadMarkup(ClipMarkup(disasmLines[idx], leftInner), leftInner)
                    : new string(' ', leftInner);
            }

            string mid = r < registerRows
                ? PadMarkup(ClipMarkup(BuildRegisterGridRow(emulator, s, r, colW, gap), midInner), midInner)
                : new string(' ', midInner);

            string memLine = BuildMemoryLine(emulator, MemoryBase, memRowSkip + r, memoryWidth);
            string mem = PadMarkupLeft(ClipMarkup(memLine, memoryWidth), memoryWidth);

            WriteMarkupLine(left + new string(' ', gap) + mid + new string(' ', gap) + mem);
        }

        DrawPinnedFooter(rom, termW, termH);
    }

    /// <summary>Last screen row: ROM + keys (plain); no trailing newline so it stays pinned.</summary>
    static void DrawPinnedFooter(string rom, int termW, int termH)
    {
        int row = Math.Min(termH - 1, Math.Max(0, Console.WindowHeight - 1));
        int width = Math.Max(1, Math.Min(termW, Console.WindowWidth));
        Console.SetCursorPosition(0, row);

        const string keys = "  S step  F frame  R run  Q quit  PgUp/PgDn  , . mem  Home";
        string romEsc = Markup.Escape(rom);
        int prefixLen = 5;
        int romMax = Math.Max(8, width - prefixLen - keys.Length);
        string line = "ROM: " + ClipPlain(romEsc, romMax) + keys;
        if (line.Length > width)
        {
            line = line[..width];
        }

        Console.Write(line.PadRight(width));
    }

    static void WriteMarkupLine(string line)
    {
        try
        {
            AnsiConsole.MarkupLine(line);
        }
        catch (Exception)
        {
            AnsiConsole.WriteLine(SpectreTag.Replace(line, ""));
        }
    }

    /// <summary>Build disassembly strings; line containing current PC gets leading > and markup highlight.</summary>
    static List<string> BuildDisassemblyLines(Emulator emulator, ushort pc, int skip, int needLines)
    {
        var lines = new List<string>();
        ushort cursor = pc;
        int consumed = 0;
        while (consumed < skip)
        {
            _ = DecodeLinePretty(emulator, cursor, out ushort sz);
            cursor += sz;
            consumed++;
        }

        int count = Math.Max(needLines + 4, 40);
        for (int i = 0; i < count; i++)
        {
            string raw = DecodeLinePretty(emulator, cursor, out ushort size);
            bool atPc = cursor == pc;
            string prefix = atPc ? "> " : "  ";
            string safe = Markup.Escape(raw);
            string line = atPc
                ? prefix + "[cyan]" + safe + "[/]"
                : prefix + safe;
            lines.Add(line);
            cursor += size;
        }
        return lines;
    }

    static string BuildRegisterGridRow(Emulator emulator, DebugState s, int row, int[] colW, int gap)
    {
        string c1 = PadMarkup(ClipMarkup(LcdLine(emulator, s, row), colW[0]), colW[0]);
        string c2 = PadMarkup(ClipMarkup(CpuIrqSerialTimerLine(emulator, s, row), colW[1]), colW[1]);
        string c3 = PadMarkup(ClipMarkup(Ch12WaveLine(emulator, row), colW[2]), colW[2]);
        string c4 = PadMarkup(ClipMarkup(Ch34SoundLine(emulator, row), colW[3]), colW[3]);
        return c1 + new string(' ', gap) + c2 + new string(' ', gap) + c3 + new string(' ', gap) + c4;
    }

    static string LcdLine(Emulator emulator, DebugState s, int r)
    {
        string y = "[yellow]";
        string x = "[/]";
        return r switch
        {
            0 => $"{y}LCD{x}",
            1 => $"[bold]FF40[/] LCDC [cyan]{emulator.Read(0xFF40):X2}[/]",
            2 => $"[bold]FF41[/] STAT [cyan]{emulator.Read(0xFF41):X2}[/]  [bold]FF42[/] SCY [cyan]{emulator.Read(0xFF42):X2}[/]",
            3 => $"[bold]FF43[/] SCX [cyan]{emulator.Read(0xFF43):X2}[/]  [bold]FF44[/] LY [cyan]{emulator.Read(0xFF44):X2}[/]",
            4 => $"[bold]FF45[/] LYC [cyan]{emulator.Read(0xFF45):X2}[/]  DMA [cyan]{emulator.Read(0xFF46):X2}[/]",
            5 => $"BGP [cyan]{emulator.Read(0xFF47):X2}[/] OBP0 [cyan]{emulator.Read(0xFF48):X2}[/]",
            6 => $"OBP1 [cyan]{emulator.Read(0xFF49):X2}[/] WY [cyan]{emulator.Read(0xFF4A):X2}[/]",
            7 => $"WX [cyan]{emulator.Read(0xFF4B):X2}[/]",
            8 => $"{y}LCD (internal){x}",
            9 => $"VBlank {(emulator.Read(0xFF44) >= 0x90 ? "yes" : "no")}  Dots {s.TotalCycles % 456}",
            10 => $"Mode {emulator.Read(0xFF41) & 0x3}  Next {(456 - (s.TotalCycles % 456))}",
            11 => $"{y}vRAM DMA{x}",
            12 => $"Src [cyan]{emulator.Read(0xFF51):X2}{emulator.Read(0xFF52):X2}[/]",
            13 => $"Dest [cyan]{emulator.Read(0xFF53):X2}{emulator.Read(0xFF54):X2}[/]  Len [cyan]{emulator.Read(0xFF55):X2}[/]",
            _ => "",
        };
    }

    static string CpuIrqSerialTimerLine(Emulator emulator, DebugState s, int r)
    {
        string y = "[yellow]";
        string x = "[/]";
        return r switch
        {
            0 => $"{y}CPU{x}",
            1 => $"AF [cyan]{s.AF:X4}[/]  BC [cyan]{s.BC:X4}[/]",
            2 => $"DE [cyan]{s.DE:X4}[/]  HL [cyan]{s.HL:X4}[/]",
            3 => $"PC [cyan]{s.PC:X4}[/]  SP [cyan]{s.SP:X4}[/]",
            4 => $"{y}Interrupts{x}",
            5 => $"IF [cyan]{s.IF:X2}[/] KEY1 [cyan]{emulator.Read(0xFF4D):X2}[/]",
            6 => $"IE [cyan]{s.IE:X2}[/]  IME [cyan]{(s.Ime ? "on" : "off")}[/]",
            7 => $"{y}Serial{x}",
            8 => $"SB [cyan]{emulator.Read(0xFF01):X2}[/]  SC [cyan]{emulator.Read(0xFF02):X2}[/]",
            9 => $"{y}Timer{x}",
            10 => $"DIV [cyan]{emulator.Read(0xFF04):X2}[/] TIMA [cyan]{emulator.Read(0xFF05):X2}[/]",
            11 => $"TMA [cyan]{emulator.Read(0xFF06):X2}[/] TAC [cyan]{emulator.Read(0xFF07):X2}[/]",
            _ => "",
        };
    }

    static string Ch12WaveLine(Emulator emulator, int r)
    {
        string y = "[yellow]";
        string x = "[/]";
        return r switch
        {
            0 => $"{y}Ch1 (Square){x}",
            1 => LineReg(emulator, 0xFF10, "NR10"),
            2 => LineReg(emulator, 0xFF11, "NR11"),
            3 => LineReg(emulator, 0xFF12, "NR12"),
            4 => LineReg(emulator, 0xFF13, "NR13"),
            5 => LineReg(emulator, 0xFF14, "NR14"),
            6 => $"{y}Ch2 (Square){x}",
            7 => LineReg(emulator, 0xFF16, "NR21"),
            8 => LineReg(emulator, 0xFF17, "NR22"),
            9 => LineReg(emulator, 0xFF18, "NR23"),
            10 => LineReg(emulator, 0xFF19, "NR24"),
            11 => $"{y}Wave RAM (FF30-F){x}",
            12 => WaveRow(emulator, 0),
            13 => WaveRow(emulator, 4),
            14 => WaveRow(emulator, 8),
            15 => WaveRow(emulator, 12),
            _ => "",
        };
    }

    static string LineReg(Emulator emulator, ushort a, string name) =>
        $"[bold]{a:X4}[/] {name} [cyan]{emulator.Read(a):X2}[/]";

    static string WaveRow(Emulator emulator, int i)
    {
        var sb = new StringBuilder();
        for (int j = 0; j < 4; j++)
        {
            ushort a = (ushort)(0xFF30 + i + j);
            sb.Append($"{emulator.Read(a):X2} ");
        }
        return sb.ToString().TrimEnd();

    }

    static string Ch34SoundLine(Emulator emulator, int r)
    {
        string y = "[yellow]";
        string x = "[/]";
        return r switch
        {
            0 => $"{y}Ch3 (Wave){x}",
            1 => LineReg(emulator, 0xFF1A, "NR30"),
            2 => LineReg(emulator, 0xFF1B, "NR31"),
            3 => LineReg(emulator, 0xFF1C, "NR32"),
            4 => LineReg(emulator, 0xFF1D, "NR33"),
            5 => LineReg(emulator, 0xFF1E, "NR34"),
            6 => $"{y}Ch4 (Noise){x}",
            7 => LineReg(emulator, 0xFF20, "NR41"),
            8 => LineReg(emulator, 0xFF21, "NR42"),
            9 => LineReg(emulator, 0xFF22, "NR43"),
            10 => LineReg(emulator, 0xFF23, "NR44"),
            11 => $"{y}Sound Ctrl{x}",
            12 => LineReg(emulator, 0xFF24, "NR50"),
            13 => LineReg(emulator, 0xFF25, "NR51"),
            14 => LineReg(emulator, 0xFF26, "NR52"),
            _ => "",
        };
    }

    static string BuildMemoryLine(Emulator emulator, ushort baseAddr, int rowIndex, int maxWidth)
    {
        ushort addr = (ushort)(baseAddr + rowIndex * 16);
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
        string asciiEsc = Markup.Escape(ascii.ToString());
        string line = $"[bold]{addr:X4}[/]  [cyan]{hex}[/]| {asciiEsc}";
        return ClipMarkup(line, maxWidth);
    }

    static int[] DistributeWidths(int total, int gap, int[] weights)
    {
        int n = weights.Length;
        int inner = total - (gap * (n - 1));
        inner = Math.Max(n * 5, inner);
        int sum = weights.Sum();
        int[] w = new int[n];
        for (int i = 0; i < n; i++)
        {
            w[i] = Math.Max(5, inner * weights[i] / sum);
        }
        return w;
    }

    static int VisibleLen(string markup) => SpectreTag.Replace(markup, "").Length;

    static string ClipPlain(string s, int maxLen) => s.Length <= maxLen ? s : s[..maxLen];

    static string ClipMarkup(string markup, int maxVisible)
    {
        int v = VisibleLen(markup);
        if (v <= maxVisible)
        {
            return markup;
        }
        var plain = SpectreTag.Replace(markup, "");
        return plain.Length <= maxVisible ? markup : plain[..maxVisible];
    }

    static string PadMarkup(string markup, int width)
    {
        int v = VisibleLen(markup);
        if (v >= width)
        {
            return ClipMarkup(markup, width);
        }
        return markup + new string(' ', width - v);
    }

    static string PadMarkupLeft(string markup, int width)
    {
        int v = VisibleLen(markup);
        if (v >= width)
        {
            return ClipMarkup(markup, width);
        }
        return new string(' ', width - v) + markup;
    }

    static string DecodeLinePretty(Emulator emulator, ushort pc, out ushort size)
    {
        byte op = emulator.Read(pc);
        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            size = 1;
            return $"{pc:X4}: {op:X2}        DB ${op:X2}";
        }

        size = instruction.Bytes;
        string bytes = string.Join(' ', Enumerable.Range(0, instruction.Bytes).Select(x => emulator.Read((ushort)(pc + x)).ToString("X2")));
        string pad = bytes.Length < 8 ? bytes + new string(' ', 8 - bytes.Length) : bytes;
        string mnemonic = instruction.Mnemonic.ToString();
        string operands = string.Join(' ', instruction.Operands.Select(o => FormatOperandPretty(o, emulator, pc)));
        return $"{pc:X4}: {pad} {mnemonic} {operands}".TrimEnd();
    }


    static string FormatOperandPretty(Operand operand, Emulator emulator, ushort pc)
    {
        return operand switch
        {
            Operand.n8 => $"${emulator.Read((ushort)(pc + 1)):X2}",
            Operand.n16 or Operand.a16 => $"{emulator.Read((ushort)(pc + 2)):X2}{emulator.Read((ushort)(pc + 1)):X2}",
            Operand.e8 => $"{(sbyte)emulator.Read((ushort)(pc + 1))}",
            Operand.a8 => $"FF{emulator.Read((ushort)(pc + 1)):X2}",
            _ => operand.ToString()
        };
    }
}
