namespace Monoboy.Desktop;

using Raylib_cs;

public static class RaylibSafe
{
    public static unsafe void UpdateTexture(Texture2D texture, Image data)
    {
        Raylib.UpdateTexture(texture, data.data);
    }
}
