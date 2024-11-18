namespace Monoboy;

using System;
using System.IO;

using Monoboy.Cartridge;
using Monoboy.Constants;
using Monoboy.Utility;

public class Emulator
{
    public const int CyclesPerFrame = 70224 / 4; // 17556 M-Cycles
    public const byte WindowWidth = 160;
    public const byte WindowHeight = 144;
    const int IFReg = 0xFF0F;
    const int IEReg = 0xFFFF;

    /// <summary> #AABBGGRR </summary>
    public byte[] Framebuffer { get; set; }
    public string GameTitle { get; private set; }

    byte[] bios;

    // Hardware
    public Register Registers { get; set; }
    public Memory Memory { get; set; }
    IMemoryBankController mbc;
    bool biosEnabled = true;
    Cpu cpu;
    Ppu ppu;
    Joypad joypad;
    Timer timer;

    // CPU
    internal bool Halted { get; set; }
    internal bool HaltBug { get; set; }

    public byte IF { get => Read(IFReg); set => Write(IFReg, value); }
    public byte IE { get => Read(IEReg); set => Write(IEReg, value); }

    // Interupt
    internal bool Ime { get; set; } // Master interupt enabled

    /// <summary> Machine Cycles </summary>
    internal int Cycles { get; set; }
    /// <summary> Machine Cycles </summary>
    internal long TotalCycles { get; set; }

    public Emulator(byte[] bios)
    {
        this.bios = bios;
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
        int mCycles = 0;

        byte op = cpu.NextByte();

        // Interrupt Handling
        if (Ime)
        {
            Ime = false;


            for (byte i = 5; i >= 0; i--)
            {
                if ((((IE & IF) >> i) & 1) == 1)
                {
                    Console.WriteLine("Handling Interupts: " + (IE & IF).ToString("B8"));
                    cpu.Push(Registers.PC);
                    Registers.PC = (ushort)(0x40 + (0x8 * i));

                    Ime = false;
                    IF = IF.SetBit((byte)(0b1 << i), false);
                    break;
                }
            }

            mCycles += 5;
        }
        else if (Halted) // Halt Handling
        {
            Console.WriteLine("Halted");
            if (IF != 0)
            {
                Registers.PC++;
                Halted = false;
            }
        }
        else
        {
            mCycles = cpu.Execute(op);
        }

        timer.Step(mCycles);
        ppu.Step(mCycles);

        // Disable the bios/boot rom in the bus
        if (biosEnabled && Registers.PC >= 0x100)
        {
            biosEnabled = false;
        }
    }

    public void StepFrame()
    {
        if (mbc == null && biosEnabled == false)
        {
            return;
        }

        while (Cycles < CyclesPerFrame)
        {
            Step();
        }
        Cycles -= CyclesPerFrame;
    }

    public void Open(string rom)
    {
        byte[] data = File.ReadAllBytes(rom);
        Open(data);

        string saveFile = rom.Replace(".gb", ".sav");
        if (File.Exists(saveFile))
        {
            mbc.SetRam(File.ReadAllBytes(saveFile));
        }
    }

    public void Open(byte[] data)
    {
        mbc?.Save(data);

        Reset();

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

        mbc.Load(data);
        mbc.Save(data);

        Console.WriteLine();
        Console.WriteLine("Cartridge Header Info");
        Console.WriteLine("Title: " + GameTitle);
        Console.WriteLine("CGB flag: " + data[0x143]);
        Console.WriteLine("New licensee code: " + data[0x144] + "" + data[0x145]);
        Console.WriteLine("SGB flag: " + data[0x146]);
        Console.WriteLine("Cartridge type: " + data[0x147]);

        mbc.Rom = data;
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
        Halted = false;
        HaltBug = false;
        Ime = false;
        biosEnabled = true;

        timer.Reset();
        ppu.Reset();
        joypad.Reset();
        Memory.Reset();
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

            // if (address is >= 0x0104 and <= 0x0133)
            // {
            //     // Custom Header Monoboy logo for bootix boot rom
            //     return new byte[]{
            //         0b11001110,0b11101101,
            //         0b00110111,0b01111011,
            //         0b00000000,0b00110110,
            //         0b00000000,0b11000110,
            //         0b00000000,0b11011110,
            //         0b00000000,0b10001101,
            //         0b00000000,0b11111001,
            //         0b00110011,0b00111011,
            //         0b00000000,0b11100011,
            //         0b00000000,0b00110110,
            //         0b00000000,0b11000110,
            //         0b00000000,0b11011101,
            //         0b11011100,0b11000000,
            //         0b10110011,0b00110000,
            //         0b01100110,0b00110000,
            //         0b01100110,0b11000000,
            //         0b11001100,0b11000000,
            //         0b11011101,0b11000000,
            //         0b10011001,0b11110000,
            //         0b10111011,0b00110000,
            //         0b00110011,0b11100000,
            //         0b01100110,0b00110000,
            //         0b01100110,0b11000000,
            //         0b11010111,0b00111110,
            //     }
            //     [address - 0x0104];
            // }
        }

        return address switch
        {
            <= 0x3FFF => mbc?.ReadBank00(address) ?? 0,       // 16 KiB ROM bank 00
            <= 0x7FFF => mbc?.ReadBankNN(address) ?? 0,       // 16 KiB ROM Bank 01~NN
            <= 0x9FFF => Memory[address],                     // 8 KiB Video RAM (VRAM)
            <= 0xBFFF => mbc?.ReadRam(address) ?? 0,          // 8 KiB External RAM
            <= 0xCFFF => Memory[address],                     // 4 KiB Work RAM (WRAM)
            <= 0xDFFF => Memory[address],                     // 4 KiB Work RAM (WRAM)
            <= 0xFDFF => Memory[address - 0x2000],            // Mirror of C000~DDFF (ECHO RAM)
            <= 0xFE9F => Memory[address],                     // Sprite attribute table (OAM)
            <= 0xFEFF => 0x00,                                // Not Usable
            <= 0xFF7F => ReadIO(address),                     // I/O Registers
            <= 0xFFFE => Memory[address],                     // High RAM (HRAM)
            IEReg => Memory[address],                         // Interrupt Enable register (IE)
        };
    }

    public void Write(ushort address, byte data)
    {
        switch (address)
        {
            case <= 0x7FFF: mbc?.WriteBank(address, data); break;           // 16KB ROM Bank > 00 (in cartridge, every other bank)
            case <= 0x9FFF: Memory[address] = data; break;                  // 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            case <= 0xBFFF: mbc?.WriteRam(address, data); break;            // 8KB External RAM (in cartridge, switchable bank, if any)
            case <= 0xDFFF: Memory[address] = data; break;                  // 4KB Work RAM Bank 0 (WRAM) (switchable bank 1-7 in CGB Mode)
            case <= 0xFDFF: Memory[address] = data; break;                  // Same as C000-DDFF (ECHO) (typically not used)
            case <= 0xFE9F: Memory[address] = data; break;                  // Sprite Attribute Table (OAM)
            case <= 0xFEFF: break;                                          // Not Usable
            case <= 0xFF7F: WriteIO(address, data); break;                  // I/O Ports
            case <= 0xFFFE: Memory[address] = data; break;                  // Zero Page RAM
            case <= IEReg: Memory[address] = data; break;                   // Interrupt Enable register (IE)
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

        // DMG0 Boot

        // https://gbdev.io/pandocs/Power_Up_Sequence.html?highlight=Register%20name#cpu-registers
        Registers.A = 0x01;
        Registers.F = 0b0000;
        Registers.B = 0xFF;
        Registers.C = 0x13;
        Registers.D = 0x00;
        Registers.E = 0xC1;
        Registers.H = 0x84;
        Registers.L = 0x03;

        Registers.PC = 0x0100;
        Registers.SP = 0xFFFE;

        // https://gbdev.io/pandocs/Power_Up_Sequence.html?highlight=Register%20name#hardware-registers
        Write(Reg.P1, 0xCF);
        Write(Reg.SB, 0x0);
        Write(Reg.SC, 0x7E);
        Write(Reg.DIV, 0x18);
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
        Write(Reg.STAT, 0x81);
        Write(Reg.SCY, 0x0);
        Write(Reg.SCX, 0x0);
        Write(Reg.LY, 0x91);
        Write(Reg.LYC, 0x0);
        Write(Reg.DMA, 0xFF);
        Write(Reg.BGP, 0xFC);
        Write(Reg.OBP0, (byte)Random.Shared.Next());
        Write(Reg.OBP1, (byte)Random.Shared.Next());
        Write(Reg.WY, 0x0);
        Write(Reg.WX, 0x0);
        Write(Reg.IE, 0x0);
    }

    public static void Save()
    {
        // mbc?.Save(romPath);
    }

}
