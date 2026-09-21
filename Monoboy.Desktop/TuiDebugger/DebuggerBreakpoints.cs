namespace Monoboy.Desktop.TuiDebugger;

using System.Collections.Generic;

/// <summary>CPU addresses that stop execution when PC reaches them.</summary>
sealed class DebuggerBreakpoints
{
    readonly HashSet<ushort> addresses = new();

    public int Count => addresses.Count;

    public bool Contains(ushort address) => addresses.Contains(address);

    /// <summary>Add or remove <paramref name="address"/>; returns true if now set.</summary>
    public bool Toggle(ushort address)
    {
        if (addresses.Remove(address))
        {
            return false;
        }

        addresses.Add(address);
        return true;
    }

    public void Clear() => addresses.Clear();
}
