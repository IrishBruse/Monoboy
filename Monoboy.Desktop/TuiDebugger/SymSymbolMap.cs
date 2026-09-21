namespace Monoboy.Desktop.TuiDebugger;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

/// <summary>RGBDS-style .sym companion (bank:address label per line).</summary>
sealed class SymSymbolMap
{
    readonly Dictionary<(uint bank, ushort addr), List<string>> labels = new();
    readonly Dictionary<byte, ushort[]> anchorsByRomBank = new();

    public static SymSymbolMap? TryLoadForRom(string romPath)
    {
        if (string.IsNullOrWhiteSpace(romPath))
        {
            return null;
        }

        string symPath = Path.ChangeExtension(romPath, ".sym");
        if (!File.Exists(symPath))
        {
            return null;
        }

        return Load(symPath);
    }

    public static SymSymbolMap Load(string symPath)
    {
        var map = new SymSymbolMap();
        foreach (string rawLine in File.ReadLines(symPath))
        {
            map.TryAddLine(rawLine);
        }

        map.BuildAnchorIndex();
        return map;
    }

    /// <summary>Greatest symbol address &lt;= <paramref name="addr"/> for this ROM bank.</summary>
    public bool TryGetAnchorAtOrBefore(byte romBank, ushort addr, out ushort anchor)
    {
        anchor = 0;
        if (!TryGetAnchors(romBank, out ushort[] anchors) || anchors.Length == 0)
        {
            return false;
        }

        int lo = 0;
        int hi = anchors.Length - 1;
        int best = -1;
        while (lo <= hi)
        {
            int mid = lo + ((hi - lo) >> 1);
            if (anchors[mid] <= addr)
            {
                best = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        if (best < 0)
        {
            return false;
        }

        anchor = anchors[best];
        return true;
    }

    bool TryGetAnchors(byte romBank, out ushort[] anchors) =>
        anchorsByRomBank.TryGetValue(romBank, out anchors!) && anchors.Length > 0;

    void BuildAnchorIndex()
    {
        var sets = new Dictionary<byte, SortedSet<ushort>>();
        foreach (var ((bank, addr), _) in labels)
        {
            byte romBank = (byte)Math.Min(bank, byte.MaxValue);
            if (!sets.TryGetValue(romBank, out SortedSet<ushort>? set))
            {
                set = new SortedSet<ushort>();
                sets[romBank] = set;
            }

            set.Add(addr);
        }

        if (!sets.TryGetValue(0, out SortedSet<ushort>? bank0))
        {
            bank0 = new SortedSet<ushort>();
        }

        foreach (var (romBank, set) in sets)
        {
            if (romBank == 0)
            {
                continue;
            }

            foreach (ushort addr in bank0)
            {
                set.Add(addr);
            }

            anchorsByRomBank[romBank] = [.. set];
        }

        if (bank0.Count > 0)
        {
            anchorsByRomBank[0] = [.. bank0];
        }
    }

    /// <summary>All labels at a CPU address (file order); bank 0 symbols apply in other banks too.</summary>
    public bool TryGetLabels(ushort cpuAddr, byte romBank, out IReadOnlyList<string> names)
    {
        if (TryGetLabelList((romBank, cpuAddr), out List<string>? list))
        {
            names = list;
            return true;
        }

        if (romBank != 0 && TryGetLabelList((0, cpuAddr), out list))
        {
            names = list;
            return true;
        }

        names = Array.Empty<string>();
        return false;
    }

    bool TryGetLabelList((uint bank, ushort addr) key, out List<string>? list) =>
        labels.TryGetValue(key, out list) && list is { Count: > 0 };

    void TryAddLine(string rawLine)
    {
        int semi = rawLine.IndexOf(';');
        if (semi >= 0)
        {
            rawLine = rawLine[..semi];
        }

        string[] tokens = rawLine.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2)
        {
            return;
        }

        if (!TryParseLocation(tokens[0], out uint bank, out ushort addr))
        {
            return;
        }

        var key = (bank, addr);
        if (!labels.TryGetValue(key, out List<string>? list))
        {
            list = new List<string>();
            labels[key] = list;
        }

        string name = tokens[1];
        if (list.Contains(name))
        {
            return;
        }

        list.Add(name);
    }

    static bool TryParseLocation(string token, out uint bank, out ushort addr)
    {
        bank = 0;
        addr = 0;

        int colon = token.IndexOf(':');
        if (colon < 0)
        {
            return TryParseU16(token, out addr);
        }

        string bankPart = token[..colon];
        string addrPart = token[(colon + 1)..];
        if (!TryParseHex(bankPart, out bank) || !TryParseU16(addrPart, out addr))
        {
            return false;
        }

        return true;
    }

    static bool TryParseU16(string s, out ushort value)
    {
        if (!TryParseHex(s, out uint parsed) || parsed > ushort.MaxValue)
        {
            value = 0;
            return false;
        }

        value = (ushort)parsed;
        return true;
    }

    static bool TryParseHex(string s, out uint value)
    {
        return uint.TryParse(
            s,
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture,
            out value);
    }
}
