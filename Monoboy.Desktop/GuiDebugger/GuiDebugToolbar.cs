namespace Monoboy.Desktop.GuiDebugger;

using System.Numerics;

using ImGuiNET;

using Monoboy;

using Raylib_cs;

/// <summary>VS Code–style debug control strip under the menu bar.</summary>
static class GuiDebugToolbar
{
    const float ButtonSize = 28f;

    static Vector4 V(Color c) => new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

    public static void Draw(Emulator emulator, ref bool running)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 6));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2, 0));
        ImGui.PushStyleColor(ImGuiCol.ChildBg, V(GuiDebuggerTheme.PanelBackground));

        float barH = ButtonSize + 12f;
        ImGui.BeginChild("DebugToolbar", new Vector2(0, barH), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar);

        bool canStep = !running;

        if (running)
        {
            if (IconButton("pause", "Pause (Shift+F9)", GuiDebuggerTheme.DebugToolbarAction, GuiCodicons.DebugPause, enabled: true))
            {
                GuiDebugRunCommands.Apply(GuiDebugRunCommands.Pause, emulator, ref running);
            }
        }
        else
        {
            if (IconButton("continue", "Continue (F9)", GuiDebuggerTheme.DebugToolbarAction, GuiCodicons.DebugContinue, enabled: true))
            {
                GuiDebugRunCommands.Apply(GuiDebugRunCommands.Continue, emulator, ref running);
            }
        }

        ImGui.SameLine();
        StepIconButton("stepOver", "Step Over (F8)", GuiDebugRunCommands.StepOver, emulator, ref running, canStep, GuiCodicons.DebugStepOver);
        ImGui.SameLine();
        StepIconButton("stepInto", "Step Into (F3)", GuiDebugRunCommands.StepInto, emulator, ref running, canStep, GuiCodicons.DebugStepInto);
        ImGui.SameLine();
        StepIconButton("stepOut", "Step Out (Shift+F11)", GuiDebugRunCommands.StepOut, emulator, ref running, canStep, GuiCodicons.DebugStepOut);

        ImGui.SameLine();
        ImGui.TextColored(V(GuiDebuggerTheme.PanelBorder), "|");
        ImGui.SameLine();

        if (IconButton("restart", "Restart (Ctrl+R)", GuiDebuggerTheme.DebugToolbarRestart, GuiCodicons.DebugRestart, enabled: true))
        {
            GuiDebugRunCommands.Apply(GuiDebugRunCommands.Reset, emulator, ref running);
        }

        ImGui.EndChild();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(2);
    }

    static void StepIconButton(
        string id,
        string tooltip,
        int command,
        Emulator emulator,
        ref bool running,
        bool enabled,
        string glyph)
    {
        if (IconButton(id, tooltip, GuiDebuggerTheme.DebugToolbarAction, glyph, enabled))
        {
            GuiDebugRunCommands.Apply(command, emulator, ref running);
        }
    }

    static bool IconButton(string id, string tooltip, Color accent, string glyph, bool enabled)
    {
        if (!enabled)
        {
            ImGui.BeginDisabled();
        }

        ImGui.PushID(id);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

        bool pressed = ImGui.InvisibleButton("##", new Vector2(ButtonSize, ButtonSize));

        Vector2 min = ImGui.GetItemRectMin();
        Vector2 max = ImGui.GetItemRectMax();
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        if (enabled && ImGui.IsItemHovered())
        {
            drawList.AddRectFilled(min, max, ImGui.ColorConvertFloat4ToU32(V(GuiDebuggerTheme.MenuHover)));
        }

        if (ImGui.IsItemActive())
        {
            drawList.AddRectFilled(min, max, ImGui.ColorConvertFloat4ToU32(V(GuiDebuggerTheme.MenuBackground)));
        }

        Color iconColor = enabled ? accent : GuiDebuggerTheme.Label;
        uint iconCol = ImGui.ColorConvertFloat4ToU32(V(iconColor));

        if (GuiDebuggerFonts.HasCodiconFont)
        {
            ImGui.PushFont(GuiDebuggerFonts.CodiconFont);
            Vector2 textSize = ImGui.CalcTextSize(glyph);
            Vector2 pos = new(
                min.X + ((ButtonSize - textSize.X) * 0.5f),
                min.Y + ((ButtonSize - textSize.Y) * 0.5f));
            drawList.AddText(GuiDebuggerFonts.CodiconFont, GuiDebuggerFonts.CodiconSizePx, pos, iconCol, glyph);
            ImGui.PopFont();
        }

        ImGui.PopStyleVar();
        ImGui.PopID();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(tooltip);
        }

        if (!enabled)
        {
            ImGui.EndDisabled();
        }

        return pressed && enabled;
    }
}
