namespace Monoboy.Desktop;

using System.Numerics;
using Raylib_cs;

static class Program
{
    static Texture2D texture;
    static Image screen;
    static Emulator emulator;

    public static void Main(string[] args)
    {
        int scale = 4;

        emulator = new();
        emulator.Open("C:\\Users\\Econn\\Desktop\\Tetris.gb");
        // emulator.Open("/home/econn/Downloads/Tetris.gb");
        emulator.ppu.DrawFrame += () => Render();


        Raylib.InitWindow(Emulator.WindowWidth * scale, Emulator.WindowHeight * scale, "Monoboy");

        screen = Raylib.GenImageColor(Emulator.WindowWidth, Emulator.WindowHeight, Color.GREEN);
        texture = Raylib.LoadTextureFromImage(screen);

        Raylib.SetTargetFPS(60);

        while (!(Raylib.WindowShouldClose() && !Raylib.IsKeyDown(KeyboardKey.KEY_ESCAPE)))
        {
            emulator.BackgroundEnabled |= Raylib.IsKeyPressed(KeyboardKey.KEY_F1);
            emulator.WindowEnabled |= Raylib.IsKeyPressed(KeyboardKey.KEY_F2);
            emulator.SpritesEnabled |= Raylib.IsKeyPressed(KeyboardKey.KEY_F3);

            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL) && Raylib.IsKeyPressed(KeyboardKey.KEY_EQUAL))
            {
                scale++;
                scale = Math.Min(scale, 7);
                Raylib.SetWindowSize(Emulator.WindowWidth * scale, Emulator.WindowHeight * scale);
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL) && Raylib.IsKeyPressed(KeyboardKey.KEY_MINUS))
            {
                scale--;
                scale = Math.Max(scale, 1);
                Raylib.SetWindowSize(Emulator.WindowWidth * scale, Emulator.WindowHeight * scale);
            }

            emulator.joypad.SetButton(Joypad.Button.A, Raylib.IsKeyDown(KeyboardKey.KEY_SPACE));
            emulator.joypad.SetButton(Joypad.Button.B, Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT));
            emulator.joypad.SetButton(Joypad.Button.Select, Raylib.IsKeyDown(KeyboardKey.KEY_ESCAPE));
            emulator.joypad.SetButton(Joypad.Button.Start, Raylib.IsKeyDown(KeyboardKey.KEY_ENTER));
            emulator.joypad.SetButton(Joypad.Button.Up, Raylib.IsKeyDown(KeyboardKey.KEY_W));
            emulator.joypad.SetButton(Joypad.Button.Down, Raylib.IsKeyDown(KeyboardKey.KEY_S));
            emulator.joypad.SetButton(Joypad.Button.Left, Raylib.IsKeyDown(KeyboardKey.KEY_A));
            emulator.joypad.SetButton(Joypad.Button.Right, Raylib.IsKeyDown(KeyboardKey.KEY_D));
            emulator.RunFrame();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);

            Raylib.DrawTextureEx(texture, new Vector2((Raylib.GetScreenWidth() / 2) - (screen.width / 2 * scale), (Raylib.GetScreenHeight() / 2) - (screen.height / 2 * scale)), 0, scale, Color.WHITE);
            Raylib.DrawFPS(0, 0);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static void Render()
    {
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

        RaylibSafe.UpdateTexture(texture, screen);
    }
}
