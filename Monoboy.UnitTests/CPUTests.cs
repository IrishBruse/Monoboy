using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monoboy.Emulator;

namespace Monoboy.UnitTests
{
    [TestClass]
    public class CPUTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            MonoboyEmulator emulator = new MonoboyEmulator();
            emulator.Boot();
            emulator.LoadRom(@"Roms\cpu test.gb");

            //emulator.



        }
    }
}
