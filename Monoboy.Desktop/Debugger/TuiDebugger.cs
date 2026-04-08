namespace Monoboy.Desktop;

using System;
using System.IO;
using System.Linq;
using System.Threading;

using Monoboy;
using Monoboy.Desktop.Debugger;

/// <summary>
/// Debugger TUI: left = disassembly; center = register grid; right = memory dump (full height, flush right).
/// Resize uses Console size each frame. Tab switches focus between disassembly and memory; arrow keys scroll the focused pane (disassembly can scroll before the current PC); Page Up/Down jump by ~one screen.
/// </summary>
public static class TuiDebugger
{
    public static void Run(string[] args)
    {
        bool logHeader = args.Contains("--log-header");
        var emulator = new Emulator
        {
            LogCartridgeHeader = logHeader
        };

        string romPath = args.FirstOrDefault(x => !x.StartsWith("--", StringComparison.Ordinal)) ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(romPath) && File.Exists(romPath))
        {
            emulator.Open(romPath);
        }
        else
        {
            emulator.Open(new byte[0x10000]);
        }

        int disasmSkip = 0;
        int memRowSkip = 0;
        var paneFocus = DebuggerPaneFocus.Disassembly;
        bool quit = false;
        bool needsRedraw = true;
        int lastTermW = -1;
        int lastTermH = -1;

        try
        {
            Console.CursorVisible = false;
            while (!quit)
            {
                int w = Math.Max(Console.WindowWidth, 40);
                int h = Math.Max(Console.WindowHeight, 12);
                bool resized = w != lastTermW || h != lastTermH;
                if (resized)
                {
                    lastTermW = w;
                    lastTermH = h;
                    needsRedraw = true;
                }

                if (Console.KeyAvailable)
                {
                    needsRedraw = true;
                    var key = Console.ReadKey(intercept: true);
                    int pageJump = Math.Max(8, h - 3);
                    switch (key.Key)
                    {
                        case ConsoleKey.S:
                        emulator.Step();
                        disasmSkip = 0;
                        break;
                        case ConsoleKey.F:
                        emulator.StepFrame();
                        disasmSkip = 0;
                        break;
                        case ConsoleKey.R:
                        for (int i = 0; i < 500; i++)
                        {
                            emulator.Step();
                        }
                        disasmSkip = 0;
                        break;
                        case ConsoleKey.Q:
                        quit = true;
                        break;
                        case ConsoleKey.Tab:
                        paneFocus = paneFocus == DebuggerPaneFocus.Disassembly
                            ? DebuggerPaneFocus.Memory
                            : DebuggerPaneFocus.Disassembly;
                        break;
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.UpArrow:
                        if (paneFocus == DebuggerPaneFocus.Disassembly)
                        {
                            disasmSkip--;
                        }
                        else
                        {
                            memRowSkip = Math.Max(0, memRowSkip - 1);
                        }

                        break;
                        case ConsoleKey.RightArrow:
                        case ConsoleKey.DownArrow:
                        if (paneFocus == DebuggerPaneFocus.Disassembly)
                        {
                            disasmSkip++;
                        }
                        else
                        {
                            memRowSkip++;
                        }

                        break;
                        case ConsoleKey.PageDown:
                        if (paneFocus == DebuggerPaneFocus.Disassembly)
                        {
                            disasmSkip += pageJump;
                        }
                        else
                        {
                            memRowSkip += pageJump;
                        }

                        break;
                        case ConsoleKey.PageUp:
                        if (paneFocus == DebuggerPaneFocus.Disassembly)
                        {
                            disasmSkip -= pageJump;
                        }
                        else
                        {
                            memRowSkip = Math.Max(0, memRowSkip - pageJump);
                        }

                        break;
                        case ConsoleKey.Home:
                        disasmSkip = 0;
                        memRowSkip = 0;
                        break;
                        case ConsoleKey.P:
                        FramebufferPreviewWindow.Show(emulator);
                        break;
                        default:
                        break;
                    }
                }
                else if (!needsRedraw)
                {
                    Thread.Sleep(50);
                    continue;
                }

                if (quit)
                {
                    break;
                }

                if (resized)
                {
                    Console.Clear();
                }
                else
                {
                    Console.SetCursorPosition(0, 0);
                }

                TuiDebuggerView.DrawFrame(emulator, w, h, disasmSkip, memRowSkip, paneFocus);
                needsRedraw = false;
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }

        Console.Clear();
    }
}
