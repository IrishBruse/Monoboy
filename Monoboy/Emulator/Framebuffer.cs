using Monoboy.Constants;

namespace Monoboy
{
    public class Framebuffer
    {
        public readonly byte[] pixels;

        public Framebuffer(int width, int height)
        {
            pixels = new byte[width * height * 3];
        }

        public void SetPixel(int x, int y, uint pixel)
        {
            pixels[(((Constant.WindowWidth * y) + x) * 3) + 0] = (byte)(pixel >> 16);
            pixels[(((Constant.WindowWidth * y) + x) * 3) + 1] = (byte)(pixel >> 08);
            pixels[(((Constant.WindowWidth * y) + x) * 3) + 2] = (byte)(pixel >> 00);
        }

        public uint GetPixel(int x, int y)
        {
            byte r = pixels[(((Constant.WindowWidth * y) + x) * 3) + 0];
            byte g = pixels[(((Constant.WindowWidth * y) + x) * 3) + 1];
            byte b = pixels[(((Constant.WindowWidth * y) + x) * 3) + 2];

            uint result = (uint)((r << 16) | (g << 8) | (b << 0));

            return result;
        }
    }
}
