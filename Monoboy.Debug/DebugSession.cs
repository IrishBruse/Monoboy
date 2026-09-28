namespace Monoboy.Debug;

using Monoboy;

/// <summary>Headless Monoboy session stopped on gblang sequence points.</summary>
public sealed class DebugSession
{
    public const int ThreadId = 1;

    readonly Dictionary<int, HashSet<ushort>> _byFile = new();
    readonly HashSet<ushort> _addresses = new();

    Emulator? _emulator;
    GblDebugMap? _map;

    public bool IsLaunched => _emulator != null;

    public bool StopOnEntry { get; private set; }

    public string Log { get; private set; } = "";

    public bool Launch(string program, string? cwd, bool stopOnEntry, out string error)
    {
        Log = "";
        _emulator = null;
        _map = null;
        _byFile.Clear();
        _addresses.Clear();
        StopOnEntry = stopOnEntry;

        if (!TryResolveProgram(program, cwd, out string romPath, out error))
        {
            return false;
        }

        var emulator = new Emulator();
        try
        {
            emulator.Open(romPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            error = $"Failed to open ROM: {ex.Message}";
            return false;
        }

        _emulator = emulator;
        _map = GblDebugMap.TryLoadForRom(romPath);
        if (stopOnEntry && _map != null && _map.PointCount > 0)
        {
            GblStatementStepper.BreakAtEntry(emulator, _map);
        }

        return true;
    }

    public IReadOnlyList<LineBreakpoint> SetBreakpoints(string? path, int sourceReference, IReadOnlyList<int> lines)
    {
        var results = new List<LineBreakpoint>(lines.Count);
        if (_map == null || !TryResolveFile(path, sourceReference, out int fileId))
        {
            foreach (int line in lines)
            {
                results.Add(new LineBreakpoint(false, line));
            }

            return results;
        }

        var addresses = new HashSet<ushort>();
        foreach (int line in lines)
        {
            bool verified = _map.LineHasPoint(fileId, line);
            if (verified)
            {
                _map.CollectLineAddresses(fileId, line, addresses);
            }

            results.Add(new LineBreakpoint(verified, line));
        }

        _byFile[fileId] = addresses;
        Rebuild();
        return results;
    }

    public StopReason Continue(Func<bool>? interrupt = null, int maxSteps = int.MaxValue)
    {
        if (_emulator == null)
        {
            return StopReason.Pause;
        }

        ushort startPc = _emulator.GetDebugState().PC;
        bool left = false;
        for (int i = 0; i < maxSteps; i++)
        {
            if (interrupt?.Invoke() == true)
            {
                return StopReason.Pause;
            }

            _emulator.Step();
            ushort pc = _emulator.GetDebugState().PC;
            if (pc != startPc)
            {
                left = true;
            }

            if (left && _addresses.Contains(pc))
            {
                return StopReason.Breakpoint;
            }
        }

        return StopReason.Pause;
    }

    public StopReason StepOver(Func<bool>? interrupt = null) => Step(GblStatementStepper.StepOver, interrupt);

    public StopReason StepInto(Func<bool>? interrupt = null) => Step(GblStatementStepper.StepInto, interrupt);

    public StopReason StepOut(Func<bool>? interrupt = null) => Step(GblStatementStepper.StepOut, interrupt);

    public SourceFrame CurrentFrame()
    {
        if (_emulator == null)
        {
            return new SourceFrame("$0000", null, 0, 1, 0);
        }

        DebugState state = _emulator.GetDebugState();
        if (_map == null || !_map.TryGetActive(_emulator.RomBank, state.PC, out GblSequencePoint point))
        {
            return new SourceFrame($"${state.PC:X4}", null, 0, 1, state.PC);
        }

        string name = "$" + state.PC.ToString("X4");
        if (_map.TryGetEnclosingFunction(_emulator.RomBank, state.PC, out GblSequencePoint function))
        {
            name = FunctionLabel(_map, function);
        }

        if (!_map.TryGetFile(point.FileId, out string fileName, out string? fullPath))
        {
            return new SourceFrame(name, null, 0, point.Line, state.PC);
        }

        if (fullPath == null)
        {
            return new SourceFrame(name, fileName, point.FileId + 1, point.Line, state.PC);
        }

        return new SourceFrame(name, fullPath, 0, point.Line, state.PC);
    }

    public IReadOnlyList<DebugVariable> Registers()
    {
        if (_emulator == null)
        {
            return [];
        }

        DebugState state = _emulator.GetDebugState();
        return
        [
            new DebugVariable("A", Hex(state.A)),
            new DebugVariable("F", Hex(state.F)),
            new DebugVariable("B", Hex(state.B)),
            new DebugVariable("C", Hex(state.C)),
            new DebugVariable("D", Hex(state.D)),
            new DebugVariable("E", Hex(state.E)),
            new DebugVariable("H", Hex(state.H)),
            new DebugVariable("L", Hex(state.L)),
            new DebugVariable("AF", Hex(state.AF)),
            new DebugVariable("BC", Hex(state.BC)),
            new DebugVariable("DE", Hex(state.DE)),
            new DebugVariable("HL", Hex(state.HL)),
            new DebugVariable("PC", Hex(state.PC)),
            new DebugVariable("SP", Hex(state.SP)),
            new DebugVariable("IME", state.Ime ? "true" : "false"),
            new DebugVariable("Halted", state.Halted ? "true" : "false"),
        ];
    }

    public bool TryGetFrame(out int width, out int height, out string rgbaBase64)
    {
        width = Emulator.WindowWidth;
        height = Emulator.WindowHeight;
        rgbaBase64 = "";
        if (_emulator == null)
        {
            return false;
        }

        rgbaBase64 = Convert.ToBase64String(_emulator.Framebuffer);
        return true;
    }

    public void SetButton(string? name, bool pressed)
    {
        if (_emulator == null || string.IsNullOrEmpty(name))
        {
            return;
        }

        if (!Enum.TryParse(name, ignoreCase: true, out GameboyButton button))
        {
            return;
        }

        _emulator.SetButtonState(button, pressed);
    }

    public bool TryReadSource(int sourceReference, out string name, out string text)
    {
        name = "";
        text = "";
        if (_map == null || sourceReference <= 0)
        {
            return false;
        }

        return _map.TryGetSource(sourceReference - 1, out name, out text);
    }

    StopReason Step(Func<Emulator, GblDebugMap, Func<bool>?, bool> step, Func<bool>? interrupt)
    {
        if (_emulator == null)
        {
            return StopReason.Pause;
        }

        if (_map == null || _map.PointCount == 0)
        {
            if (interrupt?.Invoke() == true)
            {
                return StopReason.Pause;
            }

            _emulator.Step();
            return StopReason.Step;
        }

        return step(_emulator, _map, interrupt) ? StopReason.Step : StopReason.Pause;
    }

    bool TryResolveFile(string? path, int sourceReference, out int fileId)
    {
        fileId = -1;
        if (_map == null)
        {
            return false;
        }

        if (sourceReference > 0)
        {
            fileId = sourceReference - 1;
            return (uint)fileId < (uint)_map.FileCount;
        }

        if (path == null)
        {
            return false;
        }

        return _map.TryMatchFile(path, out fileId);
    }

    bool TryResolveProgram(string program, string? cwd, out string romPath, out string error)
    {
        romPath = "";
        error = "";
        string path = program;
        if (!Path.IsPathRooted(path))
        {
            string root = string.IsNullOrWhiteSpace(cwd) ? Directory.GetCurrentDirectory() : cwd;
            path = Path.GetFullPath(Path.Combine(root, path));
        }

        if (path.EndsWith(".gbl", StringComparison.OrdinalIgnoreCase))
        {
            if (!GbcHost.TryBuild(path, out romPath, out string log, out error))
            {
                Log = log;
                return false;
            }

            Log = log;
            return true;
        }

        if (!File.Exists(path))
        {
            error = $"ROM not found: {path}";
            return false;
        }

        romPath = path;
        return true;
    }

    void Rebuild()
    {
        _addresses.Clear();
        foreach (HashSet<ushort> addresses in _byFile.Values)
        {
            _addresses.UnionWith(addresses);
        }
    }

    static string FunctionLabel(GblDebugMap map, GblSequencePoint function)
    {
        if (!map.TryGetSource(function.FileId, out _, out string text))
        {
            return "function";
        }

        string[] lines = text.Split('\n');
        int index = function.Line - 1;
        if ((uint)index >= (uint)lines.Length)
        {
            return "function";
        }

        string line = lines[index].Trim().TrimEnd('\r');
        if (line.Length == 0)
        {
            return "function";
        }

        return line.Length > 80 ? line[..80] : line;
    }

    static string Hex(byte value) => $"0x{value:X2}";

    static string Hex(ushort value) => $"0x{value:X4}";
}

public enum StopReason
{
    Entry,
    Step,
    Breakpoint,
    Pause,
}

public readonly record struct LineBreakpoint(bool Verified, int Line);

public readonly record struct DebugVariable(string Name, string Value);

public sealed class SourceFrame
{
    public SourceFrame(string name, string? path, int sourceReference, int line, ushort pc)
    {
        Name = name;
        Path = path;
        SourceReference = sourceReference;
        Line = line;
        PC = pc;
    }

    public string Name { get; }

    /// <summary>Absolute path, embedded display name, or null when the address has no source.</summary>
    public string? Path { get; }

    public int SourceReference { get; }

    public int Line { get; }

    public ushort PC { get; }

    public bool HasSource => Path != null || SourceReference > 0;
}
