namespace Monoboy.Desktop.Data;

public static class BuiltinShader
{
    public static readonly string VertCode = @"
    #version 450

    layout(location = 0) in vec2 Position;
    layout(location = 1) in vec2 TexCoords;
    layout(location = 0) out vec2 fsin_texCoords;

    void main()
    {
        gl_Position = vec4(Position, 0, 1);
        fsin_texCoords = TexCoords;
    }
";
    public static readonly string FragCode = @"
    #version 450

    layout(location = 0) in vec2 fsin_texCoords;
    layout(location = 0) out vec4 fsout_Color;

    layout(set = 0, binding = 0) uniform texture2D SurfaceTexture;
    layout(set = 0, binding = 1) uniform sampler SurfaceSampler;

    void main()
    {
        fsout_Color = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords);
    }
";
}
