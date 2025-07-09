namespace Monoboy;

using System;
using System.IO;
using System.Text;

using Monoboy.Constants;
using Monoboy.MemoryBankControllers;
using Monoboy.Utility;

public class Emulator
{
    public const int CyclesPerFrame = 70224 / 4; // 17556 M-Cycles
    public const byte WindowWidth = 160;
    public const byte WindowHeight = 144;

    /// <summary> #AABBGGRR </summary>
    public byte[] Framebuffer { get; set; }
    public bool SkipBios { get; }

    // Hardware
    Register Registers { get; set; }
    Memory Memory { get; set; }
    IMemoryBankController mbc;
    bool biosEnabled = true;
    Cpu cpu;
    Ppu ppu;
    Joypad joypad;
    Timer timer;

    CartridgeHeader? cartridge;

    byte[] bios;

    internal byte IF
    {
        get => Read(0xFF0F);
        set
        {
            // Console.WriteLine("IF " + value.ToString("B8"));
            Write(0xFF0F, value);
        }
    }
    internal byte IE
    {
        get => Read(0xFFFF);

        set
        {
            // Console.WriteLine("IE " + value.ToString("B8"));
            Write(0xFFFF, value);
        }
    }

    /// <summary> Machine Cycles </summary>
    internal long TotalCycles { get; set; }

    public Emulator(byte[] bios = null)
    {
        if (bios == null)
        {
            SkipBios = true;
        }
        else
        {
            this.bios = bios;
        }

        Memory = new Memory(0x10000);
        Framebuffer = new byte[WindowWidth * WindowHeight * 4];

        Registers = new Register();

        cpu = new Cpu(Registers, this);
        timer = new Timer(Memory, cpu);
        ppu = new Ppu(Memory, this, cpu, Framebuffer);
        joypad = new Joypad(cpu);

        Reset();
    }

    public void Step()
    {
        if (cartridge == null)
        {
            return;
        }

        byte enabledFlags = (byte)(IE & IF);

        // Interrupt Handling
        if (cpu.Ime && enabledFlags != 0)
        {
            for (int i = 0; i <= 5; i++)
            {
                if (((enabledFlags >> i) & 1) == 1)
                {
                    cpu.Ime = false;

                    // https://gbdev.io/pandocs/Interrupts.html#interrupt-handling

                    // Two wait states are executed (2 M-cycles pass while nothing happens; presumably the CPU is executing nops during this time).
                    Tick(2);

                    // The current value of the PC register is pushed onto the stack, consuming 2 more M-cycles.
                    cpu.Push(Registers.PC);
                    Tick(2);

                    // The PC register is set to the address of the handler (one of: $40, $48, $50, $58, $60). This consumes one last M-cycle.
                    Registers.PC = (ushort)(0x40 + (0x8 * i));
                    Tick(1);

                    IF = IF.SetBit((byte)(1 << i), false);

                    break;
                }
            }

        }
        else if (cpu.Halted && enabledFlags != 0) // Halt Handling
        {
            Console.WriteLine("Halted");
            if (IF != 0)
            {
                Registers.PC++;
                cpu.Halted = false;
            }
        }
        else
        {
            byte op = cpu.NextByte();
            Tick(cpu.Execute(op));
        }


        // Disable the bios/boot rom in the bus
        if (biosEnabled && Registers.PC >= 0x100)
        {
            biosEnabled = false;
        }
    }

    internal void Tick(int mCycles)
    {
        timer.Step(mCycles);
        ppu.Step(mCycles);
        TotalCycles += mCycles;
    }

    public void StepFrame()
    {
        if (cartridge == null)
        {
            return;
        }

        while (!EnteredVSync)
        {
            Step();
        }
        EnteredVSync = false;
    }

    FileSystemWatcher watcher;

    public void Watch(string file)
    {
        Open(file);

        // Create a new FileSystemWatcher and set its properties.
        watcher = new(Path.GetDirectoryName(file))
        {
            NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Attributes,
            EnableRaisingEvents = true,
            Filter = Path.GetFileName(file),
        };

        // Add event handlers.
        watcher.Changed += (s, e) =>
        {
            Console.WriteLine("test " + e.FullPath);
            Open(e.FullPath);
        };
    }

    public void Open(string path)
    {
        Console.WriteLine("Opening ROM " + path);
        Open(File.ReadAllBytes(path));
    }

    public void Open(byte[] data)
    {
        cartridge = new();

        Reset();

        cartridge.EntryPoint = data[0x100..0x104];
        cartridge.NintendoLogo = data[0x104..0x133];
        cartridge.Title = Encoding.ASCII.GetString(data[0x134..0x13E]);
        cartridge.ManufacturerCode = Encoding.ASCII.GetString(data[0x13F..0x142]);
        cartridge.CGB = data[0x143];
        cartridge.LicenseCode = Encoding.ASCII.GetString(data[0x144..0x145]);
        cartridge.SGB = data[0x146];
        cartridge.CartridgeType = data[0x147];
        cartridge.RomSize = data[0x148];
        cartridge.RamSize = data[0x149];
        cartridge.DestinationCode = data[0x14A];
        cartridge.OldLicenseeCode = data[0x14B];
        cartridge.Version = data[0x14C];

        // https://gbdev.io/pandocs/MBCs#mbcs

        // https://gbdev.io/pandocs/The_Cartridge_Header#0147--cartridge-type
        mbc = cartridge.CartridgeType switch
        {
            0x00 => new NoMemoryBankController(),   // ROM ONLY
            0x01 => new MemoryBankController1(),    // MBC1 https://gbdev.io/pandocs/MBC1
            0x02 => new MemoryBankController1(),    // MBC1+RAM
            0x03 => new MemoryBankController1(),    // MBC1+RAM+BATTERY
            0x05 => new MemoryBankController2(),    // MBC2 https://gbdev.io/pandocs/MBC2
            0x06 => new MemoryBankController2(),    // MBC2+BATTERY
            0x08 => new MemoryBankController2(),    // ROM+RAM
            0x09 => new MemoryBankController2(),    // ROM+RAM+BATTERY
            0x0B => new MemoryBankController2(),    // MMM01 https://gbdev.io/pandocs/MMM01
            0x0C => new MemoryBankController2(),    // MMM01+RAM
            0x0D => new MemoryBankController2(),    // MMM01+RAM+BATTERY
            0x0F => new MemoryBankController3(),    // MBC3+TIMER+BATTERY
            0x10 => new MemoryBankController3(),    // MBC3+TIMER+RAM+BATTERY 2
            0x11 => new MemoryBankController3(),    // MBC3 https://gbdev.io/pandocs/MBC3
            0x12 => new MemoryBankController3(),    // MBC3+RAM 2
            0x13 => new MemoryBankController3(),    // MBC3+RAM+BATTERY 2
            0x19 => new MemoryBankController5(),    // MBC5 https://gbdev.io/pandocs/MBC5
            0x1A => new MemoryBankController5(),    // MBC5+RAMkl,
            0x1B => new MemoryBankController5(),    // MBC5+RAM+BATTERY
            0x1C => new MemoryBankController5(),    // MBC5+RUMBLE
            0x1D => new MemoryBankController5(),    // MBC5+RUMBLE+RAM
            0x1E => new MemoryBankController5(),    // MBC5+RUMBLE+RAM+BATTERY
            0x20 => new MemoryBankController6(),    // MBC6 https://gbdev.io/pandocs/MBC6
            0x22 => new NoMemoryBankController(),   // MBC7+SENSOR+RUMBLE+RAM+BATTERY
            0xFC => new NoMemoryBankController(),   // POCKET CAMERA
            0xFD => new NoMemoryBankController(),   // BANDAI TAMA5
            0xFE => new NoMemoryBankController(),   // HuC3
            0xFF => new NoMemoryBankController(),   // HuC1+RAM+BATTERY
            _ => new NoMemoryBankController(),
        };

        mbc.Load(data);

        Console.WriteLine(cartridge.ToString());
    }

    public void Reset()
    {
        for (int x = 0; x < WindowWidth; x++)
        {
            for (int y = 0; y < WindowHeight; y++)
            {
                int i = x + (y * WindowWidth);

                uint color = Pallet.GetColor(0);
                int index = i;
                Framebuffer[(index * 4) + 0] = (byte)color;
                Framebuffer[(index * 4) + 1] = (byte)((color & 0xff00) >> 8);
                Framebuffer[(index * 4) + 2] = (byte)((color & 0xff0000) >> 16);
                Framebuffer[(index * 4) + 3] = 255;
            }
        }

        Registers.Reset();

        // Cpu and Interrupt
        cpu.Halted = false;
        cpu.HaltBug = false;
        biosEnabled = true;
        cpu.Ime = false;

        timer.Reset();
        ppu.Reset();
        joypad.Reset();
        Memory.Reset();

        if (SkipBios)
        {
            SkipBootRom();
        }
    }

    public void SetButtonState(GameboyButton button, bool pressed)
    {
        joypad.SetButton(button, pressed);
    }

    public byte Read(ushort address)
    {
        if (biosEnabled)
        {
            if (bios.Length > 0 && address <= 0x00FF)
            {
                return bios[address];
            }

            if (CustomCartridgeLogo && address >= 0x0104 && address <= 0x0133)
            {
                // Custom Cartridge Header Monoboy logo for bootix boot rom
                return new byte[]{
                    0b11001110,0b11101101,
                    0b00110111,0b01111011,
                    0b00000000,0b00110110,
                    0b00000000,0b11000110,
                    0b00000000,0b11011110,
                    0b00000000,0b10001101,
                    0b00000000,0b11111001,
                    0b00110011,0b00111011,
                    0b00000000,0b11100011,
                    0b00000000,0b00110110,
                    0b00000000,0b11000110,
                    0b00000000,0b11011101,
                    0b11011100,0b11000000,
                    0b10110011,0b00110000,
                    0b01100110,0b00110000,
                    0b01100110,0b11000000,
                    0b11001100,0b11000000,
                    0b11011101,0b11000000,
                    0b10011001,0b11110000,
                    0b10111011,0b00110000,
                    0b00110011,0b11100000,
                    0b01100110,0b00110000,
                    0b01100110,0b11000000,
                    0b11010111,0b00111110,
                }
                [address - 0x0104];
            }
        }

        return address switch
        {
            <= 0x3FFF => mbc.ReadBank00(address),       // 16 KiB ROM bank 00
            <= 0x7FFF => mbc.ReadBankNN(address),       // 16 KiB ROM Bank 01~NN
            <= 0x9FFF => Memory[address],                     // 8 KiB Video RAM (VRAM)
            <= 0xBFFF => mbc.ReadRam(address),          // 8 KiB External RAM
            <= 0xCFFF => Memory[address],                     // 4 KiB Work RAM (WRAM)
            <= 0xDFFF => Memory[address],                     // 4 KiB Work RAM (WRAM)
            <= 0xFDFF => Memory[address - 0x2000],            // Mirror of C000~DDFF (ECHO RAM)
            <= 0xFE9F => Memory[address],                     // Sprite attribute table (OAM)
            <= 0xFEFF => 0x00,                                // Not Usable
            <= 0xFF7F => ReadIO(address),                     // I/O Registers
            <= 0xFFFE => Memory[address],                     // High RAM (HRAM)
            0xFFFF => Memory[address],                         // Interrupt Enable register (IE)
        };
    }

    public void Write(ushort address, byte data)
    {
        switch (address)
        {
            case <= 0x7FFF: mbc.WriteBank(address, data); break;           // 16KB ROM Bank > 00 (in cartridge, every other bank)
            case <= 0x9FFF: Memory[address] = data; break;                  // 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            case <= 0xBFFF: mbc.WriteRam(address, data); break;            // 8KB External RAM (in cartridge, switchable bank, if any)
            case <= 0xDFFF: Memory[address] = data; break;                  // 4KB Work RAM Bank 0 (WRAM) (switchable bank 1-7 in CGB Mode)
            case <= 0xFDFF: Memory[address] = data; break;                  // Same as C000-DDFF (ECHO) (typically not used)
            case <= 0xFE9F: Memory[address] = data; break;                  // Sprite Attribute Table (OAM)
            case <= 0xFEFF: break;                                          // Not Usable
            case <= 0xFF7F: WriteIO(address, data); break;                  // I/O Ports
            case <= 0xFFFE: Memory[address] = data; break;                  // Zero Page RAM
            case <= 0xFFFF: Memory[address] = data; break;                   // Interrupt Enable register (IE)
            default:
        }
    }

    byte ReadIO(ushort address)
    {
        return address switch
        {
            0xFF00 => joypad.JOYP,                       // Joypad input
            >= 0xFF01 and <= 0xFF02 => Memory[address],  // Serial transfer
            0xFF03 => Memory[address],                   // Unknown
            0xFF04 => Memory[address],                   // Div
            >= 0xFF05 and <= 0xFF07 => Memory[address],  // Timer and divider
            >= 0xFF10 and <= 0xFF26 => Memory[address],  // Sound
            >= 0xFF30 and <= 0xFF3F => Memory[address],  // Wave pattern
            >= 0xFF40 and <= 0xFF4B => Memory[address],  // LCD Control, Status, Position, Scrolling, and Palettes
            0xFF4F => Memory[address],                   // VRAM Bank Select
            0xFF50 => Memory[address],                   // Set to non-zero to disable boot ROM
            >= 0xFF51 and <= 0xFF55 => Memory[address],  // VRAM DMA
            >= 0xFF68 and <= 0xFF69 => Memory[address],  // BG / OBJ Palettes
            >= 0xFF70 => Memory[address],                // WRAM Bank Select

            _ => Memory[address],
        };
    }

    void WriteIO(ushort address, byte data)
    {
        switch (address)
        {
            case 0xFF00: joypad.JOYP = data; break;
            case 0xFF01: SerialTransfer(address, data); break;
            case 0xFF04: Memory[address] = 0; break;            // Reset Div
            case 0xFF44: Memory[address] = 0; break;
            case 0xFF46: OamTransfer(data); break;

            default: Memory[address] = data; break;
        };

    }

    void OamTransfer(byte data)
    {
        ushort offset = (ushort)(data << 8);
        for (byte i = 0; i < 160; i++)
        {
            Memory[0xFE00 + i] = Read((ushort)(offset + i));
        }
    }

    void SerialTransfer(ushort address, byte data)
    {
        Memory[address] = data;
    }

    public void SkipBootRom()
    {
        biosEnabled = false;

        // DMG Boot

        // https://gbdev.io/pandocs/Power_Up_Sequence.html?highlight=Register%20name#cpu-registers
        Registers.A = 0x01;
        Registers.F = 0b1000_0000;
        Registers.B = 0x00;
        Registers.C = 0x13;
        Registers.D = 0x00;
        Registers.E = 0xD8;
        Registers.H = 0x01;
        Registers.L = 0x4D;

        Registers.PC = 0x0100;
        Registers.SP = 0xFFFE;

        // https://gbdev.io/pandocs/Power_Up_Sequence.html?highlight=Register%20name#hardware-registers
        Write(Reg.P1, 0xCF);
        Write(Reg.SB, 0x0);
        Write(Reg.SC, 0x7E);
        Write(Reg.DIV, 0xAB);
        Write(Reg.TIMA, 0x0);
        Write(Reg.TMA, 0x0);
        Write(Reg.TAC, 0xF8);
        Write(Reg.IF, 0xE1);
        Write(Reg.NR10, 0x80);
        Write(Reg.NR11, 0xBF);
        Write(Reg.NR12, 0xF3);
        Write(Reg.NR13, 0xFF);
        Write(Reg.NR14, 0xBF);
        Write(Reg.NR21, 0x3F);
        Write(Reg.NR22, 0x0);
        Write(Reg.NR23, 0xFF);
        Write(Reg.NR24, 0xBF);
        Write(Reg.NR30, 0x7F);
        Write(Reg.NR31, 0xFF);
        Write(Reg.NR32, 0x9F);
        Write(Reg.NR33, 0xFF);
        Write(Reg.NR34, 0xBF);
        Write(Reg.NR41, 0xFF);
        Write(Reg.NR42, 0x0);
        Write(Reg.NR43, 0x0);
        Write(Reg.NR44, 0xBF);
        Write(Reg.NR50, 0x77);
        Write(Reg.NR51, 0xF3);
        Write(Reg.NR52, 0xF1);
        Write(Reg.LCDC, 0x91);
        Write(Reg.STAT, 0x85);
        Write(Reg.SCY, 0x0);
        Write(Reg.SCX, 0x0);
        Write(Reg.LY, 0x00);
        Write(Reg.LYC, 0x0);
        Write(Reg.DMA, 0xFF);
        Write(Reg.BGP, 0xFC);
        Write(Reg.OBP0, (byte)(Random.Shared.Next(1) == 0 ? 0x00 : 0xFF));
        Write(Reg.OBP1, (byte)(Random.Shared.Next(1) == 0 ? 0x00 : 0xFF));
        Write(Reg.WY, 0x0);
        Write(Reg.WX, 0x0);
        Write(0xFFFF, 0x0);
    }

    public bool BreakPointsEnabled { get; set; }
    public bool CustomCartridgeLogo { get; set; }

    internal bool EnteredVSync { get; set; }

    internal void BreakPoint(char breakpoint)
    {
        if (!BreakPointsEnabled)
        {
            return;
        }

        Console.WriteLine("Breakpoint hit " + breakpoint);

        Dump(breakpoint.ToString());
    }

    public void Dump(string dumpName)
    {
        _ = Directory.CreateDirectory("dumps/" + dumpName);
        File.WriteAllBytes("dumps/" + dumpName + "/ram.bin", Memory.Range(0x0000, 0xFFFF));
        File.WriteAllBytes("dumps/" + dumpName + "/vram.bin", Memory.Range(0x8000, 0x9FFF));
        File.WriteAllBytes("dumps/" + dumpName + "/wram.bin", Memory.Range(0xC000, 0xDFFF));
        File.WriteAllBytes("dumps/" + dumpName + "/oam.bin", Memory.Range(0xFE00, 0xFE9F));
        File.WriteAllBytes("dumps/" + dumpName + "/io.bin", Memory.Range(0xFF00, 0xFF7F));
        File.WriteAllBytes("dumps/" + dumpName + "/highram.bin", Memory.Range(0xFF00, 0xFF7F));

        string regs = $"""
        A:  {Registers.A:X2}   F: {Registers.F:B4}
        B:  {Registers.B:X2}   C: {Registers.C:X2}
        D:  {Registers.D:X2}   E: {Registers.E:X2}
        HL: {Registers.HL:X4}
        SP: {Registers.SP:X4}
        PC: {Registers.PC:X4}

        """;

        File.WriteAllText("dumps/" + dumpName + "/register.txt", regs);
    }
}
