namespace Monoboy.Desktop;

using System;
using System.IO;
using System.Linq;
using System.Text;

using Monoboy.Disassembler;

internal static class TuiDebugger
{
    public static void Run(string[] args)
    {
        Console.CursorVisible = false;
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

        bool running = true;
        while (running)
        {
            Render(emulator, romPath);
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            switch (key.Key)
            {
                case ConsoleKey.S:
                    emulator.Step();
                    break;
                case ConsoleKey.F:
                    emulator.StepFrame();
                    break;
                case ConsoleKey.R:
                    for (int i = 0; i < 500; i++)
                    {
                        emulator.Step();
                    }
                    break;
                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    running = false;
                    break;
            }
        }

        Console.CursorVisible = true;
        Console.Clear();
    }

    static void Render(Emulator emulator, string romPath)
    {
        var s = emulator.GetDebugState();
        Console.Clear();

        Console.WriteLine("Monoboy TUI Debugger");
        Console.WriteLine($"ROM: {(string.IsNullOrWhiteSpace(romPath) ? "<empty>" : romPath)}");
        Console.WriteLine("Controls: [S]tep  [F]rame  [R]un(500)  [Q]/[Esc] Quit");
        Console.WriteLine();

        Console.WriteLine("CPU");
        Console.WriteLine($"AF: {s.AF:X4}   BC: {s.BC:X4}   DE: {s.DE:X4}   HL: {s.HL:X4}");
        Console.WriteLine($"PC: {s.PC:X4}   SP: {s.SP:X4}   IME: {(s.Ime ? "on" : "off")}   HALT: {(s.Halted ? "on" : "off")}");
        Console.WriteLine($"IE: {s.IE:X2}   IF: {s.IF:X2}   Flags: Z{(s.F & 0x80) >> 7} N{(s.F & 0x40) >> 6} H{(s.F & 0x20) >> 5} C{(s.F & 0x10) >> 4}");
        Console.WriteLine($"Cycles: {s.TotalCycles}");
        Console.WriteLine();

        Console.WriteLine("I/O");
        Console.WriteLine($"LCDC:{emulator.Read(0xFF40):X2} STAT:{emulator.Read(0xFF41):X2} SCY:{emulator.Read(0xFF42):X2} SCX:{emulator.Read(0xFF43):X2}");
        Console.WriteLine($"LY:{emulator.Read(0xFF44):X2} LYC:{emulator.Read(0xFF45):X2} DMA:{emulator.Read(0xFF46):X2} BGP:{emulator.Read(0xFF47):X2}");
        Console.WriteLine($"OBP0:{emulator.Read(0xFF48):X2} OBP1:{emulator.Read(0xFF49):X2} WY:{emulator.Read(0xFF4A):X2} WX:{emulator.Read(0xFF4B):X2}");
        Console.WriteLine();

        Console.WriteLine("Disassembly (from PC)");
        ushort cursor = s.PC;
        for (int i = 0; i < 12; i++)
        {
            string line = DecodeLine(emulator, cursor, out ushort size);
            string prefix = i == 0 ? ">" : " ";
            Console.WriteLine($"{prefix} {line}");
            cursor += size;
        }
        Console.WriteLine();

        Console.WriteLine("Memory (16x16 from PC page)");
        ushort start = (ushort)(s.PC & 0xFFF0);
        for (int row = 0; row < 16; row++)
        {
            ushort addr = (ushort)(start + (row * 16));
            var rowText = new StringBuilder();
            rowText.Append($"{addr:X4}: ");
            for (int col = 0; col < 16; col++)
            {
                byte val = emulator.Read((ushort)(addr + col));
                rowText.Append($"{val:X2} ");
            }
            Console.WriteLine(rowText.ToString().TrimEnd());
        }
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
