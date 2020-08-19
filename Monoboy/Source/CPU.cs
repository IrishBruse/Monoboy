using System;
using Monoboy.Constants;
using Monoboy.Utility;
using static Monoboy.Utility.Helper;

namespace Monoboy
{
    public class Cpu
    {
        public bool halted;
        public bool haltBug;
        private Bus bus;

        private Register Reg { get => bus.register; }

        private byte opcode;

        public Cpu(Bus bus)
        {
            this.bus = bus;
        }

        public byte Step()
        {
            opcode = NextByte();

            if(haltBug)
            {
                haltBug = false;
                Reg.PC--;
            }

            switch(opcode)
            {
                // fine
                #region 8-Bit Loads

                // LD nn,n
                case 0x06: Reg.B = NextByte(); return 8;
                case 0x0E: Reg.C = NextByte(); return 8;
                case 0x16: Reg.D = NextByte(); return 8;
                case 0x1E: Reg.E = NextByte(); return 8;
                case 0x26: Reg.H = NextByte(); return 8;
                case 0x2E: Reg.L = NextByte(); return 8;

                // LD A,r2
                case 0x7F: Reg.A = Reg.A; return 4;
                case 0x78: Reg.A = Reg.B; return 4;
                case 0x79: Reg.A = Reg.C; return 4;
                case 0x7A: Reg.A = Reg.D; return 4;
                case 0x7B: Reg.A = Reg.E; return 4;
                case 0x7C: Reg.A = Reg.H; return 4;
                case 0x7D: Reg.A = Reg.L; return 4;
                case 0xFA: Reg.A = bus.Read(NextShort()); return 16;

                // LD B,r2
                case 0x47: Reg.B = Reg.A; return 4;
                case 0x40: Reg.B = Reg.B; return 4;
                case 0x41: Reg.B = Reg.C; return 4;
                case 0x42: Reg.B = Reg.D; return 4;
                case 0x43: Reg.B = Reg.E; return 4;
                case 0x44: Reg.B = Reg.H; return 4;
                case 0x45: Reg.B = Reg.L; return 4;
                case 0x46: Reg.B = bus.Read(Reg.HL); return 8;
                // Fine

                // LD C,r2
                case 0x4F: Reg.C = Reg.A; return 4;
                case 0x48: Reg.C = Reg.B; return 4;
                case 0x49: Reg.C = Reg.C; return 4;
                case 0x4A: Reg.C = Reg.D; return 4;
                case 0x4B: Reg.C = Reg.E; return 4;
                case 0x4C: Reg.C = Reg.H; return 4;
                case 0x4D: Reg.C = Reg.L; return 4;
                case 0x4E: Reg.C = bus.Read(Reg.HL); return 8;

                // LD D,r2
                case 0x57: Reg.D = Reg.A; return 4;
                case 0x50: Reg.D = Reg.B; return 4;
                case 0x51: Reg.D = Reg.C; return 4;
                case 0x52: Reg.D = Reg.D; return 4;
                case 0x53: Reg.D = Reg.E; return 4;
                case 0x54: Reg.D = Reg.H; return 4;
                case 0x55: Reg.D = Reg.L; return 4;
                case 0x56: Reg.D = bus.Read(Reg.HL); return 8;

                // LD E,r2
                case 0x5F: Reg.E = Reg.A; return 4;
                case 0x58: Reg.E = Reg.B; return 4;
                case 0x59: Reg.E = Reg.C; return 4;
                case 0x5A: Reg.E = Reg.D; return 4;
                case 0x5B: Reg.E = Reg.E; return 4;
                case 0x5C: Reg.E = Reg.H; return 4;
                case 0x5D: Reg.E = Reg.L; return 4;
                case 0x5E: Reg.E = bus.Read(Reg.HL); return 8;

                // LD H,r2
                case 0x67: Reg.H = Reg.A; return 4;
                case 0x60: Reg.H = Reg.B; return 4;
                case 0x61: Reg.H = Reg.C; return 4;
                case 0x62: Reg.H = Reg.D; return 4;
                case 0x63: Reg.H = Reg.E; return 4;
                case 0x64: Reg.H = Reg.H; return 4;
                case 0x65: Reg.H = Reg.L; return 4;
                case 0x66: Reg.H = bus.Read(Reg.HL); return 8;

                // LD L,r2
                case 0x6F: Reg.L = Reg.A; return 4;
                case 0x68: Reg.L = Reg.B; return 4;
                case 0x69: Reg.L = Reg.C; return 4;
                case 0x6A: Reg.L = Reg.D; return 4;
                case 0x6B: Reg.L = Reg.E; return 4;
                case 0x6C: Reg.L = Reg.H; return 4;
                case 0x6D: Reg.L = Reg.L; return 4;
                case 0x6E: Reg.L = bus.Read(Reg.HL); return 8;

                // LD (HL),r2
                case 0x77: bus.Write(Reg.HL, Reg.A); return 8;
                case 0x70: bus.Write(Reg.HL, Reg.B); return 8;
                case 0x71: bus.Write(Reg.HL, Reg.C); return 8;
                case 0x72: bus.Write(Reg.HL, Reg.D); return 8;
                case 0x73: bus.Write(Reg.HL, Reg.E); return 8;
                case 0x74: bus.Write(Reg.HL, Reg.H); return 8;
                case 0x75: bus.Write(Reg.HL, Reg.L); return 8;
                case 0x36: bus.Write(Reg.HL, NextByte()); return 12;

                // LD A,n
                case 0x0A: Reg.A = bus.Read(Reg.BC); return 8;
                case 0x1A: Reg.A = bus.Read(Reg.DE); return 8;
                case 0x7E: Reg.A = bus.Read(Reg.HL); return 8;
                case 0x3E: Reg.A = NextByte(); return 8;

                // LD n,A
                case 0x02: bus.Write(Reg.BC, Reg.A); return 8;
                case 0x12: bus.Write(Reg.DE, Reg.A); return 8;
                case 0xEA: bus.Write(NextShort(), Reg.A); return 16;

                // LD A,(C)
                case 0xF2: Reg.A = bus.Read((ushort)(0xFF00 + Reg.C)); return 8;

                // LD (C),A
                case 0xE2: bus.Write((ushort)(0xFF00 + Reg.C), Reg.A); return 8;

                // LD A,(HL-)
                case 0x3A: Reg.A = bus.Read(Reg.HL--); return 8;

                // LD (HL-),A
                case 0x32: bus.Write(Reg.HL--, Reg.A); return 8;

                // LD A,(HL+)
                case 0x2A: Reg.A = bus.Read(Reg.HL++); return 8;

                // LD (HL+),A
                case 0x22: bus.Write(Reg.HL++, Reg.A); return 8;

                // LDH (n),A
                case 0xE0: bus.Write((ushort)(0xFF00 + NextByte()), Reg.A); return 12;

                // LDH A,(n)
                case 0xF0: Reg.A = bus.Read((ushort)(0xFF00 + NextByte())); return 12;

                #endregion

                // fine
                #region 16-Bit Loads

                // LD n,nn
                case 0x01: Reg.BC = NextShort(); return 12;
                case 0x11: Reg.DE = NextShort(); return 12;
                case 0x21: Reg.HL = NextShort(); return 12;
                case 0x31: Reg.SP = NextShort(); return 12;

                // LD SP,HL
                case 0xF9: Reg.SP = Reg.HL; return 8;

                // LD HL,SP+n
                case 0xF8: LDHL(); return 12;

                // LD (nn),SP
                case 0x08: bus.WriteShort(NextShort(), Reg.SP); return 20;

                // Push nn
                case 0xF5: Push(Reg.AF); return 16;
                case 0xC5: Push(Reg.BC); return 16;
                case 0xD5: Push(Reg.DE); return 16;
                case 0xE5: Push(Reg.HL); return 16;

                // Pop nn
                case 0xF1: Reg.AF = Pop(); return 12;
                case 0xC1: Reg.BC = Pop(); return 12;
                case 0xD1: Reg.DE = Pop(); return 12;
                case 0xE1: Reg.HL = Pop(); return 12;

                #endregion

                #region 8-Bit ALU

                // ADD A,n
                case 0x87: ADD(Reg.A); return 4;
                case 0x80: ADD(Reg.B); return 4;
                case 0x81: ADD(Reg.C); return 4;
                case 0x82: ADD(Reg.D); return 4;
                case 0x83: ADD(Reg.E); return 4;
                case 0x84: ADD(Reg.H); return 4;
                case 0x85: ADD(Reg.L); return 4;
                case 0x86: ADD(bus.Read(Reg.HL)); return 8;
                case 0xC6: ADD(NextByte()); return 8;

                // ADC A,n
                case 0x8F: ADC(Reg.A); return 4;
                case 0x88: ADC(Reg.B); return 4;
                case 0x89: ADC(Reg.C); return 4;
                case 0x8A: ADC(Reg.D); return 4;
                case 0x8B: ADC(Reg.E); return 4;
                case 0x8C: ADC(Reg.H); return 4;
                case 0x8D: ADC(Reg.L); return 4;
                case 0x8E: ADC(bus.Read(Reg.HL)); return 8;
                case 0xCE: ADC(NextByte()); return 8;

                // SUB A,n
                case 0x97: SUB(Reg.A); return 4;
                case 0x90: SUB(Reg.B); return 4;
                case 0x91: SUB(Reg.C); return 4;
                case 0x92: SUB(Reg.D); return 4;
                case 0x93: SUB(Reg.E); return 4;
                case 0x94: SUB(Reg.H); return 4;
                case 0x95: SUB(Reg.L); return 4;
                case 0x96: SUB(bus.Read(Reg.HL)); return 8;
                case 0xD6: SUB(NextByte()); return 8;

                // SBC A,n
                case 0x9F: SBC(Reg.A); return 4;
                case 0x98: SBC(Reg.B); return 4;
                case 0x99: SBC(Reg.C); return 4;
                case 0x9A: SBC(Reg.D); return 4;
                case 0x9B: SBC(Reg.E); return 4;
                case 0x9C: SBC(Reg.H); return 4;
                case 0x9D: SBC(Reg.L); return 4;
                case 0x9E: SBC(bus.Read(Reg.HL)); return 8;
                case 0xDE: SBC(NextByte()); return 8;

                // AND n
                case 0xA7: AND(Reg.A); return 4;
                case 0xA0: AND(Reg.B); return 4;
                case 0xA1: AND(Reg.C); return 4;
                case 0xA2: AND(Reg.D); return 4;
                case 0xA3: AND(Reg.E); return 4;
                case 0xA4: AND(Reg.H); return 4;
                case 0xA5: AND(Reg.L); return 4;
                case 0xA6: AND(bus.Read(Reg.HL)); return 8;
                case 0xE6: AND(NextByte()); return 8;

                // OR n
                case 0xB7: OR(Reg.A); return 4;
                case 0xB0: OR(Reg.B); return 4;
                case 0xB1: OR(Reg.C); return 4;
                case 0xB2: OR(Reg.D); return 4;
                case 0xB3: OR(Reg.E); return 4;
                case 0xB4: OR(Reg.H); return 4;
                case 0xB5: OR(Reg.L); return 4;
                case 0xB6: OR(bus.Read(Reg.HL)); return 8;
                case 0xF6: OR(NextByte()); return 8;

                // XOR n
                case 0xAF: XOR(Reg.A); return 4;
                case 0xA8: XOR(Reg.B); return 4;
                case 0xA9: XOR(Reg.C); return 4;
                case 0xAA: XOR(Reg.D); return 4;
                case 0xAB: XOR(Reg.E); return 4;
                case 0xAC: XOR(Reg.H); return 4;
                case 0xAD: XOR(Reg.L); return 4;
                case 0xAE: XOR(bus.Read(Reg.HL)); return 8;
                case 0xEE: XOR(NextByte()); return 8;

                // CP n
                case 0xBF: CP(Reg.A); return 4;
                case 0xB8: CP(Reg.B); return 4;
                case 0xB9: CP(Reg.C); return 4;
                case 0xBA: CP(Reg.D); return 4;
                case 0xBB: CP(Reg.E); return 4;
                case 0xBC: CP(Reg.H); return 4;
                case 0xBD: CP(Reg.L); return 4;
                case 0xBE: CP(bus.Read(Reg.HL)); return 8;
                case 0xFE: CP(NextByte()); return 8;

                // INC n
                case 0x3C: Reg.A = INC(Reg.A); return 4;
                case 0x04: Reg.B = INC(Reg.B); return 4;
                case 0x0C: Reg.C = INC(Reg.C); return 4;
                case 0x14: Reg.D = INC(Reg.D); return 4;
                case 0x1C: Reg.E = INC(Reg.E); return 4;
                case 0x24: Reg.H = INC(Reg.H); return 4;
                case 0x2C: Reg.L = INC(Reg.L); return 4;
                case 0x34: bus.Write(Reg.HL, INC(bus.Read(Reg.HL))); return 12;

                // DEC n
                case 0x3D: Reg.A = DEC(Reg.A); return 4;
                case 0x05: Reg.B = DEC(Reg.B); return 4;
                case 0x0D: Reg.C = DEC(Reg.C); return 4;
                case 0x15: Reg.D = DEC(Reg.D); return 4;
                case 0x1D: Reg.E = DEC(Reg.E); return 4;
                case 0x25: Reg.H = DEC(Reg.H); return 4;
                case 0x2D: Reg.L = DEC(Reg.L); return 4;
                case 0x35: bus.Write(Reg.HL, DEC(bus.Read(Reg.HL))); return 12;

                #endregion

                #region 16-Bit Arithmetic

                // ADD HL,n
                case 0x09: ADD(Reg.BC); return 8;
                case 0x19: ADD(Reg.DE); return 8;
                case 0x29: ADD(Reg.HL); return 8;
                case 0x39: ADD(Reg.SP); return 8;


                // ADD SP,n
                case 0xE8: ADD((sbyte)NextByte()); return 16;

                // fine
                // INC nn
                case 0x03: Reg.BC++; return 8;
                case 0x13: Reg.DE++; return 8;
                case 0x23: Reg.HL++; return 8;
                case 0x33: Reg.SP++; return 8;

                // fine
                // DEC nn
                case 0x0B: Reg.BC--; return 8;
                case 0x1B: Reg.DE--; return 8;
                case 0x2B: Reg.HL--; return 8;
                case 0x3B: Reg.SP--; return 8;

                #endregion

                // fine
                #region Miscellaneous

                // CB Prefixed
                case 0xCB: return PrefixedTable();

                // DAA
                case 0x27: DAA(); return 4;

                // CPL
                case 0x2F: CPL(); return 4;

                // CCF
                case 0x3F: CCF(); return 4;

                // SCF
                case 0x37: SCF(); return 4;

                // NOP
                case 0x00: return 4;

                // HALT
                case 0x76: bus.interrupt.Halt(); return 4;

                // STOP
                case 0x10: return 4;

                // DI
                case 0xF3: bus.interrupt.Disable(); return 4;

                // EI
                case 0xFB: bus.interrupt.Enable(); return 4;

                #endregion

                // fine
                #region Jumps

                // JP nn
                case 0xC3: return JP(true);

                // JP cc,nn
                case 0xC2: return JP(Reg.GetFlag(Flag.Z) == false);
                case 0xCA: return JP(Reg.GetFlag(Flag.Z) == true);
                case 0xD2: return JP(Reg.GetFlag(Flag.C) == false);
                case 0xDA: return JP(Reg.GetFlag(Flag.C) == true);

                // JP (HL)
                case 0xE9: JP(Reg.HL); return 4;

                // JP n
                case 0x18: return JR(true);

                // JR cc,n
                case 0x20: return JR(Reg.GetFlag(Flag.Z) == false);
                case 0x28: return JR(Reg.GetFlag(Flag.Z) == true);
                case 0x30: return JR(Reg.GetFlag(Flag.C) == false);
                case 0x38: return JR(Reg.GetFlag(Flag.C) == true);

                #endregion

                // fine
                #region Calls

                // CALL nn
                case 0xCD: return CALL(true);

                // CALL cc,nn
                case 0xC4: return CALL(Reg.GetFlag(Flag.Z) == false);
                case 0xCC: return CALL(Reg.GetFlag(Flag.Z) == true);
                case 0xD4: return CALL(Reg.GetFlag(Flag.C) == false);
                case 0xDC: return CALL(Reg.GetFlag(Flag.C) == true);

                #endregion

                // fine
                #region Restarts

                // RST n
                case 0xC7: RST(0x00); return 16;
                case 0xCF: RST(0x08); return 16;
                case 0xD7: RST(0x10); return 16;
                case 0xDF: RST(0x18); return 16;
                case 0xE7: RST(0x20); return 16;
                case 0xEF: RST(0x28); return 16;
                case 0xF7: RST(0x30); return 16;
                case 0xFF: RST(0x38); return 16;

                #endregion

                // fine
                #region Returns

                // RET
                case 0xC9: RET(true); return 16;

                // RET cc
                case 0xC0: return RET(Reg.GetFlag(Flag.Z) == false);
                case 0xC8: return RET(Reg.GetFlag(Flag.Z) == true);
                case 0xD0: return RET(Reg.GetFlag(Flag.C) == false);
                case 0xD8: return RET(Reg.GetFlag(Flag.C) == true);

                // RETI
                case 0xD9: RET(true); bus.interrupt.Enable(); return 16;

                #endregion

                // fine
                #region Rotates

                // RLCA
                case 0x07: Reg.A = RotateLeft(Reg.A, false, false); return 4;

                //RLA
                case 0x17: Reg.A = RotateLeft(Reg.A, true, false); return 4;

                // RRCA
                case 0x0F: Reg.A = RotateRight(Reg.A, false, false); return 4;

                // RRA
                case 0x1F: Reg.A = RotateRight(Reg.A, true, false); return 4;

                #endregion

                #region Illegal

                case 0xD3: //Illegal Opcode
                case 0xDB: //Illegal Opcode
                case 0xDD: //Illegal Opcode
                case 0xE3: //Illegal Opcode
                case 0xE4: //Illegal Opcode
                case 0xEB: //Illegal Opcode
                case 0xEC: //Illegal Opcode
                case 0xED: //Illegal Opcode
                case 0xF4: //Illegal Opcode
                case 0xFC: //Illegal Opcode
                case 0xFD: //Illegal Opcode
                throw new Exception("Illegal Instruction : " + opcode);

                #endregion
            }
        }

        private byte PrefixedTable()
        {
            opcode = NextByte();

            switch(opcode)
            {
                #region Miscellaneous

                // SWAP n
                case 0x37: Reg.A = SWAP(Reg.A); return 8;
                case 0x30: Reg.B = SWAP(Reg.B); return 8;
                case 0x31: Reg.C = SWAP(Reg.C); return 8;
                case 0x32: Reg.D = SWAP(Reg.D); return 8;
                case 0x33: Reg.E = SWAP(Reg.E); return 8;
                case 0x34: Reg.H = SWAP(Reg.H); return 8;
                case 0x35: Reg.L = SWAP(Reg.L); return 8;
                case 0x36: bus.Write(Reg.HL, SWAP(bus.Read(Reg.HL))); return 16;

                #endregion

                #region Rotates & Shifts

                // RLC n
                case 0x07: Reg.A = RLC(Reg.A); return 8;
                case 0x00: Reg.B = RLC(Reg.B); return 8;
                case 0x01: Reg.C = RLC(Reg.C); return 8;
                case 0x02: Reg.D = RLC(Reg.D); return 8;
                case 0x03: Reg.E = RLC(Reg.E); return 8;
                case 0x04: Reg.H = RLC(Reg.H); return 8;
                case 0x05: Reg.L = RLC(Reg.L); return 8;
                case 0x06: bus.Write(Reg.HL, RLC(bus.Read(Reg.HL))); return 16;

                // RL n
                case 0x17: Reg.A = RL(Reg.A); return 8;
                case 0x10: Reg.B = RL(Reg.B); return 8;
                case 0x11: Reg.C = RL(Reg.C); return 8;
                case 0x12: Reg.D = RL(Reg.D); return 8;
                case 0x13: Reg.E = RL(Reg.E); return 8;
                case 0x14: Reg.H = RL(Reg.H); return 8;
                case 0x15: Reg.L = RL(Reg.L); return 8;
                case 0x16: bus.Write(Reg.HL, RL(bus.Read(Reg.HL))); return 16;

                // RRC n
                case 0x0F: Reg.A = RRC(Reg.A); return 8;
                case 0x08: Reg.B = RRC(Reg.B); return 8;
                case 0x09: Reg.C = RRC(Reg.C); return 8;
                case 0x0A: Reg.D = RRC(Reg.D); return 8;
                case 0x0B: Reg.E = RRC(Reg.E); return 8;
                case 0x0C: Reg.H = RRC(Reg.H); return 8;
                case 0x0D: Reg.L = RRC(Reg.L); return 8;
                case 0x0E: bus.Write(Reg.HL, RRC(bus.Read(Reg.HL))); return 16;

                // RR n
                case 0x1F: Reg.A = RR(Reg.A); return 8;
                case 0x18: Reg.B = RR(Reg.B); return 8;
                case 0x19: Reg.C = RR(Reg.C); return 8;
                case 0x1A: Reg.D = RR(Reg.D); return 8;
                case 0x1B: Reg.E = RR(Reg.E); return 8;
                case 0x1C: Reg.H = RR(Reg.H); return 8;
                case 0x1D: Reg.L = RR(Reg.L); return 8;
                case 0x1E: bus.Write(Reg.HL, RR(bus.Read(Reg.HL))); return 16;

                // SLA n
                case 0x27: Reg.A = SLA(Reg.A); return 8;
                case 0x20: Reg.B = SLA(Reg.B); return 8;
                case 0x21: Reg.C = SLA(Reg.C); return 8;
                case 0x22: Reg.D = SLA(Reg.D); return 8;
                case 0x23: Reg.E = SLA(Reg.E); return 8;
                case 0x24: Reg.H = SLA(Reg.H); return 8;
                case 0x25: Reg.L = SLA(Reg.L); return 8;
                case 0x26: bus.Write(Reg.HL, SLA(bus.Read(Reg.HL))); return 16;

                // SRA n
                case 0x2F: Reg.A = SRA(Reg.A); return 8;
                case 0x28: Reg.B = SRA(Reg.B); return 8;
                case 0x29: Reg.C = SRA(Reg.C); return 8;
                case 0x2A: Reg.D = SRA(Reg.D); return 8;
                case 0x2B: Reg.E = SRA(Reg.E); return 8;
                case 0x2C: Reg.H = SRA(Reg.H); return 8;
                case 0x2D: Reg.L = SRA(Reg.L); return 8;
                case 0x2E: bus.Write(Reg.HL, SRA(bus.Read(Reg.HL))); return 16;

                // SRL
                case 0x3F: Reg.A = SRL(Reg.A); return 8;
                case 0x38: Reg.B = SRL(Reg.B); return 8;
                case 0x39: Reg.C = SRL(Reg.C); return 8;
                case 0x3A: Reg.D = SRL(Reg.D); return 8;
                case 0x3B: Reg.E = SRL(Reg.E); return 8;
                case 0x3C: Reg.H = SRL(Reg.H); return 8;
                case 0x3D: Reg.L = SRL(Reg.L); return 8;
                case 0x3E: bus.Write(Reg.HL, SRL(bus.Read(Reg.HL))); return 16;

                #endregion

                #region Bit Opcodes

                // BIT 0,r
                case 0x47: BIT(Bit.Bit0, Reg.A); return 8;
                case 0x40: BIT(Bit.Bit0, Reg.B); return 8;
                case 0x41: BIT(Bit.Bit0, Reg.C); return 8;
                case 0x42: BIT(Bit.Bit0, Reg.D); return 8;
                case 0x43: BIT(Bit.Bit0, Reg.E); return 8;
                case 0x44: BIT(Bit.Bit0, Reg.H); return 8;
                case 0x45: BIT(Bit.Bit0, Reg.L); return 8;
                case 0x46: BIT(Bit.Bit0, bus.Read(Reg.HL)); return 12;

                // BIT 1,r
                case 0x4F: BIT(Bit.Bit1, Reg.A); return 8;
                case 0x48: BIT(Bit.Bit1, Reg.B); return 8;
                case 0x49: BIT(Bit.Bit1, Reg.C); return 8;
                case 0x4A: BIT(Bit.Bit1, Reg.D); return 8;
                case 0x4B: BIT(Bit.Bit1, Reg.E); return 8;
                case 0x4C: BIT(Bit.Bit1, Reg.H); return 8;
                case 0x4D: BIT(Bit.Bit1, Reg.L); return 8;
                case 0x4E: BIT(Bit.Bit1, bus.Read(Reg.HL)); return 12;

                // BIT 2,r
                case 0x57: BIT(Bit.Bit2, Reg.A); return 8;
                case 0x50: BIT(Bit.Bit2, Reg.B); return 8;
                case 0x51: BIT(Bit.Bit2, Reg.C); return 8;
                case 0x52: BIT(Bit.Bit2, Reg.D); return 8;
                case 0x53: BIT(Bit.Bit2, Reg.E); return 8;
                case 0x54: BIT(Bit.Bit2, Reg.H); return 8;
                case 0x55: BIT(Bit.Bit2, Reg.L); return 8;
                case 0x56: BIT(Bit.Bit2, bus.Read(Reg.HL)); return 12;

                // BIT 3,r
                case 0x5F: BIT(Bit.Bit3, Reg.A); return 8;
                case 0x58: BIT(Bit.Bit3, Reg.B); return 8;
                case 0x59: BIT(Bit.Bit3, Reg.C); return 8;
                case 0x5A: BIT(Bit.Bit3, Reg.D); return 8;
                case 0x5B: BIT(Bit.Bit3, Reg.E); return 8;
                case 0x5C: BIT(Bit.Bit3, Reg.H); return 8;
                case 0x5D: BIT(Bit.Bit3, Reg.L); return 8;
                case 0x5E: BIT(Bit.Bit3, bus.Read(Reg.HL)); return 12;

                // BIT 4,r
                case 0x67: BIT(Bit.Bit4, Reg.A); return 8;
                case 0x60: BIT(Bit.Bit4, Reg.B); return 8;
                case 0x61: BIT(Bit.Bit4, Reg.C); return 8;
                case 0x62: BIT(Bit.Bit4, Reg.D); return 8;
                case 0x63: BIT(Bit.Bit4, Reg.E); return 8;
                case 0x64: BIT(Bit.Bit4, Reg.H); return 8;
                case 0x65: BIT(Bit.Bit4, Reg.L); return 8;
                case 0x66: BIT(Bit.Bit4, bus.Read(Reg.HL)); return 12;

                // BIT 5,r
                case 0x6F: BIT(Bit.Bit5, Reg.A); return 8;
                case 0x68: BIT(Bit.Bit5, Reg.B); return 8;
                case 0x69: BIT(Bit.Bit5, Reg.C); return 8;
                case 0x6A: BIT(Bit.Bit5, Reg.D); return 8;
                case 0x6B: BIT(Bit.Bit5, Reg.E); return 8;
                case 0x6C: BIT(Bit.Bit5, Reg.H); return 8;
                case 0x6D: BIT(Bit.Bit5, Reg.L); return 8;
                case 0x6E: BIT(Bit.Bit5, bus.Read(Reg.HL)); return 12;

                // BIT 6,r
                case 0x77: BIT(Bit.Bit6, Reg.A); return 8;
                case 0x70: BIT(Bit.Bit6, Reg.B); return 8;
                case 0x71: BIT(Bit.Bit6, Reg.C); return 8;
                case 0x72: BIT(Bit.Bit6, Reg.D); return 8;
                case 0x73: BIT(Bit.Bit6, Reg.E); return 8;
                case 0x74: BIT(Bit.Bit6, Reg.H); return 8;
                case 0x75: BIT(Bit.Bit6, Reg.L); return 8;
                case 0x76: BIT(Bit.Bit6, bus.Read(Reg.HL)); return 12;

                // BIT 7,r
                case 0x7F: BIT(Bit.Bit7, Reg.A); return 8;
                case 0x78: BIT(Bit.Bit7, Reg.B); return 8;
                case 0x79: BIT(Bit.Bit7, Reg.C); return 8;
                case 0x7A: BIT(Bit.Bit7, Reg.D); return 8;
                case 0x7B: BIT(Bit.Bit7, Reg.E); return 8;
                case 0x7C: BIT(Bit.Bit7, Reg.H); return 8;
                case 0x7D: BIT(Bit.Bit7, Reg.L); return 8;
                case 0x7E: BIT(Bit.Bit7, bus.Read(Reg.HL)); return 12;


                // SET 0,r
                case 0xC7: Reg.A = SET(Bit.Bit0, Reg.A); return 8;
                case 0xC0: Reg.B = SET(Bit.Bit0, Reg.B); return 8;
                case 0xC1: Reg.C = SET(Bit.Bit0, Reg.C); return 8;
                case 0xC2: Reg.D = SET(Bit.Bit0, Reg.D); return 8;
                case 0xC3: Reg.E = SET(Bit.Bit0, Reg.E); return 8;
                case 0xC4: Reg.H = SET(Bit.Bit0, Reg.H); return 8;
                case 0xC5: Reg.L = SET(Bit.Bit0, Reg.L); return 8;
                case 0xC6: bus.Write(Reg.HL, SET(Bit.Bit0, bus.Read(Reg.HL))); return 16;

                // SET 1,r
                case 0xCF: Reg.A = SET(Bit.Bit1, Reg.A); return 8;
                case 0xC8: Reg.B = SET(Bit.Bit1, Reg.B); return 8;
                case 0xC9: Reg.C = SET(Bit.Bit1, Reg.C); return 8;
                case 0xCA: Reg.D = SET(Bit.Bit1, Reg.D); return 8;
                case 0xCB: Reg.E = SET(Bit.Bit1, Reg.E); return 8;
                case 0xCC: Reg.H = SET(Bit.Bit1, Reg.H); return 8;
                case 0xCD: Reg.L = SET(Bit.Bit1, Reg.L); return 8;
                case 0xCE: bus.Write(Reg.HL, SET(Bit.Bit1, bus.Read(Reg.HL))); return 16;

                // SET 2,r
                case 0xD7: Reg.A = SET(Bit.Bit2, Reg.A); return 8;
                case 0xD0: Reg.B = SET(Bit.Bit2, Reg.B); return 8;
                case 0xD1: Reg.C = SET(Bit.Bit2, Reg.C); return 8;
                case 0xD2: Reg.D = SET(Bit.Bit2, Reg.D); return 8;
                case 0xD3: Reg.E = SET(Bit.Bit2, Reg.E); return 8;
                case 0xD4: Reg.H = SET(Bit.Bit2, Reg.H); return 8;
                case 0xD5: Reg.L = SET(Bit.Bit2, Reg.L); return 8;
                case 0xD6: bus.Write(Reg.HL, SET(Bit.Bit2, bus.Read(Reg.HL))); return 16;

                // SET 3,r
                case 0xDF: Reg.A = SET(Bit.Bit3, Reg.A); return 8;
                case 0xD8: Reg.B = SET(Bit.Bit3, Reg.B); return 8;
                case 0xD9: Reg.C = SET(Bit.Bit3, Reg.C); return 8;
                case 0xDA: Reg.D = SET(Bit.Bit3, Reg.D); return 8;
                case 0xDB: Reg.E = SET(Bit.Bit3, Reg.E); return 8;
                case 0xDC: Reg.H = SET(Bit.Bit3, Reg.H); return 8;
                case 0xDD: Reg.L = SET(Bit.Bit3, Reg.L); return 8;
                case 0xDE: bus.Write(Reg.HL, SET(Bit.Bit3, bus.Read(Reg.HL))); return 16;

                // SET 4,r
                case 0xE7: Reg.A = SET(Bit.Bit4, Reg.A); return 8;
                case 0xE0: Reg.B = SET(Bit.Bit4, Reg.B); return 8;
                case 0xE1: Reg.C = SET(Bit.Bit4, Reg.C); return 8;
                case 0xE2: Reg.D = SET(Bit.Bit4, Reg.D); return 8;
                case 0xE3: Reg.E = SET(Bit.Bit4, Reg.E); return 8;
                case 0xE4: Reg.H = SET(Bit.Bit4, Reg.H); return 8;
                case 0xE5: Reg.L = SET(Bit.Bit4, Reg.L); return 8;
                case 0xE6: bus.Write(Reg.HL, SET(Bit.Bit4, bus.Read(Reg.HL))); return 16;

                // SET 5,r
                case 0xEF: Reg.A = SET(Bit.Bit5, Reg.A); return 8;
                case 0xE8: Reg.B = SET(Bit.Bit5, Reg.B); return 8;
                case 0xE9: Reg.C = SET(Bit.Bit5, Reg.C); return 8;
                case 0xEA: Reg.D = SET(Bit.Bit5, Reg.D); return 8;
                case 0xEB: Reg.E = SET(Bit.Bit5, Reg.E); return 8;
                case 0xEC: Reg.H = SET(Bit.Bit5, Reg.H); return 8;
                case 0xED: Reg.L = SET(Bit.Bit5, Reg.L); return 8;
                case 0xEE: bus.Write(Reg.HL, SET(Bit.Bit5, bus.Read(Reg.HL))); return 16;

                // SET 6,r
                case 0xF7: Reg.A = SET(Bit.Bit6, Reg.A); return 8;
                case 0xF0: Reg.B = SET(Bit.Bit6, Reg.B); return 8;
                case 0xF1: Reg.C = SET(Bit.Bit6, Reg.C); return 8;
                case 0xF2: Reg.D = SET(Bit.Bit6, Reg.D); return 8;
                case 0xF3: Reg.E = SET(Bit.Bit6, Reg.E); return 8;
                case 0xF4: Reg.H = SET(Bit.Bit6, Reg.H); return 8;
                case 0xF5: Reg.L = SET(Bit.Bit6, Reg.L); return 8;
                case 0xF6: bus.Write(Reg.HL, SET(Bit.Bit6, bus.Read(Reg.HL))); return 16;

                // SET 7,r
                case 0xFF: Reg.A = SET(Bit.Bit7, Reg.A); return 8;
                case 0xF8: Reg.B = SET(Bit.Bit7, Reg.B); return 8;
                case 0xF9: Reg.C = SET(Bit.Bit7, Reg.C); return 8;
                case 0xFA: Reg.D = SET(Bit.Bit7, Reg.D); return 8;
                case 0xFB: Reg.E = SET(Bit.Bit7, Reg.E); return 8;
                case 0xFC: Reg.H = SET(Bit.Bit7, Reg.H); return 8;
                case 0xFD: Reg.L = SET(Bit.Bit7, Reg.L); return 8;
                case 0xFE: bus.Write(Reg.HL, SET(Bit.Bit7, bus.Read(Reg.HL))); return 16;

                // RES 0,r
                case 0x87: Reg.A = RES(Bit.Bit0, Reg.A); return 8;
                case 0x80: Reg.B = RES(Bit.Bit0, Reg.B); return 8;
                case 0x81: Reg.C = RES(Bit.Bit0, Reg.C); return 8;
                case 0x82: Reg.D = RES(Bit.Bit0, Reg.D); return 8;
                case 0x83: Reg.E = RES(Bit.Bit0, Reg.E); return 8;
                case 0x84: Reg.H = RES(Bit.Bit0, Reg.H); return 8;
                case 0x85: Reg.L = RES(Bit.Bit0, Reg.L); return 8;
                case 0x86: bus.Write(Reg.HL, RES(Bit.Bit0, bus.Read(Reg.HL))); return 16;

                // RES 1,r
                case 0x8F: Reg.A = RES(Bit.Bit1, Reg.A); return 8;
                case 0x88: Reg.B = RES(Bit.Bit1, Reg.B); return 8;
                case 0x89: Reg.C = RES(Bit.Bit1, Reg.C); return 8;
                case 0x8A: Reg.D = RES(Bit.Bit1, Reg.D); return 8;
                case 0x8B: Reg.E = RES(Bit.Bit1, Reg.E); return 8;
                case 0x8C: Reg.H = RES(Bit.Bit1, Reg.H); return 8;
                case 0x8D: Reg.L = RES(Bit.Bit1, Reg.L); return 8;
                case 0x8E: bus.Write(Reg.HL, RES(Bit.Bit1, bus.Read(Reg.HL))); return 16;

                // RES 2,r
                case 0x97: Reg.A = RES(Bit.Bit2, Reg.A); return 8;
                case 0x90: Reg.B = RES(Bit.Bit2, Reg.B); return 8;
                case 0x91: Reg.C = RES(Bit.Bit2, Reg.C); return 8;
                case 0x92: Reg.D = RES(Bit.Bit2, Reg.D); return 8;
                case 0x93: Reg.E = RES(Bit.Bit2, Reg.E); return 8;
                case 0x94: Reg.H = RES(Bit.Bit2, Reg.H); return 8;
                case 0x95: Reg.L = RES(Bit.Bit2, Reg.L); return 8;
                case 0x96: bus.Write(Reg.HL, RES(Bit.Bit2, bus.Read(Reg.HL))); return 16;

                // RES 3,r
                case 0x9F: Reg.A = RES(Bit.Bit3, Reg.A); return 8;
                case 0x98: Reg.B = RES(Bit.Bit3, Reg.B); return 8;
                case 0x99: Reg.C = RES(Bit.Bit3, Reg.C); return 8;
                case 0x9A: Reg.D = RES(Bit.Bit3, Reg.D); return 8;
                case 0x9B: Reg.E = RES(Bit.Bit3, Reg.E); return 8;
                case 0x9C: Reg.H = RES(Bit.Bit3, Reg.H); return 8;
                case 0x9D: Reg.L = RES(Bit.Bit3, Reg.L); return 8;
                case 0x9E: bus.Write(Reg.HL, RES(Bit.Bit3, bus.Read(Reg.HL))); return 16;

                // RES 4,r
                case 0xA7: Reg.A = RES(Bit.Bit4, Reg.A); return 8;
                case 0xA0: Reg.B = RES(Bit.Bit4, Reg.B); return 8;
                case 0xA1: Reg.C = RES(Bit.Bit4, Reg.C); return 8;
                case 0xA2: Reg.D = RES(Bit.Bit4, Reg.D); return 8;
                case 0xA3: Reg.E = RES(Bit.Bit4, Reg.E); return 8;
                case 0xA4: Reg.H = RES(Bit.Bit4, Reg.H); return 8;
                case 0xA5: Reg.L = RES(Bit.Bit4, Reg.L); return 8;
                case 0xA6: bus.Write(Reg.HL, RES(Bit.Bit4, bus.Read(Reg.HL))); return 16;

                // RES 5,r
                case 0xAF: Reg.A = RES(Bit.Bit5, Reg.A); return 8;
                case 0xA8: Reg.B = RES(Bit.Bit5, Reg.B); return 8;
                case 0xA9: Reg.C = RES(Bit.Bit5, Reg.C); return 8;
                case 0xAA: Reg.D = RES(Bit.Bit5, Reg.D); return 8;
                case 0xAB: Reg.E = RES(Bit.Bit5, Reg.E); return 8;
                case 0xAC: Reg.H = RES(Bit.Bit5, Reg.H); return 8;
                case 0xAD: Reg.L = RES(Bit.Bit5, Reg.L); return 8;
                case 0xAE: bus.Write(Reg.HL, RES(Bit.Bit5, bus.Read(Reg.HL))); return 16;

                // RES 6,r
                case 0xB7: Reg.A = RES(Bit.Bit6, Reg.A); return 8;
                case 0xB0: Reg.B = RES(Bit.Bit6, Reg.B); return 8;
                case 0xB1: Reg.C = RES(Bit.Bit6, Reg.C); return 8;
                case 0xB2: Reg.D = RES(Bit.Bit6, Reg.D); return 8;
                case 0xB3: Reg.E = RES(Bit.Bit6, Reg.E); return 8;
                case 0xB4: Reg.H = RES(Bit.Bit6, Reg.H); return 8;
                case 0xB5: Reg.L = RES(Bit.Bit6, Reg.L); return 8;
                case 0xB6: bus.Write(Reg.HL, RES(Bit.Bit6, bus.Read(Reg.HL))); return 16;

                // RES 7,r
                case 0xBF: Reg.A = RES(Bit.Bit7, Reg.A); return 8;
                case 0xB8: Reg.B = RES(Bit.Bit7, Reg.B); return 8;
                case 0xB9: Reg.C = RES(Bit.Bit7, Reg.C); return 8;
                case 0xBA: Reg.D = RES(Bit.Bit7, Reg.D); return 8;
                case 0xBB: Reg.E = RES(Bit.Bit7, Reg.E); return 8;
                case 0xBC: Reg.H = RES(Bit.Bit7, Reg.H); return 8;
                case 0xBD: Reg.L = RES(Bit.Bit7, Reg.L); return 8;
                case 0xBE: bus.Write(Reg.HL, RES(Bit.Bit7, bus.Read(Reg.HL))); return 16;

                #endregion
            }
        }

        public void Push(ushort data)
        {
            Reg.SP -= 2;
            bus.WriteShort(Reg.SP, data);
        }

        public ushort Pop()
        {
            ushort result = bus.ReadShort(Reg.SP);
            Reg.SP += 2;

            return result;
        }


        private byte NextByte()
        {
            return bus.Read(Reg.PC++);
        }

        private ushort NextShort()
        {
            return Combine(NextByte(), NextByte());
        }

        #region 16-Bit Loads

        private void LDHL()
        {
            ushort result = (ushort)(((short)Reg.SP) + (sbyte)NextByte());

            Reg.SetFlag(Flag.Z, false);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, (result & 0xF) + (Reg.SP & 0xF) > 0xF);
            Reg.SetFlag(Flag.C, (result & 0xFF) + (Reg.SP & 0xFF) > 0xFF);

            Reg.HL = result;
        }

        #endregion

        #region 8-Bit ALU

        private void ADD(byte n)
        {
            (byte result, bool halfCarry, bool fullCarry) = Reg.A.AddOverflow(n);

            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, halfCarry);
            Reg.SetFlag(Flag.C, fullCarry);

            Reg.A = result;
        }

        private void ADC(byte n)
        {
            int carry = Reg.GetFlag(Flag.C) ? 1 : 0;
            (byte result, bool halfCarry, bool fullCarry) = Reg.A.AddOverflow(n, carry);

            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, halfCarry);
            Reg.SetFlag(Flag.C, fullCarry);

            Reg.A = result;
        }

        private void SUB(byte n)
        {
            (byte result, bool halfCarry, bool fullCarry) = Reg.A.AddOverflow(-n);

            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, true);
            Reg.SetFlag(Flag.H, halfCarry);
            Reg.SetFlag(Flag.C, fullCarry);

            Reg.A = result;
        }

        private void SBC(byte n)
        {
            int carry = Reg.GetFlag(Flag.C) ? 1 : 0;
            (byte result, bool halfCarry, bool fullCarry) = Reg.A.AddOverflow(-n, -carry);

            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, true);
            Reg.SetFlag(Flag.H, halfCarry);
            Reg.SetFlag(Flag.C, fullCarry);

            Reg.A = result;
        }

        private void AND(byte n)
        {
            byte result = (byte)(Reg.A & n);

            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, true);
            Reg.SetFlag(Flag.C, false);

            Reg.A = result;
        }

        private void OR(byte n)
        {
            byte result = (byte)(Reg.A | n);

            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.C, false);

            Reg.A = result;
        }

        private void XOR(byte n)
        {
            byte result = (byte)(Reg.A ^ n);

            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.C, false);

            Reg.A = result;
        }

        private void CP(byte n)
        {
            (byte result, bool halfCarry, bool fullCarry) = Reg.A.AddOverflow(-n);

            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, true);
            Reg.SetFlag(Flag.H, halfCarry);
            Reg.SetFlag(Flag.C, fullCarry);
        }

        private byte INC(byte n)
        {
            (byte result, bool halfCarry, _) = n.AddOverflow(1);

            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, halfCarry);

            return result;
        }

        private byte DEC(byte n)
        {
            (byte result, bool halfCarry, _) = n.AddOverflow(-1);

            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, true);
            Reg.SetFlag(Flag.H, halfCarry);

            return result;
        }

        #endregion

        #region 16-Bit Arithmetic
        private void ADD(ushort nn)
        {
            (ushort result, bool halfCarry, bool fullCarry) = Reg.HL.AddOverflow(nn);

            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, halfCarry);
            Reg.SetFlag(Flag.C, fullCarry);

            Reg.HL = result;
        }

        private void ADD(sbyte n)
        {
            (ushort result, bool halfCarry, bool fullCarry) = Reg.SP.AddOverflow(n);

            Reg.SetFlag(Flag.Z, false);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, halfCarry);
            Reg.SetFlag(Flag.C, fullCarry);

            Reg.SP = result;
        }

        #endregion

        #region Miscellaneous

        private void CPL()
        {
            Reg.A = (byte)~Reg.A;

            Reg.SetFlag(Flag.N, true);
            Reg.SetFlag(Flag.H, true);
        }

        private void SCF()
        {
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.C, true);
        }

        private void CCF()
        {
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.C, !Reg.GetFlag(Flag.C));
        }

        private byte SWAP(byte n)
        {
            byte result = n.Swap();
            Reg.SetFlag(Flag.Z, result == 0);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.C, false);

            return result;
        }

        private void DAA()
        {
            if(Reg.GetFlag(Flag.N) == true)
            {
                if(Reg.GetFlag(Flag.C))
                {
                    Reg.A -= 0x60;
                }
                if(Reg.GetFlag(Flag.H))
                {
                    Reg.A -= 0x6;
                }
            }
            else
            {
                if(Reg.GetFlag(Flag.C) || (Reg.A > 0x99))
                {
                    Reg.A += 0x60;
                    Reg.SetFlag(Flag.C, true);
                }
                if(Reg.GetFlag(Flag.H) || (Reg.A & 0xF) > 0x9)
                {
                    Reg.A += 0x6;
                }
            }

            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.Z, Reg.A == 0);
        }


        #endregion

        #region Rotates & Shifts

        private byte RLC(byte n)
        {
            return RotateLeft(n, false, true);
        }

        private byte RL(byte n)
        {
            return RotateLeft(n, true, true);
        }

        private byte RRC(byte n)
        {
            return RotateRight(n, false, true);
        }

        private byte RR(byte n)
        {
            return RotateRight(n, true, true);
        }


        private byte RotateLeft(byte n, bool includeCarry = false, bool updateZero = false)
        {
            byte bit7 = (byte)(n >> 7);
            byte result;

            if(includeCarry)
            {
                result = (byte)((n << 1) | (byte)(Reg.GetFlag(Flag.C) ? 1 : 0));
            }
            else
            {
                result = (byte)((n << 1) | (n >> 7));
            }

            Reg.SetFlag(Flag.C, (bit7 == 1));
            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.Z, (result == 0 && updateZero));
            return result;
        }

        private byte RotateRight(byte n, bool includeCarry = false, bool updateZero = false)
        {
            byte bit1 = (byte)(n & 1);
            byte result;

            if(includeCarry)
            {
                result = (byte)((n >> 1) | (byte)(Reg.GetFlag(Flag.C) ? 1 : 0) << 7);
            }
            else
            {
                result = (byte)((n >> 1) | (n << 7));
            }

            Reg.SetFlag(Flag.C, (bit1 == 1));
            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.Z, (result == 0 && updateZero));
            return result;
        }

        private byte SLA(byte n)
        {
            byte result = (byte)(n << 1);
            byte bit7 = (byte)(n >> 7);
            Reg.SetFlag(Flag.C, bit7 == 1);
            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.Z, result == 0);
            return result;
        }

        private byte SRA(byte n)
        {
            byte result = (byte)((n >> 1) | (n & 0x80));

            Reg.SetFlag(Flag.C, (n & 1) == 1);
            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.Z, result == 0);

            return result;
        }

        private byte SRL(byte n)
        {
            byte result = (byte)(n >> 1);

            Reg.SetFlag(Flag.C, (n & 1) == 1);
            Reg.SetFlag(Flag.H, false);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.Z, result == 0);

            return result;
        }

        #endregion

        #region Bit Opcodes

        private void BIT(byte bit, byte r)
        {
            Reg.SetFlag(Flag.Z, (r & bit) == 0);
            Reg.SetFlag(Flag.N, false);
            Reg.SetFlag(Flag.H, true);
        }

        private byte SET(byte bit, byte r)
        {
            return (byte)(r | bit);
        }

        private byte RES(byte bit, byte r)
        {
            return (byte)(r & ~bit);
        }

        #endregion

        #region Jumps

        private void JP(ushort nn)
        {
            Reg.PC = nn;
        }

        private byte JP(bool condition)
        {
            if(condition == true)
            {
                JP(bus.ReadShort(Reg.PC));
                return 16;
            }
            else
            {
                Reg.PC += 2;
                return 12;
            }
        }

        byte JR(bool condition)
        {
            if(condition == true)
            {
                JP((ushort)(Reg.PC + (sbyte)bus.Read(Reg.PC)));
                Reg.PC += 1;
                return 12;
            }
            else
            {
                Reg.PC += 1;
                return 8;
            }
        }

        #endregion

        #region Calls

        public byte CALL(bool condition)
        {
            if(condition == true)
            {
                Push((ushort)(Reg.PC + 2));
                JP(bus.ReadShort(Reg.PC));
                return 12;
            }
            else
            {
                Reg.PC += 2;
                return 8;
            }
        }

        #endregion

        #region Returns
        private byte RET(bool condition)
        {
            if(condition == true)
            {
                Reg.PC = Pop();
                return 20;
            }
            else
            {
                return 8;
            }
        }

        #endregion

        #region Restarts

        private void RST(byte n)
        {
            Push(Reg.PC);
            Reg.PC = n;
        }

        #endregion
    }
}