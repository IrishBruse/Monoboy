using Monoboy.Core;
using Xunit;

namespace Monoboy.Tests
{
    public class Tests
    {
        //[Theory]
        //[InlineData("Tests\\Cpu\\01-special.gb")]
        //[InlineData("Tests\\Cpu\\02-interrupts.gb")]
        //[InlineData("Tests\\Cpu\\03-op sp,hl.gb")]
        //[InlineData("Tests\\Cpu\\04-op r,imm.gb")]
        //[InlineData("Tests\\Cpu\\05-op rp.gb")]
        //[InlineData("Tests\\Cpu\\06-ld r,r.gb")]
        //[InlineData("Tests\\Cpu\\07-jr,jp,call,ret,rst.gb")]
        //[InlineData("Tests\\Cpu\\08-misc instrs.gb")]
        //[InlineData("Tests\\Cpu\\09-op r,r.gb")]
        //[InlineData("Tests\\Cpu\\10-bit ops.gb")]
        //[InlineData("Tests\\Cpu\\11-op a,(hl).gb")]
        //public void BlarggsCpuTests(string rom)
        //{
        //    Emulator emulator = new Emulator();
        //
        //    //while(emulator.bus.testOutput == null)
        //    //{
        //    //    emulator.Step();
        //    //}
        //    //
        //    //Debug.WriteLine(emulator.bus.testOutput);
        //    //
        //    //Assert.True(emulator. < 0);
        //}

        [Fact]
        public void BootRom()
        {
            Emulator emulator = new Emulator();

            while(emulator.bus.register.PC != 0x100)
            {
                emulator.Step();
            }

            bool result =
                emulator.bus.register.AF == 0x01B0 &&
                emulator.bus.register.BC == 0x0013 &&
                emulator.bus.register.DE == 0x00D8 &&
                emulator.bus.register.HL == 0x014D &&
                emulator.bus.register.SP == 0xFFFE &&
                emulator.bus.Read(0xFF05) == 0x00 &&
                emulator.bus.Read(0xFF06) == 0x00 &&
                emulator.bus.Read(0xFF07) == 0x00 &&
                emulator.bus.Read(0xFF10) == 0x80 &&
                emulator.bus.Read(0xFF11) == 0xBF &&
                emulator.bus.Read(0xFF12) == 0xF3 &&
                emulator.bus.Read(0xFF14) == 0xBF &&
                emulator.bus.Read(0xFF16) == 0x3F &&
                emulator.bus.Read(0xFF17) == 0x00 &&
                emulator.bus.Read(0xFF19) == 0xBF &&
                emulator.bus.Read(0xFF1A) == 0x7F &&
                emulator.bus.Read(0xFF1B) == 0xFF &&
                emulator.bus.Read(0xFF1C) == 0x9F &&
                emulator.bus.Read(0xFF1E) == 0xBF &&
                emulator.bus.Read(0xFF20) == 0xFF &&
                emulator.bus.Read(0xFF21) == 0x00 &&
                emulator.bus.Read(0xFF22) == 0x00 &&
                emulator.bus.Read(0xFF23) == 0xBF &&
                emulator.bus.Read(0xFF24) == 0x77 &&
                emulator.bus.Read(0xFF25) == 0xF3 &&
                emulator.bus.Read(0xFF26) == 0xF1 &&
                emulator.bus.Read(0xFF40) == 0x91 &&
                emulator.bus.Read(0xFF42) == 0x00 &&
                emulator.bus.Read(0xFF43) == 0x00 &&
                emulator.bus.Read(0xFF45) == 0x00 &&
                emulator.bus.Read(0xFF47) == 0xFC &&
                emulator.bus.Read(0xFF48) == 0xFF &&
                emulator.bus.Read(0xFF49) == 0xFF &&
                emulator.bus.Read(0xFF4A) == 0x00 &&
                emulator.bus.Read(0xFF4B) == 0x00 &&
                emulator.bus.Read(0xFFFF) == 0x00;
            Assert.True(result);
        }
    }
}
