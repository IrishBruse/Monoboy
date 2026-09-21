namespace Monoboy.Desktop.GuiDebugger;

using Raylib_cs;

/// <summary>VS Code One Dark colors from workbench.colorCustomizations.</summary>
static class GuiDebuggerTheme
{
    public static readonly Color Canvas = Hex(0x282C34);
    public static readonly Color PanelBackground = Hex(0x21252B);
    public static readonly Color PanelBorder = Hex(0x3A3F4B);
    public static readonly Color Title = Hex(0xE5C07B);
    public static readonly Color Label = Hex(0x9DA5B4);
    public static readonly Color Value = Hex(0xD7DAE0);
    public static readonly Color Address = Hex(0x636E83);
    public static readonly Color ProgramCounterRow = Hex(0x3E4451);
    public static readonly Color ToolbarLabel = Hex(0x9DA5B4);
    public static readonly Color HexDimZero = Hex(0x5C6370);
    public static readonly Color ScrollbarTrack = Hex(0x21252B);
    public static readonly Color ScrollbarThumb = new(0x4E, 0x56, 0x66, 0x80);
    public static readonly Color ScrollbarThumbHover = new(0x5A, 0x63, 0x75, 0x80);
    public static readonly Color MenuBackground = Hex(0x353B45);
    public static readonly Color MenuHover = Hex(0x2C313A);

    public static readonly Color DisasmBytes = Hex(0x56B6C2);
    public static readonly Color DisasmMnemonic = Hex(0x98C379);
    public static readonly Color DisasmImmediate = Hex(0xE5C07B);
    public static readonly Color DisasmUnknown = Hex(0xE06C75);
    public static readonly Color DisasmCondition = Hex(0xD19A66);
    public static readonly Color DisasmRst = Hex(0xC678DD);
    public static readonly Color DisasmBit = Hex(0xC678DD);
    public static readonly Color DisasmOperand = Hex(0xABB2BF);
    public static readonly Color DisasmSymbol = Hex(0xD7DAE0);

    public static Color Hex(int rgb)
    {
        byte r = (byte)((rgb >> 16) & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)(rgb & 0xFF);
        return new Color(r, g, b, (byte)255);
    }
}
