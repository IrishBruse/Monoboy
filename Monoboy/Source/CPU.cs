using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Monoboy.Utility;

namespace Monoboy
{
    public class CPU
    {
        Bus bus;

        public bool halted;

        byte opcode;

        public CPU(Bus bus)
        {
            this.bus = bus;
        }

        public byte Step()
        {
            bus.interrupt.HandleInterupts();

            if(halted == true) return 4;

            opcode = NextByte();

            bus.trace.Add(opcode);

            byte n;
            ushort nn;

            switch(opcode)
            {
                case 0x78: bus.register.A = bus.register.B; return 4;
                case 0x79: bus.register.A = bus.register.C; return 4;
                case 0x7A: bus.register.A = bus.register.D; return 4;
                case 0x7B: bus.register.A = bus.register.E; return 4;
                case 0x7C: bus.register.A = bus.register.H; return 4;
                case 0x7D: bus.register.A = bus.register.L; return 4;
                case 0xF2: bus.register.A = bus.Read((ushort)(0xFF00 + bus.register.C)); return 8;
                case 0x7E: bus.register.A = bus.Read(bus.register.HL); return 8;
                case 0x0A: bus.register.A = bus.Read(bus.register.BC); return 8;
                case 0x1A: bus.register.A = bus.Read(bus.register.DE); return 8;
                case 0xFA: bus.register.A = bus.Read(NextShort()); return 16;
                case 0x3E: bus.register.A = NextByte(); return 8;
                case 0x3A: bus.register.A = bus.Read(bus.register.HL); bus.register.HL--; return 8;
                case 0x2A: bus.register.A = bus.Read(bus.register.HL); bus.register.HL++; return 8;
                case 0xF0: bus.register.A = bus.Read((ushort)(0xFF00 + NextByte())); return 12;

                case 0x47: bus.register.B = bus.register.A; return 4;
                case 0x41: bus.register.B = bus.register.C; return 4;
                case 0x42: bus.register.B = bus.register.D; return 4;
                case 0x43: bus.register.B = bus.register.E; return 4;
                case 0x44: bus.register.B = bus.register.H; return 4;
                case 0x45: bus.register.B = bus.register.L; return 4;
                case 0x46: bus.register.B = bus.Read(bus.register.HL); return 8;
                case 0x06: bus.register.B = NextByte(); return 8;

                case 0x4F: bus.register.C = bus.register.A; return 4;
                case 0x48: bus.register.C = bus.register.B; return 4;
                case 0x4A: bus.register.C = bus.register.D; return 4;
                case 0x4B: bus.register.C = bus.register.E; return 4;
                case 0x4C: bus.register.C = bus.register.H; return 4;
                case 0x4D: bus.register.C = bus.register.L; return 4;
                case 0x4E: bus.register.C = bus.Read(bus.register.HL); return 8;
                case 0x0E: bus.register.C = NextByte(); return 8;

                case 0x57: bus.register.D = bus.register.A; return 4;
                case 0x50: bus.register.D = bus.register.B; return 4;
                case 0x51: bus.register.D = bus.register.C; return 4;
                case 0x53: bus.register.D = bus.register.E; return 4;
                case 0x54: bus.register.D = bus.register.H; return 4;
                case 0x55: bus.register.D = bus.register.L; return 4;
                case 0x56: bus.register.D = bus.Read(bus.register.HL); return 8;
                case 0x16: bus.register.D = NextByte(); return 8;

                case 0x5F: bus.register.E = bus.register.A; return 4;
                case 0x58: bus.register.E = bus.register.B; return 4;
                case 0x59: bus.register.E = bus.register.C; return 4;
                case 0x5A: bus.register.E = bus.register.D; return 4;
                case 0x5C: bus.register.E = bus.register.H; return 4;
                case 0x5D: bus.register.E = bus.register.L; return 4;
                case 0x5E: bus.register.E = bus.Read(bus.register.HL); return 8;
                case 0x1E: bus.register.E = NextByte(); return 8;

                case 0x67: bus.register.H = bus.register.A; return 4;
                case 0x60: bus.register.H = bus.register.B; return 4;
                case 0x61: bus.register.H = bus.register.C; return 4;
                case 0x62: bus.register.H = bus.register.D; return 4;
                case 0x63: bus.register.H = bus.register.E; return 4;
                case 0x65: bus.register.H = bus.register.L; return 4;
                case 0x66: bus.register.H = bus.Read(bus.register.HL); return 8;
                case 0x26: bus.register.H = NextByte(); return 8;

                case 0x6F: bus.register.L = bus.register.A; return 4;
                case 0x68: bus.register.L = bus.register.B; return 4;
                case 0x69: bus.register.L = bus.register.C; return 4;
                case 0x6A: bus.register.L = bus.register.D; return 4;
                case 0x6B: bus.register.L = bus.register.E; return 4;
                case 0x6C: bus.register.L = bus.register.H; return 4;
                case 0x6E: bus.register.L = bus.Read(bus.register.HL); return 8;
                case 0x2E: bus.register.L = NextByte(); return 8;

                case 0xE2: bus.Write((ushort)(0xFF00 + bus.register.C), bus.register.A); return 8;

                case 0x77: bus.Write(bus.register.HL, bus.register.A); return 8;
                case 0x70: bus.Write(bus.register.HL, bus.register.B); return 8;
                case 0x71: bus.Write(bus.register.HL, bus.register.C); return 8;
                case 0x72: bus.Write(bus.register.HL, bus.register.D); return 8;
                case 0x73: bus.Write(bus.register.HL, bus.register.E); return 8;
                case 0x74: bus.Write(bus.register.HL, bus.register.H); return 8;
                case 0x75: bus.Write(bus.register.HL, bus.register.L); return 8;
                case 0x32: bus.Write(bus.register.HL, bus.register.A); bus.register.HL--; return 8;
                case 0x22: bus.Write(bus.register.HL, bus.register.A); bus.register.HL++; return 8;
                case 0x36: bus.Write(bus.register.HL, NextByte()); return 12;

                case 0x02: bus.Write(bus.register.BC, bus.register.A); return 8;
                case 0x12: bus.Write(bus.register.DE, bus.register.A); return 8;
                case 0xEA: bus.Write(NextShort(), bus.register.A); return 16;
                case 0xE0: bus.Write((ushort)(0xFF00 + NextByte()), bus.register.A); return 12;

                case 0x01: bus.register.BC = NextShort(); return 12;
                case 0x11: bus.register.DE = NextShort(); return 12;
                case 0x21: bus.register.HL = NextShort(); return 12;
                case 0x31: bus.register.SP = NextShort(); return 12;

                case 0xF9: bus.register.SP = bus.register.HL; return 8;
                case 0xF8: LD_HL(); return 12;
                case 0x08: nn = NextShort(); bus.Write((ushort)(nn + 1), bus.register.SP.High()); bus.Write(nn, bus.register.SP.Low()); return 20;
                case 0xF5: Push(bus.register.AF); return 16;
                case 0xC5: Push(bus.register.BC); return 16;
                case 0xD5: Push(bus.register.DE); return 16;
                case 0xE5: Push(bus.register.HL); return 16;

                case 0xF1: bus.register.AF = (ushort)(Pop() & 0xFFF0); return 12;
                case 0xC1: bus.register.BC = Pop(); return 12;
                case 0xD1: bus.register.DE = Pop(); return 12;
                case 0xE1: bus.register.HL = Pop(); return 12;

                case 0x87: ADD(bus.register.A); return 4;
                case 0x80: ADD(bus.register.B); return 4;
                case 0x81: ADD(bus.register.C); return 4;
                case 0x82: ADD(bus.register.D); return 4;
                case 0x83: ADD(bus.register.E); return 4;
                case 0x84: ADD(bus.register.H); return 4;
                case 0x85: ADD(bus.register.L); return 4;
                case 0x86: ADD(bus.Read(bus.register.HL)); return 8;
                case 0xC6: ADD(NextByte()); return 8;

                case 0x8F: ADD(bus.register.A, true); return 4;
                case 0x88: ADD(bus.register.B, true); return 4;
                case 0x89: ADD(bus.register.C, true); return 4;
                case 0x8A: ADD(bus.register.D, true); return 4;
                case 0x8B: ADD(bus.register.E, true); return 4;
                case 0x8C: ADD(bus.register.H, true); return 4;
                case 0x8D: ADD(bus.register.L, true); return 4;
                case 0x8E: ADD(bus.Read(bus.register.HL), true); return 8;
                case 0xCE: ADD(NextByte(), true); return 8;

                case 0x97: SUB(bus.register.A); return 4;
                case 0x90: SUB(bus.register.B); return 4;
                case 0x91: SUB(bus.register.C); return 4;
                case 0x92: SUB(bus.register.D); return 4;
                case 0x93: SUB(bus.register.E); return 4;
                case 0x94: SUB(bus.register.H); return 4;
                case 0x95: SUB(bus.register.L); return 4;
                case 0x96: SUB(bus.Read(bus.register.HL)); return 8;
                case 0xD6: SUB(NextByte()); return 8;

                case 0x9F: SUB(bus.register.A, true); return 4;
                case 0x98: SUB(bus.register.B, true); return 4;
                case 0x99: SUB(bus.register.C, true); return 4;
                case 0x9A: SUB(bus.register.D, true); return 4;
                case 0x9B: SUB(bus.register.E, true); return 4;
                case 0x9C: SUB(bus.register.H, true); return 4;
                case 0x9D: SUB(bus.register.L, true); return 4;
                case 0x9E: SUB(bus.Read(bus.register.HL), true); return 8;
                case 0xDE: SUB(NextByte()); return 8;

                case 0xA7: AND(bus.register.A); return 4;
                case 0xA0: AND(bus.register.B); return 4;
                case 0xA1: AND(bus.register.C); return 4;
                case 0xA2: AND(bus.register.D); return 4;
                case 0xA3: AND(bus.register.E); return 4;
                case 0xA4: AND(bus.register.H); return 4;
                case 0xA5: AND(bus.register.L); return 4;
                case 0xA6: AND(bus.Read(bus.register.HL)); return 8;
                case 0xE6: AND(NextByte()); return 8;

                case 0xB7: OR(bus.register.A); return 4;
                case 0xB0: OR(bus.register.B); return 4;
                case 0xB1: OR(bus.register.C); return 4;
                case 0xB2: OR(bus.register.D); return 4;
                case 0xB3: OR(bus.register.E); return 4;
                case 0xB4: OR(bus.register.H); return 4;
                case 0xB5: OR(bus.register.L); return 4;
                case 0xB6: OR(bus.Read(bus.register.HL)); return 8;
                case 0xF6: OR(NextByte()); return 8;

                case 0xAF: XOR(bus.register.A); return 4;
                case 0xA8: XOR(bus.register.B); return 4;
                case 0xA9: XOR(bus.register.C); return 4;
                case 0xAA: XOR(bus.register.D); return 4;
                case 0xAB: XOR(bus.register.E); return 4;
                case 0xAC: XOR(bus.register.H); return 4;
                case 0xAD: XOR(bus.register.L); return 4;
                case 0xAE: XOR(bus.Read(bus.register.HL)); return 8;
                case 0xEE: XOR(NextByte()); return 8;

                case 0xBF: CP(bus.register.A); return 4;
                case 0xB8: CP(bus.register.B); return 4;
                case 0xB9: CP(bus.register.C); return 4;
                case 0xBA: CP(bus.register.D); return 4;
                case 0xBB: CP(bus.register.E); return 4;
                case 0xBC: CP(bus.register.H); return 4;
                case 0xBD: CP(bus.register.L); return 4;
                case 0xBE: CP(bus.Read(bus.register.HL)); return 8;
                case 0xFE: CP(NextByte()); return 8;

                case 0x3C: bus.register.A = INC(bus.register.A); return 4;
                case 0x04: bus.register.B = INC(bus.register.B); return 4;
                case 0x0C: bus.register.C = INC(bus.register.C); return 4;
                case 0x14: bus.register.D = INC(bus.register.D); return 4;
                case 0x1C: bus.register.E = INC(bus.register.E); return 4;
                case 0x24: bus.register.H = INC(bus.register.H); return 4;
                case 0x2C: bus.register.L = INC(bus.register.L); return 4;
                case 0x34: bus.Write(bus.register.HL, INC(bus.Read(bus.register.HL))); return 12;

                case 0x3D: bus.register.A = DEC(bus.register.A); return 4;
                case 0x05: bus.register.B = DEC(bus.register.B); return 4;
                case 0x0D: bus.register.C = DEC(bus.register.C); return 4;
                case 0x15: bus.register.D = DEC(bus.register.D); return 4;
                case 0x1D: bus.register.E = DEC(bus.register.E); return 4;
                case 0x25: bus.register.H = DEC(bus.register.H); return 4;
                case 0x2D: bus.register.L = DEC(bus.register.L); return 4;
                case 0x35: bus.Write(bus.register.HL, DEC(bus.Read(bus.register.HL))); return 12;

                case 0x09: ADD_HL(bus.register.BC); return 8;
                case 0x19: ADD_HL(bus.register.DE); return 8;
                case 0x29: ADD_HL(bus.register.HL); return 8;
                case 0x39: ADD_HL(bus.register.SP); return 8;

                case 0xE8: ADD_SP(NextByte()); return 16;

                case 0x03: bus.register.BC += 1; return 8;
                case 0x13: bus.register.DE += 1; return 8;
                case 0x23: bus.register.HL += 1; return 8;
                case 0x33: bus.register.SP += 1; return 8;

                case 0x0B: bus.register.BC -= 1; return 8;
                case 0x1B: bus.register.DE -= 1; return 8;
                case 0x2B: bus.register.HL -= 1; return 8;
                case 0x3B: bus.register.SP -= 1; return 8;

                case 0xCB: return SubOpcodeTable();

                case 0x27: DAA(); return 4;

                case 0x2F: CPL(); return 4;
                case 0x37: SCF(); return 4;
                case 0x3F: CCF(); return 4;

                case 0x07: bus.register.A = RotateLeft(bus.register.A, false, false); return 4;
                case 0x17: bus.register.A = RotateLeft(bus.register.A, true, false); return 4;
                case 0x0F: bus.register.A = RotateRight(bus.register.A, false, false); return 4;
                case 0x1F: bus.register.A = RotateRight(bus.register.A, true, false); return 4;

                case 0xC3: JP(NextShort()); return 12;
                case 0xC2: nn = NextShort(); if(bus.register.GetFlag(Flag.Zero) == false) { JP(nn); return 16; } else { return 12; }
                case 0xCA: nn = NextShort(); if(bus.register.GetFlag(Flag.Zero) == true) { JP(nn); return 16; } else { return 12; }
                case 0xD2: nn = NextShort(); if(bus.register.GetFlag(Flag.FullCarry) == false) { JP(nn); return 16; } else { return 12; }
                case 0xDA: nn = NextShort(); if(bus.register.GetFlag(Flag.FullCarry) == true) { JP(nn); return 16; } else { return 12; }
                case 0xE9: JP(bus.register.HL); return 4;

                case 0x18: n = NextByte(); JR((sbyte)n); return 8;

                case 0x20: n = NextByte(); if(bus.register.GetFlag(Flag.Zero) == false) { JR((sbyte)n); return 16; } else { return 12; }
                case 0x28: n = NextByte(); if(bus.register.GetFlag(Flag.Zero) == true) { JR((sbyte)n); return 16; } else { return 12; }
                case 0x30: n = NextByte(); if(bus.register.GetFlag(Flag.FullCarry) == false) { JR((sbyte)n); return 16; } else { return 12; }
                case 0x38: n = NextByte(); if(bus.register.GetFlag(Flag.FullCarry) == true) { JR((sbyte)n); return 16; } else { return 12; }

                case 0xCD: CALL(NextShort()); return 12;

                case 0xC4: if(bus.register.GetFlag(Flag.Zero) == false) { CALL(NextShort()); return 24; } else { return 12; }
                case 0xCC: if(bus.register.GetFlag(Flag.Zero) == true) { CALL(NextShort()); return 24; } else { return 12; }
                case 0xD4: if(bus.register.GetFlag(Flag.FullCarry) == false) { CALL(NextShort()); return 24; } else { return 12; }
                case 0xDC: if(bus.register.GetFlag(Flag.FullCarry) == true) { CALL(NextShort()); return 24; } else { return 12; }

                case 0xC7: RST(0x00); return 16;
                case 0xCF: RST(0x08); return 16;
                case 0xD7: RST(0x10); return 16;
                case 0xDF: RST(0x18); return 16;
                case 0xE7: RST(0x20); return 16;
                case 0xEF: RST(0x28); return 16;
                case 0xF7: RST(0x30); return 16;
                case 0xFF: RST(0x38); return 16;

                case 0xC9: RET(); return 16;
                case 0xC0: if(bus.register.GetFlag(Flag.Zero) == false) { RET(); return 20; } else { return 8; }
                case 0xC8: if(bus.register.GetFlag(Flag.Zero) == true) { RET(); return 20; } else { return 8; }
                case 0xD0: if(bus.register.GetFlag(Flag.FullCarry) == false) { RET(); return 20; } else { return 8; }
                case 0xD8: if(bus.register.GetFlag(Flag.FullCarry) == true) { RET(); return 20; } else { return 8; }

                case 0x7F: case 0x49: case 0x52: case 0x5B: case 0x64: case 0x6D: case 0x40: case 0x00: return 4;

                case 0x76: halted = true; return 4;
                case 0x10: return 4;//Stop
                case 0xF3: bus.interrupt.Disable(); return 4;
                case 0xFB: bus.interrupt.Enable(); return 4;

                case 0xD9: bus.interrupt.Enable(); RET(); return 8;

                case 0xD3: throw new Exception("Illegal Instruction : 0xD3");
                case 0xDB: throw new Exception("Illegal Instruction : 0xDB");
                case 0xDD: throw new Exception("Illegal Instruction : 0xDD");
                case 0xE3: throw new Exception("Illegal Instruction : 0xE3");
                case 0xE4: throw new Exception("Illegal Instruction : 0xE4");
                case 0xEB: throw new Exception("Illegal Instruction : 0xEB");
                case 0xEC: throw new Exception("Illegal Instruction : 0xEC");
                case 0xED: throw new Exception("Illegal Instruction : 0xED");
                case 0xF4: throw new Exception("Illegal Instruction : 0xF4");
                case 0xFC: throw new Exception("Illegal Instruction : 0xFC");
                case 0xFD: throw new Exception("Illegal Instruction : 0xFD");
                default: throw new Exception("Illegal Instruction : " + opcode);
            }
        }


        byte SubOpcodeTable()
        {
            opcode = NextByte();

            bus.trace.Add(opcode);

            switch(opcode)
            {
                case 0x07: bus.register.A = RotateLeft(bus.register.A, false, true); return 8;
                case 0x00: bus.register.B = RotateLeft(bus.register.B, false, true); return 8;
                case 0x01: bus.register.C = RotateLeft(bus.register.C, false, true); return 8;
                case 0x02: bus.register.D = RotateLeft(bus.register.D, false, true); return 8;
                case 0x03: bus.register.E = RotateLeft(bus.register.E, false, true); return 8;
                case 0x04: bus.register.H = RotateLeft(bus.register.H, false, true); return 8;
                case 0x05: bus.register.L = RotateLeft(bus.register.L, false, true); return 8;
                case 0x06: bus.Write(bus.register.HL, RotateLeft(bus.Read(bus.register.HL), false, true)); return 16;

                case 0x17: bus.register.A = RotateLeft(bus.register.A, true, true); return 8;
                case 0x10: bus.register.B = RotateLeft(bus.register.B, true, true); return 8;
                case 0x11: bus.register.C = RotateLeft(bus.register.C, true, true); return 8;
                case 0x12: bus.register.D = RotateLeft(bus.register.D, true, true); return 8;
                case 0x13: bus.register.E = RotateLeft(bus.register.E, true, true); return 8;
                case 0x14: bus.register.H = RotateLeft(bus.register.H, true, true); return 8;
                case 0x15: bus.register.L = RotateLeft(bus.register.L, true, true); return 8;
                case 0x16: bus.Write(bus.register.HL, RotateLeft(bus.Read(bus.register.HL), true, true)); return 16;

                case 0x0F: bus.register.A = RotateRight(bus.register.A, false, true); return 8;
                case 0x08: bus.register.B = RotateRight(bus.register.B, false, true); return 8;
                case 0x09: bus.register.C = RotateRight(bus.register.C, false, true); return 8;
                case 0x0A: bus.register.D = RotateRight(bus.register.D, false, true); return 8;
                case 0x0B: bus.register.E = RotateRight(bus.register.E, false, true); return 8;
                case 0x0C: bus.register.H = RotateRight(bus.register.H, false, true); return 8;
                case 0x0D: bus.register.L = RotateRight(bus.register.L, false, true); return 8;
                case 0x0E: bus.Write(bus.register.HL, RotateRight(bus.Read(bus.register.HL), false, true)); return 16;

                case 0x1F: bus.register.A = RotateRight(bus.register.A, true, true); return 8;
                case 0x18: bus.register.B = RotateRight(bus.register.B, true, true); return 8;
                case 0x19: bus.register.C = RotateRight(bus.register.C, true, true); return 8;
                case 0x1A: bus.register.D = RotateRight(bus.register.D, true, true); return 8;
                case 0x1B: bus.register.E = RotateRight(bus.register.E, true, true); return 8;
                case 0x1C: bus.register.H = RotateRight(bus.register.H, true, true); return 8;
                case 0x1D: bus.register.L = RotateRight(bus.register.L, true, true); return 8;
                case 0x1E: bus.Write(bus.register.HL, RotateRight(bus.Read(bus.register.HL), true, true)); return 16;

                case 0x27: bus.register.A = ShiftLeft(bus.register.A); return 8;
                case 0x20: bus.register.B = ShiftLeft(bus.register.B); return 8;
                case 0x21: bus.register.C = ShiftLeft(bus.register.C); return 8;
                case 0x22: bus.register.D = ShiftLeft(bus.register.D); return 8;
                case 0x23: bus.register.E = ShiftLeft(bus.register.E); return 8;
                case 0x24: bus.register.H = ShiftLeft(bus.register.H); return 8;
                case 0x25: bus.register.L = ShiftLeft(bus.register.L); return 8;
                case 0x26: bus.Write(bus.register.HL, ShiftLeft(bus.Read(bus.register.HL))); return 16;

                case 0x2F: bus.register.A = ShiftRight(bus.register.A, true); return 8;
                case 0x28: bus.register.B = ShiftRight(bus.register.B, true); return 8;
                case 0x29: bus.register.C = ShiftRight(bus.register.C, true); return 8;
                case 0x2A: bus.register.D = ShiftRight(bus.register.D, true); return 8;
                case 0x2B: bus.register.E = ShiftRight(bus.register.E, true); return 8;
                case 0x2C: bus.register.H = ShiftRight(bus.register.H, true); return 8;
                case 0x2D: bus.register.L = ShiftRight(bus.register.L, true); return 8;
                case 0x2E: bus.Write(bus.register.HL, ShiftRight(bus.Read(bus.register.HL), true)); return 16;

                case 0x3F: bus.register.A = ShiftRight(bus.register.A, false); return 8;
                case 0x38: bus.register.B = ShiftRight(bus.register.B, false); return 8;
                case 0x39: bus.register.C = ShiftRight(bus.register.C, false); return 8;
                case 0x3A: bus.register.D = ShiftRight(bus.register.D, false); return 8;
                case 0x3B: bus.register.E = ShiftRight(bus.register.E, false); return 8;
                case 0x3C: bus.register.H = ShiftRight(bus.register.H, false); return 8;
                case 0x3D: bus.register.L = ShiftRight(bus.register.L, false); return 8;
                case 0x3E: bus.Write(bus.register.HL, ShiftRight(bus.Read(bus.register.HL), false)); return 16;

                case 0x37: bus.register.A = SWAP(bus.register.A); return 8;
                case 0x30: bus.register.B = SWAP(bus.register.B); return 8;
                case 0x31: bus.register.C = SWAP(bus.register.C); return 8;
                case 0x32: bus.register.D = SWAP(bus.register.D); return 8;
                case 0x33: bus.register.E = SWAP(bus.register.E); return 8;
                case 0x34: bus.register.H = SWAP(bus.register.H); return 8;
                case 0x35: bus.register.L = SWAP(bus.register.L); return 8;
                case 0x36: bus.Write(bus.register.HL, SWAP(bus.Read(bus.register.HL))); return 16;

                case 0x47: BIT((byte)Bit.Bit0, bus.register.A); return 8;
                case 0x40: BIT((byte)Bit.Bit0, bus.register.B); return 8;
                case 0x41: BIT((byte)Bit.Bit0, bus.register.C); return 8;
                case 0x42: BIT((byte)Bit.Bit0, bus.register.D); return 8;
                case 0x43: BIT((byte)Bit.Bit0, bus.register.E); return 8;
                case 0x44: BIT((byte)Bit.Bit0, bus.register.H); return 8;
                case 0x45: BIT((byte)Bit.Bit0, bus.register.L); return 8;
                case 0x46: BIT((byte)Bit.Bit0, bus.Read(bus.register.HL)); return 12;

                case 0x4F: BIT((byte)Bit.Bit1, bus.register.A); return 8;
                case 0x48: BIT((byte)Bit.Bit1, bus.register.B); return 8;
                case 0x49: BIT((byte)Bit.Bit1, bus.register.C); return 8;
                case 0x4A: BIT((byte)Bit.Bit1, bus.register.D); return 8;
                case 0x4B: BIT((byte)Bit.Bit1, bus.register.E); return 8;
                case 0x4C: BIT((byte)Bit.Bit1, bus.register.H); return 8;
                case 0x4D: BIT((byte)Bit.Bit1, bus.register.L); return 8;
                case 0x4E: BIT((byte)Bit.Bit1, bus.Read(bus.register.HL)); return 12;

                case 0x57: BIT((byte)Bit.Bit2, bus.register.A); return 8;
                case 0x50: BIT((byte)Bit.Bit2, bus.register.B); return 8;
                case 0x51: BIT((byte)Bit.Bit2, bus.register.C); return 8;
                case 0x52: BIT((byte)Bit.Bit2, bus.register.D); return 8;
                case 0x53: BIT((byte)Bit.Bit2, bus.register.E); return 8;
                case 0x54: BIT((byte)Bit.Bit2, bus.register.H); return 8;
                case 0x55: BIT((byte)Bit.Bit2, bus.register.L); return 8;
                case 0x56: BIT((byte)Bit.Bit2, bus.Read(bus.register.HL)); return 12;

                case 0x5F: BIT((byte)Bit.Bit3, bus.register.A); return 8;
                case 0x58: BIT((byte)Bit.Bit3, bus.register.B); return 8;
                case 0x59: BIT((byte)Bit.Bit3, bus.register.C); return 8;
                case 0x5A: BIT((byte)Bit.Bit3, bus.register.D); return 8;
                case 0x5B: BIT((byte)Bit.Bit3, bus.register.E); return 8;
                case 0x5C: BIT((byte)Bit.Bit3, bus.register.H); return 8;
                case 0x5D: BIT((byte)Bit.Bit3, bus.register.L); return 8;
                case 0x5E: BIT((byte)Bit.Bit3, bus.Read(bus.register.HL)); return 12;

                case 0x67: BIT((byte)Bit.Bit4, bus.register.A); return 8;
                case 0x60: BIT((byte)Bit.Bit4, bus.register.B); return 8;
                case 0x61: BIT((byte)Bit.Bit4, bus.register.C); return 8;
                case 0x62: BIT((byte)Bit.Bit4, bus.register.D); return 8;
                case 0x63: BIT((byte)Bit.Bit4, bus.register.E); return 8;
                case 0x64: BIT((byte)Bit.Bit4, bus.register.H); return 8;
                case 0x65: BIT((byte)Bit.Bit4, bus.register.L); return 8;
                case 0x66: BIT((byte)Bit.Bit4, bus.Read(bus.register.HL)); return 12;

                case 0x6F: BIT((byte)Bit.Bit5, bus.register.A); return 8;
                case 0x68: BIT((byte)Bit.Bit5, bus.register.B); return 8;
                case 0x69: BIT((byte)Bit.Bit5, bus.register.C); return 8;
                case 0x6A: BIT((byte)Bit.Bit5, bus.register.D); return 8;
                case 0x6B: BIT((byte)Bit.Bit5, bus.register.E); return 8;
                case 0x6C: BIT((byte)Bit.Bit5, bus.register.H); return 8;
                case 0x6D: BIT((byte)Bit.Bit5, bus.register.L); return 8;
                case 0x6E: BIT((byte)Bit.Bit5, bus.Read(bus.register.HL)); return 12;

                case 0x77: BIT((byte)Bit.Bit6, bus.register.A); return 8;
                case 0x70: BIT((byte)Bit.Bit6, bus.register.B); return 8;
                case 0x71: BIT((byte)Bit.Bit6, bus.register.C); return 8;
                case 0x72: BIT((byte)Bit.Bit6, bus.register.D); return 8;
                case 0x73: BIT((byte)Bit.Bit6, bus.register.E); return 8;
                case 0x74: BIT((byte)Bit.Bit6, bus.register.H); return 8;
                case 0x75: BIT((byte)Bit.Bit6, bus.register.L); return 8;
                case 0x76: BIT((byte)Bit.Bit6, bus.Read(bus.register.HL)); return 12;

                case 0x7F: BIT((byte)Bit.Bit7, bus.register.A); return 8;
                case 0x78: BIT((byte)Bit.Bit7, bus.register.B); return 8;
                case 0x79: BIT((byte)Bit.Bit7, bus.register.C); return 8;
                case 0x7A: BIT((byte)Bit.Bit7, bus.register.D); return 8;
                case 0x7B: BIT((byte)Bit.Bit7, bus.register.E); return 8;
                case 0x7C: BIT((byte)Bit.Bit7, bus.register.H); return 8;
                case 0x7D: BIT((byte)Bit.Bit7, bus.register.L); return 8;
                case 0x7E: BIT((byte)Bit.Bit7, bus.Read(bus.register.HL)); return 12;

                case 0xC7: bus.register.A = SET((byte)Bit.Bit0, bus.register.A); return 8;
                case 0xC0: bus.register.B = SET((byte)Bit.Bit0, bus.register.B); return 8;
                case 0xC1: bus.register.C = SET((byte)Bit.Bit0, bus.register.C); return 8;
                case 0xC2: bus.register.D = SET((byte)Bit.Bit0, bus.register.D); return 8;
                case 0xC3: bus.register.E = SET((byte)Bit.Bit0, bus.register.E); return 8;
                case 0xC4: bus.register.H = SET((byte)Bit.Bit0, bus.register.H); return 8;
                case 0xC5: bus.register.L = SET((byte)Bit.Bit0, bus.register.L); return 8;
                case 0xC6: bus.Write(bus.register.HL, SET((byte)Bit.Bit0, bus.Read(bus.register.HL))); return 16;

                case 0xCF: bus.register.A = SET((byte)Bit.Bit1, bus.register.A); return 8;
                case 0xC8: bus.register.B = SET((byte)Bit.Bit1, bus.register.B); return 8;
                case 0xC9: bus.register.C = SET((byte)Bit.Bit1, bus.register.C); return 8;
                case 0xCA: bus.register.D = SET((byte)Bit.Bit1, bus.register.D); return 8;
                case 0xCB: bus.register.E = SET((byte)Bit.Bit1, bus.register.E); return 8;
                case 0xCC: bus.register.H = SET((byte)Bit.Bit1, bus.register.H); return 8;
                case 0xCD: bus.register.L = SET((byte)Bit.Bit1, bus.register.L); return 8;
                case 0xCE: bus.Write(bus.register.HL, SET((byte)Bit.Bit1, bus.Read(bus.register.HL))); return 16;

                case 0xD7: bus.register.A = SET((byte)Bit.Bit2, bus.register.A); return 8;
                case 0xD0: bus.register.B = SET((byte)Bit.Bit2, bus.register.B); return 8;
                case 0xD1: bus.register.C = SET((byte)Bit.Bit2, bus.register.C); return 8;
                case 0xD2: bus.register.D = SET((byte)Bit.Bit2, bus.register.D); return 8;
                case 0xD3: bus.register.E = SET((byte)Bit.Bit2, bus.register.E); return 8;
                case 0xD4: bus.register.H = SET((byte)Bit.Bit2, bus.register.H); return 8;
                case 0xD5: bus.register.L = SET((byte)Bit.Bit2, bus.register.L); return 8;
                case 0xD6: bus.Write(bus.register.HL, SET((byte)Bit.Bit2, bus.Read(bus.register.HL))); return 16;

                case 0xDF: bus.register.A = SET((byte)Bit.Bit3, bus.register.A); return 8;
                case 0xD8: bus.register.B = SET((byte)Bit.Bit3, bus.register.B); return 8;
                case 0xD9: bus.register.C = SET((byte)Bit.Bit3, bus.register.C); return 8;
                case 0xDA: bus.register.D = SET((byte)Bit.Bit3, bus.register.D); return 8;
                case 0xDB: bus.register.E = SET((byte)Bit.Bit3, bus.register.E); return 8;
                case 0xDC: bus.register.H = SET((byte)Bit.Bit3, bus.register.H); return 8;
                case 0xDD: bus.register.L = SET((byte)Bit.Bit3, bus.register.L); return 8;
                case 0xDE: bus.Write(bus.register.HL, SET((byte)Bit.Bit3, bus.Read(bus.register.HL))); return 16;

                case 0xE7: bus.register.A = SET((byte)Bit.Bit4, bus.register.A); return 8;
                case 0xE0: bus.register.B = SET((byte)Bit.Bit4, bus.register.B); return 8;
                case 0xE1: bus.register.C = SET((byte)Bit.Bit4, bus.register.C); return 8;
                case 0xE2: bus.register.D = SET((byte)Bit.Bit4, bus.register.D); return 8;
                case 0xE3: bus.register.E = SET((byte)Bit.Bit4, bus.register.E); return 8;
                case 0xE4: bus.register.H = SET((byte)Bit.Bit4, bus.register.H); return 8;
                case 0xE5: bus.register.L = SET((byte)Bit.Bit4, bus.register.L); return 8;
                case 0xE6: bus.Write(bus.register.HL, SET((byte)Bit.Bit4, bus.Read(bus.register.HL))); return 16;

                case 0xEF: bus.register.A = SET((byte)Bit.Bit5, bus.register.A); return 8;
                case 0xE8: bus.register.B = SET((byte)Bit.Bit5, bus.register.B); return 8;
                case 0xE9: bus.register.C = SET((byte)Bit.Bit5, bus.register.C); return 8;
                case 0xEA: bus.register.D = SET((byte)Bit.Bit5, bus.register.D); return 8;
                case 0xEB: bus.register.E = SET((byte)Bit.Bit5, bus.register.E); return 8;
                case 0xEC: bus.register.H = SET((byte)Bit.Bit5, bus.register.H); return 8;
                case 0xED: bus.register.L = SET((byte)Bit.Bit5, bus.register.L); return 8;
                case 0xEE: bus.Write(bus.register.HL, SET((byte)Bit.Bit5, bus.Read(bus.register.HL))); return 16;

                case 0xF7: bus.register.A = SET((byte)Bit.Bit6, bus.register.A); return 8;
                case 0xF0: bus.register.B = SET((byte)Bit.Bit6, bus.register.B); return 8;
                case 0xF1: bus.register.C = SET((byte)Bit.Bit6, bus.register.C); return 8;
                case 0xF2: bus.register.D = SET((byte)Bit.Bit6, bus.register.D); return 8;
                case 0xF3: bus.register.E = SET((byte)Bit.Bit6, bus.register.E); return 8;
                case 0xF4: bus.register.H = SET((byte)Bit.Bit6, bus.register.H); return 8;
                case 0xF5: bus.register.L = SET((byte)Bit.Bit6, bus.register.L); return 8;
                case 0xF6: bus.Write(bus.register.HL, SET((byte)Bit.Bit6, bus.Read(bus.register.HL))); return 16;

                case 0xFF: bus.register.A = SET((byte)Bit.Bit7, bus.register.A); return 8;
                case 0xF8: bus.register.B = SET((byte)Bit.Bit7, bus.register.B); return 8;
                case 0xF9: bus.register.C = SET((byte)Bit.Bit7, bus.register.C); return 8;
                case 0xFA: bus.register.D = SET((byte)Bit.Bit7, bus.register.D); return 8;
                case 0xFB: bus.register.E = SET((byte)Bit.Bit7, bus.register.E); return 8;
                case 0xFC: bus.register.H = SET((byte)Bit.Bit7, bus.register.H); return 8;
                case 0xFD: bus.register.L = SET((byte)Bit.Bit7, bus.register.L); return 8;
                case 0xFE: bus.Write(bus.register.HL, SET((byte)Bit.Bit7, bus.Read(bus.register.HL))); return 16;

                case 0x87: bus.register.A = RES((byte)Bit.Bit0, bus.register.A); return 8;
                case 0x80: bus.register.B = RES((byte)Bit.Bit0, bus.register.B); return 8;
                case 0x81: bus.register.C = RES((byte)Bit.Bit0, bus.register.C); return 8;
                case 0x82: bus.register.D = RES((byte)Bit.Bit0, bus.register.D); return 8;
                case 0x83: bus.register.E = RES((byte)Bit.Bit0, bus.register.E); return 8;
                case 0x84: bus.register.H = RES((byte)Bit.Bit0, bus.register.H); return 8;
                case 0x85: bus.register.L = RES((byte)Bit.Bit0, bus.register.L); return 8;
                case 0x86: bus.Write(bus.register.HL, RES((byte)Bit.Bit0, bus.Read(bus.register.HL))); return 16;

                case 0x8F: bus.register.A = RES((byte)Bit.Bit1, bus.register.A); return 8;
                case 0x88: bus.register.B = RES((byte)Bit.Bit1, bus.register.B); return 8;
                case 0x89: bus.register.C = RES((byte)Bit.Bit1, bus.register.C); return 8;
                case 0x8A: bus.register.D = RES((byte)Bit.Bit1, bus.register.D); return 8;
                case 0x8B: bus.register.E = RES((byte)Bit.Bit1, bus.register.E); return 8;
                case 0x8C: bus.register.H = RES((byte)Bit.Bit1, bus.register.H); return 8;
                case 0x8D: bus.register.L = RES((byte)Bit.Bit1, bus.register.L); return 8;
                case 0x8E: bus.Write(bus.register.HL, RES((byte)Bit.Bit1, bus.Read(bus.register.HL))); return 16;

                case 0x97: bus.register.A = RES((byte)Bit.Bit2, bus.register.A); return 8;
                case 0x90: bus.register.B = RES((byte)Bit.Bit2, bus.register.B); return 8;
                case 0x91: bus.register.C = RES((byte)Bit.Bit2, bus.register.C); return 8;
                case 0x92: bus.register.D = RES((byte)Bit.Bit2, bus.register.D); return 8;
                case 0x93: bus.register.E = RES((byte)Bit.Bit2, bus.register.E); return 8;
                case 0x94: bus.register.H = RES((byte)Bit.Bit2, bus.register.H); return 8;
                case 0x95: bus.register.L = RES((byte)Bit.Bit2, bus.register.L); return 8;
                case 0x96: bus.Write(bus.register.HL, RES((byte)Bit.Bit2, bus.Read(bus.register.HL))); return 16;

                case 0x9F: bus.register.A = RES((byte)Bit.Bit3, bus.register.A); return 8;
                case 0x98: bus.register.B = RES((byte)Bit.Bit3, bus.register.B); return 8;
                case 0x99: bus.register.C = RES((byte)Bit.Bit3, bus.register.C); return 8;
                case 0x9A: bus.register.D = RES((byte)Bit.Bit3, bus.register.D); return 8;
                case 0x9B: bus.register.E = RES((byte)Bit.Bit3, bus.register.E); return 8;
                case 0x9C: bus.register.H = RES((byte)Bit.Bit3, bus.register.H); return 8;
                case 0x9D: bus.register.L = RES((byte)Bit.Bit3, bus.register.L); return 8;
                case 0x9E: bus.Write(bus.register.HL, RES((byte)Bit.Bit3, bus.Read(bus.register.HL))); return 16;

                case 0xA7: bus.register.A = RES((byte)Bit.Bit4, bus.register.A); return 8;
                case 0xA0: bus.register.B = RES((byte)Bit.Bit4, bus.register.B); return 8;
                case 0xA1: bus.register.C = RES((byte)Bit.Bit4, bus.register.C); return 8;
                case 0xA2: bus.register.D = RES((byte)Bit.Bit4, bus.register.D); return 8;
                case 0xA3: bus.register.E = RES((byte)Bit.Bit4, bus.register.E); return 8;
                case 0xA4: bus.register.H = RES((byte)Bit.Bit4, bus.register.H); return 8;
                case 0xA5: bus.register.L = RES((byte)Bit.Bit4, bus.register.L); return 8;
                case 0xA6: bus.Write(bus.register.HL, RES((byte)Bit.Bit4, bus.Read(bus.register.HL))); return 16;

                case 0xAF: bus.register.A = RES((byte)Bit.Bit5, bus.register.A); return 8;
                case 0xA8: bus.register.B = RES((byte)Bit.Bit5, bus.register.B); return 8;
                case 0xA9: bus.register.C = RES((byte)Bit.Bit5, bus.register.C); return 8;
                case 0xAA: bus.register.D = RES((byte)Bit.Bit5, bus.register.D); return 8;
                case 0xAB: bus.register.E = RES((byte)Bit.Bit5, bus.register.E); return 8;
                case 0xAC: bus.register.H = RES((byte)Bit.Bit5, bus.register.H); return 8;
                case 0xAD: bus.register.L = RES((byte)Bit.Bit5, bus.register.L); return 8;
                case 0xAE: bus.Write(bus.register.HL, RES((byte)Bit.Bit5, bus.Read(bus.register.HL))); return 16;

                case 0xB7: bus.register.A = RES((byte)Bit.Bit6, bus.register.A); return 8;
                case 0xB0: bus.register.B = RES((byte)Bit.Bit6, bus.register.B); return 8;
                case 0xB1: bus.register.C = RES((byte)Bit.Bit6, bus.register.C); return 8;
                case 0xB2: bus.register.D = RES((byte)Bit.Bit6, bus.register.D); return 8;
                case 0xB3: bus.register.E = RES((byte)Bit.Bit6, bus.register.E); return 8;
                case 0xB4: bus.register.H = RES((byte)Bit.Bit6, bus.register.H); return 8;
                case 0xB5: bus.register.L = RES((byte)Bit.Bit6, bus.register.L); return 8;
                case 0xB6: bus.Write(bus.register.HL, RES((byte)Bit.Bit6, bus.Read(bus.register.HL))); return 16;

                case 0xBF: bus.register.A = RES((byte)Bit.Bit7, bus.register.A); return 8;
                case 0xB8: bus.register.B = RES((byte)Bit.Bit7, bus.register.B); return 8;
                case 0xB9: bus.register.C = RES((byte)Bit.Bit7, bus.register.C); return 8;
                case 0xBA: bus.register.D = RES((byte)Bit.Bit7, bus.register.D); return 8;
                case 0xBB: bus.register.E = RES((byte)Bit.Bit7, bus.register.E); return 8;
                case 0xBC: bus.register.H = RES((byte)Bit.Bit7, bus.register.H); return 8;
                case 0xBD: bus.register.L = RES((byte)Bit.Bit7, bus.register.L); return 8;
                case 0xBE: bus.Write(bus.register.HL, RES((byte)Bit.Bit7, bus.Read(bus.register.HL))); return 16;

                default: throw new Exception("Illegal Instruction : " + opcode);
            }
        }


        #region Commands

        #region 8-Bit

        void ADD(byte n, bool addCarry = false)
        {
            byte carry = (byte)(addCarry && bus.register.GetFlag(Flag.FullCarry) ? 1 : 0);
            byte result = (byte)(bus.register.A + n + carry);

            bus.register.SetFlag(Flag.Zero, result == 0);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, (bus.register.A & 0xF) + (n & 0xF) + carry > 0xF);
            bus.register.SetFlag(Flag.FullCarry, (bus.register.A + n + carry) > 0xFF);

            bus.register.A = result;
        }

        void SUB(byte n, bool subCarry = false)
        {
            byte carry = (byte)(subCarry && bus.register.GetFlag(Flag.FullCarry) ? 1 : 0);
            byte result = (byte)(bus.register.A - n - carry);
            bus.register.SetFlag(Flag.Zero, result == 0);
            bus.register.SetFlag(Flag.Negative, true);
            bus.register.SetFlag(Flag.HalfCarry, ((short)(bus.register.A & 0xF)) - ((short)(n & 0xF)) - carry < 0);
            bus.register.SetFlag(Flag.FullCarry, (short)(bus.register.A - n - carry) < 0);

            bus.register.A = result;
        }

        void AND(byte n)
        {
            byte result = (byte)(bus.register.A & n);
            bus.register.SetFlag(Flag.Zero, result == 0);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, true);
            bus.register.SetFlag(Flag.FullCarry, false);

            bus.register.A = result;
        }

        void OR(byte n)
        {
            byte result = (byte)(bus.register.A | n);
            bus.register.SetFlag(Flag.Zero, result == 0);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, false);
            bus.register.SetFlag(Flag.FullCarry, false);

            bus.register.A = result;
        }

        void XOR(byte n)
        {
            byte result = (byte)(bus.register.A ^ n);
            bus.register.SetFlag(Flag.Zero, result == 0);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, false);
            bus.register.SetFlag(Flag.FullCarry, false);

            bus.register.A = result;
        }

        void CP(byte n)
        {
            byte result = bus.register.A;
            SUB(n);

            bus.register.A = result;
        }

        byte INC(byte n)
        {
            byte result = (byte)(n + 1);
            bus.register.SetFlag(Flag.Zero, result == 0);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, (ushort)(n & 0xF) + 1 > 0xF);

            return result;
        }

        byte DEC(byte n)
        {
            byte result = (byte)(n - 1);
            bus.register.SetFlag(Flag.Zero, result == 0);
            bus.register.SetFlag(Flag.Negative, true);
            bus.register.SetFlag(Flag.HalfCarry, (short)(n & 0xF) - 1 < 0);

            return result;
        }

        void CPL()
        {
            bus.register.A = (byte)~bus.register.A;
            bus.register.SetFlag(Flag.Negative, true);
            bus.register.SetFlag(Flag.HalfCarry, true);
        }

        void SCF()
        {
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, false);
            bus.register.SetFlag(Flag.FullCarry, true);
        }

        void CCF()
        {
            byte bit = ((byte)(bus.register.GetFlag(Flag.FullCarry) ? 1 : 0));
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, false);
            bus.register.SetFlag(Flag.FullCarry, (bit ^ 1) == 1);
        }

        #endregion

        #region 16-Bit

        void LD_HL()
        {
            ushort result = (ushort)(((short)bus.register.SP) + (sbyte)NextByte());
            bus.register.SetFlag(Flag.Zero, false);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, (result & 0xF) < (bus.register.SP & 0xF));
            bus.register.SetFlag(Flag.FullCarry, (result & 0xFF) < (bus.register.SP & 0xFF));

            bus.register.HL = result;
        }

        void ADD_HL(ushort n)
        {
            ushort result = (ushort)(bus.register.HL + n);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, (((bus.register.HL & 0xFFF) + (n & 0xFFF)) & 0x1000) != 0);
            bus.register.SetFlag(Flag.FullCarry, bus.register.HL > 0xFFFF - n);

            bus.register.HL = result;
        }

        void ADD_SP(byte n)
        {
            short signedValue = (sbyte)n;
            ushort result = (ushort)(((short)bus.register.SP) + signedValue);
            bus.register.SetFlag(Flag.Zero, false);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, (result & 0xF) < (bus.register.SP & 0xF));
            bus.register.SetFlag(Flag.FullCarry, (result & 0xFF) < (bus.register.SP & 0xFF));

            bus.register.SP = result;
        }

        public void Push(ushort data)
        {
            bus.register.SP -= 1;
            bus.Write(bus.register.SP, data.High());
            bus.register.SP -= 1;
            bus.Write(bus.register.SP, data.Low());
        }

        public ushort Pop()
        {
            byte lower = bus.Read(bus.register.SP);
            bus.register.SP += 1;
            byte high = bus.Read(bus.register.SP);
            bus.register.SP += 1;
            return lower.ToShort(high);
        }

        #endregion

        #region Bit opcodes

        byte RotateLeft(byte n, bool includeCarry = false, bool updateZero = false)
        {
            byte bit7 = (byte)(n >> 7);
            byte result;

            if(includeCarry)
            {
                result = (byte)((n << 1) | (byte)(bus.register.GetFlag(Flag.FullCarry) ? 1 : 0));
            }
            else
            {
                result = (byte)((n << 1) | (n >> 7));
            }

            bus.register.SetFlag(Flag.FullCarry, (bit7 == 1));
            bus.register.SetFlag(Flag.HalfCarry, false);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.Zero, (result == 0 && updateZero));
            return result;
        }

        byte RotateRight(byte n, bool includeCarry = false, bool updateZero = false)
        {
            byte bit1 = (byte)(n & 1);
            byte result;

            if(includeCarry)
            {
                result = (byte)((n >> 1) | (byte)(bus.register.GetFlag(Flag.FullCarry) ? 1 : 0) << 7);
            }
            else
            {
                result = (byte)((n >> 1) | (n << 7));
            }

            bus.register.SetFlag(Flag.FullCarry, (bit1 == 1));
            bus.register.SetFlag(Flag.HalfCarry, false);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.Zero, (result == 0 && updateZero));
            return result;
        }

        byte ShiftLeft(byte n)
        {
            byte result = (byte)(n << 1);
            byte bit7 = (byte)(n >> 7);
            bus.register.SetFlag(Flag.FullCarry, bit7 == 1);
            bus.register.SetFlag(Flag.HalfCarry, false);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.Zero, result == 0);
            return result;
        }

        byte ShiftRight(byte n, bool keepBit7)
        {
            byte result;

            if(keepBit7)
            {
                result = (byte)((n >> 1) | (n & 0x80));
            }
            else
            {
                result = (byte)(n >> 1);
            }

            bus.register.SetFlag(Flag.FullCarry, (n & 1) == 1);
            bus.register.SetFlag(Flag.HalfCarry, false);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.Zero, result == 0);
            return result;
        }

        void BIT(byte bit, byte r)
        {
            bus.register.SetFlag(Flag.Zero, (bit & ~r) != 0);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, true);
        }

        byte SET(byte bit, byte r)
        {
            return (byte)(r | bit);
        }

        byte RES(byte bit, byte r)
        {
            return (byte)((~bit) & r);
        }

        #endregion

        #region Misc
        byte NextByte()
        {
            byte result = bus.Read(bus.register.PC);
            bus.register.PC++;

            return result;
        }

        ushort NextShort()
        {
            byte low = NextByte();
            byte high = NextByte();
            return low.ToShort(high);
        }

        void DAA()
        {
            byte a = bus.register.A;
            byte adjust = (byte)(bus.register.GetFlag(Flag.FullCarry) ? 0x60 : 0x00);

            if(bus.register.GetFlag(Flag.HalfCarry))
            {
                adjust |= 0x06;
            }

            if(bus.register.GetFlag(Flag.Negative) == false)
            {
                if((a & 0x0F) > 0x09)
                {
                    adjust |= 0x06;
                }

                if(a > 0x99)
                {
                    adjust |= 0x60;
                }

                a += adjust;
            }
            else
            {
                a -= adjust;
            }

            bus.register.SetFlag(Flag.FullCarry, adjust >= 0x60);
            bus.register.SetFlag(Flag.HalfCarry, false);
            bus.register.SetFlag(Flag.Zero, a == 0);
            bus.register.A = a;
        }

        byte SWAP(byte n)
        {
            byte result = n.Swap();
            bus.register.SetFlag(Flag.Zero, result == 0);
            bus.register.SetFlag(Flag.Negative, false);
            bus.register.SetFlag(Flag.HalfCarry, false);
            bus.register.SetFlag(Flag.FullCarry, false);

            return result;
        }

        void JP(ushort nn)
        {
            bus.register.PC = nn;
        }

        void JR(sbyte n)
        {
            short address = (short)((short)bus.register.PC + n);
            JP((ushort)address);
        }

        public void CALL(ushort nn)
        {
            Push(bus.register.PC);
            bus.register.PC = nn;
        }

        void RST(byte n)
        {
            CALL((ushort)n);
        }

        void RET()
        {
            bus.register.PC = Pop();
        }
        #endregion

        #endregion
    }
}