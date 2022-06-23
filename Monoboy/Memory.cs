namespace Monoboy;

public class Memory
{
    public byte[] boot;             //  0000-00FF   256 byte boot rom that gets toggled off after execution or Emulator.ReadBootRom
                                    //  0000-3FFF   16KB ROM bank 00        In MemoryBankControllers
                                    //  4000-7FFF   16KB ROM Bank 01~NN     In MemoryBankControllers
    public byte[] vram;             //  8000-9FFF	8KB Video RAM (VRAM)
                                    //  C000-CFFF	4KB Work RAM (WRAM) bank 0
    public byte[] workram;          //  C000-FDFF   4KB Work RAM (WRAM) bank 1~N
                                    //  E000-FDFF	Mirror of C000~DDFF (ECHO RAM)
    public byte[] oam;              //  FE00-FE9F	Sprite attribute table (OAM)
                                    //  FEA0-FEFF	Not Usable
    public byte[] io;               //  FF00-FF7F	I/O Registers
    public byte[] zp;               //  FF80-FFFE	High RAM (HRAM)
    public byte ie;                 //  FFFF-FFFF	Interrupts Enable Register (IE)

    public Memory()
    {
        Reset();
    }

    internal void Reset()
    {
        vram = new byte[8196];
        workram = new byte[8192];
        oam = new byte[160];
        io = new byte[128];
        zp = new byte[127];
        ie = 0;
    }
}