namespace Monoboy.Desktop;

using System;

using Monoboy;

using Raylib_cs;

/// <summary>
/// Opens a resizable Raylib window that mirrors <see cref="Emulator.Framebuffer"/> until closed.
/// </summary>
public static class FramebufferPreviewWindow
{
    const int InitialScale = 3;

    public static void Show(Emulator emulator)
    {
        Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(Emulator.WindowWidth * InitialScale, Emulator.WindowHeight * InitialScale, "Monoboy");
        Raylib.SetWindowMinSize(Emulator.WindowWidth, Emulator.WindowHeight);
        Raylib.SetExitKey(KeyboardKey.Escape);
        Raylib.SetTargetFPS(60);

        Image image = Raylib.GenImageColor(Emulator.WindowWidth, Emulator.WindowHeight, Color.Black);
        Texture2D texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);

        try
        {
            while (!Raylib.WindowShouldClose())
            {
                int width = Raylib.GetScreenWidth();
                int height = Raylib.GetScreenHeight();

                Raylib.UpdateTexture(texture, emulator.Framebuffer);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(0xD0, 0xD0, 0x58, 0xFF));
                int scale = Math.Min(
                    Math.Max(width / Emulator.WindowWidth, 1),
                    Math.Max(height / Emulator.WindowHeight, 1));
                Raylib.DrawTextureEx(
                    texture,
                    new((width - (Emulator.WindowWidth * scale)) * 0.5f, (height - (Emulator.WindowHeight * scale)) * 0.5f),
                    0,
                    scale,
                    Color.White);
                Raylib.EndDrawing();
            }
        }
        finally
        {
            Raylib.UnloadTexture(texture);
            Raylib.CloseWindow();
        }
    }
}
