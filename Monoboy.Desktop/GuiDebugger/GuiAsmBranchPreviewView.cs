namespace Monoboy.Desktop.GuiDebugger;

using System;
using System.Collections.Generic;

using Monoboy;
using Monoboy.Desktop.TuiDebugger;

using Raylib_cs;

/// <summary>Scrollable branch/call target disassembly beside the main list.</summary>
static class GuiAsmBranchPreviewView
{
    const int LineHeight = 18;
    const int PreviewPad = 4;
    const int MaxInstructions = 512;

    public static void Draw(
        Rectangle area,
        Emulator emulator,
        ushort target,
        float wheel,
        ref int scrollRows,
        ref ushort scrollTarget)
    {
        if (target != scrollTarget)
        {
            scrollTarget = target;
            scrollRows = 0;
        }

        int areaX = (int)area.X;
        int areaY = (int)area.Y;
        int areaW = Math.Max(0, (int)area.Width);
        int areaH = Math.Max(0, (int)area.Height);
        if (areaW <= 0 || areaH <= 0)
        {
            return;
        }

        Raylib.DrawRectangle(areaX, areaY, areaW, areaH, GuiDebuggerTheme.PanelBackground);
        Raylib.DrawLine(areaX, areaY, areaX, areaY + areaH, GuiDebuggerTheme.PanelBorder);

        var addresses = CollectInstructions(emulator, target, MaxInstructions);
        int visibleRows = Math.Max(1, areaH / LineHeight);
        int maxScroll = Math.Max(0, addresses.Count - visibleRows);
        GuiScroll.ApplyWheel(area, wheel, ref scrollRows, maxScroll);
        scrollRows = Math.Clamp(scrollRows, 0, maxScroll);

        int contentW = Math.Max(1, areaW - GuiScroll.Width - PreviewPad);
        Raylib.BeginScissorMode(areaX + PreviewPad, areaY, contentW, areaH);
        for (int row = 0; row < visibleRows; row++)
        {
            int idx = scrollRows + row;
            if (idx >= addresses.Count)
            {
                break;
            }

            int y = areaY + row * LineHeight;
            GuiDisassemblySyntax.DrawLine(areaX + PreviewPad, y, emulator, addresses[idx], isPcRow: false);
        }

        Raylib.EndScissorMode();

        var bar = new Rectangle(areaX + areaW - GuiScroll.Width, areaY, GuiScroll.Width, areaH);
        GuiScroll.Draw(2, bar, ref scrollRows, maxScroll);
    }

    static List<ushort> CollectInstructions(Emulator emulator, ushort start, int maxCount)
    {
        var list = new List<ushort>(Math.Min(maxCount, 64));
        ushort cursor = start;
        for (int i = 0; i < maxCount; i++)
        {
            list.Add(cursor);
            ushort size = TuiDisassemblyFormatter.GetInstructionByteSize(emulator, cursor);
            if (size == 0)
            {
                break;
            }

            int next = cursor + size;
            if (next > 0xFFFF)
            {
                break;
            }

            cursor = (ushort)next;
        }

        return list;
    }
}
