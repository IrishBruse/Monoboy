namespace Monoboy.Desktop;

using Veldrid;

public static class Extensions
{
    public static TextureDescription GetDescription(this Texture texture)
    {
        return new TextureDescription(
            texture.Width, texture.Height,
            texture.Depth, texture.MipLevels, texture.ArrayLayers,
            texture.Format, texture.Usage, texture.Type, texture.SampleCount
        );
    }
}
