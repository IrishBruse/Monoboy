namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.IO;

using Monoboy;
using Monoboy.Constants;
using Monoboy.Desktop.TuiDebugger;

using Raylib_cs;

/// <summary>
/// Raylib GUI debugger overlay for the graphical emulator window.
/// Toggle with F12. Does not own the window.
/// </summary>
public sealed class GuiDebugger : IDisposable
{
    public const int DefaultWidth = 1400;
    public const int DefaultHeight = 900;

    const int ToolbarHeight = 28;
    const float LeftColumnFraction = 0.42f;
    const float RegisterPaneFraction = 0.60f;
    const int MinHexPaneHeight = 96;
    const float LcdPaneFraction = 0.50f;
    const int OuterPad = 6;
    const int PanelTitleSize = 14;
    const int ToolbarTextSize = 14;
    const int PanelHeader = 22;

    readonly byte[] _lcdBuf = new byte[Emulator.WindowWidth * Emulator.WindowHeight * 4];
    readonly Texture2D _lcdTex;
    readonly Font _font;
    readonly bool _ownsFont;

    byte[]? _bgMapBuf;
    byte[]? _winMapBuf;
    byte[]? _vramBuf;
    byte[]? _oamBuf;
    Texture2D _bgMapTex;
    Texture2D _winMapTex;
    Texture2D _vramTex;
    Texture2D _oamTex;
    int _oamGridH;
    bool _showBg;
    bool _showOam;
    bool _showTiles;
    MenuId _openMenu;
    int _disasmScrollRows;
    int _hexScrollRows;
    bool _disposed;

    enum MenuId
    {
        None,
        Run,
        Ppu
    }

    static readonly string[] RunLabels =
    [
        "Step",
        "Next",
        "Continue",
        "Stop",
        "Reset",
        "Next VBlank"
    ];

    static readonly string[] RunShortcuts =
    [
        "F3",
        "F8",
        "F9",
        "Shift+F9",
        "Control+R",
        "F10"
    ];

    static readonly string[] PpuLabels =
    [
        "OAM Viewer",
        "BG Viewer",
        "TilesViewer"
    ];

    static readonly string[] PpuShortcuts =
    [
        "Shift+O",
        "Shift+B",
        "Shift+T"
    ];

    bool ShowPpuViews => _showBg || _showOam || _showTiles;

    public GuiDebugger()
    {
        (_font, _ownsFont) = TryLoadMonospaceFont();
        GuiDebuggerFont.Font = _font;
        GuiDebuggerFont.UseMono = _ownsFont;
        _lcdTex = CreateTexture(Emulator.WindowWidth, Emulator.WindowHeight);
    }

    public void HandleInput(Emulator emulator, ref bool running)
    {
        HandleShortcuts(emulator, ref running);

        if (!Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            return;
        }

        var mouse = Raylib.GetMousePosition();
        if (_openMenu == MenuId.Run)
        {
            Rectangle menu = RunMenuBounds();
            int row = GuiDebuggerMenu.HitRow(menu, RunLabels.Length, mouse);
            if (row >= 0)
            {
                ApplyRunCommand(row, emulator, ref running);
                _openMenu = MenuId.None;
                return;
            }
        }
        else if (_openMenu == MenuId.Ppu)
        {
            Rectangle menu = PpuMenuBounds();
            int row = GuiDebuggerMenu.HitRow(menu, PpuLabels.Length, mouse);
            if (row >= 0)
            {
                ApplyPpuCommand(row);
                _openMenu = MenuId.None;
                return;
            }
        }

        if (Raylib.CheckCollisionPointRec(mouse, RunHitRect()))
        {
            _openMenu = _openMenu == MenuId.Run ? MenuId.None : MenuId.Run;
            return;
        }

        if (Raylib.CheckCollisionPointRec(mouse, PpuHitRect()))
        {
            _openMenu = _openMenu == MenuId.Ppu ? MenuId.None : MenuId.Ppu;
            return;
        }

        _openMenu = MenuId.None;
    }

    void HandleShortcuts(Emulator emulator, ref bool running)
    {
        bool shift = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
        bool control = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);

        if (Raylib.IsKeyPressed(KeyboardKey.F3))
        {
            ApplyRunCommand(0, emulator, ref running);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F8))
        {
            ApplyRunCommand(1, emulator, ref running);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F9))
        {
            ApplyRunCommand(shift ? 3 : 2, emulator, ref running);
        }
        else if (control && Raylib.IsKeyPressed(KeyboardKey.R))
        {
            ApplyRunCommand(4, emulator, ref running);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F10))
        {
            ApplyRunCommand(5, emulator, ref running);
        }
        else if (shift && Raylib.IsKeyPressed(KeyboardKey.O))
        {
            ApplyPpuCommand(0);
        }
        else if (shift && Raylib.IsKeyPressed(KeyboardKey.B))
        {
            ApplyPpuCommand(1);
        }
        else if (shift && Raylib.IsKeyPressed(KeyboardKey.T))
        {
            ApplyPpuCommand(2);
        }
    }

    void ApplyRunCommand(int index, Emulator emulator, ref bool running)
    {
        switch (index)
        {
            case 0:
                running = false;
                emulator.Step();
                break;
            case 1:
                running = false;
                StepOver(emulator);
                break;
            case 2:
                running = true;
                break;
            case 3:
                running = false;
                break;
            case 4:
                running = false;
                emulator.Reset();
                break;
            case 5:
                running = false;
                emulator.StepFrame();
                break;
        }
    }

    void ApplyPpuCommand(int index)
    {
        switch (index)
        {
            case 0:
                _showOam = !_showOam;
                break;
            case 1:
                _showBg = !_showBg;
                break;
            case 2:
                _showTiles = !_showTiles;
                break;
        }
    }

    static void StepOver(Emulator emulator)
    {
        ushort pc = emulator.GetDebugState().PC;
        byte op = emulator.Read(pc);
        bool isCall = op is 0xC4 or 0xCC or 0xCD or 0xD4 or 0xDC;
        bool isRst = op is 0xC7 or 0xCF or 0xD7 or 0xDF or 0xE7 or 0xEF or 0xF7 or 0xFF;
        if (!isCall && !isRst)
        {
            emulator.Step();
            return;
        }

        ushort next = (ushort)(pc + TuiDisassemblyFormatter.GetInstructionByteSize(emulator, pc));
        for (int i = 0; i < 1_000_000; i++)
        {
            emulator.Step();
            if (emulator.GetDebugState().PC == next)
            {
                return;
            }
        }
    }

    static Rectangle RunMenuBounds()
    {
        int w = GuiDebuggerMenu.MeasureWidth(RunLabels, RunShortcuts);
        return GuiDebuggerMenu.Bounds(8, ToolbarHeight - 1, w, RunLabels.Length);
    }

    static Rectangle PpuMenuBounds()
    {
        int w = GuiDebuggerMenu.MeasureWidth(PpuLabels, PpuShortcuts);
        return GuiDebuggerMenu.Bounds(50, ToolbarHeight - 1, w, PpuLabels.Length);
    }

    public void UpdateTextures(Emulator emulator)
    {
        if (ShowPpuViews && _bgMapBuf == null)
        {
            AllocatePpuTextures(emulator);
        }

        Array.Copy(emulator.Framebuffer, _lcdBuf, _lcdBuf.Length);
        Raylib.UpdateTexture(_lcdTex, _lcdBuf);

        if (ShowPpuViews && _bgMapBuf != null)
        {
            int oamHeight = PpuDebugViewRenderer.OamGridHeight(emulator);
            if (oamHeight != _oamGridH)
            {
                Raylib.UnloadTexture(_oamTex);
                _oamGridH = oamHeight;
                _oamBuf = new byte[PpuDebugViewRenderer.OamGridWidth * _oamGridH * 4];
                _oamTex = CreateTexture(PpuDebugViewRenderer.OamGridWidth, _oamGridH);
            }

            PpuDebugViewRenderer.FillTileMap(_bgMapBuf, emulator, windowMap: false);
            PpuDebugViewRenderer.FillTileMap(_winMapBuf!, emulator, windowMap: true);
            PpuDebugViewRenderer.FillVramTiles(_vramBuf!, emulator);
            PpuDebugViewRenderer.FillOamGrid(_oamBuf!, emulator);
            Raylib.UpdateTexture(_bgMapTex, _bgMapBuf);
            Raylib.UpdateTexture(_winMapTex, _winMapBuf!);
            Raylib.UpdateTexture(_vramTex, _vramBuf!);
            Raylib.UpdateTexture(_oamTex, _oamBuf!);
        }
    }

    public void Draw(Emulator emulator, bool running)
    {
        GuiDebuggerFont.Font = _font;
        GuiDebuggerFont.UseMono = _ownsFont;

        int screenW = Raylib.GetScreenWidth();
        int screenH = Raylib.GetScreenHeight();
        ComputeLayout(screenW, screenH, ShowPpuViews, out Rectangle lcdArea, out Rectangle disasmArea, out Rectangle registerArea, out Rectangle hexArea);

        Raylib.ClearBackground(GuiDebuggerTheme.Canvas);
        DrawToolbar(screenW, running, ShowPpuViews);
        DrawLcdPanel(lcdArea, emulator);
        DrawPanel(disasmArea);
        GuiDisassemblyView.Draw(InsetForContent(disasmArea, hasTitle: false), emulator, ref _disasmScrollRows);
        DrawPanel(registerArea);
        GuiRegisterPanels.Draw(InsetForContent(registerArea, hasTitle: false), emulator);
        DrawPanel(hexArea);
        GuiMemoryDumpView.Draw(InsetForContent(hexArea, hasTitle: false), emulator, ref _hexScrollRows);
        DrawOpenMenu();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Raylib.UnloadTexture(_lcdTex);
        if (_bgMapBuf != null)
        {
            Raylib.UnloadTexture(_bgMapTex);
            Raylib.UnloadTexture(_winMapTex);
            Raylib.UnloadTexture(_vramTex);
            Raylib.UnloadTexture(_oamTex);
        }

        if (_ownsFont)
        {
            Raylib.UnloadFont(_font);
        }
    }

    static void ComputeLayout(
        int screenW,
        int screenH,
        bool showPpu,
        out Rectangle lcdArea,
        out Rectangle disasmArea,
        out Rectangle registerArea,
        out Rectangle hexArea)
    {
        int contentTop = ToolbarHeight + OuterPad;
        int contentH = screenH - contentTop - OuterPad;
        int leftW = (int)(screenW * LeftColumnFraction);
        int rightX = leftW + OuterPad;
        int rightW = screenW - rightX - OuterPad;
        int innerLeftW = leftW - (OuterPad * 2);

        float lcdFrac = showPpu ? 0.62f : LcdPaneFraction;
        int lcdH = Math.Max(120, (int)(contentH * lcdFrac));
        int disasmMin = 140;
        lcdH = Math.Min(lcdH, contentH - OuterPad - disasmMin);
        lcdArea = new(OuterPad, contentTop, innerLeftW, lcdH);
        disasmArea = new(OuterPad, contentTop + lcdH + OuterPad, innerLeftW, contentH - lcdH - OuterPad);

        int regInsetPad = 8;
        int regMinH = GuiRegisterPanels.RequiredDrawHeight + regInsetPad;
        int regH = Math.Max(regMinH, (int)(contentH * RegisterPaneFraction));
        regH = Math.Min(regH, contentH - OuterPad - MinHexPaneHeight);
        regH = Math.Max(regMinH, regH);
        registerArea = new(rightX, contentTop, rightW, regH);
        hexArea = new(rightX, contentTop + regH + OuterPad, rightW, contentH - regH - OuterPad);
    }

    static Rectangle InsetForContent(Rectangle panel, bool hasTitle = true)
    {
        int top = hasTitle ? PanelHeader : 4;
        return new(
            panel.X + 4,
            panel.Y + top,
            Math.Max(0, panel.Width - 8),
            Math.Max(0, panel.Height - top - 4));
    }

    static void DrawToolbar(int screenW, bool running, bool showPpu)
    {
        Raylib.DrawRectangle(0, 0, screenW, ToolbarHeight, GuiDebuggerTheme.PanelBackground);
        Raylib.DrawLine(0, ToolbarHeight - 1, screenW, ToolbarHeight - 1, GuiDebuggerTheme.PanelBorder);

        Color runColor = running ? GuiDebuggerTheme.Title : GuiDebuggerTheme.ToolbarLabel;
        Color ppuColor = showPpu ? GuiDebuggerTheme.Title : GuiDebuggerTheme.ToolbarLabel;

        DrawToolbarLabel("Run", 12, runColor);
        DrawToolbarLabel("PPU", 56, ppuColor);
    }

    void DrawOpenMenu()
    {
        var mouse = Raylib.GetMousePosition();
        if (_openMenu == MenuId.Run)
        {
            GuiDebuggerMenu.Draw(RunMenuBounds(), RunLabels, RunShortcuts, mouse);
        }
        else if (_openMenu == MenuId.Ppu)
        {
            GuiDebuggerMenu.Draw(PpuMenuBounds(), PpuLabels, PpuShortcuts, mouse);
        }
    }

    static Rectangle RunHitRect() => new(8, 2, 36, ToolbarHeight - 4);

    static Rectangle PpuHitRect() => new(50, 2, 40, ToolbarHeight - 4);

    static void DrawToolbarLabel(string text, int x, Color color)
    {
        int y = (ToolbarHeight - ToolbarTextSize) / 2;
        GuiDebuggerFont.Draw(text, x, y, ToolbarTextSize, color);
    }

    static void DrawPanel(Rectangle area, string title = "")
    {
        Raylib.DrawRectangleRec(area, GuiDebuggerTheme.PanelBackground);
        Raylib.DrawRectangleLinesEx(area, 1, GuiDebuggerTheme.PanelBorder);

        if (title.Length == 0)
        {
            return;
        }

        float titleW = GuiDebuggerFont.Measure(title, PanelTitleSize);
        float titleX = area.X + ((area.Width - titleW) * 0.5f);
        float titleY = area.Y + 4;
        GuiDebuggerFont.Draw(title, (int)titleX, (int)titleY, PanelTitleSize, GuiDebuggerTheme.Title);
    }

    void DrawLcdPanel(Rectangle area, Emulator emulator)
    {
        Raylib.DrawRectangleRec(area, GuiDebuggerTheme.PanelBackground);
        Raylib.DrawRectangleLinesEx(area, 1, GuiDebuggerTheme.PanelBorder);

        int ax = (int)area.X;
        int ay = (int)area.Y;
        int aw = Math.Max(0, (int)area.Width);
        int ah = Math.Max(0, (int)area.Height);
        Raylib.BeginScissorMode(ax + 1, ay + 1, Math.Max(0, aw - 2), Math.Max(0, ah - 2));

        if (_showPpu)
        {
            DrawPpuOverview(area, emulator);
        }
        else
        {
            var lcdRect = new Rectangle(area.X + 4, area.Y + 4, area.Width - 8, area.Height - 8);
            DrawLcdBezel(lcdRect, _lcdTex);
        }

        Raylib.EndScissorMode();
    }

    void DrawPpuOverview(Rectangle area, Emulator emulator)
    {
        const int pad = 6;
        const int gap = 6;
        const int labelH = 14;
        float innerX = area.X + pad;
        float innerY = area.Y + pad;
        float innerW = Math.Max(8, area.Width - (pad * 2));
        float innerH = Math.Max(8, area.Height - (pad * 2));
        float topH = innerH * 0.58f;
        float botH = innerH - topH - gap;
        float colW = (innerW - (gap * 2)) / 3f;

        var lcdCell = new Rectangle(innerX, innerY + labelH, colW, topH - labelH);
        var bgCell = new Rectangle(innerX + colW + gap, innerY + labelH, colW, topH - labelH);
        var winCell = new Rectangle(innerX + ((colW + gap) * 2), innerY + labelH, colW, topH - labelH);
        float botY = innerY + topH + gap;
        float vramW = innerW * 0.62f;
        var vramCell = new Rectangle(innerX, botY + labelH, vramW, botH - labelH);
        var oamCell = new Rectangle(innerX + vramW + gap, botY + labelH, innerW - vramW - gap, botH - labelH);

        DrawTinyLabel("LCD", innerX, innerY);
        DrawFittedLcd(lcdCell);

        DrawTinyLabel("BG", bgCell.X, innerY);
        DrawFittedTexture(_bgMapTex, bgCell, PpuDebugViewRenderer.TileMapPixels, PpuDebugViewRenderer.TileMapPixels, out float bgX, out float bgY, out float bgScale);
        DrawViewportRect(bgX, bgY, bgScale, emulator.Read(0xFF43), emulator.Read(0xFF42), Emulator.WindowWidth, Emulator.WindowHeight);

        DrawTinyLabel("WIN", winCell.X, innerY);
        DrawFittedTexture(_winMapTex, winCell, PpuDebugViewRenderer.TileMapPixels, PpuDebugViewRenderer.TileMapPixels, out float winX, out float winY, out float winScale);
        byte lcdc = emulator.Read(0xFF40);
        if ((lcdc & Flags.WindowEnabled) != 0)
        {
            int wx = emulator.Read(0xFF4B) - 7;
            int wy = emulator.Read(0xFF4A);
            int winW = Math.Max(0, Emulator.WindowWidth - Math.Max(0, wx));
            int winH = Math.Max(0, Emulator.WindowHeight - wy);
            DrawViewportRect(winX, winY, winScale, 0, 0, winW, winH);
        }

        DrawTinyLabel("VRAM", vramCell.X, botY);
        DrawFittedTexture(_vramTex, vramCell, PpuDebugViewRenderer.VramTilesWidth, PpuDebugViewRenderer.VramTilesHeight, out _, out _, out _);

        DrawTinyLabel("OAM", oamCell.X, botY);
        DrawFittedTexture(_oamTex, oamCell, PpuDebugViewRenderer.OamGridWidth, Math.Max(1, _oamGridH), out _, out _, out _);
    }

    void DrawFittedLcd(Rectangle cell)
    {
        DrawLcdBezel(cell, _lcdTex);
    }

    static void DrawFittedTexture(
        Texture2D tex,
        Rectangle cell,
        int srcW,
        int srcH,
        out float drawX,
        out float drawY,
        out float scale)
    {
        if (srcW < 1 || srcH < 1 || cell.Width < 1 || cell.Height < 1)
        {
            drawX = cell.X;
            drawY = cell.Y;
            scale = 1;
            return;
        }

        scale = Math.Min(cell.Width / srcW, cell.Height / srcH);
        if (scale <= 0)
        {
            scale = 0.01f;
        }

        float dw = srcW * scale;
        float dh = srcH * scale;
        drawX = cell.X + ((cell.Width - dw) * 0.5f);
        drawY = cell.Y + ((cell.Height - dh) * 0.5f);
        Raylib.DrawTextureEx(tex, new(drawX, drawY), 0, scale, Color.White);
    }

    static void DrawLcdBezel(Rectangle area, Texture2D lcdTex)
    {
        const int bezel = 6;
        int innerW = (int)area.Width - (bezel * 2);
        int innerH = (int)area.Height - (bezel * 2);
        int scale = Math.Max(1, Math.Min(innerW / Emulator.WindowWidth, innerH / Emulator.WindowHeight));
        int drawW = Emulator.WindowWidth * scale;
        int drawH = Emulator.WindowHeight * scale;
        float bezelX = area.X + ((area.Width - drawW - (bezel * 2)) * 0.5f);
        float bezelY = area.Y + ((area.Height - drawH - (bezel * 2)) * 0.5f);
        var bezelRect = new Rectangle(bezelX, bezelY, drawW + (bezel * 2), drawH + (bezel * 2));

        Raylib.DrawRectangleRec(bezelRect, GuiDebuggerTheme.LcdBezel);
        Raylib.DrawTextureEx(
            lcdTex,
            new(bezelRect.X + bezel, bezelRect.Y + bezel),
            0,
            scale,
            Color.White);
    }

    static void DrawTinyLabel(string text, float x, float y)
    {
        const int size = 10;
        GuiDebuggerFont.Draw(text, (int)x, (int)y, size, GuiDebuggerTheme.Label);
    }

    static void DrawViewportRect(float texX, float texY, float scale, int vx, int vy, int vw, int vh)
    {
        Raylib.DrawRectangleLines(
            (int)(texX + (vx * scale)),
            (int)(texY + (vy * scale)),
            Math.Max(1, (int)(vw * scale)),
            Math.Max(1, (int)(vh * scale)),
            new Color(0xE0, 0x40, 0x40, 0xFF));
    }

    void AllocatePpuTextures(Emulator emulator)
    {
        int mapPx = PpuDebugViewRenderer.TileMapPixels;
        _bgMapBuf = new byte[mapPx * mapPx * 4];
        _winMapBuf = new byte[mapPx * mapPx * 4];
        _vramBuf = new byte[PpuDebugViewRenderer.VramTilesWidth * PpuDebugViewRenderer.VramTilesHeight * 4];
        _oamGridH = PpuDebugViewRenderer.OamGridHeight(emulator);
        _oamBuf = new byte[PpuDebugViewRenderer.OamGridWidth * _oamGridH * 4];

        _bgMapTex = CreateTexture(mapPx, mapPx);
        _winMapTex = CreateTexture(mapPx, mapPx);
        _vramTex = CreateTexture(PpuDebugViewRenderer.VramTilesWidth, PpuDebugViewRenderer.VramTilesHeight);
        _oamTex = CreateTexture(PpuDebugViewRenderer.OamGridWidth, _oamGridH);
    }

    static Texture2D CreateTexture(int width, int height)
    {
        Image image = Raylib.GenImageColor(width, height, Color.Black);
        Texture2D texture = Raylib.LoadTextureFromImage(image);
        Raylib.SetTextureFilter(texture, TextureFilter.Point);
        Raylib.UnloadImage(image);
        return texture;
    }

    static (Font Font, bool Owns) TryLoadMonospaceFont()
    {
        ReadOnlySpan<string> paths =
        [
            "/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf",
            "/usr/share/fonts/TTF/DejaVuSansMono.ttf",
            "/usr/share/fonts/truetype/liberation/LiberationMono-Regular.ttf",
            "C:\\Windows\\Fonts\\consola.ttf",
        ];

        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                Font font = Raylib.LoadFontEx(path, 32, null, 0);
                Raylib.SetTextureFilter(font.Texture, TextureFilter.Bilinear);
                return (font, true);
            }
        }

        return (Raylib.GetFontDefault(), false);
    }
}
