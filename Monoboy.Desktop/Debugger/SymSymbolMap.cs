namespace Monoboy.Desktop.Debugger;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

/// <summary>RGBDS-style .sym companion (bank:address label per line).</summary>
sealed class SymSymbolMap
{
    readonly Dictionary<(uint bank, ushort addr), string> labels = new();

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

        return map;
    }

    /// <summary>Resolve a label for a CPU address and the active ROM bank.</summary>
    public bool TryGetLabel(ushort cpuAddr, byte romBank, out string label)
    {
        if (labels.TryGetValue((romBank, cpuAddr), out label!))
        {
            return true;
        }

        if (romBank != 0 && labels.TryGetValue((0, cpuAddr), out label!))
        {
            return true;
        }

        return false;
    }

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

        labels[(bank, addr)] = tokens[1];
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
