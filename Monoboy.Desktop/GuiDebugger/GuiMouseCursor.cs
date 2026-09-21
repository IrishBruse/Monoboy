namespace Monoboy.Desktop.GuiDebugger;

using System.Numerics;

using ImGuiNET;

using Raylib_cs;

/// <summary>
/// Applies the ImGui hover cursor every frame.
/// rlImGui sets the Raylib cursor only when the id changes, so a later Raylib poll puts the arrow back.
/// </summary>
static class GuiMouseCursor
{
    const int MaxTabBands = 16;
    const float SplitterInset = 4f;

    static int _tabBandCount;
    static readonly Vector4[] _tabBands = new Vector4[MaxTabBands];

    public static void ClearTabBands() => _tabBandCount = 0;

    /// <summary>Docked window tabs occupy the top frame of the current window.</summary>
    public static void NoteCurrentWindowTab()
    {
        if (_tabBandCount >= MaxTabBands || !ImGui.IsWindowDocked())
        {
            return;
        }

        Vector2 pos = ImGui.GetWindowPos();
        Vector2 size = ImGui.GetWindowSize();
        float tabH = ImGui.GetFrameHeight();
        if (size.X < 8f || tabH < 1f)
        {
            return;
        }

        _tabBands[_tabBandCount++] = new Vector4(pos.X, pos.Y, size.X, tabH);
    }

    public static void Apply()
    {
        if (IsMouseOverTab())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        ImGuiIOPtr io = ImGui.GetIO();
        ImGuiMouseCursor cursor = ImGui.GetMouseCursor();
        if (io.MouseDrawCursor || cursor == ImGuiMouseCursor.None)
        {
            Raylib.HideCursor();
            return;
        }

        Raylib.ShowCursor();
        Raylib.SetMouseCursor(ToRaylib(cursor));
    }

    static bool IsMouseOverTab()
    {
        Vector2 mouse = ImGui.GetMousePos();
        for (int i = 0; i < _tabBandCount; i++)
        {
            Vector4 band = _tabBands[i];
            float x0 = band.X + SplitterInset;
            float x1 = band.X + band.Z - SplitterInset;
            if (x1 <= x0)
            {
                x0 = band.X;
                x1 = band.X + band.Z;
            }

            if (mouse.X >= x0 && mouse.X < x1 && mouse.Y >= band.Y && mouse.Y < band.Y + band.W)
            {
                return true;
            }
        }

        return false;
    }

    public static void Reset()
    {
        Raylib.ShowCursor();
        Raylib.SetMouseCursor(MouseCursor.Default);
    }

    static MouseCursor ToRaylib(ImGuiMouseCursor cursor) => cursor switch
    {
        ImGuiMouseCursor.TextInput => MouseCursor.IBeam,
        ImGuiMouseCursor.ResizeAll => MouseCursor.ResizeAll,
        ImGuiMouseCursor.ResizeNS => MouseCursor.ResizeNs,
        ImGuiMouseCursor.ResizeEW => MouseCursor.ResizeEw,
        ImGuiMouseCursor.ResizeNESW => MouseCursor.ResizeNesw,
        ImGuiMouseCursor.ResizeNWSE => MouseCursor.ResizeNwse,
        ImGuiMouseCursor.Hand => MouseCursor.PointingHand,
        ImGuiMouseCursor.NotAllowed => MouseCursor.NotAllowed,
        _ => MouseCursor.Arrow,
    };
}
