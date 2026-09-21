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
        switch (index)
        {
            case StepInto:
                running = false;
                emulator.Step();
                break;
            case StepOver:
                running = false;
                StepOverInstruction(emulator);
                break;
            case Continue:
                running = true;
                break;
            case Pause:
                running = false;
                break;
            case Reset:
                running = false;
                emulator.Reset();
                break;
            case NextVBlank:
                running = false;
                emulator.StepFrame();
                break;
            case StepOut:
                running = false;
                StepOutOfFrame(emulator);
                break;
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
}
