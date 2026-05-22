namespace Monoboy.Desktop;

using System;

using Monoboy;
using Monoboy.Constants;

using Raylib_cs;

/// <summary>
/// Raylib window: LCD framebuffer plus PPU debug views (BG/window tilemaps, VRAM tiles, OAM).
/// </summary>
public static class FramebufferPreviewWindow
{
    const int Pad = 6;
    const int LabelH = 14;
    const int LcdScale = 2;
    const int MapScale = 1;
    const int VramScale = 2;
    const int OamScale = 2;

    public static void Show(Emulator emulator)
    {
        Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);

        int layoutW = Pad + (Emulator.WindowWidth * LcdScale) + Pad
            + (PpuDebugViewRenderer.TileMapPixels * MapScale) + Pad
            + (PpuDebugViewRenderer.TileMapPixels * MapScale) + Pad;
        int oamH = PpuDebugViewRenderer.OamGridHeight(emulator);
        int layoutH = LabelH + (Emulator.WindowHeight * LcdScale) + Pad
            + LabelH + Math.Max(PpuDebugViewRenderer.VramTilesHeight * VramScale, oamH * OamScale) + Pad;

        Raylib.InitWindow(layoutW, layoutH, "Monoboy");
        Raylib.SetWindowMinSize(layoutW / 2, layoutH / 2);
        Raylib.SetExitKey(KeyboardKey.Escape);
        Raylib.SetTargetFPS(30);

        var lcdBuf = new byte[Emulator.WindowWidth * Emulator.WindowHeight * 4];
        var bgMapBuf = new byte[PpuDebugViewRenderer.TileMapPixels * PpuDebugViewRenderer.TileMapPixels * 4];
        var winMapBuf = new byte[PpuDebugViewRenderer.TileMapPixels * PpuDebugViewRenderer.TileMapPixels * 4];
        var vramBuf = new byte[PpuDebugViewRenderer.VramTilesWidth * PpuDebugViewRenderer.VramTilesHeight * 4];
        int oamW = PpuDebugViewRenderer.OamGridWidth;
        var oamBuf = new byte[oamW * oamH * 4];

        Texture2D lcdTex = CreateTexture(Emulator.WindowWidth, Emulator.WindowHeight);
        Texture2D bgMapTex = CreateTexture(PpuDebugViewRenderer.TileMapPixels, PpuDebugViewRenderer.TileMapPixels);
        Texture2D winMapTex = CreateTexture(PpuDebugViewRenderer.TileMapPixels, PpuDebugViewRenderer.TileMapPixels);
        Texture2D vramTex = CreateTexture(PpuDebugViewRenderer.VramTilesWidth, PpuDebugViewRenderer.VramTilesHeight);
        Texture2D oamTex = CreateTexture(oamW, oamH);

        try
        {
            while (!Raylib.WindowShouldClose())
            {
                int oamHeight = PpuDebugViewRenderer.OamGridHeight(emulator);
                if (oamHeight != oamH)
                {
                    oamH = oamHeight;
                    Raylib.UnloadTexture(oamTex);
                    oamBuf = new byte[oamW * oamH * 4];
                    oamTex = CreateTexture(oamW, oamH);
                }

                Array.Copy(emulator.Framebuffer, lcdBuf, lcdBuf.Length);
                PpuDebugViewRenderer.FillTileMap(bgMapBuf, emulator, windowMap: false);
                PpuDebugViewRenderer.FillTileMap(winMapBuf, emulator, windowMap: true);
                PpuDebugViewRenderer.FillVramTiles(vramBuf, emulator);
                PpuDebugViewRenderer.FillOamGrid(oamBuf, emulator);

                Raylib.UpdateTexture(lcdTex, lcdBuf);
                Raylib.UpdateTexture(bgMapTex, bgMapBuf);
                Raylib.UpdateTexture(winMapTex, winMapBuf);
                Raylib.UpdateTexture(vramTex, vramBuf);
                Raylib.UpdateTexture(oamTex, oamBuf);

                byte scx = emulator.Read(0xFF43);
                byte scy = emulator.Read(0xFF42);
                byte wy = emulator.Read(0xFF4A);
                int wx = emulator.Read(0xFF4B) - 7;
                byte lcdc = emulator.Read(0xFF40);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(PpuDebugViewRenderer.PpuRegisterBackdrop);

                int x = Pad;
                int y = LabelH;

                DrawPanelLabel("LCD", x, 0);
                Raylib.DrawTextureEx(lcdTex, new(x, y), 0, LcdScale, Color.White);
                int lcdBottom = y + Emulator.WindowHeight * LcdScale;

                int mapX = x + Emulator.WindowWidth * LcdScale + Pad;
                DrawPanelLabel("BG tilemap", mapX, 0);
                Raylib.DrawTextureEx(bgMapTex, new(mapX, y), 0, MapScale, Color.White);
                DrawViewportRect(mapX, y, MapScale, scx, scy, Emulator.WindowWidth, Emulator.WindowHeight);

                int winMapX = mapX + PpuDebugViewRenderer.TileMapPixels * MapScale + Pad;
                DrawPanelLabel("Window tilemap", winMapX, 0);
                Raylib.DrawTextureEx(winMapTex, new(winMapX, y), 0, MapScale, Color.White);
                if ((lcdc & Flags.WindowEnabled) != 0)
                {
                    int winW = Math.Max(0, Emulator.WindowWidth - Math.Max(0, wx));
                    int winH = Math.Max(0, Emulator.WindowHeight - wy);
                    DrawViewportRect(winMapX, y, MapScale, 0, 0, winW, winH);
                }

                int row2Y = lcdBottom + Pad + LabelH;
                DrawPanelLabel("VRAM tiles", x, row2Y - LabelH);
                Raylib.DrawTextureEx(vramTex, new(x, row2Y), 0, VramScale, Color.White);

                int oamX = x + PpuDebugViewRenderer.VramTilesWidth * VramScale + Pad;
                DrawPanelLabel("OAM (40 sprites)", oamX, row2Y - LabelH);
                Raylib.DrawTextureEx(oamTex, new(oamX, row2Y), 0, OamScale, Color.White);

                Raylib.EndDrawing();
            }
        }
        finally
        {
            Raylib.UnloadTexture(lcdTex);
            Raylib.UnloadTexture(bgMapTex);
            Raylib.UnloadTexture(winMapTex);
            Raylib.UnloadTexture(vramTex);
            Raylib.UnloadTexture(oamTex);
            Raylib.CloseWindow();
        }
    }

    static Texture2D CreateTexture(int width, int height)
    {
        Image image = Raylib.GenImageColor(width, height, Color.Black);
        Texture2D texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);
        return texture;
    }

    static void DrawPanelLabel(string text, int x, int y)
    {
        Raylib.DrawText(text, x, y, 12, new Color(0x40, 0x50, 0x10, 0xFF));
    }

    static void DrawViewportRect(int texX, int texY, int scale, int vx, int vy, int vw, int vh)
    {
        Raylib.DrawRectangleLines(
            texX + (vx * scale),
            texY + (vy * scale),
            vw * scale,
            vh * scale,
            new Color(0xE0, 0x40, 0x40, 0xFF));
    }
}
