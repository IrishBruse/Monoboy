namespace Monoboy.Dap;

using System.Numerics;

using Monoboy;

using Raylib_cs;

/// <summary>LCD window for the emulator the debug session is controlling.</summary>
static class ScreenWindow
{
    static readonly byte[] Pixels = new byte[Emulator.WindowWidth * Emulator.WindowHeight * 4];
    static bool _failed;
    static Texture2D _texture;
    static long _lastPump;

    public static bool IsOpen { get; private set; }

    public static void EnsureOpen()
    {
        if (IsOpen || _failed)
        {
            return;
        }

        try
        {
            Raylib.SetTraceLogLevel(TraceLogLevel.None);
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            int width = Emulator.WindowWidth * 4;
            int height = Emulator.WindowHeight * 4;
            Raylib.InitWindow(width, height, "Monoboy");
            Raylib.SetExitKey(0);
            Raylib.SetTargetFPS(0);
            Image image = Raylib.GenImageColor(Emulator.WindowWidth, Emulator.WindowHeight, Color.Black);
            _texture = Raylib.LoadTextureFromImage(image);
            Raylib.UnloadImage(image);
            IsOpen = true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Monoboy window failed: " + ex.Message);
            _failed = true;
        }
    }

    public static void Pump(Emulator? emulator)
    {
        if (!IsOpen || emulator == null)
        {
            return;
        }

        long now = Environment.TickCount64;
        if (now - _lastPump < 16)
        {
            return;
        }

        _lastPump = now;
        if (Raylib.WindowShouldClose())
        {
            Close();
            return;
        }

        if (Raylib.IsWindowFocused())
        {
            ApplyKeys(emulator);
        }

        Array.Copy(emulator.Framebuffer, Pixels, Pixels.Length);
        Raylib.UpdateTexture(_texture, Pixels);

        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();
        int scale = Math.Min(
            Math.Max(width / Emulator.WindowWidth, 1),
            Math.Max(height / Emulator.WindowHeight, 1));
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(0xD0, 0xD0, 0x58, 0xFF));
        Raylib.DrawTextureEx(
            _texture,
            new Vector2(
                (width - (Emulator.WindowWidth * scale)) * 0.5f,
                (height - (Emulator.WindowHeight * scale)) * 0.5f),
            0,
            scale,
            Color.White);
        Raylib.EndDrawing();
    }

    public static void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        Raylib.UnloadTexture(_texture);
        Raylib.CloseWindow();
        IsOpen = false;
    }

    static void ApplyKeys(Emulator emulator)
    {
        emulator.SetButtonState(GameboyButton.Right, Down(KeyboardKey.Right) || Down(KeyboardKey.D));
        emulator.SetButtonState(GameboyButton.Left, Down(KeyboardKey.Left) || Down(KeyboardKey.A));
        emulator.SetButtonState(GameboyButton.Up, Down(KeyboardKey.Up) || Down(KeyboardKey.W));
        emulator.SetButtonState(GameboyButton.Down, Down(KeyboardKey.Down) || Down(KeyboardKey.S));
        emulator.SetButtonState(GameboyButton.A, Down(KeyboardKey.Space));
        emulator.SetButtonState(GameboyButton.B, Down(KeyboardKey.LeftShift));
        emulator.SetButtonState(GameboyButton.Select, Down(KeyboardKey.Escape));
        emulator.SetButtonState(GameboyButton.Start, Down(KeyboardKey.Enter));
    }

    static bool Down(KeyboardKey key) => Raylib.IsKeyDown(key);
}
