namespace Monoboy.Debug;

using Monoboy;

/// <summary>Statement step over, into, and out against a loaded <see cref="GblDebugMap"/>.</summary>
public static class GblStatementStepper
{
    public const int MaxSteps = 1_000_000;

    public static bool StepOver(Emulator emulator, GblDebugMap map, Func<bool>? interrupt = null)
    {
        return StepUntil(emulator, map, GblDebugStepping.StopOver, interrupt);
    }

    public static bool StepInto(Emulator emulator, GblDebugMap map, Func<bool>? interrupt = null)
    {
        return StepUntil(emulator, map, GblDebugStepping.StopInto, interrupt);
    }

    public static bool StepOut(Emulator emulator, GblDebugMap map, Func<bool>? interrupt = null)
    {
        ushort startSp = emulator.GetDebugState().SP;
        for (int i = 0; i < MaxSteps; i++)
        {
            if (interrupt?.Invoke() == true)
            {
                return false;
            }

            emulator.Step();
            DebugState now = emulator.GetDebugState();
            if (GblDebugStepping.StopOut(startSp, now.SP, map.IsOnPoint(emulator.RomBank, now.PC)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reset and stop on the first sequence point.</summary>
    public static void BreakAtEntry(Emulator emulator, GblDebugMap map)
    {
        if (!map.TryGetEntry(out GblSequencePoint entry))
        {
            return;
        }

        emulator.Reset();
        if (emulator.GetDebugState().PC == entry.Address)
        {
            return;
        }

        for (int i = 0; i < MaxSteps; i++)
        {
            emulator.Step();
            if (emulator.GetDebugState().PC == entry.Address)
            {
                return;
            }
        }
    }

    static bool StepUntil(
        Emulator emulator,
        GblDebugMap map,
        Func<ushort, int?, ushort, int?, bool> stop,
        Func<bool>? interrupt)
    {
        DebugState start = emulator.GetDebugState();
        ushort startSp = start.SP;
        int? startPoint = map.ActiveIndex(emulator.RomBank, start.PC);
        for (int i = 0; i < MaxSteps; i++)
        {
            if (interrupt?.Invoke() == true)
            {
                return false;
            }

            emulator.Step();
            DebugState now = emulator.GetDebugState();
            int? point = map.ActiveIndex(emulator.RomBank, now.PC);
            if (stop(startSp, startPoint, now.SP, point))
            {
                return true;
            }
        }

        return false;
    }
}
