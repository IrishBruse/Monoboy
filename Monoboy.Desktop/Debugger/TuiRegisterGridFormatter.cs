namespace Monoboy.Desktop.Debugger;

using System;
using System.Text;

using Monoboy;

/// <summary>Center column: CPU, LCD, IRQ/serial/timer, and APU register grids.</summary>
static class TuiRegisterGridFormatter
{
    internal static string BuildRow(
        Emulator emulator,
        DebugState s,
        int row,
        int[] colW,
        int gap,
        RegisterLabelDisplay labelDisplay)
    {
        string c1 = TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(CpuIrqSerialTimerLine(emulator, s, row, labelDisplay), colW[0]), colW[0]);
        string c2 = TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(LcdLine(emulator, s, row, labelDisplay), colW[1]), colW[1]);
        string c3 = TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(Ch12WaveLine(emulator, row, labelDisplay), colW[2]), colW[2]);
        string c4 = TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(Ch34SoundLine(emulator, row, labelDisplay), colW[3]), colW[3]);
        return c1 + new string(' ', gap) + c2 + new string(' ', gap) + c3 + new string(' ', gap) + c4;
    }

    static string RegLabel(ushort addr, string name, RegisterLabelDisplay display) =>
        display == RegisterLabelDisplay.Address
            ? $"[bold]{addr:X4}[/]: "
            : $"[bold]{name}[/]: ";

    static string LcdRegLine(ushort addr, string name, byte val, RegisterLabelDisplay display) =>
        RegLabel(addr, name, display) + $"[cyan]{val:X2}[/]";

    static string LcdLine(Emulator emulator, DebugState s, int r, RegisterLabelDisplay display)
    {
        string y = "[yellow]";
        string x = "[/]";
        return r switch
        {
            0 => $"{y}LCD{x}",
            1 => LcdRegLine(0xFF40, "LCDC", emulator.Read(0xFF40), display),
            2 => LcdRegLine(0xFF41, "STAT", emulator.Read(0xFF41), display),
            3 => LcdRegLine(0xFF42, "SCY", emulator.Read(0xFF42), display),
            4 => LcdRegLine(0xFF43, "SCX", emulator.Read(0xFF43), display),
            5 => LcdRegLine(0xFF44, "LY", emulator.Read(0xFF44), display),
            6 => LcdRegLine(0xFF45, "LYC", emulator.Read(0xFF45), display),
            7 => LcdRegLine(0xFF46, "DMA", emulator.Read(0xFF46), display),
            8 => LcdRegLine(0xFF47, "BGP", emulator.Read(0xFF47), display),
            9 => LcdRegLine(0xFF48, "OBP0", emulator.Read(0xFF48), display),
            10 => LcdRegLine(0xFF49, "OBP1", emulator.Read(0xFF49), display),
            11 => LcdRegLine(0xFF4A, "WY", emulator.Read(0xFF4A), display),
            12 => LcdRegLine(0xFF4B, "WX", emulator.Read(0xFF4B), display),
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

    static string CpuReg16Markup(ushort v) =>
        $"[cyan]{v >> 8:X2} {v & 0xFF:X2}[/]";

    static string IoRegLine(Emulator emulator, ushort addr, string name, RegisterLabelDisplay display) =>
        RegLabel(addr, name, display) + $"[cyan]{emulator.Read(addr):X2}[/]";

    static string CpuIrqSerialTimerLine(Emulator emulator, DebugState s, int r, RegisterLabelDisplay display)
    {
        string y = "[yellow]";
        string x = "[/]";
        return r switch
        {
            0 => $"{y}CPU{x}",
            1 => $"AF: {CpuReg16Markup(s.AF)}",
            2 => $"BC: {CpuReg16Markup(s.BC)}",
            3 => $"DE: {CpuReg16Markup(s.DE)}",
            4 => $"HL: {CpuReg16Markup(s.HL)}",
            5 => $"PC: {CpuReg16Markup(s.PC)}",
            6 => $"SP: {CpuReg16Markup(s.SP)}",
            7 => "",
            8 => $"{y}Interrupts{x}",
            9 => RegLabel(0xFF0F, "IF", display) + $"[cyan]{s.IF:X2}[/]",
            10 => IoRegLine(emulator, 0xFF4D, "KEY1", display),
            11 => RegLabel(0xFFFF, "IE", display) + $"[cyan]{s.IE:X2}[/]",
            12 => $"IME: [cyan]{(s.Ime ? "on" : "off")}[/]",
            13 => "",
            14 => $"{y}Serial Port{x}",
            15 => IoRegLine(emulator, 0xFF01, "SB", display),
            16 => IoRegLine(emulator, 0xFF02, "SC", display),
            17 => "",
            18 => $"{y}Timer{x}",
            19 => IoRegLine(emulator, 0xFF04, "DIV", display),
            20 => IoRegLine(emulator, 0xFF05, "TIMA", display),
            21 => IoRegLine(emulator, 0xFF06, "TMA", display),
            22 => IoRegLine(emulator, 0xFF07, "TAC", display),
            _ => "",
        };
    }

    static string Ch12WaveLine(Emulator emulator, int r, RegisterLabelDisplay display)
    {
        string y = "[yellow]";
        string x = "[/]";
        return r switch
        {
            0 => $"{y}Ch1 (Square){x}",
            1 => LineReg(emulator, 0xFF10, "NR10", display),
            2 => LineReg(emulator, 0xFF11, "NR11", display),
            3 => LineReg(emulator, 0xFF12, "NR12", display),
            4 => LineReg(emulator, 0xFF13, "NR13", display),
            5 => LineReg(emulator, 0xFF14, "NR14", display),
            6 => "",
            7 => $"{y}Ch2 (Square){x}",
            8 => LineReg(emulator, 0xFF16, "NR21", display),
            9 => LineReg(emulator, 0xFF17, "NR22", display),
            10 => LineReg(emulator, 0xFF18, "NR23", display),
            11 => LineReg(emulator, 0xFF19, "NR24", display),
            12 => "",
            13 => $"{y}Wave RAM (FF30-F){x}",
            14 => WaveRow(emulator, 0),
            15 => WaveRow(emulator, 4),
            16 => WaveRow(emulator, 8),
            17 => WaveRow(emulator, 12),
            _ => "",
        };
    }

    static string LineReg(Emulator emulator, ushort a, string name, RegisterLabelDisplay display) =>
        RegLabel(a, name, display) + $"[cyan]{emulator.Read(a):X2}[/]";

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

    static string Ch34SoundLine(Emulator emulator, int r, RegisterLabelDisplay display)
    {
        string y = "[yellow]";
        string x = "[/]";
        return r switch
        {
            0 => $"{y}Ch3 (Wave){x}",
            1 => LineReg(emulator, 0xFF1A, "NR30", display),
            2 => LineReg(emulator, 0xFF1B, "NR31", display),
            3 => LineReg(emulator, 0xFF1C, "NR32", display),
            4 => LineReg(emulator, 0xFF1D, "NR33", display),
            5 => LineReg(emulator, 0xFF1E, "NR34", display),
            6 => "",
            7 => $"{y}Ch4 (Noise){x}",
            8 => LineReg(emulator, 0xFF20, "NR41", display),
            9 => LineReg(emulator, 0xFF21, "NR42", display),
            10 => LineReg(emulator, 0xFF22, "NR43", display),
            11 => LineReg(emulator, 0xFF23, "NR44", display),
            12 => "",
            13 => $"{y}Sound Ctrl{x}",
            14 => LineReg(emulator, 0xFF24, "NR50", display),
            15 => LineReg(emulator, 0xFF25, "NR51", display),
            16 => LineReg(emulator, 0xFF26, "NR52", display),
            _ => "",
        };
    }
}
