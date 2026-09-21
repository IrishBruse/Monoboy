namespace Monoboy.Desktop.TuiDebugger;

using System;

/// <summary>Clears terminal modes left by prior runs (e.g. mouse capture) so text selection works.</summary>
static class TuiTerminalReset
{
    /// <summary>Disable mouse reporting and related capture modes.</summary>
    internal static void ReleaseMouseCapture()
    {
        Console.Write("\x1b[?1000l\x1b[?1002l\x1b[?1003l\x1b[?1005l\x1b[?1006l");
    }
}
