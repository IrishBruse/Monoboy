namespace Monoboy;

using System;
using System.IO;

using Monoboy.Cartridge;

public class Emulator
{
    public const int CpuCyclesPerSecond = 1048576;
    public const int CyclesPerFrame = 17556;
    public const byte WindowWidth = 160;
    public const byte WindowHeight = 144;

    public bool BackgroundEnabled { get; set; } = true;
    public bool WindowEnabled { get; set; } = true;
    public bool SpritesEnabled { get; set; } = true;

    // Hardware
    private Register register;
    private byte[] memory;
    private IMemoryBankController mbc;
    private bool biosEnabled;
    private Cpu cpu;
    private Ppu ppu;
    private Joypad joypad;
    private Timer timer;

    // CPU
    internal bool Halted { get; set; }
    internal bool HaltBug { get; set; }

    // Interupt
    internal bool Ime { get; set; } // Master interupt enabled
    internal bool ImeDelay { get; set; } // Master interupt enabled delay

    public byte DIV
    {
        get => (byte)(timer.SystemInternalClock >> 8);
        set => timer.SystemInternalClock = 0;
    }

    private byte[] bios;

    public long Cycles { get; set; }

    public Emulator()
    {
        memory = new byte[0x10000];
        register = new Register();

        cpu = new Cpu(register, this);
        timer = new Timer(memory, cpu);
        ppu = new Ppu(memory, this, cpu);
        joypad = new Joypad(cpu);

        Reset();
    }

    public byte Step()
    {
        byte ticks = cpu.Step();
        Cycles += ticks;

        timer.Step(ticks);
        ppu.Step(ticks);

        cpu.HandleInterupts();

        // Disable the bios/boot rom in the bus
        if (biosEnabled && register.PC >= 0x100)
        {
            biosEnabled = false;
        }

        return ticks;
    }

    public void StepFrame()
    {
        while (Cycles < CyclesPerFrame)
        {
            Cycles += Step();
        }
        Cycles -= CyclesPerFrame;
    }

    public void Open(string rom)
    {
        Reset();

        byte cartridgeType;
        using (BinaryReader reader = new(new FileStream(rom, FileMode.Open)))
        {
            _ = reader.BaseStream.Seek(0x147, SeekOrigin.Begin);
            cartridgeType = reader.ReadByte();
        }

        // Console.WriteLine(cartridgeType);

        mbc = cartridgeType switch
        {
            byte when cartridgeType <= 0x00 => new MemoryBankController0(),
            byte when cartridgeType <= 0x03 => new MemoryBankController1(),
            byte when cartridgeType <= 0x06 => new MemoryBankController2(),
            byte when cartridgeType <= 0x13 => new MemoryBankController3(),
            byte when cartridgeType <= 0x1E => new MemoryBankController5(),
            byte when cartridgeType <= 0x20 => new MemoryBankController6(),
            _ => throw new NotImplementedException()
        };

        mbc.Rom = File.ReadAllBytes(rom);

        string saveFile = rom.Replace(".gb", ".sav");
        if (File.Exists(saveFile))
        {
            mbc.SetRam(File.ReadAllBytes(saveFile));
        }
    }

    public void Reset()
    {
        memory = new byte[0x10000];
        register.Reset();

        // Cpu and Interrupt
        Halted = false;
        HaltBug = false;
        Ime = false;
        ImeDelay = false;

        timer.Reset();
        ppu.Reset();
        joypad.Reset();

        // TODO change to cli flag
        // memory.boot = File.ReadAllBytes("./dmg.boot");
        // biosEnabled = true;

        SkipBootRom();
    }

    public void SkipBootRom()
    {
        biosEnabled = false;

        register.ZFlag = true;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = false;

        register.A = 0x01;
        register.B = 0x00;
        register.C = 0x13;
        register.D = 0x00;
        register.E = 0xD8;
        register.H = 0x01;
        register.L = 0x4D;
        register.PC = 0x0100;
        register.SP = 0xFFFE;

        Write(0xFF00, 0xCF);
        Write(0xFF01, 0x00);
        Write(0xFF02, 0x7E);
        Write(0xFF04, 0xAB);
        Write(0xFF05, 0x00);
        Write(0xFF06, 0x00);
        Write(0xFF07, 0xF8);
        Write(0xFF0F, 0xE1);
        Write(0xFF10, 0x80);
        Write(0xFF11, 0xBF);
        Write(0xFF12, 0xF3);
        Write(0xFF13, 0xFF);
        Write(0xFF14, 0xBF);
        Write(0xFF16, 0x3F);
        Write(0xFF17, 0x00);
        Write(0xFF18, 0xFF);
        Write(0xFF19, 0xBF);
        Write(0xFF1A, 0x7F);
        Write(0xFF1B, 0xFF);
        Write(0xFF1C, 0x9F);
        Write(0xFF1D, 0xFF);
        Write(0xFF1E, 0xBF);
        Write(0xFF20, 0xFF);
        Write(0xFF21, 0x00);
        Write(0xFF22, 0x00);
        Write(0xFF23, 0xBF);
        Write(0xFF24, 0x77);
        Write(0xFF25, 0xF3);
        Write(0xFF26, 0xF1);
        Write(0xFF40, 0x91);
        Write(0xFF41, 0x85);
        Write(0xFF42, 0x00);
        Write(0xFF43, 0x00);
        Write(0xFF44, 0x00);
        Write(0xFF45, 0x00);
        Write(0xFF46, 0xFF);
        Write(0xFF47, 0xFC);
        Write(0xFF48, 0xFF);
        Write(0xFF49, 0xFF);
        Write(0xFF4A, 0x00);
        Write(0xFF4B, 0x00);
        Write(0xFF4D, 0xFF);
        Write(0xFF4F, 0xFF);
        Write(0xFF51, 0xFF);
        Write(0xFF52, 0xFF);
        Write(0xFF53, 0xFF);
        Write(0xFF54, 0xFF);
        Write(0xFF55, 0xFF);
        Write(0xFF56, 0xFF);
        Write(0xFF68, 0xFF);
        Write(0xFF69, 0xFF);
        Write(0xFF6A, 0xFF);
        Write(0xFF6B, 0xFF);
        Write(0xFF70, 0xFF);
        Write(0xFFFF, 0x00);
    }

    internal byte Read(ushort address)
    {
        if (biosEnabled && address <= 0x00FF)
        {
            return bios[address];
        }

        return address switch
        {
            ushort when address >= 0x0000 && address <= 0x3FFF => mbc.ReadBank00(address),      // 16 KiB ROM bank 00
            ushort when address >= 0x4000 && address <= 0x7FFF => mbc.ReadBankNN(address),      // 16 KiB ROM Bank 01~NN
            ushort when address >= 0x8000 && address <= 0x9FFF => memory[address],              // 8 KiB Video RAM (VRAM)
            ushort when address >= 0xA000 && address <= 0xBFFF => mbc.ReadRam(address),         // 8 KiB External RAM
            ushort when address >= 0xC000 && address <= 0xCFFF => memory[address],              // 4 KiB Work RAM (WRAM)
            ushort when address >= 0xD000 && address <= 0xDFFF => memory[address],              // 4 KiB Work RAM (WRAM)
            ushort when address >= 0xE000 && address <= 0xFDFF => memory[address - 0xE000],     // Mirror of C000~DDFF (ECHO RAM)
            ushort when address >= 0xFE00 && address <= 0xFE9F => memory[address],              // Sprite attribute table (OAM)
            ushort when address >= 0xFEA0 && address <= 0xFEFF => 0x00,                         // Not Usable
            ushort when address >= 0xFF00 && address <= 0xFF7F => ReadIO(address),              // I/O Registers
            ushort when address >= 0xFF80 && address <= 0xFFFE => memory[address],              // High RAM (HRAM)
            ushort when address == 0xFFFF => memory[address],                                   // Interrupt Enable register (IE)
            _ => throw new Exception($"Unhandled I/O read at address 0x{address:X4}"),
        };
    }

    internal void Write(ushort address, byte data)
    {
        switch (address)
        {
            case ushort when address <= 0x7FFF: mbc.WriteBank(address, data); break;            // 16KB ROM Bank > 00 (in cartridge, every other bank)
            case ushort when address <= 0x9FFF: memory[address] = data; break;                  // 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            case ushort when address <= 0xBFFF: mbc.WriteRam(address, data); break;             // 8KB External RAM (in cartridge, switchable bank, if any)
            case ushort when address <= 0xDFFF: memory[address] = data; break;                  // 4KB Work RAM Bank 0 (WRAM) (switchable bank 1-7 in CGB Mode)
            case ushort when address <= 0xFDFF: memory[address] = data; break;                  // Same as C000-DDFF (ECHO) (typically not used)
            case ushort when address <= 0xFE9F: memory[address] = data; break;                  // Sprite Attribute Table (OAM)
            case ushort when address <= 0xFEFF: break;                                          // Not Usable
            case ushort when address <= 0xFF7F: WriteIO(address, data); break;                  // I/O Ports
            case ushort when address <= 0xFFFE: memory[address] = data; break;                  // Zero Page RAM
            case ushort when address <= 0xFFFF: memory[address] = data; break;                  // Interrupt Enable register (IE)

            default: throw new Exception($"Unhandled I/O write at address 0x{address:X4}");
        }
    }

    private byte ReadIO(ushort address)
    {
        return address switch
        {
            ushort when address == 0xFF00 => joypad.JOYP,                           // Joypad input
            ushort when address >= 0xFF01 && address <= 0xFF02 => memory[address],  // Serial transfer
            ushort when address == 0xFF04 => DIV,                                   // Div
            ushort when address >= 0xFF05 && address <= 0xFF07 => memory[address],  // Timer and divider
            ushort when address >= 0xFF10 && address <= 0xFF26 => memory[address],  // Sound
            ushort when address >= 0xFF30 && address <= 0xFF3F => memory[address],  // Wave pattern
            ushort when address >= 0xFF40 && address <= 0xFF4B => memory[address],  // LCD Control, Status, Position, Scrolling, and Palettes
            ushort when address >= 0xFF4F => memory[address],                       // VRAM Bank Select
            ushort when address >= 0xFF50 => memory[address],                       // Set to non-zero to disable boot ROM
            ushort when address >= 0xFF51 && address <= 0xFF55 => memory[address],  // VRAM DMA
            ushort when address >= 0xFF68 && address <= 0xFF69 => memory[address],  // BG / OBJ Palettes
            ushort when address >= 0xFF70 => memory[address],                       // WRAM Bank Select

            _ => memory[address],
        };
    }

    private void WriteIO(ushort address, byte data)
    {
        switch (address)
        {
            case ushort when address == 0xFF00: joypad.JOYP = data; break;
            case ushort when address == 0xFF04: DIV = data; break;

            default: memory[address] = data; break;
        };

    }
}
