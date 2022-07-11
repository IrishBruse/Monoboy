namespace Monoboy.Constants;

using System;
using System.IO;

public static class Pallet
{
    private static readonly uint[] Colors;

    static Pallet()
    {
        uint[] pallet = new uint[4];

        pallet[0] = 0xD0D058;
        pallet[1] = 0xA0A840;
        pallet[2] = 0x708028;
        pallet[3] = 0x405010;

        if (File.Exists("Pallet.txt"))
        {
            string[] lines = File.ReadAllLines("Pallet.txt");
            for (int i = 0; i < 4; i++)
            {
                pallet[i] = Convert.ToUInt32(lines[i], 16);
            }
        }
        else
        {
            File.WriteAllLines("Pallet.txt", new[] { "D0D058", "A0A840", "708028", "405010" });
        }

        Colors = new uint[4];
        for (int i = 0; i < 4; i++)
        {
            uint color = pallet[i];

            Colors[i] = (uint)(((color & 0xff) << 16) | (color & 0xff00) | ((color >> 16) & 0xff) | (0xFF << 24));
        }
    }

    public static uint GetColor(byte index)
    {
        return Colors[index];
    }
}
