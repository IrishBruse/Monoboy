namespace Monoboy.Desktop.TuiDebugger;

using System;

using Monoboy;

/// <summary>Register column: CPU, LCD, and IRQ/serial/timer grids.</summary>
static class TuiRegisterGridFormatter
{
    const int CpuColumnLeftPad = 5;

    internal static string BuildRow(
        Emulator emulator,
        DebugState s,
        int row,
        int[] colW,
        int gap,
        RegisterLabelDisplay labelDisplay)
    {
        int cpuContentW = Math.Max(1, colW[0] - CpuColumnLeftPad);
        string c1 = new string(' ', CpuColumnLeftPad) + TuiMarkup.PadMarkup(
            TuiMarkup.ClipMarkup(CpuIrqSerialTimerLine(emulator, s, row, labelDisplay), cpuContentW),
            cpuContentW);
        string c2 = TuiMarkup.PadMarkup(
            TuiMarkup.ClipMarkup(LcdLine(emulator, s, row, labelDisplay), colW[1]),
            colW[1]);
        return c1 + new string(' ', gap) + c2;
    }

    /// <summary>Visible width of <c>[bold]FF40[/]: </c> / <c>[bold]OBP0[/]: </c> label prefix.</summary>
    const int RegLabelVisibleWidth = 6;

    static string RegLabel(ushort addr, string name, RegisterLabelDisplay display) =>
        display == RegisterLabelDisplay.Address
            ? $"[bold]{addr:X4}[/]: "
            : $"[bold]{name}[/]: ";

    static string LabelPlusValue(string label, string value)
    {
        int pad = Math.Max(0, RegLabelVisibleWidth - TuiMarkup.VisibleLen(label));
        return label + new string(' ', pad) + value;
    }

    static string LcdRegLine(ushort addr, string name, byte val, RegisterLabelDisplay display) =>
        LabelPlusValue(RegLabel(addr, name, display), $"[cyan]{val:X2}[/]");

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
        LabelPlusValue(RegLabel(addr, name, display), $"[cyan]{emulator.Read(addr):X2}[/]");

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
            9 => LabelPlusValue(RegLabel(0xFF0F, "IF", display), $"[cyan]{s.IF:X2}[/]"),
            10 => IoRegLine(emulator, 0xFF4D, "KEY1", display),
            11 => LabelPlusValue(RegLabel(0xFFFF, "IE", display), $"[cyan]{s.IE:X2}[/]"),
            12 => LabelPlusValue("[bold]IME[/]: ", $"[cyan]{(s.Ime ? "on" : "off")}[/]"),
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

}
