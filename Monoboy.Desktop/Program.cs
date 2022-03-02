namespace Monoboy.Desktop;
using System.Numerics;
using Raylib_cs;

static class Program
{
    const int Width = 1280;
    const int Height = 720;

    static Texture2D texture;
    static Image screen;

    static Emulator emulator;

    public static unsafe void Main()
    {
        emulator = new();
        emulator.Open("/home/econn/Downloads/Tetris.gb");
        emulator.ppu.DrawFrame += () => Render();

        Raylib.InitWindow(Width, Height, "Monoboy");

        screen = Raylib.GenImageColor(Emulator.WindowWidth, Emulator.WindowHeight, Color.GREEN);
        texture = Raylib.LoadTextureFromImage(screen);


        while (!Raylib.WindowShouldClose())
        {
            int cycles = 0;
            while (cycles < Emulator.CyclesPerFrame)
            {
                cycles += emulator.Step();
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);


            Raylib.DrawTextureEx(texture, new Vector2((Width / 2) - (screen.width / 2 * 2), (Height / 2) - (screen.height / 2 * 2)), 0, 2, Color.WHITE);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static unsafe void Render()
    {
        Console.WriteLine("Render");

        for (int x = 0; x < Emulator.WindowWidth; x++)
        {
            for (int y = 0; y < Emulator.WindowHeight; y++)
            {
                Color col = emulator.ppu.framebuffer[x, y] switch
                {
                    0 => new Color(208, 208, 88, 255),
                    1 => new Color(160, 168, 64, 255),
                    2 => new Color(112, 128, 40, 255),
                    3 => new Color(64, 80, 16, 255),
                    _ => new Color(255, 0, 255, 255),
                };
                Raylib.ImageDrawPixel(ref screen, x, y, col);
            }
        }

        Raylib.UpdateTexture(texture, screen.data);
    }
}
