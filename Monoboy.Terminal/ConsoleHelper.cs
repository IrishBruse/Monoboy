namespace Monoboy.Terminal;

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
#pragma warning disable
public static class ConsoleHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern Int32 SetCurrentConsoleFontEx(IntPtr ConsoleOutput, bool MaximumWindow, ref CONSOLE_FONT_INFO_EX ConsoleCurrentFontEx);

    [DllImport("kernel32")]
    public static extern IntPtr GetStdHandle(StdHandle index);


    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern SafeFileHandle CreateFile(string fileName, [MarshalAs(UnmanagedType.U4)] uint fileAccess, [MarshalAs(UnmanagedType.U4)] uint fileShare, IntPtr securityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition, [MarshalAs(UnmanagedType.U4)] int flags, IntPtr template);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteConsoleOutputW(SafeFileHandle hConsoleOutput, CharInfo[] lpBuffer, Coord dwBufferSize, Coord dwBufferCoord, ref SmallRect lpWriteRegion);




}
[StructLayout(LayoutKind.Sequential)]
public struct COORD
{
    public short X;
    public short Y;

    public COORD(short X, short Y)
    {
        this.X = X;
        this.Y = Y;
    }
};
public enum StdHandle
{
    OutputHandle = -11
}
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct CONSOLE_FONT_INFO_EX
{
    public uint cbSize;
    public uint nFont;
    public COORD dwFontSize;
    public int FontFamily;
    public int FontWeight;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] // Edit sizeconst if the font name is too big
    public string FaceName;
}
[StructLayout(LayoutKind.Sequential)]
public struct Coord
{
    public short X;
    public short Y;

    public Coord(short X, short Y)
    {
        this.X = X;
        this.Y = Y;
    }
};
[StructLayout(LayoutKind.Explicit)]
public struct CharUnion
{
    [FieldOffset(0)] public ushort UnicodeChar;
    [FieldOffset(0)] public byte AsciiChar;
}

[StructLayout(LayoutKind.Explicit)]
public struct CharInfo
{
    [FieldOffset(0)] public CharUnion Char;
    [FieldOffset(2)] public short Attributes;
}

[StructLayout(LayoutKind.Sequential)]
public struct SmallRect
{
    public short Left;
    public short Top;
    public short Right;
    public short Bottom;
}


#pragma warning restore
