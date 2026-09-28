using System.Text;
using Monoboy.Desktop.TuiDebugger;

int failed = 0;

failed += CheckMap();
failed += CheckStepping();

if (failed != 0)
{
    Console.Error.WriteLine($"{failed} debug map check(s) failed");
    return 1;
}

Console.WriteLine("debug map checks passed");
return 0;

static int CheckMap()
{
    string root = Path.Combine(Path.GetTempPath(), "gbldbg-" + Guid.NewGuid().ToString("N"));
    string bin = Path.Combine(root, "bin");
    Directory.CreateDirectory(bin);
    try
    {
        File.WriteAllText(Path.Combine(root, "demo.gbl"), "Main()\n{\n}\n");
        string embed = "hello";
        string mapText =
            "gbl-debug 1\n"
            + "file 0 path ../demo.gbl\n"
            + $"file 1 embed Sample.gbl {Encoding.UTF8.GetByteCount(embed)}\n"
            + embed + "\n"
            + "00:0150 0 3 func\n"
            + "00:0160 0 8 call\n"
            + "00:0180 1 1 func\n";
        string mapPath = Path.Combine(bin, "demo.gbldbg");
        File.WriteAllText(mapPath, mapText);

        using FileStream stream = File.OpenRead(mapPath);
        GblDebugMap? map = GblDebugMap.TryParse(stream, bin);
        if (map == null)
        {
            Console.Error.WriteLine("TryParse returned null");
            return 1;
        }

        if (map.ActiveIndex(1, 0x0155) is not int mid || !map.TryGetPoint(mid, out GblSequencePoint midPoint))
        {
            Console.Error.WriteLine("Missing active point inside the first statement");
            return 1;
        }

        if (midPoint.Address != 0x0150 || midPoint.Line != 3 || midPoint.Kind != "func" || midPoint.FileId != 0)
        {
            Console.Error.WriteLine($"Mid point was {midPoint.Address:X4} file {midPoint.FileId} line {midPoint.Line} {midPoint.Kind}");
            return 1;
        }

        if (map.IsOnPoint(1, 0x0155))
        {
            Console.Error.WriteLine("Address inside a statement was treated as a sequence point");
            return 1;
        }

        if (!map.IsOnPoint(0, 0x0160) || map.ActiveIndex(0, 0x0160) is not int callIndex)
        {
            Console.Error.WriteLine("Call site was not on its sequence point");
            return 1;
        }

        if (!map.TryGetPoint(callIndex, out GblSequencePoint call) || call.Kind != "call" || call.Line != 8)
        {
            Console.Error.WriteLine("Call point did not win at its address");
            return 1;
        }

        if (!map.TryGetSource(0, out string userName, out string userText)
            || userName != "demo.gbl"
            || !userText.Contains("Main()", StringComparison.Ordinal))
        {
            Console.Error.WriteLine("User source was not loaded from the relative path");
            return 1;
        }

        if (!map.TryGetSource(1, out string embedName, out string embedText)
            || embedName != "Sample.gbl"
            || embedText != embed)
        {
            Console.Error.WriteLine("Embedded source did not round-trip");
            return 1;
        }

        if (map.ActiveIndex(0, 0x0180) is not int library || !map.TryGetPoint(library, out GblSequencePoint libraryPoint)
            || libraryPoint.FileId != 1)
        {
            Console.Error.WriteLine("Library point was not selected");
            return 1;
        }

        if (!map.TryGetEntry(out GblSequencePoint entry) || entry.Address != 0x0150)
        {
            Console.Error.WriteLine("Entry point was not the first statement");
            return 1;
        }

        GblSourceBreakpoints.Clear();
        if (!map.LineHasPoint(0, 8) || map.LineHasPoint(0, 4))
        {
            Console.Error.WriteLine("LineHasPoint disagreed with the sample map");
            return 1;
        }

        if (!GblSourceBreakpoints.Toggle(map, 0, 8) || !GblSourceBreakpoints.IsHit(0x0160) || GblSourceBreakpoints.IsHit(0x0155))
        {
            Console.Error.WriteLine("Source breakpoint did not bind to the call address");
            return 1;
        }

        if (GblSourceBreakpoints.Toggle(map, 0, 8) || GblSourceBreakpoints.IsHit(0x0160))
        {
            Console.Error.WriteLine("Source breakpoint did not clear");
            return 1;
        }

        return 0;
    }
    finally
    {
        Directory.Delete(root, recursive: true);
    }
}

static int CheckStepping()
{
    if (GblDebugStepping.StopOver(0xFFFE, 0, 0xFFFC, 1))
    {
        Console.Error.WriteLine("Step over stopped inside a call");
        return 1;
    }

    if (GblDebugStepping.StopOver(0xFFFE, 0, 0xFFFE, 0))
    {
        Console.Error.WriteLine("Step over stopped on the same statement");
        return 1;
    }

    if (!GblDebugStepping.StopOver(0xFFFE, 0, 0xFFFE, 1))
    {
        Console.Error.WriteLine("Step over did not stop on the next statement");
        return 1;
    }

    if (!GblDebugStepping.StopInto(0xFFFE, 0, 0xFFFC, 2))
    {
        Console.Error.WriteLine("Step into did not stop when the stack grew");
        return 1;
    }

    if (!GblDebugStepping.StopInto(0xFFFE, 0, 0xFFFE, 1))
    {
        Console.Error.WriteLine("Step into did not move to the next statement when nothing was called");
        return 1;
    }

    if (GblDebugStepping.StopOut(0xFFFC, 0xFFFE, false))
    {
        Console.Error.WriteLine("Step out stopped before a sequence point");
        return 1;
    }

    if (!GblDebugStepping.StopOut(0xFFFC, 0xFFFE, true))
    {
        Console.Error.WriteLine("Step out did not stop on the caller's sequence point");
        return 1;
    }

    return 0;
}
