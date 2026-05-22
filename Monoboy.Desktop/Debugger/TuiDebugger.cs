namespace Monoboy.Desktop;

using System;
using System.IO;
using System.Linq;
using System.Threading;

using Monoboy;
using Monoboy.Desktop.Debugger;

/// <summary>
/// Debugger TUI: left = disassembly; center = register grid; right = memory dump (full height, flush right).
/// Resize uses Console size each frame. Tab switches focus between disassembly and memory; arrow keys scroll the focused pane one display line at a time; Page Up/Down jump by ~one screen. V runs to VBlank. Ctrl+R reloads the ROM and resets scroll.
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
        SymSymbolMap? symbols = null;
        void ReloadRom()
        {
            if (!string.IsNullOrWhiteSpace(romPath) && File.Exists(romPath))
            {
                emulator.Open(romPath);
                symbols = SymSymbolMap.TryLoadForRom(romPath);
            }
            else
            {
                emulator.Open(new byte[0x10000]);
                symbols = null;
            }
        }

        ReloadRom();

        int disasmLineSkip = 0;
        int memRowSkip = 0;
        var paneFocus = DebuggerPaneFocus.Disassembly;
        var registerLabelDisplay = RegisterLabelDisplay.Name;
        bool showDisasmSymbols = true;
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
                        disasmLineSkip = 0;
                        break;
                        case ConsoleKey.F:
                        emulator.StepFrame();
                        disasmLineSkip = 0;
                        break;
                        case ConsoleKey.V:
                        emulator.RunUntilVBlank();
                        disasmLineSkip = 0;
                        break;
                        case ConsoleKey.R when (key.Modifiers & ConsoleModifiers.Control) != 0:
                        ReloadRom();
                        disasmLineSkip = 0;
                        memRowSkip = 0;
                        break;
                        case ConsoleKey.R:
                        for (int i = 0; i < 500; i++)
                        {
                            emulator.Step();
                        }
                        disasmLineSkip = 0;
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
                            disasmLineSkip--;
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
                            disasmLineSkip++;
                        }
                        else
                        {
                            memRowSkip++;
                        }

                        break;
                        case ConsoleKey.PageDown:
                        if (paneFocus == DebuggerPaneFocus.Disassembly)
                        {
                            disasmLineSkip += pageJump;
                        }
                        else
                        {
                            memRowSkip += pageJump;
                        }

                        break;
                        case ConsoleKey.PageUp:
                        if (paneFocus == DebuggerPaneFocus.Disassembly)
                        {
                            disasmLineSkip -= pageJump;
                        }
                        else
                        {
                            memRowSkip = Math.Max(0, memRowSkip - pageJump);
                        }

                        break;
                        case ConsoleKey.Home:
                        disasmLineSkip = 0;
                        memRowSkip = 0;
                        break;
                        case ConsoleKey.A:
                        registerLabelDisplay = registerLabelDisplay == RegisterLabelDisplay.Address
                            ? RegisterLabelDisplay.Name
                            : RegisterLabelDisplay.Address;
                        showDisasmSymbols = !showDisasmSymbols;
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

                TuiDebuggerView.DrawFrame(
                    emulator, w, h, disasmLineSkip, memRowSkip, paneFocus, registerLabelDisplay, showDisasmSymbols, symbols);
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
