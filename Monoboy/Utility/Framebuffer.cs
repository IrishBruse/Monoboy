namespace Monoboy.Utility;

using System.Numerics;

public class Framebuffer
{
    public byte[] Pixels { get; }
    public Vector2 Size { get; }

    public Framebuffer(int width, int height)
    {
        Size = new Vector2(width, height);
        Pixels = new byte[width * height * 3];
    }

    public void SetPixel(int x, int y, uint pixel)
    {
        Pixels[(((((int)Size.X) * y) + x) * 3) + 0] = (byte)(pixel >> 16);
        Pixels[(((((int)Size.X) * y) + x) * 3) + 1] = (byte)(pixel >> 08);
        Pixels[(((((int)Size.X) * y) + x) * 3) + 2] = (byte)(pixel >> 00);
    }

    public uint GetPixel(int x, int y)
    {
        byte r = Pixels[((((int)Size.X * y) + x) * 3) + 0];
        byte g = Pixels[((((int)Size.X * y) + x) * 3) + 1];
        byte b = Pixels[((((int)Size.X * y) + x) * 3) + 2];

        uint result = (uint)((r << 16) | (g << 8) | (b << 0));

        return result;
    }
}
