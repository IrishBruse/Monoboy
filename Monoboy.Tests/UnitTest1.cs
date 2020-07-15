using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Monoboy.Core;
using Xunit;

namespace Monoboy.Tests
{
    public class UnitTest1
    {
        [Theory]
        [InlineData("Tests\\Cpu\\01-special.gb")]
        [InlineData("Tests\\Cpu\\02-interrupts.gb")]
        [InlineData("Tests\\Cpu\\03-op sp,hl.gb")]
        [InlineData("Tests\\Cpu\\04-op r,imm.gb")]
        [InlineData("Tests\\Cpu\\05-op rp.gb")]
        [InlineData("Tests\\Cpu\\06-ld r,r.gb")]
        [InlineData("Tests\\Cpu\\07-jr,jp,call,ret,rst.gb")]
        [InlineData("Tests\\Cpu\\08-misc instrs.gb")]
        [InlineData("Tests\\Cpu\\09-op r,r.gb")]
        [InlineData("Tests\\Cpu\\10-bit ops.gb")]
        [InlineData("Tests\\Cpu\\11-op a,(hl).gb")]
        public void BlarggsCpuTests(string rom)
        {
            Bus emulator = new Bus();
            emulator.SkipBootRom();
            emulator.LoadRom(rom);

            //while(emulator.bus.testOutput == null)
            //{
            //    emulator.Step();
            //}
            //
            //Debug.WriteLine(emulator.bus.testOutput);
            //
            //Assert.True(emulator. < 0);
        }
    }
}
