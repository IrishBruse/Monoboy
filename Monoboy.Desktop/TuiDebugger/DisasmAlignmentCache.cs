namespace Monoboy.Desktop.TuiDebugger;

using System;
using System.Collections.Generic;

using Monoboy;

/// <summary>Known instruction boundaries from execution stops and fixed entry points.</summary>
sealed class DisasmAlignmentCache
{
    static readonly ushort[] FixedAnchors = [0x0100, 0x0040, 0x0048, 0x0050, 0x0058, 0x0060];

    readonly Dictionary<(byte bank, ushort addr), ushort> instructionSizes = new();
    byte? lastStopBank;
    ushort lastStopStart;
    ushort lastStopSize;

    public void Clear()
    {
        instructionSizes.Clear();
        lastStopBank = null;
        lastStopStart = 0;
        lastStopSize = 0;
        foreach (ushort addr in FixedAnchors)
        {
            instructionSizes[(0, addr)] = 0;
        }
    }

    /// <summary>Record PC at an instruction boundary after a debugger stop.</summary>
    public void RecordStop(byte romBank, ushort instructionStart, ushort instructionSize)
    {
        instructionSizes[(romBank, instructionStart)] = instructionSize;
        lastStopBank = romBank;
        lastStopStart = instructionStart;
        lastStopSize = instructionSize;
    }

    public bool TryGetPreviousInstructionStart(
        Emulator emulator,
        byte romBank,
        ushort addr,
        SymSymbolMap? symbols,
        out ushort prevStart)
    {
        if (TryGetPreviousFromLastStop(romBank, addr, out prevStart))
        {
            return true;
        }

        if (TryGetPreviousFromVisitedChain(romBank, addr, out prevStart))
        {
            return true;
        }

        if (TryGetPreviousFromForwardAnchor(emulator, romBank, addr, symbols, out prevStart))
        {
            return true;
        }

        return TuiDisassemblyFormatter.TryGetPreviousInstructionStartHeuristic(emulator, addr, out prevStart);
    }

    bool TryGetPreviousFromLastStop(byte romBank, ushort addr, out ushort prevStart)
    {
        prevStart = 0;
        if (lastStopBank != romBank)
        {
            return false;
        }

        if ((ushort)(lastStopStart + lastStopSize) != addr)
        {
            return false;
        }

        prevStart = lastStopStart;
        return true;
    }

    bool TryGetPreviousFromVisitedChain(byte romBank, ushort addr, out ushort prevStart)
    {
        prevStart = 0;
        ushort bestStart = 0;

        foreach (var ((bank, start), size) in instructionSizes)
        {
            if (bank != romBank || size == 0)
            {
                continue;
            }

            if ((ushort)(start + size) != addr)
            {
                continue;
            }

            if (start > bestStart)
            {
                bestStart = start;
            }
        }

        if (bestStart == 0)
        {
            return false;
        }

        prevStart = bestStart;
        return true;
    }

    bool TryGetPreviousFromForwardAnchor(
        Emulator emulator,
        byte romBank,
        ushort addr,
        SymSymbolMap? symbols,
        out ushort prevStart)
    {
        prevStart = 0;
        if (!TryGetBestForwardAnchor(romBank, addr, symbols, out ushort anchor))
        {
            return false;
        }

        ushort cursor = anchor;
        ushort? previous = null;
        while (cursor < addr)
        {
            previous = cursor;
            cursor += TuiDisassemblyFormatter.GetInstructionByteSize(emulator, cursor);
        }

        if (cursor != addr || previous == null)
        {
            return false;
        }

        prevStart = previous.Value;
        return true;
    }

    bool TryGetBestForwardAnchor(byte romBank, ushort addr, SymSymbolMap? symbols, out ushort anchor)
    {
        anchor = 0;
        if (addr == 0)
        {
            return false;
        }

        ushort limit = (ushort)(addr - 1);
        ushort best = 0;

        void Consider(ushort candidate)
        {
            if (candidate <= limit && candidate > best)
            {
                best = candidate;
            }
        }

        foreach (ushort fixedAddr in FixedAnchors)
        {
            Consider(fixedAddr);
        }

        if (symbols != null && symbols.TryGetAnchorAtOrBefore(romBank, limit, out ushort symAnchor))
        {
            Consider(symAnchor);
        }

        foreach (var ((bank, start), size) in instructionSizes)
        {
            if (bank != romBank || size == 0)
            {
                continue;
            }

            Consider(start);
        }

        if (best == 0)
        {
            return false;
        }

        anchor = best;
        return true;
    }
}
