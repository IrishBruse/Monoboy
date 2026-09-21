namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using ImGuiNET;

/// <summary>ImGui fonts for the GUI debugger (UI text + VS Code codicons for the toolbar).</summary>
static class GuiDebuggerFonts
{
    const string CodiconResource = "Monoboy.Desktop.Data.codicon.ttf";
    public const float CodiconSizePx = 20f;

    static GCHandle _codiconDataHandle;
    static byte[]? _codiconBytes;

    public static ImFontPtr CodiconFont { get; private set; }

    public static bool HasCodiconFont { get; private set; }

    public static void Configure(ImGuiIOPtr io)
    {
        if (!TryAddMonospaceFont(io, 16f))
        {
            io.Fonts.AddFontDefault();
        }

        LoadCodiconFont(io);
    }

    static void LoadCodiconFont(ImGuiIOPtr io)
    {
        _codiconBytes = LoadEmbeddedBytes(CodiconResource);
        HasCodiconFont = false;
        if (_codiconBytes == null || _codiconBytes.Length == 0)
        {
            return;
        }

        if (_codiconDataHandle.IsAllocated)
        {
            _codiconDataHandle.Free();
        }

        _codiconDataHandle = GCHandle.Alloc(_codiconBytes, GCHandleType.Pinned);
        ushort[] ranges = [0xEA00, 0xED00, 0];

        unsafe
        {
            ImFontConfigPtr fontConfig = ImGuiNative.ImFontConfig_ImFontConfig();
            fontConfig.GlyphMinAdvanceX = CodiconSizePx;
            fontConfig.PixelSnapH = true;

            fixed (ushort* rangePtr = &ranges[0])
            {
                CodiconFont = io.Fonts.AddFontFromMemoryTTF(
                    _codiconDataHandle.AddrOfPinnedObject(),
                    _codiconBytes.Length,
                    CodiconSizePx,
                    fontConfig,
                    (IntPtr)rangePtr);
                HasCodiconFont = true;
            }
        }
    }

    static bool TryAddMonospaceFont(ImGuiIOPtr io, float sizePx)
    {
        ReadOnlySpan<string> paths =
        [
            "/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf",
            "/usr/share/fonts/TTF/DejaVuSansMono.ttf",
            "/usr/share/fonts/truetype/liberation/LiberationMono-Regular.ttf",
            "C:\\Windows\\Fonts\\consola.ttf",
        ];

        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                io.Fonts.AddFontFromFileTTF(path, sizePx);
                return true;
            }
        }

        return false;
    }

    static byte[]? LoadEmbeddedBytes(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return null;
        }

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
