namespace Monoboy.Desktop.GuiDebugger;

using Monoboy;
using Monoboy.Desktop.TuiDebugger;

/// <summary>Debugger run control (menu, toolbar, and keyboard shortcuts).</summary>
static class GuiDebugRunCommands
{
    internal const int StepInto = 0;
    internal const int StepOver = 1;
    internal const int Continue = 2;
    internal const int Pause = 3;
    internal const int Reset = 4;
    internal const int NextVBlank = 5;
    internal const int StepOut = 6;

    public static void Apply(int index, Emulator emulator, ref bool running)
    {
        var map = GblDebugMap.ForRom(emulator.RomPath);
        bool sourceSteps = map != null && map.PointCount > 0;

        switch (index)
        {
            case StepInto:
            running = false;
            if (sourceSteps)
            {
                StepIntoStatement(emulator, map!);
            }
            else
            {
                emulator.Step();
            }

            break;
            case StepOver:
            running = false;
            if (sourceSteps)
            {
                StepUntil(emulator, map!, GblDebugStepping.StopOver);
            }
            else
            {
                StepOverInstruction(emulator);
            }

            break;
            case Continue:
            running = true;
            break;
            case Pause:
            running = false;
            break;
            case Reset:
            running = false;
            if (sourceSteps)
            {
                BreakAtEntry(emulator);
            }
            else
            {
                emulator.Reset();
            }

            break;
            case NextVBlank:
            running = false;
            emulator.StepFrame();
            break;
            case StepOut:
            running = false;
            if (sourceSteps)
            {
                StepOutOfStatement(emulator, map!);
            }
            else
            {
                StepOutOfFrame(emulator);
            }

            break;
        }
    }

    static void StepIntoStatement(Emulator emulator, GblDebugMap map)
    {
        StepUntil(emulator, map, GblDebugStepping.StopInto);
    }

    static void StepUntil(
        Emulator emulator,
        GblDebugMap map,
        System.Func<ushort, int?, ushort, int?, bool> stop)
    {
        DebugState start = emulator.GetDebugState();
        ushort startSp = start.SP;
        int? startPoint = map.ActiveIndex(emulator.RomBank, start.PC);
        for (int i = 0; i < 1_000_000; i++)
        {
            emulator.Step();
            DebugState now = emulator.GetDebugState();
            int? point = map.ActiveIndex(emulator.RomBank, now.PC);
            if (stop(startSp, startPoint, now.SP, point))
            {
                return;
            }
        }
    }

    static void StepOutOfStatement(Emulator emulator, GblDebugMap map)
    {
        ushort startSp = emulator.GetDebugState().SP;
        for (int i = 0; i < 1_000_000; i++)
        {
            emulator.Step();
            DebugState now = emulator.GetDebugState();
            if (GblDebugStepping.StopOut(startSp, now.SP, map.IsOnPoint(emulator.RomBank, now.PC)))
            {
                return;
            }
        }
    }

    static void StepOverInstruction(Emulator emulator)
    {
        ushort pc = emulator.GetDebugState().PC;
        byte op = emulator.Read(pc);
        bool isCall = op is 0xC4 or 0xCC or 0xCD or 0xD4 or 0xDC;
        bool isRst = op is 0xC7 or 0xCF or 0xD7 or 0xDF or 0xE7 or 0xEF or 0xF7 or 0xFF;
        if (!isCall && !isRst)
        {
            emulator.Step();
            return;
        }

        ushort next = (ushort)(pc + TuiDisassemblyFormatter.GetInstructionByteSize(emulator, pc));
        for (int i = 0; i < 1_000_000; i++)
        {
            emulator.Step();
            if (emulator.GetDebugState().PC == next)
            {
                return;
            }
        }
    }

    static void StepOutOfFrame(Emulator emulator)
    {
        ushort sp = emulator.GetDebugState().SP;
        if (sp >= 0xFFFE)
        {
            emulator.Step();
            return;
        }

        ushort returnPc = ReadU16(emulator, sp);
        for (int i = 0; i < 1_000_000; i++)
        {
            emulator.Step();
            if (emulator.GetDebugState().PC == returnPc)
            {
                return;
            }
        }
    }

    static ushort ReadU16(Emulator emulator, ushort addr) =>
        (ushort)(emulator.Read(addr) | (emulator.Read((ushort)(addr + 1)) << 8));

    /// <summary>Reset and stop on the first GBL statement. No-op when the ROM has no statement map.</summary>
    public static void BreakAtEntry(Emulator emulator)
    {
        var map = GblDebugMap.ForRom(emulator.RomPath);
        if (map == null || !map.TryGetEntry(out GblSequencePoint entry))
        {
            return;
        }

        emulator.Reset();
        if (emulator.GetDebugState().PC == entry.Address)
        {
            return;
        }

        for (int i = 0; i < 1_000_000; i++)
        {
            emulator.Step();
            if (emulator.GetDebugState().PC == entry.Address)
            {
                return;
            }
        }
    }
}
