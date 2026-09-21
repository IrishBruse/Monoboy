namespace Monoboy.Desktop.GuiDebugger;

using ImGuiNET;

/// <summary>
/// ImGuiListClipperPtr has no default constructor that allocates native memory.
/// A zero pointer crashes inside Begin.
/// </summary>
static class GuiImGuiClipper
{
    public static unsafe ImGuiListClipperPtr Create() =>
        new(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
}
