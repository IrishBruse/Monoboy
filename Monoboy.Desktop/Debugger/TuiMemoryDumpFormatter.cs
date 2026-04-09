namespace Monoboy.Desktop.Debugger;

using System.Text;

using Monoboy;

using Spectre.Console;

/// <summary>Memory column: hex dump with GB memory-map coloring.</summary>
static class TuiMemoryDumpFormatter
{
    internal const ushort DefaultBaseAddress = 0x0000;

    internal static string BuildLine(Emulator emulator, ushort baseAddr, int rowIndex, int maxWidth)
    {
        ushort rowAddr = (ushort)(baseAddr + rowIndex * 16);
        var line = new StringBuilder();
        line.Append("[grey]");
        line.Append($"{rowAddr:X4}");
        line.Append("[/]  ");

        var ascii = new StringBuilder();
        for (int col = 0; col < 16; col++)
        {
            ushort absAddr = (ushort)(rowAddr + col);
            byte val = emulator.Read(absAddr);
            if (col == 8)
            {
                line.Append("[grey]│[/] ");
            }

            if (val == 0)
            {
                line.Append($"{val:X2} ");
            }
            else
            {
                string style = NonZeroByteStyle(absAddr);
                if (style.Length > 0)
                {
                    line.Append(style);
                    line.Append($"{val:X2}");
                    line.Append("[/] ");
                }
                else
                {
                    line.Append($"{val:X2} ");
                }
            }

            if (val >= 32 && val < 127)
            {
                ascii.Append(Markup.Escape(((char)val).ToString()));
            }
            else
            {
                ascii.Append('.');
            }
        }

        line.Append("[grey]│[/] ");
        line.Append(ascii);
        return TuiMarkup.ClipMarkup(line.ToString(), maxWidth);
    }

    /// <summary>GB-ish map: ROM / VRAM / cart RAM / WRAM+echo / OAM / dead zone / I/O / HRAM.</summary>
    static string NonZeroByteStyle(ushort absoluteAddr) => absoluteAddr switch
    {
        < 0x8000 => "",
        < 0xA000 => "[magenta]",
        < 0xC000 => "[yellow]",
        < 0xFE00 => "[blue]",
        < 0xFEA0 => "[green]",
        < 0xFF00 => "[grey]",
        < 0xFF80 => "[orange1]",
        _ => "[silver]",
    };
}
