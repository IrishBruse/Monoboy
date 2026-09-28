#nullable enable

namespace Monoboy.Desktop.TuiDebugger;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// gblang statement map (<c>.gbldbg</c> beside the ROM).
/// Sequence points are the first instruction of a statement or function.
/// </summary>
public sealed class GblDebugMap
{
    readonly List<GblSequencePoint> _points = new();
    readonly List<SourceEntry> _files = new();
    readonly string _baseDirectory;

    GblDebugMap(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    public int PointCount => _points.Count;

    static string? _cachedRomPath;
    static DateTime _cachedWriteTime;
    static GblDebugMap? _cached;

    public static GblDebugMap? ForRom(string? romPath)
    {
        if (string.IsNullOrWhiteSpace(romPath))
        {
            return null;
        }

        string dbgPath = Path.ChangeExtension(romPath, ".gbldbg");
        DateTime writeTime = File.Exists(dbgPath) ? File.GetLastWriteTimeUtc(dbgPath) : default;
        if (_cachedRomPath == romPath && _cachedWriteTime == writeTime)
        {
            return _cached;
        }

        _cachedRomPath = romPath;
        _cachedWriteTime = writeTime;
        _cached = TryLoadForRom(romPath);
        return _cached;
    }

    public static GblDebugMap? TryLoadForRom(string romPath)
    {
        if (string.IsNullOrWhiteSpace(romPath))
        {
            return null;
        }

        string dbgPath = Path.ChangeExtension(romPath, ".gbldbg");
        if (!File.Exists(dbgPath))
        {
            return null;
        }

        try
        {
            using FileStream stream = File.OpenRead(dbgPath);
            return TryParse(stream, Path.GetDirectoryName(Path.GetFullPath(dbgPath)) ?? ".");
        }
        catch (IOException)
        {
            return null;
        }
    }

    public static GblDebugMap? TryParse(Stream stream, string baseDirectory)
    {
        var map = new GblDebugMap(baseDirectory);
        string? header = ReadLine(stream);
        if (header != "gbl-debug 1")
        {
            return null;
        }

        while (true)
        {
            string? line = ReadLine(stream);
            if (line == null)
            {
                break;
            }

            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith("file ", StringComparison.Ordinal))
            {
                if (!map.TryAddFile(stream, line))
                {
                    return null;
                }

                continue;
            }

            if (!map.TryAddPoint(line))
            {
                return null;
            }
        }

        return map;
    }

    /// <summary>Last sequence point at or before <paramref name="pc"/> in this bank.</summary>
    public int? ActiveIndex(byte romBank, ushort pc)
    {
        int? best = null;
        for (int i = 0; i < _points.Count; i++)
        {
            GblSequencePoint point = _points[i];
            if (!BankMatches(point, romBank, pc) || point.Address > pc)
            {
                continue;
            }

            if (best == null || point.Address >= _points[best.Value].Address)
            {
                best = i;
            }
        }

        return best;
    }

    public bool TryGetEntry(out GblSequencePoint point)
    {
        point = default;
        if (_points.Count == 0)
        {
            return false;
        }

        int best = 0;
        for (int i = 1; i < _points.Count; i++)
        {
            if (_points[i].Address < _points[best].Address)
            {
                best = i;
            }
        }

        point = _points[best];
        return true;
    }

    public bool LineHasPoint(int fileId, int line)
    {
        for (int i = 0; i < _points.Count; i++)
        {
            GblSequencePoint point = _points[i];
            if (point.FileId == fileId && point.Line == line)
            {
                return true;
            }
        }

        return false;
    }

    public void CollectLineAddresses(int fileId, int line, HashSet<ushort> addresses)
    {
        for (int i = 0; i < _points.Count; i++)
        {
            GblSequencePoint point = _points[i];
            if (point.FileId == fileId && point.Line == line)
            {
                addresses.Add(point.Address);
            }
        }
    }

    public bool IsOnPoint(byte romBank, ushort pc)
    {
        int? index = ActiveIndex(romBank, pc);
        return index != null && _points[index.Value].Address == pc;
    }

    public bool TryGetPoint(int index, out GblSequencePoint point)
    {
        if ((uint)index >= (uint)_points.Count)
        {
            point = default;
            return false;
        }

        point = _points[index];
        return true;
    }

    public bool TryGetSource(int fileId, out string displayName, out string text)
    {
        displayName = "";
        text = "";
        if ((uint)fileId >= (uint)_files.Count)
        {
            return false;
        }

        SourceEntry file = _files[fileId];
        if (file.Text != null)
        {
            displayName = file.Name;
            text = file.Text;
            return true;
        }

        if (file.Loaded == null)
        {
            if (file.Path == null || !File.Exists(file.Path))
            {
                return false;
            }

            file.Loaded = File.ReadAllText(file.Path);
        }

        displayName = Path.GetFileName(file.Path ?? file.Name);
        text = file.Loaded;
        return true;
    }

    static bool BankMatches(GblSequencePoint point, byte romBank, ushort pc)
    {
        byte bank = pc < 0x4000 ? (byte)0 : romBank;
        return point.Bank == bank;
    }

    bool TryAddFile(Stream stream, string line)
    {
        // file <id> path <relative>
        // file <id> embed <name> <byte-length>
        string rest = line["file ".Length..];
        int space = rest.IndexOf(' ');
        if (space <= 0 || !int.TryParse(rest[..space], NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
        {
            return false;
        }

        if (id != _files.Count)
        {
            return false;
        }

        rest = rest[(space + 1)..];
        if (rest.StartsWith("path ", StringComparison.Ordinal))
        {
            string relative = rest["path ".Length..];
            if (relative.Length == 0)
            {
                return false;
            }

            string full = Path.GetFullPath(Path.Combine(_baseDirectory, relative));
            _files.Add(new SourceEntry(relative, full, null));
            return true;
        }

        if (!rest.StartsWith("embed ", StringComparison.Ordinal))
        {
            return false;
        }

        string embed = rest["embed ".Length..];
        int lengthSpace = embed.LastIndexOf(' ');
        if (lengthSpace <= 0
            || !int.TryParse(embed[(lengthSpace + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out int length)
            || length < 0)
        {
            return false;
        }

        string name = embed[..lengthSpace];
        byte[] body = new byte[length];
        int read = 0;
        while (read < length)
        {
            int n = stream.Read(body, read, length - read);
            if (n == 0)
            {
                return false;
            }

            read += n;
        }

        // Separator newline after the embedded body.
        int separator = stream.ReadByte();
        if (separator == '\r')
        {
            separator = stream.ReadByte();
        }

        if (separator != '\n' && separator != -1)
        {
            return false;
        }

        _files.Add(new SourceEntry(name, null, Encoding.UTF8.GetString(body)));
        return true;
    }

    bool TryAddPoint(string line)
    {
        string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length != 4)
        {
            return false;
        }

        int colon = tokens[0].IndexOf(':');
        if (colon <= 0)
        {
            return false;
        }

        if (!byte.TryParse(tokens[0][..colon], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte bank))
        {
            return false;
        }

        if (!ushort.TryParse(tokens[0][(colon + 1)..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort address))
        {
            return false;
        }

        if (!int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int fileId)
            || !int.TryParse(tokens[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int sourceLine))
        {
            return false;
        }

        string kind = tokens[3];
        if (kind is not ("stmt" or "call" or "func"))
        {
            return false;
        }

        if ((uint)fileId >= (uint)_files.Count)
        {
            return false;
        }

        _points.Add(new GblSequencePoint(bank, address, fileId, sourceLine, kind));
        return true;
    }

    static string? ReadLine(Stream stream)
    {
        var bytes = new List<byte>();
        int value;
        while ((value = stream.ReadByte()) >= 0)
        {
            if (value == '\n')
            {
                break;
            }

            if (value != '\r')
            {
                bytes.Add((byte)value);
            }
        }

        if (value < 0 && bytes.Count == 0)
        {
            return null;
        }

        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    sealed class SourceEntry
    {
        public SourceEntry(string name, string? path, string? text)
        {
            Name = name;
            Path = path;
            Text = text;
        }

        public string Name { get; }
        public string? Path { get; }
        public string? Text { get; }
        public string? Loaded { get; set; }
    }
}

public readonly struct GblSequencePoint
{
    public GblSequencePoint(byte bank, ushort address, int fileId, int line, string kind)
    {
        Bank = bank;
        Address = address;
        FileId = fileId;
        Line = line;
        Kind = kind;
    }

    public byte Bank { get; }
    public ushort Address { get; }
    public int FileId { get; }
    public int Line { get; }
    public string Kind { get; }
}
