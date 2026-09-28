#nullable enable

namespace Monoboy.Debug;

using Monoboy;

/// <summary>Source-line breakpoints resolved to sequence-point addresses.</summary>
public static class GblSourceBreakpoints
{
    static readonly HashSet<(int FileId, int Line)> _lines = new();
    static readonly HashSet<ushort> _addresses = new();
    static string? _romPath;

    public static bool HasAny => _addresses.Count > 0;

    public static void Bind(string? romPath)
    {
        if (_romPath == romPath)
        {
            return;
        }

        _romPath = romPath;
        Clear();
    }

    public static void Clear()
    {
        _lines.Clear();
        _addresses.Clear();
    }

    public static bool IsSet(int fileId, int line) => _lines.Contains((fileId, line));

    public static bool IsHit(ushort pc) => _addresses.Contains(pc);

    /// <summary>Toggle a breakpoint on a statement line. Returns true when the breakpoint is now set.</summary>
    public static bool Toggle(GblDebugMap map, int fileId, int line)
    {
        if (!map.LineHasPoint(fileId, line))
        {
            return false;
        }

        var key = (fileId, line);
        if (!_lines.Add(key))
        {
            _lines.Remove(key);
        }

        Rebuild(map);
        return _lines.Contains(key);
    }

    /// <summary>Run one frame, or until a source breakpoint. Returns true when a breakpoint hit.</summary>
    public static bool RunFrame(Emulator emulator)
    {
        if (!HasAny)
        {
            emulator.StepFrame();
            return false;
        }

        GblDebugMap? map = GblDebugMap.ForRom(emulator.RomPath);
        if (map == null)
        {
            emulator.StepFrame();
            return false;
        }

        return emulator.RunUntil(() => IsHit(emulator.GetDebugState().PC));
    }

    static void Rebuild(GblDebugMap map)
    {
        _addresses.Clear();
        foreach ((int fileId, int line) in _lines)
        {
            map.CollectLineAddresses(fileId, line, _addresses);
        }
    }
}
