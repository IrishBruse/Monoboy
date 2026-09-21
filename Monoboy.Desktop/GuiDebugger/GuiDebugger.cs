namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using ImGuiNET;

using Monoboy;
using Monoboy.Constants;

using Raylib_cs;

using rlImGui_cs;

/// <summary>
/// Raylib + ImGui dockable GUI debugger. Toggle with F12. Does not own the window.
/// </summary>
public sealed class GuiDebugger : IDisposable
{
    public const int DefaultWidth = 1400;
    public const int DefaultHeight = 900;

    const string DockSpaceName = "MonoboyDock";

    readonly byte[] _lcdBuf = new byte[Emulator.WindowWidth * Emulator.WindowHeight * 4];
    readonly Texture2D _lcdTex;
    readonly string _iniPath;
    static byte[]? _iniFilenameUtf8;
    static GCHandle _iniFilenameHandle;

    byte[]? _bgMapBuf;
    byte[]? _winMapBuf;
    byte[]? _vramBuf;
    byte[]? _oamBuf;
    Texture2D _bgMapTex;
    Texture2D _winMapTex;
    Texture2D _vramTex;
    Texture2D _oamTex;
    int _oamGridH;
    bool _defaultDockAttempted;
    bool _disposed;
    bool _wantsKeyboard;

    bool _showLcd = true;
    bool _showBg = true;
    bool _showWin = true;
    bool _showVram = true;
    bool _showOam = true;
    bool _showDisassembly = true;
    bool _showRegisters = true;
    bool _showMemory = true;

    public GuiDebugger()
    {
        rlImGui.SetupUserFonts = GuiDebuggerFonts.Configure;
        rlImGui.Setup(darkTheme: true, enableDocking: true);
        GuiDebuggerTheme.ApplyImGuiStyle();

        _iniPath = Path.Combine(AppContext.BaseDirectory, "monoboy-layout.ini");
        SetIniFilename(_iniPath);

        _lcdTex = CreateTexture(Emulator.WindowWidth, Emulator.WindowHeight);
    }

    public bool WantsKeyboard => _wantsKeyboard;

    bool ShowPpuTextures => _showBg || _showWin || _showVram || _showOam;

    public void HandleInput(Emulator emulator, ref bool running)
    {
        HandleShortcuts(emulator, ref running);
    }

    void HandleShortcuts(Emulator emulator, ref bool running)
    {
        bool shift = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
        bool control = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);

        if (Raylib.IsKeyPressed(KeyboardKey.F3))
        {
            GuiDebugRunCommands.Apply(GuiDebugRunCommands.StepInto, emulator, ref running);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F8))
        {
            GuiDebugRunCommands.Apply(GuiDebugRunCommands.StepOver, emulator, ref running);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F9))
        {
            GuiDebugRunCommands.Apply(shift ? GuiDebugRunCommands.Pause : GuiDebugRunCommands.Continue, emulator, ref running);
        }
        else if (control && Raylib.IsKeyPressed(KeyboardKey.R))
        {
            GuiDebugRunCommands.Apply(GuiDebugRunCommands.Reset, emulator, ref running);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F10))
        {
            GuiDebugRunCommands.Apply(GuiDebugRunCommands.NextVBlank, emulator, ref running);
        }
        else if (shift && Raylib.IsKeyPressed(KeyboardKey.F11))
        {
            GuiDebugRunCommands.Apply(GuiDebugRunCommands.StepOut, emulator, ref running);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.F11))
        {
            GuiDebugRunCommands.Apply(GuiDebugRunCommands.StepInto, emulator, ref running);
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

    void ApplyPpuCommand(int index)
    {
        switch (index)
        {
            case 0:
                _showOam = !_showOam;
                break;
            case 1:
                bool on = !_showBg;
                _showBg = on;
                _showWin = on;
                break;
            case 2:
                _showVram = !_showVram;
                break;
        }
    }

    public void UpdateTextures(Emulator emulator)
    {
        if (ShowPpuTextures && _bgMapBuf == null)
        {
            AllocatePpuTextures(emulator);
        }

        Array.Copy(emulator.Framebuffer, _lcdBuf, _lcdBuf.Length);
        Raylib.UpdateTexture(_lcdTex, _lcdBuf);

        if (ShowPpuTextures && _bgMapBuf != null)
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

    public void Draw(Emulator emulator, ref bool running)
    {
        Raylib.ClearBackground(GuiDebuggerTheme.Canvas);
        rlImGui.Begin();
        GuiDebuggerTheme.ApplyImGuiStyle();
        GuiMouseCursor.ClearTabBands();
        DrawDockAndWindows(emulator, ref running);
        _wantsKeyboard = ImGui.GetIO().WantCaptureKeyboard;
        GuiMouseCursor.Apply();
        rlImGui.End();
    }

    void DrawDockAndWindows(Emulator emulator, ref bool running)
    {
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        ImGuiWindowFlags hostFlags =
            ImGuiWindowFlags.MenuBar
            | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoNavFocus;

        ImGui.Begin("##MonoboyDebuggerHost", hostFlags);
        ImGui.PopStyleVar(3);

        DrawMainMenuBar(emulator, ref running);
        GuiDebugToolbar.Draw(emulator, ref running);

        uint dockSpaceId = ImGui.GetID(DockSpaceName);
        ImGui.DockSpace(dockSpaceId, Vector2.Zero, ImGuiDockNodeFlags.None);
        if (!_defaultDockAttempted)
        {
            _defaultDockAttempted = true;
            if (!File.Exists(_iniPath))
            {
                BuildDefaultDock(dockSpaceId, viewport.WorkSize);
            }
        }

        ImGui.End();

        DrawToolWindows(emulator, running);
    }

    void DrawMainMenuBar(Emulator emulator, ref bool running)
    {
        if (!ImGui.BeginMenuBar())
        {
            return;
        }

        if (ImGui.BeginMenu("Run"))
        {
            if (ImGui.MenuItem("Step Into", "F3"))
            {
                GuiDebugRunCommands.Apply(GuiDebugRunCommands.StepInto, emulator, ref running);
            }

            if (ImGui.MenuItem("Step Over", "F8"))
            {
                GuiDebugRunCommands.Apply(GuiDebugRunCommands.StepOver, emulator, ref running);
            }

            if (ImGui.MenuItem("Step Out", "Shift+F11"))
            {
                GuiDebugRunCommands.Apply(GuiDebugRunCommands.StepOut, emulator, ref running);
            }

            if (ImGui.MenuItem("Continue", "F9"))
            {
                GuiDebugRunCommands.Apply(GuiDebugRunCommands.Continue, emulator, ref running);
            }

            if (ImGui.MenuItem("Pause", "Shift+F9"))
            {
                GuiDebugRunCommands.Apply(GuiDebugRunCommands.Pause, emulator, ref running);
            }

            if (ImGui.MenuItem("Reset", "Ctrl+R"))
            {
                GuiDebugRunCommands.Apply(GuiDebugRunCommands.Reset, emulator, ref running);
            }

            if (ImGui.MenuItem("Next VBlank", "F10"))
            {
                GuiDebugRunCommands.Apply(GuiDebugRunCommands.NextVBlank, emulator, ref running);
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("View"))
        {
            ToggleViewMenuItem("LCD", ref _showLcd);
            ToggleViewMenuItem("BG", ref _showBg);
            ToggleViewMenuItem("WIN", ref _showWin);
            ToggleViewMenuItem("VRAM", ref _showVram);
            ToggleViewMenuItem("OAM", ref _showOam);
            ToggleViewMenuItem("Disassembly", ref _showDisassembly);
            ToggleViewMenuItem("Registers", ref _showRegisters);
            ToggleViewMenuItem("Memory", ref _showMemory);
            ImGui.EndMenu();
        }

        ImGui.EndMenuBar();
    }

    static void ToggleViewMenuItem(string label, ref bool visible)
    {
        if (ImGui.MenuItem(label, null, visible))
        {
            visible = !visible;
        }
    }

    void DrawToolWindows(Emulator emulator, bool running)
    {
        DrawOptionalWindow("LCD", ref _showLcd, DrawLcdImage);

        DrawOptionalWindow("BG", ref _showBg, () =>
        {
            DrawFittedTexture(
                _bgMapTex,
                PpuDebugViewRenderer.TileMapPixels,
                PpuDebugViewRenderer.TileMapPixels,
                integerScale: false,
                out Vector2 origin,
                out float scale);
            DrawViewportRect(
                origin,
                scale,
                PpuDebugViewRenderer.TileMapPixels,
                PpuDebugViewRenderer.TileMapPixels,
                emulator.Read(0xFF43),
                emulator.Read(0xFF42),
                Emulator.WindowWidth,
                Emulator.WindowHeight);
        });

        DrawOptionalWindow("WIN", ref _showWin, () =>
        {
            DrawFittedTexture(
                _winMapTex,
                PpuDebugViewRenderer.TileMapPixels,
                PpuDebugViewRenderer.TileMapPixels,
                integerScale: false,
                out Vector2 origin,
                out float scale);
            byte lcdc = emulator.Read(0xFF40);
            if ((lcdc & Flags.WindowEnabled) != 0)
            {
                int wx = emulator.Read(0xFF4B) - 7;
                int wy = emulator.Read(0xFF4A);
                int winW = Math.Max(0, Emulator.WindowWidth - Math.Max(0, wx));
                int winH = Math.Max(0, Emulator.WindowHeight - wy);
                DrawViewportRect(
                    origin,
                    scale,
                    PpuDebugViewRenderer.TileMapPixels,
                    PpuDebugViewRenderer.TileMapPixels,
                    0,
                    0,
                    winW,
                    winH);
            }
        });

        DrawOptionalWindow("VRAM", ref _showVram, () =>
        {
            DrawFittedTexture(
                _vramTex,
                PpuDebugViewRenderer.VramTilesWidth,
                PpuDebugViewRenderer.VramTilesHeight,
                integerScale: false,
                out _,
                out _);
        });

        DrawOptionalWindow("OAM", ref _showOam, () =>
        {
            int gridH = Math.Max(1, _oamGridH);
            Vector2 avail = ImGui.GetContentRegionAvail();
            DrawFittedTexture(
                _oamTex,
                PpuDebugViewRenderer.OamGridWidth,
                gridH,
                integerScale: false,
                out Vector2 origin,
                out float scale);
            if (avail.X >= 1 && avail.Y >= 1)
            {
                int cellH = Math.Max(1, gridH / PpuDebugViewRenderer.OamGridRows);
                DrawCellGrid(
                    origin,
                    scale,
                    PpuDebugViewRenderer.OamGridCols,
                    PpuDebugViewRenderer.OamGridRows,
                    PpuDebugViewRenderer.OamCellWidth,
                    cellH);
            }
        });

        DrawOptionalWindow("Disassembly", ref _showDisassembly, () => GuiDisassemblyView.Draw(emulator, running));
        DrawOptionalWindow("Registers", ref _showRegisters, () => GuiRegisterPanels.Draw(emulator));
        DrawOptionalWindow("Memory", ref _showMemory, () => GuiMemoryDumpView.Draw(emulator));
    }

    static void DrawOptionalWindow(string title, ref bool open, Action draw)
    {
        if (!open)
        {
            return;
        }

        if (ImGui.Begin(title, ref open))
        {
            GuiMouseCursor.NoteCurrentWindowTab();
            draw();
        }

        ImGui.End();
    }

    void DrawLcdImage()
    {
        Vector2 avail = ImGui.GetContentRegionAvail();
        int scale = Math.Max(
            1,
            Math.Min((int)(avail.X / Emulator.WindowWidth), (int)(avail.Y / Emulator.WindowHeight)));
        int drawW = Emulator.WindowWidth * scale;
        int drawH = Emulator.WindowHeight * scale;
        float offsetX = (avail.X - drawW) * 0.5f;
        float offsetY = (avail.Y - drawH) * 0.5f;
        if (offsetX > 0)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offsetX);
        }

        if (offsetY > 0)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + offsetY);
        }

        rlImGui.ImageSize(_lcdTex, drawW, drawH);
    }

    void DrawFittedTexture(
        Texture2D tex,
        int srcW,
        int srcH,
        bool integerScale,
        out Vector2 imageOrigin,
        out float scale)
    {
        Vector2 avail = ImGui.GetContentRegionAvail();
        imageOrigin = ImGui.GetCursorScreenPos();
        scale = 1;

        if (srcW < 1 || srcH < 1 || avail.X < 1 || avail.Y < 1)
        {
            return;
        }

        if (integerScale)
        {
            int intScale = Math.Max(
                1,
                Math.Min((int)(avail.X / srcW), (int)(avail.Y / srcH)));
            scale = intScale;
        }
        else
        {
            scale = Math.Min(avail.X / srcW, avail.Y / srcH);
            if (scale <= 0)
            {
                scale = 0.01f;
            }
        }

        int drawW = Math.Max(1, (int)(srcW * scale));
        int drawH = Math.Max(1, (int)(srcH * scale));
        float offsetX = (avail.X - drawW) * 0.5f;
        float offsetY = (avail.Y - drawH) * 0.5f;
        if (offsetX > 0)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offsetX);
        }

        if (offsetY > 0)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + offsetY);
        }

        imageOrigin = ImGui.GetCursorScreenPos();
        rlImGui.ImageSize(tex, drawW, drawH);
    }

    static void DrawViewportRect(
        Vector2 imageOrigin,
        float scale,
        int mapW,
        int mapH,
        int vx,
        int vy,
        int vw,
        int vh)
    {
        if (mapW < 1 || mapH < 1 || vw < 1 || vh < 1)
        {
            return;
        }

        vx = ((vx % mapW) + mapW) % mapW;
        vy = ((vy % mapH) + mapH) % mapH;

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        uint color = GuiDebuggerTheme.ViewportOutlineU32;

        int widthLeft = vw;
        int mapX = vx;
        while (widthLeft > 0)
        {
            int segmentW = Math.Min(widthLeft, mapW - mapX);
            int heightLeft = vh;
            int mapY = vy;
            while (heightLeft > 0)
            {
                int segmentH = Math.Min(heightLeft, mapH - mapY);
                Vector2 min = imageOrigin + new Vector2(mapX * scale, mapY * scale);
                Vector2 max = min + new Vector2(Math.Max(1, segmentW * scale), Math.Max(1, segmentH * scale));
                drawList.AddRect(min, max, color, 0, ImDrawFlags.None, 1f);
                heightLeft -= segmentH;
                mapY = 0;
            }

            widthLeft -= segmentW;
            mapX = 0;
        }
    }

    static void DrawCellGrid(Vector2 origin, float scale, int cols, int rows, int cellW, int cellH)
    {
        if (cols < 1 || rows < 1 || cellW < 1 || cellH < 1 || scale <= 0)
        {
            return;
        }

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        uint color = ImGui.ColorConvertFloat4ToU32(new Vector4(
            GuiDebuggerTheme.PanelBorder.R / 255f,
            GuiDebuggerTheme.PanelBorder.G / 255f,
            GuiDebuggerTheme.PanelBorder.B / 255f,
            1f));

        float width = cols * cellW * scale;
        float height = rows * cellH * scale;
        for (int col = 0; col <= cols; col++)
        {
            float x = origin.X + (col * cellW * scale);
            drawList.AddLine(new Vector2(x, origin.Y), new Vector2(x, origin.Y + height), color);
        }

        for (int row = 0; row <= rows; row++)
        {
            float y = origin.Y + (row * cellH * scale);
            drawList.AddLine(new Vector2(origin.X, y), new Vector2(origin.X + width, y), color);
        }
    }

    static void BuildDefaultDock(uint dockSpaceId, Vector2 viewportSize)
    {
        ImGuiDockBuilder.RemoveNode(dockSpaceId);
        ImGuiDockBuilder.AddNode(dockSpaceId, ImGuiDockNodeFlagsDockSpace);
        ImGuiDockBuilder.SetNodeSize(dockSpaceId, viewportSize);

        ImGuiDockBuilder.SplitNode(dockSpaceId, ImGuiDir.Left, 0.42f, out uint dockLeftId, out uint dockRightId);
        ImGuiDockBuilder.SplitNode(dockLeftId, ImGuiDir.Up, 0.62f, out uint dockLeftTopId, out uint dockLeftBottomId);
        ImGuiDockBuilder.SplitNode(dockLeftTopId, ImGuiDir.Up, 0.58f, out uint dockTopBandId, out uint dockLowerBandId);

        ImGuiDockBuilder.SplitNode(dockTopBandId, ImGuiDir.Left, 1f / 3f, out uint dockLcdId, out uint dockTopRestId);
        ImGuiDockBuilder.SplitNode(dockTopRestId, ImGuiDir.Left, 0.5f, out uint dockBgId, out uint dockWinId);
        ImGuiDockBuilder.SplitNode(dockLowerBandId, ImGuiDir.Left, 0.62f, out uint dockVramId, out uint dockOamId);
        ImGuiDockBuilder.SplitNode(dockRightId, ImGuiDir.Up, 0.60f, out uint dockRegistersId, out uint dockMemoryId);

        ImGuiDockBuilder.DockWindow("LCD", dockLcdId);
        ImGuiDockBuilder.DockWindow("BG", dockBgId);
        ImGuiDockBuilder.DockWindow("WIN", dockWinId);
        ImGuiDockBuilder.DockWindow("VRAM", dockVramId);
        ImGuiDockBuilder.DockWindow("OAM", dockOamId);
        ImGuiDockBuilder.DockWindow("Disassembly", dockLeftBottomId);
        ImGuiDockBuilder.DockWindow("Registers", dockRegistersId);
        ImGuiDockBuilder.DockWindow("Memory", dockMemoryId);

        ImGuiDockBuilder.Finish(dockSpaceId);
    }

    const ImGuiDockNodeFlags ImGuiDockNodeFlagsDockSpace = (ImGuiDockNodeFlags)1024;

    static unsafe void SetIniFilename(string path)
    {
        if (_iniFilenameHandle.IsAllocated)
        {
            _iniFilenameHandle.Free();
        }

        _iniFilenameUtf8 = Encoding.UTF8.GetBytes(path + "\0");
        _iniFilenameHandle = GCHandle.Alloc(_iniFilenameUtf8, GCHandleType.Pinned);
        ImGui.GetIO().NativePtr->IniFilename = (byte*)_iniFilenameHandle.AddrOfPinnedObject();
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

        rlImGui.Shutdown();
        if (_iniFilenameHandle.IsAllocated)
        {
            _iniFilenameHandle.Free();
        }
    }
}

/// <summary>cimgui DockBuilder API (not exposed on ImGui.NET).</summary>
static unsafe class ImGuiDockBuilder
{
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    static extern void igDockBuilderRemoveNode(uint nodeId);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    static extern void igDockBuilderAddNode(uint nodeId, ImGuiDockNodeFlags flags);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    static extern void igDockBuilderSetNodeSize(uint nodeId, Vector2 size);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    static extern uint igDockBuilderSplitNode(
        uint nodeId,
        ImGuiDir splitDir,
        float sizeRatioForNodeAtDir,
        uint* outIdAtDir,
        uint* outIdAtOppositeDir);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    static extern void igDockBuilderDockWindow(byte* windowName, uint nodeId);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    static extern void igDockBuilderFinish(uint nodeId);

    public static void RemoveNode(uint nodeId) => igDockBuilderRemoveNode(nodeId);

    public static void AddNode(uint nodeId, ImGuiDockNodeFlags flags) => igDockBuilderAddNode(nodeId, flags);

    public static void SetNodeSize(uint nodeId, Vector2 size) => igDockBuilderSetNodeSize(nodeId, size);

    public static uint SplitNode(uint nodeId, ImGuiDir splitDir, float ratio, out uint idAtDir, out uint idOpposite)
    {
        uint atDir;
        uint opposite;
        igDockBuilderSplitNode(nodeId, splitDir, ratio, &atDir, &opposite);
        idAtDir = atDir;
        idOpposite = opposite;
        return atDir;
    }

    public static void DockWindow(string windowName, uint nodeId)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(windowName + "\0");
        fixed (byte* ptr = bytes)
        {
            igDockBuilderDockWindow(ptr, nodeId);
        }
    }

    public static void Finish(uint nodeId) => igDockBuilderFinish(nodeId);
}
