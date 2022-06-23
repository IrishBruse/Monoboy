namespace Monoboy.Constants;

public static class Pallet
{
    public static uint GetColor(byte index)
    {
        return index switch
        {
            0 => 0xD0D058,
            1 => 0xA0A840,
            2 => 0x708028,
            3 => 0x405010,
            _ => 0xFF00FF,
        };
    }
}