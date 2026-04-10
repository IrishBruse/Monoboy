#nullable enable

namespace Monoboy.Desktop;

using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Monoboy;

/// <summary>
/// Headless mode: same ROM/bootstrap setup as <see cref="TuiDebugger"/>, runs for
/// <c>--steps</c> or <c>--frames</c>, then prints JSON to stdout (<c>cpu</c> always;
/// <c>memory</c> only with <c>--memory START:LENGTH</c> or <c>START,LENGTH</c>).
/// </summary>
public static class TestDebugger
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public static void Run(string[] args)
    {
        var emulator = new Emulator
        {
            LogCartridgeHeader = false,
        };

        string romPath = "";
        foreach (string a in args)
        {
            if (!a.StartsWith("--", StringComparison.Ordinal))
            {
                romPath = a;
                break;
            }
        }
        if (!string.IsNullOrWhiteSpace(romPath) && File.Exists(romPath))
        {
            emulator.Open(File.ReadAllBytes(romPath));
        }
        else
        {
            emulator.Open(new byte[0x10000]);
        }

        int? steps = TryParseFlagInt(args, "--steps");
        if (steps.HasValue)
        {
            for (int i = 0; i < steps.Value; i++)
            {
                emulator.Step();
            }
        }
        else
        {
            int frames = TryParseFlagInt(args, "--frames") ?? 1;
            for (int i = 0; i < frames; i++)
            {
                emulator.StepFrame();
            }
        }

        DebugState s = emulator.GetDebugState();
        var memoryRange = TryParseMemoryRange(args);
        MemorySnapshot? memory = null;
        if (memoryRange.HasValue)
        {
            memory = ReadMemorySnapshot(emulator, memoryRange.Value.Start, memoryRange.Value.Length);
        }

        var report = new TestReport
        {
            SchemaVersion = 1,
            Cpu = CpuSnapshot.From(s),
            Memory = memory,
        };

        Console.Out.WriteLine(JsonSerializer.Serialize(report, JsonOptions));
    }

    static int? TryParseFlagInt(string[] args, string flag)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == flag && int.TryParse(args[i + 1], out int v))
            {
                return v;
            }
        }

        return null;
    }

    static (ushort Start, int Length)? TryParseMemoryRange(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] != "--memory")
            {
                continue;
            }

            string spec = args[i + 1];
            if (!TryParseMemorySpec(spec, out ushort start, out int length))
            {
                throw new ArgumentException(
                    $"Invalid --memory value '{spec}'. Expected START:LENGTH or START,LENGTH (e.g. 0x0000:256).");
            }

            return (start, length);
        }

        return null;
    }

    static bool TryParseMemorySpec(string spec, out ushort start, out int length)
    {
        start = 0;
        length = 0;
        int sep = spec.IndexOfAny([':', ',']);
        if (sep <= 0 || sep >= spec.Length - 1)
        {
            return false;
        }

        string a = spec[..sep].Trim();
        string b = spec[(sep + 1)..].Trim();
        if (!TryParseUInt16Flexible(a, out int startInt) || startInt > 0xFFFF)
        {
            return false;
        }

        if (!TryParseInt32Flexible(b, out int len) || len <= 0)
        {
            return false;
        }

        start = (ushort)startInt;
        long end = (long)start + len;
        if (end > 0x10000)
        {
            len = 0x10000 - start;
        }

        length = len;
        return true;
    }

    static bool TryParseUInt16Flexible(string s, out int value)
    {
        s = s.Trim();
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(s.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    static bool TryParseInt32Flexible(string s, out int value)
    {
        s = s.Trim();
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(s.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    static MemorySnapshot ReadMemorySnapshot(Emulator emulator, ushort start, int length)
    {
        var bytes = new int[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = emulator.Read((ushort)(start + i));
        }

        return new MemorySnapshot { StartAddress = start, Bytes = bytes };
    }

    sealed class TestReport
    {
        public int SchemaVersion { get; init; }
        public CpuSnapshot Cpu { get; init; } = null!;
        public MemorySnapshot? Memory { get; init; }
    }

    sealed class CpuSnapshot
    {
        public int A { get; init; }
        public int F { get; init; }
        public int B { get; init; }
        public int C { get; init; }
        public int D { get; init; }
        public int E { get; init; }
        public int H { get; init; }
        public int L { get; init; }
        public int Af { get; init; }
        public int Bc { get; init; }
        public int De { get; init; }
        public int Hl { get; init; }
        public int Pc { get; init; }
        public int Sp { get; init; }
        public int Ie { get; init; }

        [JsonPropertyName("if")]
        public int InterruptFlag { get; init; }

        public bool Ime { get; init; }
        public bool Halted { get; init; }
        public long TotalCycles { get; init; }

        public static CpuSnapshot From(DebugState s) =>
            new()
            {
                A = s.A,
                F = s.F,
                B = s.B,
                C = s.C,
                D = s.D,
                E = s.E,
                H = s.H,
                L = s.L,
                Af = s.AF,
                Bc = s.BC,
                De = s.DE,
                Hl = s.HL,
                Pc = s.PC,
                Sp = s.SP,
                Ie = s.IE,
                InterruptFlag = s.IF,
                Ime = s.Ime,
                Halted = s.Halted,
                TotalCycles = s.TotalCycles,
            };
    }

    sealed class MemorySnapshot
    {
        public int StartAddress { get; init; }
        public int[] Bytes { get; init; } = null!;
    }
}
