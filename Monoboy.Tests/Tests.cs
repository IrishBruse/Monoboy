namespace Monoboy.Tests;

using Xunit;

public class Tests
{
    [Fact]
    public void BootRom()
    {
        Emulator emulator = new();
        emulator.Reset();

        while (emulator.register.PC != 0x100)
        {
            _ = emulator.Step();
        }

        Assert.True(emulator.register.AF == 0x01B0);
        Assert.True(emulator.register.BC == 0x0013);
        Assert.True(emulator.register.DE == 0x00D8);
        Assert.True(emulator.register.HL == 0x014D);
        Assert.True(emulator.register.SP == 0xFFFE);
        Assert.True(emulator.Read(0xFF05) == 0x00);
        Assert.True(emulator.Read(0xFF06) == 0x00);
        Assert.True(emulator.Read(0xFF07) == 0x00);
        Assert.True(emulator.Read(0xFF10) == 0x80);
        Assert.True(emulator.Read(0xFF11) == 0xBF);
        Assert.True(emulator.Read(0xFF12) == 0xF3);
        Assert.True(emulator.Read(0xFF14) == 0xBF);
        Assert.True(emulator.Read(0xFF16) == 0x3F);
        Assert.True(emulator.Read(0xFF17) == 0x00);
        Assert.True(emulator.Read(0xFF19) == 0xBF);
        Assert.True(emulator.Read(0xFF1A) == 0x7F);
        Assert.True(emulator.Read(0xFF1B) == 0xFF);
        Assert.True(emulator.Read(0xFF1C) == 0x9F);
        Assert.True(emulator.Read(0xFF1E) == 0xBF);
        Assert.True(emulator.Read(0xFF20) == 0xFF);
        Assert.True(emulator.Read(0xFF21) == 0x00);
        Assert.True(emulator.Read(0xFF22) == 0x00);
        Assert.True(emulator.Read(0xFF23) == 0xBF);
        Assert.True(emulator.Read(0xFF24) == 0x77);
        Assert.True(emulator.Read(0xFF25) == 0xF3);
        Assert.True(emulator.Read(0xFF26) == 0xF1);
        Assert.True(emulator.Read(0xFF40) == 0x91);
        Assert.True(emulator.Read(0xFF42) == 0x00);
        Assert.True(emulator.Read(0xFF43) == 0x00);
        Assert.True(emulator.Read(0xFF45) == 0x00);
        Assert.True(emulator.Read(0xFF47) == 0xFC);
        Assert.True(emulator.Read(0xFF48) == 0xFF);
        Assert.True(emulator.Read(0xFF49) == 0xFF);
        Assert.True(emulator.Read(0xFF4A) == 0x00);
        Assert.True(emulator.Read(0xFF4B) == 0x00);
        Assert.True(emulator.Read(0xFFFF) == 0x00);
    }
}
