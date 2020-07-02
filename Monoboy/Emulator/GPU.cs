using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monoboy.Emulator.Utility;

namespace Monoboy.Emulator
{
    public class GPU
    {
        public int clock;

        Mode mode = Mode.Hblank;

        public Memory memory;
        public Interrupt interrupt;
        public Texture2D screen;

        Color[] pallet = new Color[] { new Color(0), new Color(76), new Color(107), new Color(255) };

        public void Step(int cycles)
        {
            clock += cycles;

            switch(mode)
            {
                case Mode.OAM:
                if(clock >= 80)
                {
                    clock = 0;
                    mode = Mode.VRAM;
                }
                break;

                case Mode.VRAM:
                if(clock >= 172)
                {
                    clock = 0;
                    mode = Mode.Hblank;

                    DrawScanline();
                }
                break;

                case Mode.Hblank:
                if(clock >= 204)
                {
                    clock = 0;
                    memory.Scanline++;

                    if(memory.Scanline == 143)
                    {
                        mode = Mode.Vblank;
                        // Draw to screen
                    }
                    else
                    {
                        mode = Mode.OAM;
                    }
                }
                break;

                case Mode.Vblank:
                if(clock >= 456)
                {
                    clock = 0;
                    memory.Scanline++;

                    if(memory.Scanline > 153)
                    {
                        memory.Scanline = 0;
                        mode = Mode.OAM;
                    }
                }
                break;
            }
        }

        void DrawScanline()
        {

        }
    }
}