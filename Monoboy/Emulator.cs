namespace Monoboy;

using System;
using System.IO;
using Monoboy.Cartridge;

public class Emulator
{
    public const int CpuCyclesPerSecond = 4194304;
    public const int CyclesPerFrame = 69905;
    public const byte WindowWidth = 160;
    public const byte WindowHeight = 144;

    public long cyclesRan;
    public bool paused = true;

    string openRom;

    public bool BackgroundEnabled = true;
    public bool WindowEnabled = true;
    public bool SpritesEnabled = true;
    public bool WidescreenEnabled;
    public bool UseBootRom;

    // Hardware
    public Interrupt interrupt;
    public Register register;
    public Memory memory;
    public IMemoryBankController memoryBankController;
    public Cpu cpu;
    public Ppu ppu;
    public Joypad joypad;
    public Timer timer;
    bool biosEnabled;

    public Emulator()
    {
        register = new Register();
        memory = new Memory();

        timer = new Timer(this);
        cpu = new Cpu(this);
        ppu = new Ppu(this);
        joypad = new Joypad(this);
        interrupt = new Interrupt(this);

        Reset();
    }

    public byte Step()
    {
        byte cycles = cpu.Step();
        cyclesRan += cycles;

        timer.Step(cycles);
        ppu.Step(cycles);

        interrupt.HandleInterupts();

        // Disable the bios in the bus
        if (biosEnabled && register.PC >= 0x100)
        {
            biosEnabled = false;
        }

        return cycles;
    }

    public void Open()
    {
        Open(openRom);
    }

    public void Open(string rom)
    {
        openRom = rom;

        Reset();

        paused = false;

        byte cartridgeType;
        using (BinaryReader reader = new(new FileStream(openRom, FileMode.Open)))
        {
            reader.BaseStream.Seek(0x147, SeekOrigin.Begin);
            cartridgeType = reader.ReadByte();
        }

        Console.WriteLine(cartridgeType);

        memoryBankController = cartridgeType switch
        {
            byte when cartridgeType <= 0x00 => new MemoryBankController0(),
            byte when cartridgeType <= 0x03 => new MemoryBankController1(),
            byte when cartridgeType <= 0x06 => new MemoryBankController2(),
            byte when cartridgeType <= 0x13 => new MemoryBankController3(),
            byte when cartridgeType <= 0x1E => new MemoryBankController5(),
            byte when cartridgeType <= 0x20 => new MemoryBankController6(),
            _ => throw new NotImplementedException()
        };

        memoryBankController.Load(openRom);

        string saveFile = openRom.Replace(".gb", ".sav");
        if (File.Exists(saveFile))
        {
            memoryBankController.SetRam(File.ReadAllBytes(saveFile));
        }
    }

    public void Reset()
    {
        register.Reset();
        memory.Reset();

        timer.Reset();
        cpu.Reset();
        ppu.Reset();
        joypad.Reset();
        interrupt.Reset();

        if (File.Exists("dmg_boot.bin") && UseBootRom)
        {
            memory.boot = File.ReadAllBytes("dmg_boot.bin");
            biosEnabled = true;
        }
        else
        {
            SkipBootRom();
        }
    }

    public void SkipBootRom()
    {
        biosEnabled = false;
        register.AF = 0x01B0;
        register.BC = 0x0013;
        register.DE = 0x00D8;
        register.HL = 0x014D;
        register.SP = 0xFFFE;
        register.PC = 0x100;
        Write(0xFF05, 0x00);
        Write(0xFF06, 0x00);
        Write(0xFF07, 0x00);
        Write(0xFF10, 0x80);
        Write(0xFF11, 0xBF);
        Write(0xFF12, 0xF3);
        Write(0xFF14, 0xBF);
        Write(0xFF16, 0x3F);
        Write(0xFF17, 0x00);
        Write(0xFF19, 0xBF);
        Write(0xFF1A, 0x7F);
        Write(0xFF1B, 0xFF);
        Write(0xFF1C, 0x9F);
        Write(0xFF1E, 0xBF);
        Write(0xFF20, 0xFF);
        Write(0xFF21, 0x00);
        Write(0xFF22, 0x00);
        Write(0xFF23, 0xBF);
        Write(0xFF24, 0x77);
        Write(0xFF25, 0xF3);
        Write(0xFF26, 0xF1);
        Write(0xFF40, 0x91);
        Write(0xFF42, 0x00);
        Write(0xFF43, 0x00);
        Write(0xFF45, 0x00);
        Write(0xFF47, 0xFC);
        Write(0xFF48, 0xFF);
        Write(0xFF49, 0xFF);
        Write(0xFF4A, 0x00);
        Write(0xFF4B, 0x00);
        Write(0xFFFF, 0x00);
    }

    #region Settings

    public void ToggleWidescreen()
    {
        throw new NotImplementedException();
        // if (WidescreenEnabled == true)
        // {
        //     WindowWidth = 248;
        // }
        // else
        // {
        //     WindowWidth = 160;
        // }
        // ppu.framebuffer = new Framebuffer(WindowWidth, WindowHeight);
    }

    #endregion

    #region Bus

    public byte Read(ushort address)
    {
        return address switch
        {
            ushort when address <= 0x00FF && biosEnabled => memory.boot[address], // Boot rom only enabled while pc has not reached 0x100 yet
            ushort when address <= 0x3FFF => memoryBankController.ReadBank00(address),    // 16KB ROM Bank = 00 (in cartridge, fixed at bank 00)
            ushort when address <= 0x7FFF => memoryBankController.ReadBankNN(address),    // 16KB ROM Bank > 00 (in cartridge, every other bank)
            ushort when address <= 0x9FFF => memory.vram[address - 0x8000],               // 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            ushort when address <= 0xBFFF => memoryBankController.ReadRam(address),       // 8KB External RAM (in cartridge, switchable bank, if any)
            ushort when address <= 0xDFFF => memory.workram[address - 0xC000],            // 4KB Work RAM Bank 0 (WRAM) (switchable bank 1-7 in CGB Mode)
            ushort when address <= 0xFDFF => memory.workram[address - 0xE000],            // Same as C000-DDFF (ECHO) (typically not used)
            ushort when address <= 0xFE9F => memory.oam[address - 0xFE00],                // Sprite Attribute Table (OAM)
            ushort when address <= 0xFEFF => 0x00,                                        // Not Usable
            ushort when address <= 0xFF7F => ReadIO(address),                             // I/O Ports
            ushort when address <= 0xFFFE => memory.zp[address - 0xFF80],                 // Zero Page RAM
            ushort when address <= 0xFFFF => memory.ie,                                   // Interrupts Enable Register (IE)
            _ => 0xFF,
        };
    }

    public ushort ReadShort(ushort address)
    {
        return (ushort)((Read((ushort)(address + 1)) << 8) | Read(address));
    }

    public void Write(ushort address, byte data)
    {
        switch (address)
        {
            case ushort when address <= 0x7FFF:
                memoryBankController.WriteBank(address, data);
                break;     // 16KB ROM Bank > 00 (in cartridge, every other bank)
            case ushort when address <= 0x9FFF:
                memory.vram[address - 0x8000] = data;
                break;              // 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            case ushort when address <= 0xBFFF:
                memoryBankController.WriteRam(address, data);
                break;      // 8KB External RAM (in cartridge, switchable bank, if any)
            case ushort when address <= 0xDFFF:
                memory.workram[address - 0xC000] = data;
                break;           // 4KB Work RAM Bank 0 (WRAM) (switchable bank 1-7 in CGB Mode)
            case ushort when address <= 0xFDFF:
                memory.workram[address - 0xE000] = data;
                break;           // Same as C000-DDFF (ECHO) (typically not used)
            case ushort when address <= 0xFE9F:
                memory.oam[address - 0xFE00] = data;
                break;               // Sprite Attribute Table (OAM)
            case ushort when address <= 0xFEFF:
                break;                                                    // Not Usable
            case ushort when address <= 0xFF7F:
                WriteIO(address, data);
                break;                            // I/O Ports
            case ushort when address <= 0xFFFE:
                memory.zp[address - 0xFF80] = data;
                break;                // Zero Page RAM
            case ushort when address <= 0xFFFF:
                memory.ie = data;
                break;                                  // Zero Page RAM
            default:
                break;
        }
    }

    public void WriteShort(ushort address, ushort data)
    {
        Write((ushort)(address + 1), (byte)(data >> 8));
        Write(address, (byte)data);
    }

    byte ReadIO(ushort address)
    {
        switch (address)
        {
            case 0xFF00:// JOYP
            {
                return joypad.JOYP;
            }

            case 0xFF04:// DIV
            {
                return timer.DIV;
            }

            default:
            {
                return memory.io[address - 0xFF00];
            }
        }
    }

    void WriteIO(ushort address, byte data)
    {
        switch (address)
        {
            case 0xFF00:// JOYP
            {
                joypad.JOYP = data;
            }
            break;

            case 0xFF04:// DIV
            {
                timer.DIV = data;// is set to zero
            }
            break;

            case 0xFF44:// LY
            {
                data = 0;
            }
            break;

            case 0xFF46:
            {
                ushort offset = (ushort)(data << 8);
                for (byte i = 0; i < memory.oam.Length; i++)
                {
                    memory.oam[i] = Read((ushort)(offset + i));
                }
            }
            break;
            default:
                break;

                //case 0xFF02:
                //{
                //    if(data == 0x81)
                //    {
                //        Debug.Write(Convert.ToChar(Read(0xFF01)));
                //    }
                //}
                //break;
        }

        memory.io[address - 0xFF00] = data;
    }

    #endregion
}
