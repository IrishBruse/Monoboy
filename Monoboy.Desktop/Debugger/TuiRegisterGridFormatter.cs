namespace Monoboy.Desktop.Debugger;

using System.Text;

using Monoboy;

/// <summary>Center column: CPU, LCD, IRQ/serial/timer, and APU register grids.</summary>
internal static class TuiRegisterGridFormatter
{
    internal static string BuildRow(Emulator emulator, DebugState s, int row, int[] colW, int gap)
    {
        string c1 = TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(CpuIrqSerialTimerLine(emulator, s, row), colW[0]), colW[0]);
        string c2 = TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(LcdLine(emulator, s, row), colW[1]), colW[1]);
        string c3 = TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(Ch12WaveLine(emulator, row), colW[2]), colW[2]);
        string c4 = TuiMarkup.PadMarkup(TuiMarkup.ClipMarkup(Ch34SoundLine(emulator, row), colW[3]), colW[3]);
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

    static string CpuReg16Markup(ushort v) =>
        $"[cyan]{v >> 8:X2} {v & 0xFF:X2}[/]";

    static string CpuIrqSerialTimerLine(Emulator emulator, DebugState s, int r)
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
}
