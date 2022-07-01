namespace Monoboy.Constants;

using System;

public static class Pallet
{
    private static readonly uint[] Colors;

    static Pallet()
    {
        Colors = new uint[4];
        for (int i = 0; i < 4; i++)
        {
            uint color = i switch
            {
                0 => 0xD0D058,//0xD0D058
                1 => 0xA0A840,
                2 => 0x708028,
                3 => 0x405010,
                _ => 0xFF00FF,
            };

            Colors[i] = (uint)(((color & 0xff) << 16) | (color & 0xff00) | ((color >> 16) & 0xff) | (0xFF << 24));
        }
    }

    public static uint GetColor(byte index)
    {
        return Colors[index];
    }
}
