namespace Monoboy.Desktop;

using Raylib_cs;

/// <summary>Override WM-restored geometry so new windows open on the active monitor.</summary>
static class RaylibWindowPlacement
{
    public static int LaunchMonitor() => Raylib.GetCurrentMonitor();

    public static void CenterOnMonitor(int monitor, int windowWidth, int windowHeight)
    {
        var origin = Raylib.GetMonitorPosition(monitor);
        int monW = Raylib.GetMonitorWidth(monitor);
        int monH = Raylib.GetMonitorHeight(monitor);
        Raylib.SetWindowPosition(
            (int)origin.X + (monW - windowWidth) / 2,
            (int)origin.Y + (monH - windowHeight) / 2);
        Raylib.SetWindowFocused();
    }
}
