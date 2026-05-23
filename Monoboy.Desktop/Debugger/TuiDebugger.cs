namespace Monoboy.Desktop;

using System;
using System.IO;
using System.Linq;
using System.Threading;

using Monoboy;
using Monoboy.Desktop.Debugger;

/// <summary>
/// Debugger TUI: disassembly, branch preview (at PC row), registers (flush right), memory dump.
/// Resize uses Console size each frame. Tab switches focus between disassembly and memory; arrow keys scroll the focused pane one display line at a time; Page Up/Down jump by ~one screen. B prompts for a hex breakpoint (toggle); R runs until a breakpoint. Ctrl+R reloads the ROM and resets scroll.
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
        var alignment = new DisasmAlignmentCache();
        var breakpoints = new DebuggerBreakpoints();
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

            alignment.Clear();
            breakpoints.Clear();
            RecordStop(alignment, emulator);
        }

        static void RecordStop(DisasmAlignmentCache cache, Emulator emulator)
        {
            DebugState state = emulator.GetDebugState();
            ushort size = TuiDisassemblyFormatter.GetInstructionByteSize(emulator, state.PC);
            cache.RecordStop(emulator.RomBank, state.PC, size);
        }

        static void StepRecorded(Emulator emulator, DisasmAlignmentCache cache, DebuggerBreakpoints breakpoints)
        {
            if (breakpoints.Contains(emulator.GetDebugState().PC))
            {
                RecordStop(cache, emulator);
                return;
            }

            RecordStop(cache, emulator);
            emulator.Step();
        }

        /// <returns>Status text when run stops early without hitting a breakpoint.</returns>
        static string? RunUntilBreakpointRecorded(
            Emulator emulator,
            DisasmAlignmentCache cache,
            DebuggerBreakpoints breakpoints,
            Action redraw)
        {
            const int maxSteps = 2_000_000;
            const int minRedrawIntervalMs = 100;
            long lastRedrawMs = Environment.TickCount64 - minRedrawIntervalMs;

            void MaybeRedraw(bool force)
            {
                long now = Environment.TickCount64;
                if (!force && now - lastRedrawMs < minRedrawIntervalMs)
                {
                    return;
                }

                lastRedrawMs = now;
                redraw();
            }

            for (int i = 0; i < maxSteps; i++)
            {
                DebugState before = emulator.GetDebugState();
                RecordStop(cache, emulator);
                emulator.Step();
                DebugState after = emulator.GetDebugState();

                if (breakpoints.Contains(after.PC))
                {
                    MaybeRedraw(force: true);
                    return null;
                }

                if (after.PC == before.PC)
                {
                    MaybeRedraw(force: true);
                    if (after.Halted)
                    {
                        return "CPU halted - cannot run";
                    }

                    return "PC did not advance";
                }

                MaybeRedraw(force: false);
            }

            MaybeRedraw(force: true);
            return "Breakpoint not reached";
        }

        ReloadRom();

        int disasmLineSkip = 0;
        int memRowSkip = 0;
        var paneFocus = DebuggerPaneFocus.Disassembly;
        var registerLabelDisplay = RegisterLabelDisplay.Name;
        bool showDisasmSymbols = true;
        bool quit = false;
        bool needsRedraw = true;
        string? statusMessage = null;
        int lastTermW = -1;
        int lastTermH = -1;

        try
        {
            TuiTerminalReset.ReleaseMouseCapture();
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

                void DrawCurrentFrame()
                {
                    Console.SetCursorPosition(0, 0);
                    TuiDebuggerView.DrawFrame(
                        emulator,
                        w,
                        h,
                        disasmLineSkip,
                        memRowSkip,
                        paneFocus,
                        registerLabelDisplay,
                        showDisasmSymbols,
                        symbols,
                        alignment,
                        breakpoints,
                        statusMessage);
                }

                if (Console.KeyAvailable)
                {
                    needsRedraw = true;
                    var key = Console.ReadKey(intercept: true);
                    statusMessage = null;
                    int pageJump = Math.Max(8, h - 3);
                    switch (key.Key)
                    {
                        case ConsoleKey.S:
                        StepRecorded(emulator, alignment, breakpoints);
                        disasmLineSkip = 0;
                        break;
                        case ConsoleKey.B:
                        if (TuiHexPrompt.TryReadAddress("Breakpoint hex: ", out ushort bpAddr))
                        {
                            breakpoints.Toggle(bpAddr);
                            disasmLineSkip = 0;
                        }

                        break;
                        case ConsoleKey.R when (key.Modifiers & ConsoleModifiers.Control) != 0:
                        ReloadRom();
                        disasmLineSkip = 0;
                        memRowSkip = 0;
                        break;
                        case ConsoleKey.R:
                        disasmLineSkip = 0;
                        if (breakpoints.Count == 0)
                        {
                            statusMessage = "No breakpoints - press B to add one";
                        }
                        else
                        {
                            statusMessage = RunUntilBreakpointRecorded(
                                emulator,
                                alignment,
                                breakpoints,
                                () => DrawCurrentFrame());
                        }

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

                DrawCurrentFrame();
                needsRedraw = false;
            }
        }
        finally
        {
            TuiTerminalReset.ReleaseMouseCapture();
            Console.CursorVisible = true;
        }

        Console.Clear();
    }
}
