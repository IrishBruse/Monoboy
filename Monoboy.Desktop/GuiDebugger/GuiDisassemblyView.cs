namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.Collections.Generic;
using System.Numerics;

using ImGuiNET;

using Monoboy;
using Monoboy.Desktop.TuiDebugger;

/// <summary>
/// Instruction list for the GUI debugger pane, with optional branch-target asm preview.
/// The current instruction stays a fixed number of lines from the top.
/// </summary>
public static class GuiDisassemblyView
{
    const int PcLinesFromTop = 3;
    const int PreviewMinWidth = 140;
    const int MaxWalk = 8192;

    static ushort _frozenPc;
    static bool _wasRunning;
    static ushort _anchoredPc;
    static int _lineSkip;
    static float _wheelAccum;
    static string? _loadedRomPath;
    static SymSymbolMap? _symbols;
    static readonly DisasmAlignmentCache _alignment = new();

    public static void Draw(Emulator emulator, bool running)
    {
        Vector2 avail = ImGui.GetContentRegionAvail();
        if (avail.X <= 0 || avail.Y <= 0)
        {
            return;
        }

        ushort pc = FollowPc(emulator, running);
        SyncSymbols(emulator);
        ushort size = TuiDisassemblyFormatter.GetInstructionByteSize(emulator, pc);
        _alignment.RecordStop(emulator.RomBank, pc, size);

        bool showPreview = TuiDisassemblyFormatter.TryGetBranchTarget(emulator, pc, out ushort branchTarget);
        int previewW = 0;
        if (showPreview)
        {
            previewW = Math.Clamp((int)(avail.X * 0.38f), PreviewMinWidth, Math.Max(PreviewMinWidth, (int)avail.X / 2));
            if (previewW + PreviewMinWidth > avail.X)
            {
                previewW = Math.Max(0, (int)avail.X - PreviewMinWidth);
            }
        }

        if (previewW > 0 && avail.X - previewW < 1)
        {
            previewW = 0;
        }

        float mainW = Math.Max(1f, avail.X - previewW);
        float childH = Math.Max(1f, avail.Y);

        ImGui.BeginChild(
            "DisasmMain",
            new Vector2(mainW, childH),
            ImGuiChildFlags.None,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        DrawMainList(emulator, pc);
        ImGui.EndChild();

        if (showPreview && previewW > 0)
        {
            ImGui.SameLine(0, 0);
            ImGui.BeginChild("DisasmPreview", new Vector2(previewW, childH), ImGuiChildFlags.None);
            GuiAsmBranchPreviewView.Draw(emulator, branchTarget, _symbols);
            ImGui.EndChild();
        }
    }

    static ushort FollowPc(Emulator emulator, bool running)
    {
        ushort livePc = emulator.GetDebugState().PC;
        if (!running)
        {
            _frozenPc = livePc;
        }
        else if (!_wasRunning)
        {
            _frozenPc = livePc;
        }

        _wasRunning = running;
        if (_frozenPc != _anchoredPc)
        {
            _anchoredPc = _frozenPc;
            _lineSkip = 0;
            _wheelAccum = 0;
        }

        return _frozenPc;
    }

    static void SyncSymbols(Emulator emulator)
    {
        string? path = emulator.RomPath;
        if (path == _loadedRomPath)
        {
            return;
        }

        _loadedRomPath = path;
        _symbols = string.IsNullOrEmpty(path) ? null : SymSymbolMap.TryLoadForRom(path);
        _alignment.Clear();
        _lineSkip = 0;
    }

    static void DrawMainList(Emulator emulator, ushort pc)
    {
        ApplyWheel();

        float lineH = ImGui.GetTextLineHeightWithSpacing();
        float availY = ImGui.GetContentRegionAvail().Y;
        int visibleRows = Math.Max(1, (int)(availY / lineH));
        List<GuiDisassemblySyntax.Line> lines = CollectVisibleLines(emulator, pc, visibleRows);

        foreach (GuiDisassemblySyntax.Line line in lines)
        {
            bool isPc = !line.IsLabel && line.Address == pc;
            if (isPc)
            {
                Vector2 pos = ImGui.GetCursorScreenPos();
                float rowWidth = ImGui.GetContentRegionAvail().X;
                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(
                    pos,
                    new Vector2(pos.X + rowWidth, pos.Y + lineH),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(
                        GuiDebuggerTheme.ProgramCounterRow.R / 255f,
                        GuiDebuggerTheme.ProgramCounterRow.G / 255f,
                        GuiDebuggerTheme.ProgramCounterRow.B / 255f,
                        GuiDebuggerTheme.ProgramCounterRow.A / 255f)));
            }

            if (line.IsLabel)
            {
                GuiDisassemblySyntax.DrawLabel(line.Label);
            }
            else
            {
                GuiDisassemblySyntax.DrawLine(emulator, line.Address, isPc, _symbols);
            }
        }
    }

    /// <summary>One wheel detent moves the list by one display line. The PC change resets this offset.</summary>
    static void ApplyWheel()
    {
        if (!ImGui.IsWindowHovered())
        {
            return;
        }

        float wheel = ImGui.GetIO().MouseWheel;
        if (wheel == 0)
        {
            return;
        }

        _wheelAccum += wheel;
        ImGui.GetIO().MouseWheel = 0;
        int steps = (int)_wheelAccum;
        if (steps == 0)
        {
            return;
        }

        _lineSkip -= steps;
        _wheelAccum -= steps;
    }

    static List<GuiDisassemblySyntax.Line> CollectVisibleLines(Emulator emulator, ushort pc, int visibleRows)
    {
        int margin = Math.Min(PcLinesFromTop, Math.Max(0, visibleRows - 1));
        var instructions = new List<ushort>(visibleRows + margin + 8);
        ushort walk = pc;
        int walked = 0;
        while (walked < MaxWalk
            && TuiDisassemblyFormatter.TryGetPreviousInstructionStart(
                emulator,
                emulator.RomBank,
                walk,
                _symbols,
                _alignment,
                out ushort prev)
            && prev < walk)
        {
            instructions.Add(prev);
            walk = prev;
            walked++;
            if (instructions.Count >= margin + Math.Max(0, -_lineSkip) + 8)
            {
                break;
            }
        }

        instructions.Reverse();
        instructions.Add(pc);

        ushort cursor = pc;
        int forwardNeed = visibleRows + Math.Max(0, _lineSkip) + 8;
        for (int i = 0; i < forwardNeed && i < MaxWalk; i++)
        {
            ushort step = TuiDisassemblyFormatter.GetInstructionByteSize(emulator, cursor);
            int next = cursor + step;
            if (step == 0 || next > 0xFFFF)
            {
                break;
            }

            cursor = (ushort)next;
            instructions.Add(cursor);
        }

        var all = new List<GuiDisassemblySyntax.Line>(instructions.Count * 2);
        foreach (ushort addr in instructions)
        {
            GuiDisassemblySyntax.AppendBlock(all, emulator, addr, _symbols);
        }

        int pcLine = 0;
        for (int i = 0; i < all.Count; i++)
        {
            if (!all[i].IsLabel && all[i].Address == pc)
            {
                pcLine = i;
                break;
            }
        }

        int minSkip = margin - pcLine;
        int maxStart = Math.Max(0, all.Count - visibleRows);
        int maxSkip = maxStart - (pcLine - margin);
        if (_lineSkip < minSkip)
        {
            _lineSkip = minSkip;
        }

        if (_lineSkip > maxSkip)
        {
            _lineSkip = maxSkip;
        }

        int start = pcLine - margin + _lineSkip;
        if (start < 0)
        {
            start = 0;
        }

        if (start > all.Count)
        {
            start = all.Count;
        }

        int count = Math.Min(visibleRows, all.Count - start);
        if (count <= 0)
        {
            return [];
        }

        return all.GetRange(start, count);
    }
}
