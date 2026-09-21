namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.Numerics;

using ImGuiNET;

using Monoboy;
using Monoboy.Constants;

using Raylib_cs;

/// <summary>
/// Four-column register mosaic. Theme: Title, Label, Value, PanelBackground, PanelBorder.
/// </summary>
public static class GuiRegisterPanels
{
    const int PanelGap = 6;
    const int PanelPad = 8;
    const int TitleRowHeight = 18;
    const int DataRowHeight = 16;
    const float ValueColumnWidth = 130f;

    static Vector4 C(Color color) =>
        new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    public static int RequiredDrawHeight
    {
        get
        {
            static int Panel(int rows) => PanelPad + TitleRowHeight + rows * DataRowHeight + PanelPad;
            static int Column(int[] rows)
            {
                int h = 0;
                for (int i = 0; i < rows.Length; i++)
                {
                    h += Panel(rows[i]);
                    if (i < rows.Length - 1)
                    {
                        h += PanelGap;
                    }
                }

                return h;
            }

            int c1 = Column([11, 4, 3]);
            int c2 = Column([6, 4, 2, 4]);
            int c3 = Column([5, 4, 4]);
            int c4 = Column([5, 4, 3]);
            return Math.Max(Math.Max(c1, c2), Math.Max(c3, c4));
        }
    }

    public static void Draw(Emulator emulator)
    {
        Vector2 avail = ImGui.GetContentRegionAvail();
        if (avail.X <= 0 || avail.Y <= 0)
        {
            return;
        }

        DebugState s = emulator.GetDebugState();

        const float minCol = 160f;
        const float gap = 8f;
        int cols = Math.Clamp((int)((avail.X + gap) / (minCol + gap)), 1, 4);
        float colW = Math.Max(1f, (avail.X - gap * (cols - 1)) / cols);

        ImGui.PushStyleColor(ImGuiCol.ChildBg, C(GuiDebuggerTheme.PanelBackground));
        ImGui.BeginChild("RegistersScroll", new Vector2(Math.Max(1f, avail.X), Math.Max(1f, avail.Y)), ImGuiChildFlags.None);

        for (int i = 0; i < 4; i++)
        {
            if (i % cols != 0)
            {
                ImGui.SameLine(0, gap);
            }

            ImGui.BeginChild($"reg-col-{i}", new Vector2(colW, 0), ImGuiChildFlags.AutoResizeY);
            switch (i)
            {
                case 0:
                    DrawColumn1(emulator, s);
                    break;
                case 1:
                    DrawColumn2(emulator, s);
                    break;
                case 2:
                    DrawColumn3(emulator);
                    break;
                default:
                    DrawColumn4(emulator);
                    break;
            }

            ImGui.EndChild();
        }

        ImGui.EndChild();
        ImGui.PopStyleColor();
    }

    static void DrawColumn1(Emulator emulator, DebugState s)
    {
        var w = new ColumnWriter();
        w.BeginPanel("LCD", 11);
        IoRow(w, emulator, Reg.LCDC, "LCDC");
        IoRow(w, emulator, Reg.STAT, "STAT");
        IoRow(w, emulator, Reg.SCY, "SCY");
        IoRow(w, emulator, Reg.SCX, "SCX");
        IoRow(w, emulator, Reg.LY, "LY");
        IoRow(w, emulator, Reg.LYC, "LYC");
        IoRow(w, emulator, Reg.DMA, "DMA");
        IoRow(w, emulator, Reg.BGP, "BGP");
        IoRow(w, emulator, Reg.OBP0, "OBP0");
        IoRow(w, emulator, Reg.OBP1, "OBP1");
        IoRow(w, emulator, Reg.WY, "WY");
        w.EndPanel();

        int dots = (int)(s.TotalCycles % 456);
        byte ly = emulator.Read(Reg.LY);
        string lcdStatus = ly < 0x90 ? "Drawing" : "VBlank";
        w.BeginPanel("LCD (internal)", 4);
        GoldLabelRow(w, lcdStatus);
        LabelValueRow(w, "Dots", dots.ToString());
        LabelValueRow(w, "Mode", (emulator.Read(Reg.STAT) & 0x3).ToString());
        LabelValueRow(w, "Next State", (456 - dots).ToString());
        w.EndPanel();

        w.BeginPanel("vRAM DMA", 3);
        LabelValueRow(w, "FF51-52 Src", $"{emulator.Read(Reg.HDMA1):X2}{emulator.Read(Reg.HDMA2):X2}");
        LabelValueRow(w, "FF53-54 Dest", $"{emulator.Read(Reg.HDMA3):X2}{emulator.Read(Reg.HDMA4):X2}");
        LabelValueRow(w, "FF55 Length", $"{emulator.Read(Reg.HDMA5):X2}");
        w.EndPanel();
    }

    static void DrawColumn2(Emulator emulator, DebugState s)
    {
        var w = new ColumnWriter();
        w.BeginPanel("CPU", 6);
        Reg16Row(w, "AF", s.AF);
        Reg16Row(w, "BC", s.BC);
        Reg16Row(w, "DE", s.DE);
        Reg16Row(w, "HL", s.HL);
        Reg16Row(w, "PC", s.PC);
        Reg16Row(w, "SP", s.SP);
        w.EndPanel();

        w.BeginPanel("Interrupts", 4);
        IoRow(w, emulator, 0xFF0F, "IF");
        IoRow(w, emulator, Reg.KEY1, "KEY1");
        LabelValueRow(w, "FFFF IE", $"{s.IE:X2}");
        LabelValueRow(w, "IME", s.Ime ? "on" : "off");
        w.EndPanel();

        w.BeginPanel("Serial Port", 2);
        IoRow(w, emulator, Reg.SB, "SB");
        IoRow(w, emulator, Reg.SC, "SC");
        w.EndPanel();

        w.BeginPanel("Timer", 4);
        IoRow(w, emulator, Reg.DIV, "DIV");
        IoRow(w, emulator, Reg.TIMA, "TIMA");
        IoRow(w, emulator, Reg.TMA, "TMA");
        IoRow(w, emulator, Reg.TAC, "TAC");
        w.EndPanel();
    }

    static void DrawColumn3(Emulator emulator)
    {
        var w = new ColumnWriter();
        w.BeginPanel("Ch1 (Square)", 5);
        IoRow(w, emulator, Reg.NR10, "NR10");
        IoRow(w, emulator, Reg.NR11, "NR11");
        IoRow(w, emulator, Reg.NR12, "NR12");
        IoRow(w, emulator, Reg.NR13, "NR13");
        IoRow(w, emulator, Reg.NR14, "NR14");
        w.EndPanel();

        w.BeginPanel("Ch2 (Square)", 4);
        IoRow(w, emulator, Reg.NR21, "NR21");
        IoRow(w, emulator, Reg.NR22, "NR22");
        IoRow(w, emulator, Reg.NR23, "NR23");
        IoRow(w, emulator, Reg.NR24, "NR24");
        w.EndPanel();

        w.BeginPanel("Wave RAM (FF30-F)", 4);
        DrawWaveRamGrid(w, emulator);
        w.EndPanel();
    }

    static void DrawColumn4(Emulator emulator)
    {
        var w = new ColumnWriter();
        w.BeginPanel("Ch3 (Wave)", 5);
        IoRow(w, emulator, Reg.NR30, "NR30");
        IoRow(w, emulator, Reg.NR31, "NR31");
        IoRow(w, emulator, Reg.NR32, "NR32");
        IoRow(w, emulator, Reg.NR33, "NR33");
        IoRow(w, emulator, Reg.NR34, "NR34");
        w.EndPanel();

        w.BeginPanel("Ch4 (Noise)", 4);
        IoRow(w, emulator, Reg.NR41, "NR41");
        IoRow(w, emulator, Reg.NR42, "NR42");
        IoRow(w, emulator, Reg.NR43, "NR43");
        IoRow(w, emulator, Reg.NR44, "NR44");
        w.EndPanel();

        w.BeginPanel("Sound Control", 3);
        IoRow(w, emulator, Reg.NR50, "NR50");
        IoRow(w, emulator, Reg.NR51, "NR51");
        IoRow(w, emulator, Reg.NR52, "NR52");
        w.EndPanel();
    }

    static void DrawWaveRamGrid(ColumnWriter w, Emulator emulator)
    {
        const int cols = 4;
        const int rows = 4;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (col > 0)
                {
                    ImGui.SameLine();
                }

                int index = row * cols + col;
                ushort addr = (ushort)(0xFF30 + index);
                string text = $"{emulator.Read(addr):X2}";
                ImGui.TextColored(C(GuiDebuggerTheme.Value), text);
            }

            if (row < rows - 1)
            {
                ImGui.Spacing();
            }
        }
    }

    static void IoRow(ColumnWriter w, Emulator emulator, ushort addr, string name) =>
        LabelValueRow(w, $"{addr:X4} {name}", $"{emulator.Read(addr):X2}");

    static void Reg16Row(ColumnWriter w, string name, ushort value) =>
        LabelValueRow(w, name, $"{value >> 8:X2} {value & 0xFF:X2}");

    static void GoldLabelRow(ColumnWriter w, string text)
    {
        _ = w;
        ImGui.TextColored(C(GuiDebuggerTheme.Title), text);
    }

    static void LabelValueRow(ColumnWriter w, string label, string value)
    {
        _ = w;
        ImGui.TextColored(C(GuiDebuggerTheme.Label), $"{label}:");
        ImGui.SameLine(ValueColumnWidth);
        ImGui.TextColored(C(GuiDebuggerTheme.Value), value);
    }

    sealed class ColumnWriter
    {
        public void BeginPanel(string title, int contentRows)
        {
            float lineH = ImGui.GetTextLineHeightWithSpacing();
            float panelH = PanelPad * 2 + lineH + TitleRowHeight + contentRows * DataRowHeight;

            ImGui.PushStyleColor(ImGuiCol.ChildBg, C(GuiDebuggerTheme.PanelBackground));
            ImGui.PushStyleColor(ImGuiCol.Border, C(GuiDebuggerTheme.PanelBorder));
            ImGui.BeginChild(title, new Vector2(-1, panelH), ImGuiChildFlags.Borders);

            float titleW = ImGui.CalcTextSize(title).X;
            float regionW = ImGui.GetContentRegionAvail().X;
            ImGui.SetCursorPosX(Math.Max(0, (regionW - titleW) * 0.5f));
            ImGui.TextColored(C(GuiDebuggerTheme.Title), title);
            ImGui.Separator();
        }

        public void EndPanel()
        {
            ImGui.EndChild();
            ImGui.PopStyleColor(2);
            ImGui.Dummy(new Vector2(0, PanelGap));
        }
    }
}
