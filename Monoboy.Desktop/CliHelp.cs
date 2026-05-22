namespace Monoboy.Desktop;

using System;

static class CliHelp
{
    public static bool WantsHelp(string[] args) =>
        args.Contains("-h") || args.Contains("--help");

    public static void Print()
    {
        string name = "monoboy";
        Console.WriteLine($"""
            Monoboy - Game Boy emulator

            Usage:
              {name} [options] [rom.gb]
              {name} --debug [options] [rom.gb]
              {name} --test [options] [rom.gb]

            Modes:
              (default)   Raylib window (graphical emulator)
              --debug     Terminal UI debugger
              --test      Headless run; prints one JSON object on stdout

            Options:
              -h, --help       Show this help and exit
              --log-header     Print cartridge header when opening a ROM (TUI and graphical)
              --custom-boot    Use embedded bootix boot ROM instead of built-in boot

            --test options:
              --steps N        Run N CPU steps (--frames ignored if set)
              --frames N       Run N frames (default 1 if --steps not set)
              --memory SPEC    Include bus snapshot: START:LENGTH or START,LENGTH (hex ok)

            ROM path is the first argument that does not start with --.
            If omitted or missing, a 64 KiB empty buffer is used.

            Examples:
              {name} game.gb
              {name} --debug --log-header game.gb
              {name} --test game.gb --steps 5000
              {name} --test game.gb --frames 10 --memory 0xC000:256
            """);
    }
}
