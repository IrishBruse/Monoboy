namespace Monoboy;

using System;

using Monoboy.Utility;

public class Cpu
{
    private Register register;
    private Emulator emulator;

    public Cpu(Register register, Emulator emulator)
    {
        this.register = register;
        this.emulator = emulator;
    }

    public byte Step()
    {
        byte op = NextByte();

        if (emulator.HaltBug)
        {
            emulator.HaltBug = false;
            register.PC--;
        }

        byte conditionalCycles = 0;

        switch (op)
        {
            case 0x06: register.B = NextByte(); break;// LD nn,n
            case 0x0E: register.C = NextByte(); break;
            case 0x16: register.D = NextByte(); break;
            case 0x1E: register.E = NextByte(); break;
            case 0x26: register.H = NextByte(); break;
            case 0x2E: register.L = NextByte(); break;

            case 0x7F: register.A = register.A; break;// LD A,r2
            case 0x78: register.A = register.B; break;
            case 0x79: register.A = register.C; break;
            case 0x7A: register.A = register.D; break;
            case 0x7B: register.A = register.E; break;
            case 0x7C: register.A = register.H; break;
            case 0x7D: register.A = register.L; break;

            case 0x47: register.B = register.A; break;// LD B,r2
            case 0x40: register.B = register.B; break;
            case 0x41: register.B = register.C; break;
            case 0x42: register.B = register.D; break;
            case 0x43: register.B = register.E; break;
            case 0x44: register.B = register.H; break;
            case 0x45: register.B = register.L; break;

            case 0x4F: register.C = register.A; break;// LD C,r2
            case 0x48: register.C = register.B; break;
            case 0x49: register.C = register.C; break;
            case 0x4A: register.C = register.D; break;
            case 0x4B: register.C = register.E; break;
            case 0x4C: register.C = register.H; break;
            case 0x4D: register.C = register.L; break;

            case 0x57: register.D = register.A; break;// LD D,r2
            case 0x50: register.D = register.B; break;
            case 0x51: register.D = register.C; break;
            case 0x52: register.D = register.D; break;
            case 0x53: register.D = register.E; break;
            case 0x54: register.D = register.H; break;
            case 0x55: register.D = register.L; break;

            case 0x5F: register.E = register.A; break;// LD E,r2
            case 0x58: register.E = register.B; break;
            case 0x59: register.E = register.C; break;
            case 0x5A: register.E = register.D; break;
            case 0x5B: register.E = register.E; break;
            case 0x5C: register.E = register.H; break;
            case 0x5D: register.E = register.L; break;

            case 0x67: register.H = register.A; break;// LD H,r2
            case 0x60: register.H = register.B; break;
            case 0x61: register.H = register.C; break;
            case 0x62: register.H = register.D; break;
            case 0x63: register.H = register.E; break;
            case 0x64: register.H = register.H; break;
            case 0x65: register.H = register.L; break;

            case 0x6F: register.L = register.A; break;// LD L,r2
            case 0x68: register.L = register.B; break;
            case 0x69: register.L = register.C; break;
            case 0x6A: register.L = register.D; break;
            case 0x6B: register.L = register.E; break;
            case 0x6C: register.L = register.H; break;
            case 0x6D: register.L = register.L; break;

            case 0xFA: register.A = emulator.Read(NextShort()); break;// LD L,r2
            case 0x46: register.B = emulator.Read(register.HL); break;
            case 0x4E: register.C = emulator.Read(register.HL); break;
            case 0x56: register.D = emulator.Read(register.HL); break;
            case 0x5E: register.E = emulator.Read(register.HL); break;
            case 0x66: register.H = emulator.Read(register.HL); break;
            case 0x6E: register.L = emulator.Read(register.HL); break;

            case 0x77: emulator.Write(register.HL, register.A); break;// LD (HL),r2
            case 0x70: emulator.Write(register.HL, register.B); break;
            case 0x71: emulator.Write(register.HL, register.C); break;
            case 0x72: emulator.Write(register.HL, register.D); break;
            case 0x73: emulator.Write(register.HL, register.E); break;
            case 0x74: emulator.Write(register.HL, register.H); break;
            case 0x75: emulator.Write(register.HL, register.L); break;
            case 0x36: emulator.Write(register.HL, NextByte()); break;

            case 0x0A: register.A = emulator.Read(register.BC); break;// LD A,n
            case 0x1A: register.A = emulator.Read(register.DE); break;
            case 0x7E: register.A = emulator.Read(register.HL); break;
            case 0x3E: register.A = NextByte(); break;

            case 0x02: emulator.Write(register.BC, register.A); break;// LD n,A
            case 0x12: emulator.Write(register.DE, register.A); break;
            case 0xEA: emulator.Write(NextShort(), register.A); break;

            case 0xF2: register.A = emulator.Read((ushort)(0xFF00 + register.C)); break;        // LD A,(C)
            case 0xE2: emulator.Write((ushort)(0xFF00 + register.C), register.A); break;  // LD (C),A
            case 0x3A: register.A = emulator.Read(register.HL--); break;                        // LD A,(HL-)
            case 0x32: emulator.Write(register.HL--, register.A); break;                  // LD (HL-),A
            case 0x2A: register.A = emulator.Read(register.HL++); break;                        // LD A,(HL+)
            case 0x22: emulator.Write(register.HL++, register.A); break;                  // LD (HL+),A
            case 0xE0: emulator.Write((ushort)(0xFF00 + NextByte()), register.A); break;  // LDH (n),A
            case 0xF0: register.A = emulator.Read((ushort)(0xFF00 + NextByte())); break;        // LDH A,(n)

            case 0x01: register.BC = NextShort(); break;// LD n,nn
            case 0x11: register.DE = NextShort(); break;
            case 0x21: register.HL = NextShort(); break;
            case 0x31: register.SP = NextShort(); break;

            case 0xF9: register.SP = register.HL; break;// LD SP,HL
            case 0xF8: register.HL = ADDS(register.SP); break;// LD HL,SP+n

            case 0x08: WriteSPtoAddress(); break;// LD (nn),SP

            case 0xF5: Push(register.AF); break;// Push nn
            case 0xC5: Push(register.BC); break;
            case 0xD5: Push(register.DE); break;
            case 0xE5: Push(register.HL); break;

            case 0xF1: register.AF = Pop(); break;// Pop nn
            case 0xC1: register.BC = Pop(); break;
            case 0xD1: register.DE = Pop(); break;
            case 0xE1: register.HL = Pop(); break;

            case 0x87: ADD(register.A); break;// ADD A,n
            case 0x80: ADD(register.B); break;
            case 0x81: ADD(register.C); break;
            case 0x82: ADD(register.D); break;
            case 0x83: ADD(register.E); break;
            case 0x84: ADD(register.H); break;
            case 0x85: ADD(register.L); break;
            case 0x86: ADD(emulator.Read(register.HL)); break;
            case 0xC6: ADD(NextByte()); break;

            case 0x8F: ADC(register.A); break;// ADC A,n
            case 0x88: ADC(register.B); break;
            case 0x89: ADC(register.C); break;
            case 0x8A: ADC(register.D); break;
            case 0x8B: ADC(register.E); break;
            case 0x8C: ADC(register.H); break;
            case 0x8D: ADC(register.L); break;
            case 0x8E: ADC(emulator.Read(register.HL)); break;
            case 0xCE: ADC(NextByte()); break;

            case 0x97: SUB(register.A); break;// SUB A,n
            case 0x90: SUB(register.B); break;
            case 0x91: SUB(register.C); break;
            case 0x92: SUB(register.D); break;
            case 0x93: SUB(register.E); break;
            case 0x94: SUB(register.H); break;
            case 0x95: SUB(register.L); break;
            case 0x96: SUB(emulator.Read(register.HL)); break;
            case 0xD6: SUB(NextByte()); break;

            case 0x9F: SBC(register.A); break;// SBC A,n
            case 0x98: SBC(register.B); break;
            case 0x99: SBC(register.C); break;
            case 0x9A: SBC(register.D); break;
            case 0x9B: SBC(register.E); break;
            case 0x9C: SBC(register.H); break;
            case 0x9D: SBC(register.L); break;
            case 0x9E: SBC(emulator.Read(register.HL)); break;
            case 0xDE: SBC(NextByte()); break;

            case 0xA7: AND(register.A); break;// AND n
            case 0xA0: AND(register.B); break;
            case 0xA1: AND(register.C); break;
            case 0xA2: AND(register.D); break;
            case 0xA3: AND(register.E); break;
            case 0xA4: AND(register.H); break;
            case 0xA5: AND(register.L); break;
            case 0xA6: AND(emulator.Read(register.HL)); break;
            case 0xE6: AND(NextByte()); break;

            case 0xB7: OR(register.A); break;// OR n
            case 0xB0: OR(register.B); break;
            case 0xB1: OR(register.C); break;
            case 0xB2: OR(register.D); break;
            case 0xB3: OR(register.E); break;
            case 0xB4: OR(register.H); break;
            case 0xB5: OR(register.L); break;
            case 0xB6: OR(emulator.Read(register.HL)); break;
            case 0xF6: OR(NextByte()); break;

            case 0xAF: XOR(register.A); break;// XOR n
            case 0xA8: XOR(register.B); break;
            case 0xA9: XOR(register.C); break;
            case 0xAA: XOR(register.D); break;
            case 0xAB: XOR(register.E); break;
            case 0xAC: XOR(register.H); break;
            case 0xAD: XOR(register.L); break;
            case 0xAE: XOR(emulator.Read(register.HL)); break;
            case 0xEE: XOR(NextByte()); break;

            case 0xBF: CP(register.A); break;// CP n
            case 0xB8: CP(register.B); break;
            case 0xB9: CP(register.C); break;
            case 0xBA: CP(register.D); break;
            case 0xBB: CP(register.E); break;
            case 0xBC: CP(register.H); break;
            case 0xBD: CP(register.L); break;
            case 0xBE: CP(emulator.Read(register.HL)); break;
            case 0xFE: CP(NextByte()); break;

            case 0x3C: register.A = INC(register.A); break;// INC n
            case 0x04: register.B = INC(register.B); break;
            case 0x0C: register.C = INC(register.C); break;
            case 0x14: register.D = INC(register.D); break;
            case 0x1C: register.E = INC(register.E); break;
            case 0x24: register.H = INC(register.H); break;
            case 0x2C: register.L = INC(register.L); break;
            case 0x34: emulator.Write(register.HL, INC(emulator.Read(register.HL))); break;

            case 0x3D: register.A = DEC(register.A); break;// DEC n
            case 0x05: register.B = DEC(register.B); break;
            case 0x0D: register.C = DEC(register.C); break;
            case 0x15: register.D = DEC(register.D); break;
            case 0x1D: register.E = DEC(register.E); break;
            case 0x25: register.H = DEC(register.H); break;
            case 0x2D: register.L = DEC(register.L); break;
            case 0x35: emulator.Write(register.HL, DEC(emulator.Read(register.HL))); break;

            case 0x09: ADD(register.BC); break;// ADD HL,n
            case 0x19: ADD(register.DE); break;
            case 0x29: ADD(register.HL); break;
            case 0x39: ADD(register.SP); break;

            case 0xE8: register.SP = ADDS(register.SP); break;// ADD SP,n

            case 0x03: register.BC++; break;// INC nn
            case 0x13: register.DE++; break;
            case 0x23: register.HL++; break;
            case 0x33: register.SP++; break;

            case 0x0B: register.BC--; break;// DEC nn
            case 0x1B: register.DE--; break;
            case 0x2B: register.HL--; break;
            case 0x3B: register.SP--; break;

            case 0xCB: return PrefixedTable(); // CB Prefixed

            case 0x27: DAA(); break;// DAA
            case 0x2F: CPL(); break;// CPL
            case 0x3F: CCF(); break;// CCF
            case 0x37: SCF(); break;// SCF

            case 0xF3: Disable(); break;// DI
            case 0xFB: EnableInterrupt(); break;// EI

            case 0xC3: JP(true); break;// JP nn

            case 0xC2: JP(register.ZFlag == false); break;// JP cc,nn
            case 0xCA: JP(register.ZFlag); break;
            case 0xD2: JP(register.CFlag == false); break;
            case 0xDA: JP(register.CFlag); break;

            case 0xE9: JP(register.HL); break;// JP (HL)
            case 0x18: JR(true); break;// JP n
            case 0x20: JR(register.ZFlag == false); break;// JR cc,n
            case 0x28: JR(register.ZFlag); break;
            case 0x30: JR(register.CFlag == false); break;
            case 0x38: JR(register.CFlag); break;

            case 0xCD: CALL(true); break;// CALL nn
            case 0xC4: CALL(register.ZFlag == false); break;// CALL cc,nn
            case 0xCC: CALL(register.ZFlag); break;
            case 0xD4: CALL(register.CFlag == false); break;
            case 0xDC: CALL(register.CFlag); break;

            case 0xC7: RST(0x00); break;// RST n
            case 0xCF: RST(0x08); break;
            case 0xD7: RST(0x10); break;
            case 0xDF: RST(0x18); break;
            case 0xE7: RST(0x20); break;
            case 0xEF: RST(0x28); break;
            case 0xF7: RST(0x30); break;
            case 0xFF: RST(0x38); break;

            case 0xC9: RET(true); break;// RET
            case 0xC0: RET(register.ZFlag == false); break;// RET cc
            case 0xC8: RET(register.ZFlag); break;
            case 0xD0: RET(register.CFlag == false); break;
            case 0xD8: RET(register.CFlag); break;
            case 0xD9: RET(true); EnableInterrupt(); break;// RETI

            case 0x07: RLCA(); break;   // RLCA
            case 0x17: RLA(); break;    // RLA
            case 0x0F: RRCA(); break;   // RRCA
            case 0x1F: RRA(); break;    // RRA

            case 0x76: Halt(); break;   // HALT

            case 0x00: // NOP
            case 0x10: // STOP
            case 0xD3:
            case 0xDB:
            case 0xDD:
            case 0xE3:
            case 0xE4:
            case 0xEB:
            case 0xEC:
            case 0xED:
            case 0xF4:
            case 0xFC:
            case 0xFD:
            default: break;
        }

        switch (op)
        {
            case 0x20: case 0x30: if (!register.ZFlag) { conditionalCycles = 1; } break;
            case 0xc0: case 0xd0: if (!register.ZFlag) { conditionalCycles = 3; } break;
            case 0xc2: case 0xd2: if (!register.ZFlag) { conditionalCycles = 3; } break;
            case 0xc4: case 0xd4: if (!register.ZFlag) { conditionalCycles = 3; } break;
            case 0x28: case 0x38: if (register.ZFlag) { conditionalCycles = 1; } break;
            case 0xc8: case 0xcc: if (register.ZFlag) { conditionalCycles = 3; } break;
            case 0xd8: case 0xdc: if (register.ZFlag) { conditionalCycles = 3; } break;
            case 0xca: case 0xda: if (register.ZFlag) { conditionalCycles = 1; } break;
        }

        return (byte)(Timings.MainTimes[op] + conditionalCycles);
    }

    private byte PrefixedTable()
    {
        byte op = NextByte();

        switch (op)
        {
            case 0x37: register.A = SWAP(register.A); break;// SWAP n
            case 0x30: register.B = SWAP(register.B); break;
            case 0x31: register.C = SWAP(register.C); break;
            case 0x32: register.D = SWAP(register.D); break;
            case 0x33: register.E = SWAP(register.E); break;
            case 0x34: register.H = SWAP(register.H); break;
            case 0x35: register.L = SWAP(register.L); break;
            case 0x36: emulator.Write(register.HL, SWAP(emulator.Read(register.HL))); break;

            case 0x07: register.A = RLC(register.A); break;// RLC n
            case 0x00: register.B = RLC(register.B); break;
            case 0x01: register.C = RLC(register.C); break;
            case 0x02: register.D = RLC(register.D); break;
            case 0x03: register.E = RLC(register.E); break;
            case 0x04: register.H = RLC(register.H); break;
            case 0x05: register.L = RLC(register.L); break;
            case 0x06: emulator.Write(register.HL, RLC(emulator.Read(register.HL))); break;

            case 0x17: register.A = RL(register.A); break;// RL n
            case 0x10: register.B = RL(register.B); break;
            case 0x11: register.C = RL(register.C); break;
            case 0x12: register.D = RL(register.D); break;
            case 0x13: register.E = RL(register.E); break;
            case 0x14: register.H = RL(register.H); break;
            case 0x15: register.L = RL(register.L); break;
            case 0x16: emulator.Write(register.HL, RL(emulator.Read(register.HL))); break;

            case 0x0F: register.A = RRC(register.A); break;// RRC n
            case 0x08: register.B = RRC(register.B); break;
            case 0x09: register.C = RRC(register.C); break;
            case 0x0A: register.D = RRC(register.D); break;
            case 0x0B: register.E = RRC(register.E); break;
            case 0x0C: register.H = RRC(register.H); break;
            case 0x0D: register.L = RRC(register.L); break;
            case 0x0E: emulator.Write(register.HL, RRC(emulator.Read(register.HL))); break;

            case 0x1F: register.A = RR(register.A); break;// RR n
            case 0x18: register.B = RR(register.B); break;
            case 0x19: register.C = RR(register.C); break;
            case 0x1A: register.D = RR(register.D); break;
            case 0x1B: register.E = RR(register.E); break;
            case 0x1C: register.H = RR(register.H); break;
            case 0x1D: register.L = RR(register.L); break;
            case 0x1E: emulator.Write(register.HL, RR(emulator.Read(register.HL))); break;

            case 0x27: register.A = SLA(register.A); break;// SLA n
            case 0x20: register.B = SLA(register.B); break;
            case 0x21: register.C = SLA(register.C); break;
            case 0x22: register.D = SLA(register.D); break;
            case 0x23: register.E = SLA(register.E); break;
            case 0x24: register.H = SLA(register.H); break;
            case 0x25: register.L = SLA(register.L); break;
            case 0x26: emulator.Write(register.HL, SLA(emulator.Read(register.HL))); break;

            case 0x2F: register.A = SRA(register.A); break;// SRA n
            case 0x28: register.B = SRA(register.B); break;
            case 0x29: register.C = SRA(register.C); break;
            case 0x2A: register.D = SRA(register.D); break;
            case 0x2B: register.E = SRA(register.E); break;
            case 0x2C: register.H = SRA(register.H); break;
            case 0x2D: register.L = SRA(register.L); break;
            case 0x2E: emulator.Write(register.HL, SRA(emulator.Read(register.HL))); break;

            case 0x3F: register.A = SRL(register.A); break;// SRL
            case 0x38: register.B = SRL(register.B); break;
            case 0x39: register.C = SRL(register.C); break;
            case 0x3A: register.D = SRL(register.D); break;
            case 0x3B: register.E = SRL(register.E); break;
            case 0x3C: register.H = SRL(register.H); break;
            case 0x3D: register.L = SRL(register.L); break;
            case 0x3E: emulator.Write(register.HL, SRL(emulator.Read(register.HL))); break;

            case 0x47: BIT(Bit0, register.A); break;// BIT 0,r
            case 0x40: BIT(Bit0, register.B); break;
            case 0x41: BIT(Bit0, register.C); break;
            case 0x42: BIT(Bit0, register.D); break;
            case 0x43: BIT(Bit0, register.E); break;
            case 0x44: BIT(Bit0, register.H); break;
            case 0x45: BIT(Bit0, register.L); break;
            case 0x46: BIT(Bit0, emulator.Read(register.HL)); break;

            case 0x4F: BIT(Bit1, register.A); break;// BIT 1,r
            case 0x48: BIT(Bit1, register.B); break;
            case 0x49: BIT(Bit1, register.C); break;
            case 0x4A: BIT(Bit1, register.D); break;
            case 0x4B: BIT(Bit1, register.E); break;
            case 0x4C: BIT(Bit1, register.H); break;
            case 0x4D: BIT(Bit1, register.L); break;
            case 0x4E: BIT(Bit1, emulator.Read(register.HL)); break;

            case 0x57: BIT(Bit2, register.A); break;// BIT 2,r
            case 0x50: BIT(Bit2, register.B); break;
            case 0x51: BIT(Bit2, register.C); break;
            case 0x52: BIT(Bit2, register.D); break;
            case 0x53: BIT(Bit2, register.E); break;
            case 0x54: BIT(Bit2, register.H); break;
            case 0x55: BIT(Bit2, register.L); break;
            case 0x56: BIT(Bit2, emulator.Read(register.HL)); break;

            case 0x5F: BIT(Bit3, register.A); break;// BIT 3,r
            case 0x58: BIT(Bit3, register.B); break;
            case 0x59: BIT(Bit3, register.C); break;
            case 0x5A: BIT(Bit3, register.D); break;
            case 0x5B: BIT(Bit3, register.E); break;
            case 0x5C: BIT(Bit3, register.H); break;
            case 0x5D: BIT(Bit3, register.L); break;
            case 0x5E: BIT(Bit3, emulator.Read(register.HL)); break;

            case 0x67: BIT(Bit4, register.A); break;// BIT 4,r
            case 0x60: BIT(Bit4, register.B); break;
            case 0x61: BIT(Bit4, register.C); break;
            case 0x62: BIT(Bit4, register.D); break;
            case 0x63: BIT(Bit4, register.E); break;
            case 0x64: BIT(Bit4, register.H); break;
            case 0x65: BIT(Bit4, register.L); break;
            case 0x66: BIT(Bit4, emulator.Read(register.HL)); break;

            case 0x6F: BIT(Bit5, register.A); break;// BIT 5,r
            case 0x68: BIT(Bit5, register.B); break;
            case 0x69: BIT(Bit5, register.C); break;
            case 0x6A: BIT(Bit5, register.D); break;
            case 0x6B: BIT(Bit5, register.E); break;
            case 0x6C: BIT(Bit5, register.H); break;
            case 0x6D: BIT(Bit5, register.L); break;
            case 0x6E: BIT(Bit5, emulator.Read(register.HL)); break;

            case 0x77: BIT(Bit6, register.A); break;// BIT 6,r
            case 0x70: BIT(Bit6, register.B); break;
            case 0x71: BIT(Bit6, register.C); break;
            case 0x72: BIT(Bit6, register.D); break;
            case 0x73: BIT(Bit6, register.E); break;
            case 0x74: BIT(Bit6, register.H); break;
            case 0x75: BIT(Bit6, register.L); break;
            case 0x76: BIT(Bit6, emulator.Read(register.HL)); break;

            case 0x7F: BIT(Bit7, register.A); break;// BIT 7,r
            case 0x78: BIT(Bit7, register.B); break;
            case 0x79: BIT(Bit7, register.C); break;
            case 0x7A: BIT(Bit7, register.D); break;
            case 0x7B: BIT(Bit7, register.E); break;
            case 0x7C: BIT(Bit7, register.H); break;
            case 0x7D: BIT(Bit7, register.L); break;
            case 0x7E: BIT(Bit7, emulator.Read(register.HL)); break;

            case 0xC7: register.A = SET(Bit0, register.A); break;// SET 0,r
            case 0xC0: register.B = SET(Bit0, register.B); break;
            case 0xC1: register.C = SET(Bit0, register.C); break;
            case 0xC2: register.D = SET(Bit0, register.D); break;
            case 0xC3: register.E = SET(Bit0, register.E); break;
            case 0xC4: register.H = SET(Bit0, register.H); break;
            case 0xC5: register.L = SET(Bit0, register.L); break;
            case 0xC6: emulator.Write(register.HL, SET(Bit0, emulator.Read(register.HL))); break;

            case 0xCF: register.A = SET(Bit1, register.A); break;// SET 1,r
            case 0xC8: register.B = SET(Bit1, register.B); break;
            case 0xC9: register.C = SET(Bit1, register.C); break;
            case 0xCA: register.D = SET(Bit1, register.D); break;
            case 0xCB: register.E = SET(Bit1, register.E); break;
            case 0xCC: register.H = SET(Bit1, register.H); break;
            case 0xCD: register.L = SET(Bit1, register.L); break;
            case 0xCE: emulator.Write(register.HL, SET(Bit1, emulator.Read(register.HL))); break;

            case 0xD7: register.A = SET(Bit2, register.A); break;// SET 2,r
            case 0xD0: register.B = SET(Bit2, register.B); break;
            case 0xD1: register.C = SET(Bit2, register.C); break;
            case 0xD2: register.D = SET(Bit2, register.D); break;
            case 0xD3: register.E = SET(Bit2, register.E); break;
            case 0xD4: register.H = SET(Bit2, register.H); break;
            case 0xD5: register.L = SET(Bit2, register.L); break;
            case 0xD6: emulator.Write(register.HL, SET(Bit2, emulator.Read(register.HL))); break;

            case 0xDF: register.A = SET(Bit3, register.A); break;// SET 3,r
            case 0xD8: register.B = SET(Bit3, register.B); break;
            case 0xD9: register.C = SET(Bit3, register.C); break;
            case 0xDA: register.D = SET(Bit3, register.D); break;
            case 0xDB: register.E = SET(Bit3, register.E); break;
            case 0xDC: register.H = SET(Bit3, register.H); break;
            case 0xDD: register.L = SET(Bit3, register.L); break;
            case 0xDE: emulator.Write(register.HL, SET(Bit3, emulator.Read(register.HL))); break;

            case 0xE7: register.A = SET(Bit4, register.A); break;// SET 4,r
            case 0xE0: register.B = SET(Bit4, register.B); break;
            case 0xE1: register.C = SET(Bit4, register.C); break;
            case 0xE2: register.D = SET(Bit4, register.D); break;
            case 0xE3: register.E = SET(Bit4, register.E); break;
            case 0xE4: register.H = SET(Bit4, register.H); break;
            case 0xE5: register.L = SET(Bit4, register.L); break;
            case 0xE6: emulator.Write(register.HL, SET(Bit4, emulator.Read(register.HL))); break;

            case 0xEF: register.A = SET(Bit5, register.A); break;// SET 5,r
            case 0xE8: register.B = SET(Bit5, register.B); break;
            case 0xE9: register.C = SET(Bit5, register.C); break;
            case 0xEA: register.D = SET(Bit5, register.D); break;
            case 0xEB: register.E = SET(Bit5, register.E); break;
            case 0xEC: register.H = SET(Bit5, register.H); break;
            case 0xED: register.L = SET(Bit5, register.L); break;
            case 0xEE: emulator.Write(register.HL, SET(Bit5, emulator.Read(register.HL))); break;

            case 0xF7: register.A = SET(Bit6, register.A); break;// SET 6,r
            case 0xF0: register.B = SET(Bit6, register.B); break;
            case 0xF1: register.C = SET(Bit6, register.C); break;
            case 0xF2: register.D = SET(Bit6, register.D); break;
            case 0xF3: register.E = SET(Bit6, register.E); break;
            case 0xF4: register.H = SET(Bit6, register.H); break;
            case 0xF5: register.L = SET(Bit6, register.L); break;
            case 0xF6: emulator.Write(register.HL, SET(Bit6, emulator.Read(register.HL))); break;

            case 0xFF: register.A = SET(Bit7, register.A); break;// SET 7,r
            case 0xF8: register.B = SET(Bit7, register.B); break;
            case 0xF9: register.C = SET(Bit7, register.C); break;
            case 0xFA: register.D = SET(Bit7, register.D); break;
            case 0xFB: register.E = SET(Bit7, register.E); break;
            case 0xFC: register.H = SET(Bit7, register.H); break;
            case 0xFD: register.L = SET(Bit7, register.L); break;
            case 0xFE: emulator.Write(register.HL, SET(Bit7, emulator.Read(register.HL))); break;

            case 0x87: register.A = RES(Bit0, register.A); break;// RES 0,r
            case 0x80: register.B = RES(Bit0, register.B); break;
            case 0x81: register.C = RES(Bit0, register.C); break;
            case 0x82: register.D = RES(Bit0, register.D); break;
            case 0x83: register.E = RES(Bit0, register.E); break;
            case 0x84: register.H = RES(Bit0, register.H); break;
            case 0x85: register.L = RES(Bit0, register.L); break;
            case 0x86: emulator.Write(register.HL, RES(Bit0, emulator.Read(register.HL))); break;

            case 0x8F: register.A = RES(Bit1, register.A); break;// RES 1,r
            case 0x88: register.B = RES(Bit1, register.B); break;
            case 0x89: register.C = RES(Bit1, register.C); break;
            case 0x8A: register.D = RES(Bit1, register.D); break;
            case 0x8B: register.E = RES(Bit1, register.E); break;
            case 0x8C: register.H = RES(Bit1, register.H); break;
            case 0x8D: register.L = RES(Bit1, register.L); break;
            case 0x8E: emulator.Write(register.HL, RES(Bit1, emulator.Read(register.HL))); break;

            case 0x97: register.A = RES(Bit2, register.A); break;// RES 2,r
            case 0x90: register.B = RES(Bit2, register.B); break;
            case 0x91: register.C = RES(Bit2, register.C); break;
            case 0x92: register.D = RES(Bit2, register.D); break;
            case 0x93: register.E = RES(Bit2, register.E); break;
            case 0x94: register.H = RES(Bit2, register.H); break;
            case 0x95: register.L = RES(Bit2, register.L); break;
            case 0x96: emulator.Write(register.HL, RES(Bit2, emulator.Read(register.HL))); break;

            case 0x9F: register.A = RES(Bit3, register.A); break;// RES 3,r
            case 0x98: register.B = RES(Bit3, register.B); break;
            case 0x99: register.C = RES(Bit3, register.C); break;
            case 0x9A: register.D = RES(Bit3, register.D); break;
            case 0x9B: register.E = RES(Bit3, register.E); break;
            case 0x9C: register.H = RES(Bit3, register.H); break;
            case 0x9D: register.L = RES(Bit3, register.L); break;
            case 0x9E: emulator.Write(register.HL, RES(Bit3, emulator.Read(register.HL))); break;

            case 0xA7: register.A = RES(Bit4, register.A); break;// RES 4,r
            case 0xA0: register.B = RES(Bit4, register.B); break;
            case 0xA1: register.C = RES(Bit4, register.C); break;
            case 0xA2: register.D = RES(Bit4, register.D); break;
            case 0xA3: register.E = RES(Bit4, register.E); break;
            case 0xA4: register.H = RES(Bit4, register.H); break;
            case 0xA5: register.L = RES(Bit4, register.L); break;
            case 0xA6: emulator.Write(register.HL, RES(Bit4, emulator.Read(register.HL))); break;

            case 0xAF: register.A = RES(Bit5, register.A); break;// RES 5,r
            case 0xA8: register.B = RES(Bit5, register.B); break;
            case 0xA9: register.C = RES(Bit5, register.C); break;
            case 0xAA: register.D = RES(Bit5, register.D); break;
            case 0xAB: register.E = RES(Bit5, register.E); break;
            case 0xAC: register.H = RES(Bit5, register.H); break;
            case 0xAD: register.L = RES(Bit5, register.L); break;
            case 0xAE: emulator.Write(register.HL, RES(Bit5, emulator.Read(register.HL))); break;

            case 0xB7: register.A = RES(Bit6, register.A); break;// RES 6,r
            case 0xB0: register.B = RES(Bit6, register.B); break;
            case 0xB1: register.C = RES(Bit6, register.C); break;
            case 0xB2: register.D = RES(Bit6, register.D); break;
            case 0xB3: register.E = RES(Bit6, register.E); break;
            case 0xB4: register.H = RES(Bit6, register.H); break;
            case 0xB5: register.L = RES(Bit6, register.L); break;
            case 0xB6: emulator.Write(register.HL, RES(Bit6, emulator.Read(register.HL))); break;

            case 0xBF: register.A = RES(Bit7, register.A); break;// RES 7,r
            case 0xB8: register.B = RES(Bit7, register.B); break;
            case 0xB9: register.C = RES(Bit7, register.C); break;
            case 0xBA: register.D = RES(Bit7, register.D); break;
            case 0xBB: register.E = RES(Bit7, register.E); break;
            case 0xBC: register.H = RES(Bit7, register.H); break;
            case 0xBD: register.L = RES(Bit7, register.L); break;
            case 0xBE: emulator.Write(register.HL, RES(Bit7, emulator.Read(register.HL))); break;
        }

        return Timings.CbTimes[op];
    }

    private void WriteSPtoAddress()
    {
        ushort address = NextShort();
        ushort data = register.SP;
        emulator.Write((ushort)(address + 1), (byte)(data >> 8));
        emulator.Write(address, (byte)data);
    }

    public void Push(ushort data)
    {
        register.SP--;
        emulator.Write(register.SP, data.High());
        register.SP--;
        emulator.Write(register.SP, data.Low());
    }

    public ushort Pop()
    {
        byte low = emulator.Read(register.SP++);
        byte high = emulator.Read(register.SP++);

        return low.Combine(high);
    }

    private byte NextByte()
    {
        return emulator.Read(register.PC++);
    }

    private ushort NextShort()
    {
        byte low = NextByte();
        byte high = NextByte();
        return low.Combine(high);
    }


    private void ADD(byte n)
    {
        int result = register.A + n;

        register.ZFlag = (byte)result == 0;
        register.NFlag = false;
        register.HFlag = ((register.A & 0xF) + (n & 0xF)) > 0xF;
        register.CFlag = (result >> 8) != 0;

        register.A = (byte)result;
    }

    private void ADC(byte n)
    {
        int carry = register.CFlag ? 1 : 0;
        int result = register.A + n + carry;

        register.ZFlag = (byte)result == 0;
        register.NFlag = false;
        if (register.CFlag)
        {
            register.HFlag = (register.A & 0xF) + (n & 0xF) >= 0xF;
        }
        else
        {
            register.HFlag = (register.A & 0xF) + (n & 0xF) > 0xF;
        }
        register.CFlag = (result >> 8) != 0;

        register.A = (byte)result;
    }

    private void SUB(byte n)
    {
        int result = register.A - n;

        register.ZFlag = (byte)result == 0;
        register.NFlag = true;
        register.HFlag = (register.A & 0xF) < (n & 0xF);
        register.CFlag = (result >> 8) != 0;

        register.A = (byte)result;
    }

    private void SBC(byte n)
    {
        int carry = register.CFlag ? 1 : 0;
        int result = register.A - n - carry;

        register.ZFlag = (byte)result == 0;
        register.NFlag = true;
        register.HFlag = (register.A & 0xF) < (n & 0xF) + carry;
        register.CFlag = (result >> 8) != 0;

        register.A = (byte)result;
    }

    private void AND(byte n)
    {
        byte result = (byte)(register.A & n);

        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = true;
        register.CFlag = false;

        register.A = result;
    }

    private void OR(byte n)
    {
        byte result = (byte)(register.A | n);

        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = false;

        register.A = result;
    }

    private void XOR(byte n)
    {
        byte result = (byte)(register.A ^ n);

        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = false;

        register.A = result;
    }

    private void CP(byte n)
    {
        int result = register.A - n;

        register.ZFlag = (byte)result == 0;
        register.NFlag = true;
        register.HFlag = (register.A & 0xF) < (n & 0xF);
        register.CFlag = (result >> 8) != 0;
    }

    private byte INC(byte n)
    {
        int result = n + 1;

        register.ZFlag = (byte)result == 0;
        register.NFlag = false;
        register.HFlag = ((n & 0xF) + (1 & 0xF)) > 0xF;

        return (byte)result;
    }

    private byte DEC(byte n)
    {
        int result = n - 1;

        register.ZFlag = (byte)result == 0;
        register.NFlag = true;
        register.HFlag = (n & 0xF) < (1 & 0xF);

        return (byte)result;
    }


    private void ADD(ushort nn)
    {
        int result = register.HL + nn;

        register.NFlag = false;
        register.HFlag = ((register.HL & 0xFFF) + (nn & 0xFFF)) > 0xFFF;
        register.CFlag = (result >> 16) != 0;

        register.HL = (ushort)result;
    }

    private ushort ADDS(ushort nn)
    {
        byte n = NextByte();

        register.ZFlag = false;
        register.NFlag = false;
        register.HFlag = (((byte)nn & 0xF) + (n & 0xF)) > 0xF;
        register.CFlag = (((byte)nn + n) >> 8) != 0;

        return (ushort)(nn + (sbyte)n);
    }



    private void CPL()
    {
        register.A = (byte)~register.A;

        register.NFlag = true;
        register.HFlag = true;
    }

    private void SCF()
    {
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = true;
    }

    private void CCF()
    {
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = !register.CFlag;
    }

    private byte SWAP(byte n)
    {
        byte result = (byte)(((n & 0xF0) >> 4) | ((n & 0x0F) << 4));

        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = false;

        return result;
    }

    private void DAA()
    {
        if (register.NFlag)
        {
            if (register.CFlag)
            {
                register.A -= 0x60;
            }
            if (register.HFlag)
            {
                register.A -= 0x6;
            }
        }
        else
        {
            if (register.CFlag || (register.A > 0x99))
            {
                register.A += 0x60;
                register.CFlag = true;
            }
            if (register.HFlag || (register.A & 0xF) > 0x9)
            {
                register.A += 0x6;
            }
        }

        register.HFlag = false;
        register.ZFlag = register.A == 0;
    }




    private void RLCA()
    {
        register.ZFlag = false;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (byte)(register.A & Bit7) != 0;

        register.A = (byte)((register.A << 1) | (register.A >> 7));
    }

    private void RLA()
    {
        int carry = register.CFlag ? 1 : 0;

        register.ZFlag = false;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (byte)(register.A & Bit7) != 0;

        register.A = (byte)((register.A << 1) | carry);
    }

    private void RRCA()
    {
        register.ZFlag = false;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (byte)(register.A & Bit0) != 0;

        register.A = (byte)((register.A >> 1) | (register.A << 7));
    }

    private void RRA()
    {
        int carry = register.CFlag ? Bit7 : 0;

        register.ZFlag = false;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (byte)(register.A & Bit0) != 0;

        register.A = (byte)((register.A >> 1) | carry);
    }

    private byte RLC(byte n)
    {
        byte result = (byte)((n << 1) | (n >> 7));

        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (n & Bit7) != 0;

        return result;
    }

    private byte RL(byte n)
    {
        int carry = register.CFlag ? 1 : 0;
        byte result = (byte)((n << 1) | carry);

        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (n & Bit7) != 0;

        return result;
    }

    private byte RRC(byte n)
    {
        byte result = (byte)((n >> 1) | (n << 7));

        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (n & Bit0) != 0;

        return result;
    }

    private byte RR(byte n)
    {
        int carry = register.CFlag ? Bit7 : 0;
        byte result = (byte)((n >> 1) | carry);

        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (n & Bit0) != 0;

        return result;
    }

    private byte SLA(byte n)
    {
        byte result = (byte)(n << 1);
        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (n & Bit7) != 0;
        return result;
    }

    private byte SRA(byte n)
    {
        byte result = (byte)((n >> 1) | (n & Bit7));

        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (n & 1) != 0;

        return result;
    }

    private byte SRL(byte n)
    {
        byte result = (byte)(n >> 1);

        register.ZFlag = result == 0;
        register.NFlag = false;
        register.HFlag = false;
        register.CFlag = (n & 1) != 0;

        return result;
    }

    private void BIT(byte bit, byte r)
    {
        register.ZFlag = (r & bit) == 0;
        register.NFlag = false;
        register.HFlag = true;
    }

    private static byte SET(byte bit, byte r)
    {
        return (byte)(r | bit);
    }

    private static byte RES(byte bit, byte r)
    {
        return (byte)(r & ~bit);
    }


    private void JP(ushort nn)
    {
        register.PC = nn;
    }

    private void JP(bool condition)
    {
        if (condition)
        {
            ushort address = register.PC;
            register.PC = (ushort)((emulator.Read((ushort)(address + 1)) << 8) | emulator.Read(address));
        }
        else
        {
            register.PC += 2;
        }
    }

    private void JR(bool condition)
    {
        if (condition)
        {
            JP((ushort)(register.PC + (sbyte)emulator.Read(register.PC)));
            register.PC += 1;
        }
        else
        {
            register.PC += 1;
        }
    }

    public void CALL(bool condition)
    {
        if (condition)
        {
            Push((ushort)(register.PC + 2));
            ushort address = register.PC;
            JP((ushort)((emulator.Read((ushort)(address + 1)) << 8) | emulator.Read(address)));
        }
        else
        {
            register.PC += 2;
        }
    }

    private void RET(bool condition)
    {
        if (condition)
        {
            register.PC = Pop();
        }
    }

    private void RST(byte n)
    {
        Push(register.PC);
        register.PC = n;
    }


    public void Disable()
    {
        emulator.Ime = false;
    }

    public void EnableInterrupt()
    {
        emulator.ImeDelay = true;
    }

    public void Halt()
    {
        if (emulator.Ime == false)
        {
            if ((emulator.IE & emulator.IF & 0b11111) == 0)
            {
                emulator.Halted = true;
                register.PC--;
            }
            else
            {
                emulator.HaltBug = true;
            }
        }
    }

    public void RequestInterrupt(byte bit)
    {
        emulator.IF = emulator.IF.SetBit(bit, true);
    }

    public void HandleInterupts()
    {
        for (byte i = 0; i < 5; i++)
        {
            if ((((emulator.IE & emulator.IF) >> i) & 1) == 1)
            {
                if (emulator.Halted)
                {
                    register.PC++;
                    emulator.Halted = false;
                }
                if (emulator.Ime)
                {
                    Push(register.PC);
                    register.PC = (ushort)(64 + (8 * i));
                    emulator.Ime = false;
                    emulator.IF = emulator.IF.SetBit((byte)(Bit0 << i), false);
                }
            }
        }

        emulator.Ime |= emulator.ImeDelay;
        emulator.ImeDelay = false;
    }
}
