namespace Monoboy.Desktop.Debugger;

using Raylib_cs;

/// <summary>Tokyo Night–style colors for the Raylib GUI debugger shell.</summary>
static class GuiDebuggerTheme
{
    public static readonly Color Canvas = Hex(0x1E1C28);
    public static readonly Color PanelBackground = Hex(0x22202E);
    public static readonly Color PanelBorder = Hex(0x3E4148);
    public static readonly Color Title = Hex(0xCCDA8B);
    public static readonly Color Label = Hex(0xA9B1D6);
    public static readonly Color Value = Hex(0xFEFBFF);
    public static readonly Color ProgramCounterRow = Hex(0x28786D);
    public static readonly Color LcdBezel = Hex(0xD2C9A5);
    public static readonly Color ToolbarLabel = Hex(0xD1D5DB);
    public static readonly Color HexDimZero = Hex(0x414868);
    public static readonly Color ScrollbarTrack = Hex(0x1A1B26);
    public static readonly Color ScrollbarThumb = Hex(0x5B6B7A);

    static Color Hex(int rgb)
    {
        byte r = (byte)((rgb >> 16) & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)(rgb & 0xFF);
        return new Color(r, g, b, (byte)255);
    }
}
