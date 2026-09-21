namespace Monoboy.Desktop;

using System;
using System.IO;

using Monoboy;
using Monoboy.Constants;
using Monoboy.Desktop.Debugger;

using Raylib_cs;

/// <summary>
/// Raylib GUI debugger overlay for the graphical emulator window.
/// Toggle with F12. Does not own the window.
/// </summary>
public sealed class GuiDebugger : IDisposable
{
    public const int DefaultWidth = 1280;
    public const int DefaultHeight = 832;

    const int ToolbarHeight = 28;
    const float LeftColumnFraction = 0.42f;
    const float RegisterPaneFraction = 0.60f;
    const int MinHexPaneHeight = 96;
    const float LcdPaneFraction = 0.38f;
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
    bool _showPpu;
    int _disasmScrollRows;
    int _hexScrollRows;
    bool _disposed;

    public GuiDebugger()
    {
        (_font, _ownsFont) = TryLoadMonospaceFont();
        GuiDebuggerFont.Font = _font;
        GuiDebuggerFont.UseMono = _ownsFont;
        _lcdTex = CreateTexture(Emulator.WindowWidth, Emulator.WindowHeight);
    }

    public void HandleInput(Emulator emulator, ref bool running)
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            var mouse = Raylib.GetMousePosition();
            if (Raylib.CheckCollisionPointRec(mouse, RunHitRect()))
            {
                running = !running;
            }
            else if (Raylib.CheckCollisionPointRec(mouse, PpuHitRect()))
            {
                _showPpu = !_showPpu;
            }
        }

        if (!running && Raylib.IsKeyPressed(KeyboardKey.F10))
        {
            emulator.Step();
        }
    }

    public void UpdateTextures(Emulator emulator)
    {
        if (_showPpu && _bgMapBuf == null)
        {
            AllocatePpuTextures(emulator);
        }

        Array.Copy(emulator.Framebuffer, _lcdBuf, _lcdBuf.Length);
        Raylib.UpdateTexture(_lcdTex, _lcdBuf);

        if (_showPpu && _bgMapBuf != null)
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
        ComputeLayout(screenW, screenH, out Rectangle lcdArea, out Rectangle disasmArea, out Rectangle registerArea, out Rectangle hexArea);

        Raylib.ClearBackground(GuiDebuggerTheme.Canvas);
        DrawToolbar(screenW, running, _showPpu);
        DrawLcdPanel(lcdArea, emulator);
        DrawPanel(disasmArea);
        GuiDisassemblyView.Draw(InsetForContent(disasmArea, hasTitle: false), emulator, ref _disasmScrollRows);
        DrawPanel(registerArea);
        GuiRegisterPanels.Draw(InsetForContent(registerArea, hasTitle: false), emulator);
        DrawPanel(hexArea);
        GuiMemoryDumpView.Draw(InsetForContent(hexArea, hasTitle: false), emulator, ref _hexScrollRows);
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

        int lcdH = Math.Max(120, (int)(contentH * LcdPaneFraction));
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

        if (_showPpu)
        {
            int split = (int)(area.Width * 0.45f);
            var lcdRect = new Rectangle(area.X + 4, area.Y + PanelHeader, split - 8, area.Height - PanelHeader - 4);
            var ppuRect = new Rectangle(area.X + split, area.Y + PanelHeader, area.Width - split - 4, area.Height - PanelHeader - 4);
            DrawLcdBezel(lcdRect, _lcdTex);
            DrawCompactPpuViews(ppuRect, emulator);
        }
        else
        {
            var lcdRect = new Rectangle(area.X + 4, area.Y + 4, area.Width - 8, area.Height - 8);
            DrawLcdBezel(lcdRect, _lcdTex);
        }
    }

    static void DrawLcdBezel(Rectangle area, Texture2D lcdTex)
    {
        const int bezel = 3;
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

    void DrawCompactPpuViews(Rectangle area, Emulator emulator)
    {
        if (area.Width < 8 || area.Height < 8)
        {
            return;
        }

        const int labelH = 12;
        int mapPx = PpuDebugViewRenderer.TileMapPixels;
        int mapScale = Math.Max(1, Math.Min((int)(area.Width * 0.48f) / mapPx, (int)(area.Height * 0.45f) / mapPx));

        float x = area.X;
        float y = area.Y;
        DrawTinyLabel("BG", x, y);
        Raylib.DrawTextureEx(_bgMapTex, new(x, y + labelH), 0, mapScale, Color.White);
        DrawTilemapViewport(x, y + labelH, mapScale, emulator.Read(0xFF43), emulator.Read(0xFF42));

        float winX = x + (mapPx * mapScale) + 4;
        DrawTinyLabel("WIN", winX, y);
        Raylib.DrawTextureEx(_winMapTex, new(winX, y + labelH), 0, mapScale, Color.White);
        byte lcdc = emulator.Read(0xFF40);
        if ((lcdc & Flags.WindowEnabled) != 0)
        {
            int wx = emulator.Read(0xFF4B) - 7;
            int wy = emulator.Read(0xFF4A);
            int winW = Math.Max(0, Emulator.WindowWidth - Math.Max(0, wx));
            int winH = Math.Max(0, Emulator.WindowHeight - wy);
            DrawViewportRect(winX, y + labelH, mapScale, 0, 0, winW, winH);
        }

        float row2 = y + labelH + (mapPx * mapScale) + 6;
        int vramScale = Math.Max(1, Math.Min((int)area.Width / PpuDebugViewRenderer.VramTilesWidth, (int)((area.Height - row2 + area.Y) * 0.55f) / PpuDebugViewRenderer.VramTilesHeight));
        DrawTinyLabel("VRAM", x, row2);
        Raylib.DrawTextureEx(_vramTex, new(x, row2 + labelH), 0, vramScale, Color.White);

        float oamX = x + (PpuDebugViewRenderer.VramTilesWidth * vramScale) + 4;
        DrawTinyLabel("OAM", oamX, row2);
        int oamScale = Math.Max(1, Math.Min((int)(area.Width - (oamX - area.X)) / PpuDebugViewRenderer.OamGridWidth, vramScale));
        Raylib.DrawTextureEx(_oamTex, new(oamX, row2 + labelH), 0, oamScale, Color.White);
    }

    static void DrawTinyLabel(string text, float x, float y)
    {
        const int size = 10;
        GuiDebuggerFont.Draw(text, (int)x, (int)y, size, GuiDebuggerTheme.Label);
    }

    static void DrawTilemapViewport(float texX, float texY, int scale, int scx, int scy)
    {
        DrawViewportRect(texX, texY, scale, scx, scy, Emulator.WindowWidth, Emulator.WindowHeight);
    }

    static void DrawViewportRect(float texX, float texY, int scale, int vx, int vy, int vw, int vh)
    {
        Raylib.DrawRectangleLines(
            (int)(texX + (vx * scale)),
            (int)(texY + (vy * scale)),
            vw * scale,
            vh * scale,
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
                return (Raylib.LoadFontEx(path, 14, null, 0), true);
            }
        }

        return (Raylib.GetFontDefault(), false);
    }
}
