using System;
using System.Security.Permissions;
using System.Threading;

namespace Monoboy.Emulator
{
    public class Monoboy
    {
        const int CYCLES_PER_FRAME = 69905; // For 60 FPS

        public CPU cpu;

        public Monoboy()
        {
            cpu = new CPU();
        }
    }
}
