namespace Monoboy;

using System;
using System.IO;

using Monoboy.Cartridge;
using Monoboy.Constants;

public class Emulator
{
    public const int CpuCyclesPerSecond = 1048576;
    public const int CyclesPerFrame = 17556;
    public const byte WindowWidth = 160;
    public const byte WindowHeight = 144;

    public bool BackgroundEnabled { get; set; } = true;
    public bool WindowEnabled { get; set; } = true;
    public bool SpritesEnabled { get; set; } = true;

    /// <summary> #AABBGGRR </summary>
    public uint[] Framebuffer { get; set; }
    public string GameTitle { get; private set; }
    byte[] bios;

    // Hardware
    private Register register;
    private Memory memory;
    private IMemoryBankController mbc;
    private bool biosEnabled = true;
    private Cpu cpu;
    private Ppu ppu;
    private Joypad joypad;
    private Timer timer;

    // CPU
    internal bool Halted { get; set; }
    internal bool HaltBug { get; set; }

    public byte IE { get => Read(0xFFFF); set => Write(0xFFFF, value); }
    public byte IF { get => Read(0xFF0F); set => Write(0xFF0F, value); }

    // Interupt
    internal bool Ime { get; set; } // Master interupt enabled
    internal bool ImeDelay { get; set; } // Master interupt enabled delay

    public byte DIV
    {
        get => (byte)(timer.SystemInternalClock >> 8);
        set => timer.SystemInternalClock = 0;
    }

    internal int Cycles { get; set; }
    internal long TotalCycles { get; set; }

    public Emulator(byte[] bios)
    {
        this.bios = bios;

        memory = new Memory(0x10000);
        Framebuffer = new uint[WindowWidth * WindowHeight];

        register = new Register();

        cpu = new Cpu(register, this);
        timer = new Timer(memory, cpu);
        ppu = new Ppu(memory, this, cpu, Framebuffer);
        joypad = new Joypad(cpu);

        Reset();
    }

    public byte Step()
    {
        byte ticks = cpu.Step();
        Cycles += ticks;
        TotalCycles += ticks;

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

        byte[] data = File.ReadAllBytes(rom);
        byte cartridgeType = data[0x147];

        GameTitle = "";

        bool lower = false;

        // Clean up title
        foreach (byte item in data[0x134..(0x134 + 0xf)])
        {
            if (item == 0)
            {
                break;
            }

            if (lower)
            {
                GameTitle += char.ToLowerInvariant((char)item);
            }
            else
            {
                GameTitle += (char)item;
                lower = true;
            }

            if (item == 32)
            {
                lower = false;
            }
        }

        mbc = cartridgeType switch
        {
            0x00 => new MemoryBankController0(),    // ROM ONLY
            0x01 => new MemoryBankController1(),    // MBC1
            0x02 => new MemoryBankController1(),    // MBC1+RAM
            0x03 => new MemoryBankController1(),    // MBC1+RAM+BATTERY
            0x05 => new MemoryBankController2(),    // MBC2
            0x06 => new MemoryBankController2(),    // MBC2+BATTERY
            0x0B => new MemoryBankController2(),    // MMM01
            0x0C => new MemoryBankController2(),    // MMM01+RAM
            0x0D => new MemoryBankController2(),    // MMM01+RAM+BATTERY
            0x0F => new MemoryBankController3(),    // MBC3+TIMER+BATTERY
            0x10 => new MemoryBankController3(),    // MBC3+TIMER+RAM+BATTERY 2
            0x11 => new MemoryBankController3(),    // MBC3
            0x12 => new MemoryBankController3(),    // MBC3+RAM 2
            0x13 => new MemoryBankController3(),    // MBC3+RAM+BATTERY 2
            0x19 => new MemoryBankController5(),    // MBC5
            0x1A => new MemoryBankController5(),    // MBC5+RAM
            0x1B => new MemoryBankController5(),    // MBC5+RAM+BATTERY
            0x1C => new MemoryBankController5(),    // MBC5+RUMBLE
            0x1D => new MemoryBankController5(),    // MBC5+RUMBLE+RAM
            0x1E => new MemoryBankController5(),    // MBC5+RUMBLE+RAM+BATTERY
            0x20 => new MemoryBankController6(),    // MBC6
            0x22 => new MemoryBankController6(),    // MBC7+SENSOR+RUMBLE+RAM+BATTERY
            0xFC => new MemoryBankController6(),    // POCKET CAMERA
            0xFD => new MemoryBankController6(),    // BANDAI TAMA5
            0xFE => new MemoryBankController6(),    // HuC3
            0xFF => new MemoryBankController6(),    // HuC1+RAM+BATTERY
            _ => throw new NotImplementedException()
        };

        mbc.Rom = data;

        string saveFile = rom.Replace(".gb", ".sav");
        if (File.Exists(saveFile))
        {
            mbc.SetRam(File.ReadAllBytes(saveFile));
        }
    }

    public void Reset()
    {
        for (int i = 0; i < Framebuffer.Length; i++)
        {
            Framebuffer[i] = Pallet.GetColor(0);
        }

        register.Reset();

        // Cpu and Interrupt
        Halted = false;
        HaltBug = false;
        Ime = false;
        ImeDelay = false;

        timer.Reset();
        ppu.Reset();
        joypad.Reset();
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

    public void SetButtonState(GameboyButton button, bool pressed)
    {
        joypad.SetButton(button, pressed);
    }

    internal byte Read(ushort address)
    {
        if (biosEnabled && bios.Length > 0 && address <= 0x00FF)
        {
            return bios[address];
        }

        return address switch
        {
            <= 0x3FFF => mbc.ReadBank00(address),       // 16 KiB ROM bank 00
            <= 0x7FFF => mbc.ReadBankNN(address),       // 16 KiB ROM Bank 01~NN
            <= 0x9FFF => memory[address],               // 8 KiB Video RAM (VRAM)
            <= 0xBFFF => mbc.ReadRam(address),          // 8 KiB External RAM
            <= 0xCFFF => memory[address],               // 4 KiB Work RAM (WRAM)
            <= 0xDFFF => memory[address],               // 4 KiB Work RAM (WRAM)
            <= 0xFDFF => memory[address - 0x2000],      // Mirror of C000~DDFF (ECHO RAM)
            <= 0xFE9F => memory[address],               // Sprite attribute table (OAM)
            <= 0xFEFF => 0x00,                          // Not Usable
            <= 0xFF7F => ReadIO(address),               // I/O Registers
            <= 0xFFFE => memory[address],               // High RAM (HRAM)
            0xFFFF => memory[address],                  // Interrupt Enable register (IE)
        };
    }

    internal void Write(ushort address, byte data)
    {
        switch (address)
        {
            case <= 0x7FFF: mbc.WriteBank(address, data); break;            // 16KB ROM Bank > 00 (in cartridge, every other bank)
            case <= 0x9FFF: memory[address] = data; break;                  // 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            case <= 0xBFFF: mbc.WriteRam(address, data); break;             // 8KB External RAM (in cartridge, switchable bank, if any)
            case <= 0xDFFF: memory[address] = data; break;                  // 4KB Work RAM Bank 0 (WRAM) (switchable bank 1-7 in CGB Mode)
            case <= 0xFDFF: memory[address] = data; break;                  // Same as C000-DDFF (ECHO) (typically not used)
            case <= 0xFE9F: memory[address] = data; break;                  // Sprite Attribute Table (OAM)
            case <= 0xFEFF: Console.WriteLine($"Read from not usable 0x{address:X4}"); break;// Not Usable
            case <= 0xFF7F: WriteIO(address, data); break;                  // I/O Ports
            case <= 0xFFFE: memory[address] = data; break;                  // Zero Page RAM
            case <= 0xFFFF: memory[address] = data; break;                  // Interrupt Enable register (IE)
        }
    }

    private byte ReadIO(ushort address)
    {
        return address switch
        {
            0xFF00 => joypad.JOYP,                       // Joypad input
            >= 0xFF01 and <= 0xFF02 => memory[address],  // Serial transfer
            0xFF03 => memory[address],                   // Unknown
            0xFF04 => DIV,                               // Div
            >= 0xFF05 and <= 0xFF07 => memory[address],  // Timer and divider
            >= 0xFF10 and <= 0xFF26 => memory[address],  // Sound
            >= 0xFF30 and <= 0xFF3F => memory[address],  // Wave pattern
            >= 0xFF40 and <= 0xFF4B => memory[address],  // LCD Control, Status, Position, Scrolling, and Palettes
            0xFF4F => memory[address],                   // VRAM Bank Select
            0xFF50 => memory[address],                   // Set to non-zero to disable boot ROM
            >= 0xFF51 and <= 0xFF55 => memory[address],  // VRAM DMA
            >= 0xFF68 and <= 0xFF69 => memory[address],  // BG / OBJ Palettes
            >= 0xFF70 => memory[address],                // WRAM Bank Select

            _ => memory[address],
        };
    }

    private void WriteIO(ushort address, byte data)
    {
        switch (address)
        {
            case 0xFF00: joypad.JOYP = data; break;
            case 0xFF01: SerialTransfer(address, data); break;
            case 0xFF04: DIV = data; break;
            case 0xFF44: memory[address] = 0; break;
            case 0xFF46: OamTransfer(data); break;

            default: memory[address] = data; break;
        };

    }

    private void OamTransfer(byte data)
    {
        ushort offset = (ushort)(data / 0x100);
        for (byte i = 0; i < 160; i++)
        {
            memory[0xFE00 + i] = Read((ushort)(offset + i));
        }
    }

    private void SerialTransfer(ushort address, byte data)
    {
        Console.WriteLine($"Serial transfer: {data:X2}");
        memory[address] = data;
    }
}
