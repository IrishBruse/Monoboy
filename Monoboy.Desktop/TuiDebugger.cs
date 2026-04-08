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
/// Debugger TUI: left = disassembly; center = register grid; right = memory dump (full height, flush right).
/// Resize uses Console size each frame. Tab switches focus between disassembly and memory; arrow keys scroll the focused pane (disassembly can scroll before the current PC); Page Up/Down jump by ~one screen.
/// </summary>
public static class TuiDebugger
{
    const ushort MemoryBase = 0x0000;

    static readonly Regex SpectreTag = new(@"\[[^\]]*\]", RegexOptions.Compiled);

    enum DebuggerPaneFocus
    {
        Disassembly,
        Memory,
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

        int disasmSkip = 0;
        int memRowSkip = 0;
        var paneFocus = DebuggerPaneFocus.Disassembly;
        bool quit = false;
        bool needsRedraw = true;
        int lastTermW = -1;
        int lastTermH = -1;

        try
        {
            Console.CursorVisible = false;
            while (!quit)
            {
                int w = Math.Max(Console.WindowWidth, 40);
                int h = Math.Max(Console.WindowHeight, 12);
                bool resized = w != lastTermW || h != lastTermH;
                if (resized)
                {
                    lastTermW = w;
                    lastTermH = h;
                    needsRedraw = true;
                }

                if (Console.KeyAvailable)
                {
                    needsRedraw = true;
                    var key = Console.ReadKey(intercept: true);
                    int pageJump = Math.Max(8, h - 3);
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
                        case ConsoleKey.Tab:
                        paneFocus = paneFocus == DebuggerPaneFocus.Disassembly
                            ? DebuggerPaneFocus.Memory
                            : DebuggerPaneFocus.Disassembly;
                        break;
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.UpArrow:
                        if (paneFocus == DebuggerPaneFocus.Disassembly)
                        {
                            disasmSkip--;
                        }
                        else
                        {
                            memRowSkip = Math.Max(0, memRowSkip - 1);
                        }

                        break;
                        case ConsoleKey.RightArrow:
                        case ConsoleKey.DownArrow:
                        if (paneFocus == DebuggerPaneFocus.Disassembly)
                        {
                            disasmSkip++;
                        }
                        else
                        {
                            memRowSkip++;
                        }

                        break;
                        case ConsoleKey.PageDown:
                        if (paneFocus == DebuggerPaneFocus.Disassembly)
                        {
                            disasmSkip += pageJump;
                        }
                        else
                        {
                            memRowSkip += pageJump;
                        }

                        break;
                        case ConsoleKey.PageUp:
                        if (paneFocus == DebuggerPaneFocus.Disassembly)
                        {
                            disasmSkip -= pageJump;
                        }
                        else
                        {
                            memRowSkip = Math.Max(0, memRowSkip - pageJump);
                        }

                        break;
                        case ConsoleKey.Home:
                        disasmSkip = 0;
                        memRowSkip = 0;
                        break;
                        case ConsoleKey.V:
                        FramebufferPreviewWindow.Show(emulator);
                        break;
                        default:
                        break;
                    }
                }
                else if (!needsRedraw)
                {
                    Thread.Sleep(50);
                    continue;
                }

                if (quit)
                {
                    break;
                }

                if (resized)
                {
                    Console.Clear();
                }
                else
                {
                    Console.SetCursorPosition(0, 0);
                }

                DrawFrame(emulator, w, h, disasmSkip, memRowSkip, paneFocus);
                needsRedraw = false;
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }

        Console.Clear();
    }

    static void DrawFrame(Emulator emulator, int termW, int termH, int disasmSkip, int memRowSkip, DebuggerPaneFocus paneFocus)
    {
        var s = emulator.GetDebugState();

        // Last row is pinned footer; everything else is main content (no top header).
        int maxContentLines = Math.Max(8, termH - 1);

        // Keep register rows close to real content depth to avoid huge empty area.
        const int registerContentRows = 24;
        int registerRows = Math.Min(maxContentLines, registerContentRows);

        int gap = 1;
        int leftPct = 26;
        int leftInner = termW * leftPct / 100;
        // Leave enough columns for register grid + memory; shrink disassembly column on narrow terminals.
        leftInner = Math.Clamp(leftInner, 8, Math.Max(8, termW - 28));
        // Middle + memory must sum to rowBudget exactly so each row is ≤ termW (no wrap → no broken markup).
        int rowBudget = Math.Max(1, termW - leftInner - 2 * gap);
        int memoryWidth = (rowBudget * 42 + 50) / 100;
        memoryWidth = Math.Clamp(memoryWidth, 1, Math.Max(1, rowBudget - 1));
        int midInner = rowBudget - memoryWidth;

        int[] subWeights = [19, 19, 19, 19];
        int[] colW = DistributeWidths(midInner, gap, subWeights);

        var disasmLines = BuildDisassemblyLines(emulator, s.PC, disasmSkip, maxContentLines + 4);

        string gapStr = new string(' ', gap);
        int estChars = maxContentLines * (leftInner + midInner + memoryWidth + 64);
        var frame = new StringBuilder(estChars);
        for (int r = 0; r < maxContentLines; r++)
        {
            string left;
            if (r == 0)
            {
                string disTitle = paneFocus == DebuggerPaneFocus.Disassembly
                    ? "[bold yellow]Disassembly[/]"
                    : "[dim]Disassembly[/]";
                left = PadMarkup(disTitle, leftInner);
            }
            else
            {
                int idx = r - 1;
                left = idx >= 0 && idx < disasmLines.Count
                    ? PadMarkup(ClipMarkup(disasmLines[idx], leftInner), leftInner)
                    : new string(' ', leftInner);
            }

            string mid = r < registerRows
                ? PadMarkup(ClipMarkup(BuildRegisterGridRow(emulator, s, r, colW, gap), midInner), midInner)
                : new string(' ', midInner);

            string memLine = BuildMemoryLine(emulator, MemoryBase, memRowSkip + r, memoryWidth);
            string mem = PadMarkupLeft(ClipMarkup(memLine, memoryWidth), memoryWidth);

            string row = left + gapStr + mid + gapStr + mem;
            row = ClipMarkupToVisibleWidth(row, termW);
            frame.Append(row);
            frame.Append('\n');
        }

        string frameText = frame.ToString();
        try
        {
            AnsiConsole.Markup(frameText);
        }
        catch (Exception)
        {
            // Never AnsiConsole.Write(string): it binds to the composite-format overload and throws on "{".
            Console.Write(SpectreTag.Replace(frameText, ""));
        }

        DrawPinnedFooter(termW, termH, paneFocus);
    }

    /// <summary>Last screen row: key hints (first letter of each action in red); no trailing newline so it stays pinned.</summary>
    static void DrawPinnedFooter(int termW, int termH, DebuggerPaneFocus paneFocus)
    {
        int row = Math.Min(termH - 1, Math.Max(0, Console.WindowHeight - 1));
        int width = Math.Max(1, Math.Min(termW, Console.WindowWidth));
        Console.SetCursorPosition(0, row);

        string focusStr = paneFocus == DebuggerPaneFocus.Disassembly
            ? "[bold cyan]Disasm[/] [dim grey]│ Mem[/]"
            : "[dim grey]Disasm │[/] [bold cyan]Mem[/]";
        string keysMarkup =
            "  [red]S[/]tep  [red]F[/] frame  [red]R[/] run  [red]Q[/] quit  [red]V[/] preview  [red]Tab[/] focus  [grey]↑↓[/]scroll  [red]Pg[/] jump  [red]H[/]ome  "
            + focusStr;

        string plain = Markup.Remove(keysMarkup);
        if (plain.Length >= width)
        {
            Console.Write(plain[..width]);
            return;
        }

        string pad = new string(' ', width - plain.Length);
        try
        {
            AnsiConsole.Markup(keysMarkup + pad);
        }
        catch (Exception)
        {
            Console.Write(plain.PadRight(width));
        }
    }

    /// <summary>Build disassembly strings with syntax highlighting; current PC gets a bold marker. <paramref name="skip"/> advances the window: positive skips forward in instruction-sized steps from PC; negative walks backward to earlier instruction boundaries.</summary>
    static List<string> BuildDisassemblyLines(Emulator emulator, ushort pc, int skip, int needLines)
    {
        var lines = new List<string>();
        ushort cursor = pc;
        if (skip > 0)
        {
            int consumed = 0;
            while (consumed < skip)
            {
                ushort sz = GetInstructionByteSize(emulator, cursor);
                cursor += sz;
                consumed++;
            }
        }
        else if (skip < 0)
        {
            for (int i = 0; i < -skip; i++)
            {
                if (!TryGetPreviousInstructionStart(emulator, cursor, out ushort prev))
                {
                    break;
                }

                cursor = prev;
            }
        }

        int count = Math.Max(needLines + 4, 40);
        for (int i = 0; i < count; i++)
        {
            bool atPc = cursor == pc;
            string body = FormatDisassemblyLineMarkup(emulator, cursor, pc, out ushort size);
            string prefix = atPc ? "[bold yellow]>[/] " : "  ";
            lines.Add(prefix + body);
            cursor += size;
        }
        return lines;
    }

    /// <summary>Instruction length in bytes, matching <see cref="FormatDisassemblyLineMarkup"/> / <see cref="DecodeLinePretty"/>.</summary>
    static ushort GetInstructionByteSize(Emulator emulator, ushort addr)
    {
        byte op = emulator.Read(addr);
        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            return 1;
        }

        return instruction.Bytes;
    }

    /// <summary>Finds <paramref name="prevStart"/> such that one instruction beginning at <paramref name="prevStart"/> ends exactly at <paramref name="addr"/>.</summary>
    static bool TryGetPreviousInstructionStart(Emulator emulator, ushort addr, out ushort prevStart)
    {
        const int maxLookback = 4;
        int a = addr;
        for (int delta = 1; delta <= maxLookback; delta++)
        {
            int candidate = a - delta;
            if (candidate < 0)
            {
                break;
            }

            ushort c = (ushort)candidate;
            ushort size = GetInstructionByteSize(emulator, c);
            if ((int)c + size == a)
            {
                prevStart = c;
                return true;
            }
        }

        prevStart = 0;
        return false;
    }

    static string FormatDisassemblyLineMarkup(Emulator emulator, ushort lineAddr, ushort focusPc, out ushort size)
    {
        byte op = emulator.Read(lineAddr);
        string addrMk = $"[grey]{lineAddr:X4}[/]";

        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            size = 1;
            string byteMk = $"[cyan]{op:X2}[/]";
            return $"{addrMk}: {byteMk}{new string(' ', 8)} [bold red]DB[/] [yellow]${op:X2}[/]";
        }

        size = instruction.Bytes;
        string bytesPlain = string.Join(' ', Enumerable.Range(0, size).Select(i =>
            emulator.Read((ushort)(lineAddr + i)).ToString("X2")));
        string padBytes = bytesPlain.Length < 8 ? bytesPlain + new string(' ', 8 - bytesPlain.Length) : bytesPlain;
        string bytesMk = $"[cyan]{padBytes}[/]";

        bool focus = lineAddr == focusPc;
        string mnStyle = focus ? "[bold green]" : "[green]";
        string mnMk = $"{mnStyle}{Markup.Escape(instruction.Mnemonic.ToString())}[/]";

        var ops = instruction.Operands.Select(o => OperandToMarkup(o, emulator, lineAddr));
        string opsStr = string.Join(' ', ops);
        string tail = opsStr.Length > 0 ? $" {opsStr}" : "";

        return $"{addrMk}: {bytesMk} {mnMk}{tail}";
    }

    static string OperandToMarkup(Operand operand, Emulator emulator, ushort atPc)
    {
        string s = FormatOperandPretty(operand, emulator, atPc);
        string esc = Markup.Escape(s);
        return operand switch
        {
            Operand.n8 or Operand.n16 or Operand.a16 or Operand.a8 or Operand.e8 => $"[yellow]{esc}[/]",
            Operand.NZ or Operand.NC or Operand.Z => $"[darkorange]{esc}[/]",
            Operand.RST00 or Operand.RST08 or Operand.RST10 or Operand.RST18
                or Operand.RST20 or Operand.RST28 or Operand.RST30 or Operand.RST38 => $"[magenta]{esc}[/]",
            Operand.Bit0 or Operand.Bit1 or Operand.Bit2 or Operand.Bit3
                or Operand.Bit4 or Operand.Bit5 or Operand.Bit6 or Operand.Bit7 => $"[orchid]{esc}[/]",
            _ => $"[silver]{esc}[/]",
        };
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
            1 => $"[bold]FF40[/] LCDC: [cyan]{emulator.Read(0xFF40):X2}[/]",
            2 => $"[bold]FF41[/] STAT: [cyan]{emulator.Read(0xFF41):X2}[/]",
            3 => $"[bold]FF42[/] SCY: [cyan]{emulator.Read(0xFF42):X2}[/]",
            4 => $"[bold]FF43[/] SCX: [cyan]{emulator.Read(0xFF43):X2}[/]",
            5 => $"[bold]FF44[/] LY: [cyan]{emulator.Read(0xFF44):X2}[/]",
            6 => $"[bold]FF45[/] LYC: [cyan]{emulator.Read(0xFF45):X2}[/]",
            7 => $"[bold]FF46[/] DMA: [cyan]{emulator.Read(0xFF46):X2}[/]",
            8 => $"[bold]FF47[/] BGP: [cyan]{emulator.Read(0xFF47):X2}[/]",
            9 => $"[bold]FF48[/] OBP0: [cyan]{emulator.Read(0xFF48):X2}[/]",
            10 => $"[bold]FF49[/] OBP1: [cyan]{emulator.Read(0xFF49):X2}[/]",
            11 => $"[bold]FF4A[/] WY: [cyan]{emulator.Read(0xFF4A):X2}[/]",
            12 => $"[bold]FF4B[/] WX: [cyan]{emulator.Read(0xFF4B):X2}[/]",
            13 => "",
            14 => $"{y}LCD (internal){x}",
            15 => $"VBlank: {(emulator.Read(0xFF44) >= 0x90 ? "yes" : "no")}",
            16 => $"Dots: {s.TotalCycles % 456}",
            17 => $"Mode: {emulator.Read(0xFF41) & 0x3}",
            18 => $"Next State: {(456 - (s.TotalCycles % 456))}",
            19 => "",
            20 => $"{y}vRAM DMA{x}",
            21 => $"FF51-52 Src: [cyan]{emulator.Read(0xFF51):X2}{emulator.Read(0xFF52):X2}[/]",
            22 => $"FF53-54 Dest: [cyan]{emulator.Read(0xFF53):X2}{emulator.Read(0xFF54):X2}[/]",
            23 => $"FF55 Length: [cyan]{emulator.Read(0xFF55):X2}[/]",
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
            1 => $"AF: [cyan]{s.AF:X4}[/]",
            2 => $"BC: [cyan]{s.BC:X4}[/]",
            3 => $"DE: [cyan]{s.DE:X4}[/]",
            4 => $"HL: [cyan]{s.HL:X4}[/]",
            5 => $"PC: [cyan]{s.PC:X4}[/]",
            6 => $"SP: [cyan]{s.SP:X4}[/]",
            7 => "",
            8 => $"{y}Interrupts{x}",
            9 => $"FF0F IF: [cyan]{s.IF:X2}[/]",
            10 => $"FF4D KEY1: [cyan]{emulator.Read(0xFF4D):X2}[/]",
            11 => $"FFFF IE: [cyan]{s.IE:X2}[/]",
            12 => $"IME: [cyan]{(s.Ime ? "on" : "off")}[/]",
            13 => "",
            14 => $"{y}Serial Port{x}",
            15 => $"FF01 SB: [cyan]{emulator.Read(0xFF01):X2}[/]",
            16 => $"FF02 SC: [cyan]{emulator.Read(0xFF02):X2}[/]",
            17 => "",
            18 => $"{y}Timer{x}",
            19 => $"FF04 DIV: [cyan]{emulator.Read(0xFF04):X2}[/]",
            20 => $"FF05 TIMA: [cyan]{emulator.Read(0xFF05):X2}[/]",
            21 => $"FF06 TMA: [cyan]{emulator.Read(0xFF06):X2}[/]",
            22 => $"FF07 TAC: [cyan]{emulator.Read(0xFF07):X2}[/]",
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
            6 => "",
            7 => $"{y}Ch2 (Square){x}",
            8 => LineReg(emulator, 0xFF16, "NR21"),
            9 => LineReg(emulator, 0xFF17, "NR22"),
            10 => LineReg(emulator, 0xFF18, "NR23"),
            11 => LineReg(emulator, 0xFF19, "NR24"),
            12 => "",
            13 => $"{y}Wave RAM (FF30-F){x}",
            14 => WaveRow(emulator, 0),
            15 => WaveRow(emulator, 4),
            16 => WaveRow(emulator, 8),
            17 => WaveRow(emulator, 12),
            _ => "",
        };
    }

    static string LineReg(Emulator emulator, ushort a, string name) =>
        $"[bold]{a:X4}[/] {name}: [cyan]{emulator.Read(a):X2}[/]";

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
            6 => "",
            7 => $"{y}Ch4 (Noise){x}",
            8 => LineReg(emulator, 0xFF20, "NR41"),
            9 => LineReg(emulator, 0xFF21, "NR42"),
            10 => LineReg(emulator, 0xFF22, "NR43"),
            11 => LineReg(emulator, 0xFF23, "NR44"),
            12 => "",
            13 => $"{y}Sound Ctrl{x}",
            14 => LineReg(emulator, 0xFF24, "NR50"),
            15 => LineReg(emulator, 0xFF25, "NR51"),
            16 => LineReg(emulator, 0xFF26, "NR52"),
            _ => "",
        };
    }

    static string BuildMemoryLine(Emulator emulator, ushort baseAddr, int rowIndex, int maxWidth)
    {
        ushort rowAddr = (ushort)(baseAddr + rowIndex * 16);
        var line = new StringBuilder();
        line.Append("[bold yellow]");
        line.Append($"{rowAddr:X4}");
        line.Append("[/]  ");

        var ascii = new StringBuilder();
        for (int col = 0; col < 16; col++)
        {
            ushort absAddr = (ushort)(rowAddr + col);
            byte val = emulator.Read(absAddr);
            if (col == 8)
            {
                line.Append("[grey]|[/] ");
            }

            if (val == 0)
            {
                line.Append("[dim grey]");
                line.Append($"{val:X2}");
                line.Append("[/] ");
            }
            else
            {
                line.Append(NonZeroByteStyle(absAddr));
                line.Append($"{val:X2}");
                line.Append("[/] ");
            }

            if (val >= 32 && val < 127)
            {
                ascii.Append("[green]");
                ascii.Append(Markup.Escape(((char)val).ToString()));
                ascii.Append("[/]");
            }
            else
            {
                ascii.Append("[grey].[/]");
            }
        }

        line.Append("[grey]|[/] ");
        line.Append(ascii);
        return ClipMarkup(line.ToString(), maxWidth);
    }

    /// <summary>GB-ish map: ROM / VRAM / cart RAM / WRAM+echo / OAM / dead zone / I/O / HRAM.</summary>
    static string NonZeroByteStyle(ushort absoluteAddr) => absoluteAddr switch
    {
        < 0x8000 => "[cyan]",
        < 0xA000 => "[magenta]",
        < 0xC000 => "[yellow]",
        < 0xFE00 => "[blue]",
        < 0xFEA0 => "[green]",
        < 0xFF00 => "[grey]",
        < 0xFF80 => "[orange1]",
        _ => "[silver]",
    };

    /// <summary>Split <paramref name="total"/> (full width incl. gaps between cols) across weighted columns. Never inflates past <paramref name="total"/> — the old Math.Max(n*5, inner) caused overflow on narrow terminals and line wrap.</summary>
    static int[] DistributeWidths(int total, int gap, int[] weights)
    {
        int n = weights.Length;
        int inner = total - (gap * (n - 1));
        inner = Math.Max(0, inner);
        if (inner == 0)
        {
            return new int[n];
        }

        int sum = weights.Sum();
        int[] w = new int[n];
        int assigned = 0;
        for (int i = 0; i < n; i++)
        {
            w[i] = inner * weights[i] / sum;
            assigned += w[i];
        }

        int rem = inner - assigned;
        for (int i = 0; rem > 0; i++)
        {
            w[i % n]++;
            rem--;
        }

        return w;
    }

    /// <summary>Guarantee visible width ≤ <paramref name="maxVisible"/> so the terminal never wraps mid-line (which splits markup and breaks colors).</summary>
    static string ClipMarkupToVisibleWidth(string markup, int maxVisible)
    {
        int v = VisibleLen(markup);
        if (v <= maxVisible)
        {
            return markup;
        }

        return ClipMarkup(markup, maxVisible);
    }

    static int VisibleLen(string markup) => SpectreTag.Replace(markup, "").Length;

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
        size = GetInstructionByteSize(emulator, pc);
        byte op = emulator.Read(pc);
        if (!Ops.Unprefixed.TryGetValue(op, out var instruction))
        {
            return $"{pc:X4}: {op:X2}        DB ${op:X2}";
        }

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
