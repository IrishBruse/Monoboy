namespace Monoboy.Desktop.GuiDebugger;

using System;

using Monoboy;
using Monoboy.Constants;

using Raylib_cs;

/// <summary>
/// Four-column register mosaic. Theme: Title, Label, Value, PanelBackground, PanelBorder.
/// </summary>
public static class GuiRegisterPanels
{
    const int ColumnGutter = 8;
    const int PanelGap = 6;
    const int PanelPad = 8;
    const int TitleSize = 13;
    const int RowSize = 13;
    const int TitleRowHeight = 18;
    const int DataRowHeight = 16;

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

    public static void Draw(Rectangle area, Emulator emulator, float wheel, ref int scrollY)
    {
        int areaX = (int)area.X;
        int areaY = (int)area.Y;
        int areaW = Math.Max(0, (int)area.Width);
        int areaH = Math.Max(0, (int)area.Height);
        if (areaW <= 0 || areaH <= 0)
        {
            return;
        }

        int maxScroll = Math.Max(0, RequiredDrawHeight - areaH);
        GuiScroll.ApplyWheel(area, wheel, ref scrollY, maxScroll);
        scrollY = Math.Clamp(scrollY, 0, maxScroll);

        DebugState s = emulator.GetDebugState();
        int colW = Math.Max(1, (areaW - GuiScroll.Width - ColumnGutter * 3) / 4);
        int valueX = GuiDebuggerFont.CharWidth(RowSize) * 13;
        int originY = areaY - scrollY;

        Raylib.BeginScissorMode(areaX, areaY, Math.Max(0, areaW - GuiScroll.Width), areaH);
        DrawColumn1(new Rectangle(areaX, originY, colW, areaH + scrollY), emulator, s, valueX);
        DrawColumn2(new Rectangle(areaX + colW + ColumnGutter, originY, colW, areaH + scrollY), emulator, s, valueX);
        DrawColumn3(new Rectangle(areaX + (colW + ColumnGutter) * 2, originY, colW, areaH + scrollY), emulator, valueX);
        DrawColumn4(new Rectangle(areaX + (colW + ColumnGutter) * 3, originY, colW, areaH + scrollY), emulator, valueX);
        Raylib.EndScissorMode();

        var bar = new Rectangle(areaX + areaW - GuiScroll.Width, areaY, GuiScroll.Width, areaH);
        GuiScroll.Draw(3, bar, ref scrollY, maxScroll);
    }

    static void DrawColumn1(Rectangle col, Emulator emulator, DebugState s, int valueX)
    {
        var w = new ColumnWriter(col, valueX);
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

    static void DrawColumn2(Rectangle col, Emulator emulator, DebugState s, int valueX)
    {
        var w = new ColumnWriter(col, valueX);
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

    static void DrawColumn3(Rectangle col, Emulator emulator, int valueX)
    {
        var w = new ColumnWriter(col, valueX);
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

    static void DrawColumn4(Rectangle col, Emulator emulator, int valueX)
    {
        var w = new ColumnWriter(col, valueX);
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
        int cellW = Math.Max(GuiDebuggerFont.CharWidth(RowSize) * 3, (w.ContentWidth - 6) / cols);
        for (int row = 0; row < rows; row++)
        {
            int y = w.NextRowY();
            for (int col = 0; col < cols; col++)
            {
                int index = row * cols + col;
                ushort addr = (ushort)(0xFF30 + index);
                string text = $"{emulator.Read(addr):X2}";
                int x = w.ContentX + col * cellW;
                GuiDebuggerFont.Draw(text, x, y, RowSize, PanelValueColor);
            }
        }
    }

    static void IoRow(ColumnWriter w, Emulator emulator, ushort addr, string name) =>
        LabelValueRow(w, $"{addr:X4} {name}", $"{emulator.Read(addr):X2}");

    static void Reg16Row(ColumnWriter w, string name, ushort value) =>
        LabelValueRow(w, name, $"{value >> 8:X2} {value & 0xFF:X2}");

    static void GoldLabelRow(ColumnWriter w, string text)
    {
        int y = w.NextRowY();
        GuiDebuggerFont.Draw(text, w.ContentX, y, RowSize, PanelTitleColor);
    }

    static void LabelValueRow(ColumnWriter w, string label, string value)
    {
        int y = w.NextRowY();
        string prefix = $"{label}:";
        GuiDebuggerFont.Draw(prefix, w.ContentX, y, RowSize, PanelLabelColor);
        int vx = w.ContentX + w.ValueX;
        if (vx < w.ContentX + GuiDebuggerFont.Measure(prefix, RowSize) + 6)
        {
            vx = w.ContentX + GuiDebuggerFont.Measure(prefix, RowSize) + 6;
        }

        GuiDebuggerFont.Draw(value, vx, y, RowSize, PanelValueColor);
    }

    static Color PanelTitleColor => GuiDebuggerTheme.Title;
    static Color PanelLabelColor => GuiDebuggerTheme.Label;
    static Color PanelValueColor => GuiDebuggerTheme.Value;
    static Color PanelFillColor => GuiDebuggerTheme.PanelBackground;
    static Color PanelBorderColor => GuiDebuggerTheme.PanelBorder;

    sealed class ColumnWriter
    {
        readonly Rectangle _col;
        int _y;
        int _contentY;
        int _contentRows;

        public ColumnWriter(Rectangle col, int valueX)
        {
            _col = col;
            ValueX = valueX;
            _y = (int)col.Y;
            _contentY = 0;
            _contentRows = 0;
        }

        public int ContentX => (int)_col.X + PanelPad;
        public int ContentWidth => Math.Max(1, (int)_col.Width - PanelPad * 2);
        public int ContentY => _contentY;
        public int ValueX { get; }

        public void BeginPanel(string title, int contentRows)
        {
            _contentRows = contentRows;
            int panelH = PanelPad + TitleRowHeight + contentRows * DataRowHeight + PanelPad;
            var panel = new Rectangle(_col.X, _y, _col.Width, panelH);
            Raylib.DrawRectangle((int)panel.X, (int)panel.Y, (int)panel.Width, (int)panel.Height, PanelFillColor);
            Raylib.DrawRectangleLines((int)panel.X, (int)panel.Y, (int)panel.Width, (int)panel.Height, PanelBorderColor);

            int titleW = GuiDebuggerFont.Measure(title, TitleSize);
            int titleX = (int)panel.X + Math.Max(0, ((int)panel.Width - titleW) / 2);
            int titleY = (int)panel.Y + PanelPad;
            GuiDebuggerFont.Draw(title, titleX, titleY, TitleSize, PanelTitleColor);

            _contentY = (int)panel.Y + PanelPad + TitleRowHeight;
        }

        public void EndPanel()
        {
            int panelH = PanelPad + TitleRowHeight + _contentRows * DataRowHeight + PanelPad;
            _y += panelH + PanelGap;
        }

        public int NextRowY()
        {
            int y = _contentY;
            _contentY += DataRowHeight;
            return y;
        }
    }
}
