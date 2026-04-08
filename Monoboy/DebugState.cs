namespace Monoboy;

public sealed class DebugState
{
    public required byte A { get; init; }
    public required byte F { get; init; }
    public required byte B { get; init; }
    public required byte C { get; init; }
    public required byte D { get; init; }
    public required byte E { get; init; }
    public required byte H { get; init; }
    public required byte L { get; init; }
    public required ushort AF { get; init; }
    public required ushort BC { get; init; }
    public required ushort DE { get; init; }
    public required ushort HL { get; init; }
    public required ushort PC { get; init; }
    public required ushort SP { get; init; }
    public required byte IE { get; init; }
    public required byte IF { get; init; }
    public required bool Ime { get; init; }
    public required bool Halted { get; init; }
    public required long TotalCycles { get; init; }
}
