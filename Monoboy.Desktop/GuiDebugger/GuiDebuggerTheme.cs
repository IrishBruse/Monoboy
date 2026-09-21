namespace Monoboy.Desktop.GuiDebugger;

using System.Numerics;

using ImGuiNET;

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
    public static readonly Color DebugToolbarAction = Hex(0x75BEFF);
    public static readonly Color DebugToolbarRestart = Hex(0x89D185);
    public static readonly Color HexDimZero = Hex(0x5C6370);
    public static readonly Color ScrollbarTrack = Hex(0x21252B);
    public static readonly Color ScrollbarThumb = new(0x4E, 0x56, 0x66, 0x80);
    public static readonly Color ScrollbarThumbHover = new(0x5A, 0x63, 0x75, 0x80);
    public static readonly Color MenuBackground = Hex(0x353B45);
    public static readonly Color MenuHover = Hex(0x2C313A);
    /// <summary>VS Code tab.inactiveBackground / editor group header.</summary>
    public static readonly Color TabInactive = Hex(0x21252B);
    /// <summary>VS Code tab.activeBackground (matches editor canvas).</summary>
    public static readonly Color TabActive = Hex(0x282C34);
    /// <summary>Hover fill behind a tab close mark.</summary>
    public static readonly Color CloseHover = Hex(0xE06C75);
    /// <summary>Pressed fill behind a tab close mark.</summary>
    public static readonly Color CloseActive = Hex(0xBE5046);

    public static readonly Color DisasmBytes = Hex(0x56B6C2);
    public static readonly Color DisasmMnemonic = Hex(0x98C379);
    public static readonly Color DisasmImmediate = Hex(0xE5C07B);
    public static readonly Color DisasmUnknown = Hex(0xE06C75);
    public static readonly Color DisasmCondition = Hex(0xD19A66);
    public static readonly Color DisasmRst = Hex(0xC678DD);
    public static readonly Color DisasmBit = Hex(0xC678DD);
    public static readonly Color DisasmOperand = Hex(0xABB2BF);
    public static readonly Color DisasmSymbol = Hex(0xD7DAE0);

    public static readonly uint ViewportOutlineU32 = ImGui.ColorConvertFloat4ToU32(new Vector4(0xE0 / 255f, 0x6C / 255f, 0x75 / 255f, 1f));

    public static Color Hex(int rgb)
    {
        byte r = (byte)((rgb >> 16) & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)(rgb & 0xFF);
        return new Color(r, g, b, (byte)255);
    }

    public static void ApplyImGuiStyle()
    {
        var style = ImGui.GetStyle();
        style.WindowRounding = 2;
        style.ChildRounding = 2;
        style.FrameRounding = 2;
        style.PopupRounding = 2;
        style.ScrollbarRounding = 2;
        style.GrabRounding = 2;
        style.TabRounding = 0;
        style.TabBorderSize = 0;
        style.TabBarBorderSize = 0;
        style.TabBarOverlineSize = 0;
        style.WindowMenuButtonPosition = ImGuiDir.None;
        style.WindowBorderSize = 1;
        style.FramePadding = new Vector2(8, 4);
        style.ItemSpacing = new Vector2(6, 4);

        style.Colors[(int)ImGuiCol.Text] = ToVec4(Value);
        style.Colors[(int)ImGuiCol.TextDisabled] = ToVec4(Label);
        style.Colors[(int)ImGuiCol.WindowBg] = ToVec4(Canvas);
        style.Colors[(int)ImGuiCol.ChildBg] = ToVec4(PanelBackground);
        style.Colors[(int)ImGuiCol.PopupBg] = ToVec4(MenuBackground);
        style.Colors[(int)ImGuiCol.Border] = ToVec4(PanelBorder);
        style.Colors[(int)ImGuiCol.BorderShadow] = Vector4.Zero;
        style.Colors[(int)ImGuiCol.FrameBg] = ToVec4(PanelBackground);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = ToVec4(MenuHover);
        style.Colors[(int)ImGuiCol.FrameBgActive] = ToVec4(MenuBackground);
        style.Colors[(int)ImGuiCol.TitleBg] = ToVec4(PanelBackground);
        style.Colors[(int)ImGuiCol.TitleBgActive] = ToVec4(PanelBackground);
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = ToVec4(PanelBackground);
        style.Colors[(int)ImGuiCol.MenuBarBg] = ToVec4(PanelBackground);
        style.Colors[(int)ImGuiCol.ScrollbarBg] = ToVec4(ScrollbarTrack);
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = ToVec4(ScrollbarThumb);
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = ToVec4(ScrollbarThumbHover);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = ToVec4(ScrollbarThumbHover);
        style.Colors[(int)ImGuiCol.Button] = new Vector4(0, 0, 0, 0);
        style.Colors[(int)ImGuiCol.ButtonHovered] = ToVec4(CloseHover);
        style.Colors[(int)ImGuiCol.ButtonActive] = ToVec4(CloseActive);
        style.Colors[(int)ImGuiCol.CheckMark] = ToVec4(Title);
        style.Colors[(int)ImGuiCol.SliderGrab] = ToVec4(Title);
        style.Colors[(int)ImGuiCol.SliderGrabActive] = ToVec4(Title);
        style.Colors[(int)ImGuiCol.Header] = ToVec4(TabInactive);
        style.Colors[(int)ImGuiCol.HeaderHovered] = ToVec4(MenuHover);
        style.Colors[(int)ImGuiCol.HeaderActive] = ToVec4(TabActive);
        style.Colors[(int)ImGuiCol.Separator] = ToVec4(PanelBorder);
        style.Colors[(int)ImGuiCol.SeparatorHovered] = ToVec4(PanelBorder);
        style.Colors[(int)ImGuiCol.SeparatorActive] = ToVec4(PanelBorder);
        style.Colors[(int)ImGuiCol.ResizeGrip] = ToVec4(PanelBorder);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = ToVec4(MenuHover);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = ToVec4(Title);
        style.Colors[(int)ImGuiCol.Tab] = ToVec4(TabInactive);
        style.Colors[(int)ImGuiCol.TabHovered] = ToVec4(MenuHover);
        style.Colors[(int)ImGuiCol.TabSelected] = ToVec4(TabActive);
        style.Colors[(int)ImGuiCol.TabSelectedOverline] = Vector4.Zero;
        style.Colors[(int)ImGuiCol.TabDimmed] = ToVec4(TabInactive);
        style.Colors[(int)ImGuiCol.TabDimmedSelected] = ToVec4(TabActive);
        style.Colors[(int)ImGuiCol.TabDimmedSelectedOverline] = Vector4.Zero;
        style.Colors[(int)ImGuiCol.DockingPreview] = ToVec4(MenuHover);
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = ToVec4(Canvas);
    }

    static Vector4 ToVec4(Color c) => new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
}
