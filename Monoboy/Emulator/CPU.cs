using System;
using Monoboy.Utility;

namespace Monoboy.Emulator
{
    public class CPU
    {
        public Register register;
        public Memory memory;
        public Interrupt interrupt;

        public byte opcode;

        public byte currentOpcode;

        // Returns number of cycles to jump

        public byte Step()
        {
            byte result = StepCPU();
            currentOpcode = opcode;
            return result;
        }

        byte StepCPU()
        {
            // TODO Handle interupts
            Debug.DisableNext();
            opcode = NextByte();

            byte n;
            ushort nn;

            switch(opcode)
            {
                case 0x7F: return 4;
                case 0x78: register.A = register.B; return 4;
                case 0x79: register.A = register.C; return 4;
                case 0x7A: register.A = register.D; return 4;
                case 0x7B: register.A = register.E; return 4;
                case 0x7C: register.A = register.H; return 4;
                case 0x7D: register.A = register.L; return 4;
                case 0xF2: register.A = memory.Read(register.C); return 8;
                case 0x7E: register.A = memory.Read(register.HL); return 8;
                case 0x0A: register.A = memory.Read(register.BC); return 8;
                case 0x1A: register.A = memory.Read(register.DE); return 8;
                case 0xFA: register.A = memory.Read(NextShort()); return 16;
                case 0x3E: register.A = NextByte(); return 8;
                case 0x3A: register.A = memory.Read(register.HL--); return 8;
                case 0x2A: register.A = memory.Read(register.HL++); return 8;
                case 0xF0: register.A = memory.Read((ushort)(0xFF00 | NextByte())); return 12;

                case 0x47: register.B = register.A; return 4;
                case 0x40: return 4;
                case 0x41: register.B = register.C; return 4;
                case 0x42: register.B = register.D; return 4;
                case 0x43: register.B = register.E; return 4;
                case 0x44: register.B = register.H; return 4;
                case 0x45: register.B = register.L; return 4;
                case 0x46: register.B = memory.Read(register.HL); return 8;
                case 0x06: register.B = NextByte(); return 8;

                case 0x4F: register.C = register.A; return 4;
                case 0x48: register.C = register.B; return 4;
                case 0x49: return 4;
                case 0x4A: register.C = register.D; return 4;
                case 0x4B: register.C = register.E; return 4;
                case 0x4C: register.C = register.H; return 4;
                case 0x4D: register.C = register.L; return 4;
                case 0x4E: register.C = memory.Read(register.HL); return 8;
                case 0x0E: register.C = NextByte(); return 8;

                case 0x57: register.D = register.A; return 4;
                case 0x50: register.D = register.B; return 4;
                case 0x51: register.D = register.C; return 4;
                case 0x52: return 4;
                case 0x53: register.D = register.E; return 4;
                case 0x54: register.D = register.H; return 4;
                case 0x55: register.D = register.L; return 4;
                case 0x56: register.D = memory.Read(register.HL); return 8;
                case 0x16: register.D = NextByte(); return 8;

                case 0x5F: register.E = register.A; return 4;
                case 0x58: register.E = register.B; return 4;
                case 0x59: register.E = register.C; return 4;
                case 0x5A: register.E = register.D; return 4;
                case 0x5B: return 4;
                case 0x5C: register.E = register.H; return 4;
                case 0x5D: register.E = register.L; return 4;
                case 0x5E: register.E = memory.Read(register.HL); return 8;
                case 0x1E: register.E = NextByte(); return 8;

                case 0x67: register.H = register.A; return 4;
                case 0x60: register.H = register.B; return 4;
                case 0x61: register.H = register.C; return 4;
                case 0x62: register.H = register.D; return 4;
                case 0x63: register.H = register.E; return 4;
                case 0x64: return 4;
                case 0x65: register.H = register.L; return 4;
                case 0x66: register.H = memory.Read(register.HL); return 8;
                case 0x26: register.H = NextByte(); return 8;

                case 0x6F: register.L = register.A; return 4;
                case 0x68: register.L = register.B; return 4;
                case 0x69: register.L = register.C; return 4;
                case 0x6A: register.L = register.D; return 4;
                case 0x6B: register.L = register.E; return 4;
                case 0x6C: register.L = register.H; return 4;
                case 0x6D: return 4;
                case 0x6E: register.L = memory.Read(register.HL); return 8;
                case 0x2E: register.L = NextByte(); return 8;

                case 0xE2: memory.Write((ushort)(0xFF00 + register.C), register.A); return 8;

                case 0x77: memory.Write(register.HL, register.A); return 8;
                case 0x70: memory.Write(register.HL, register.B); return 8;
                case 0x71: memory.Write(register.HL, register.C); return 8;
                case 0x72: memory.Write(register.HL, register.D); return 8;
                case 0x73: memory.Write(register.HL, register.E); return 8;
                case 0x74: memory.Write(register.HL, register.H); return 8;
                case 0x75: memory.Write(register.HL, register.L); return 8;
                case 0x32: memory.Write(register.HL--, register.A); return 8;
                case 0x22: memory.Write(register.HL++, register.A); return 8;
                case 0x36: memory.Write(register.HL, NextByte()); return 12;

                case 0x02: memory.Write(register.BC, register.A); return 8;
                case 0x12: memory.Write(register.DE, register.A); return 8;
                case 0xEA: memory.Write(NextShort(), register.A); return 16;
                case 0xE0: memory.Write((ushort)(0xFF00 + NextByte()), register.A); return 12;

                case 0x01: register.BC = NextShort(); return 12;
                case 0x11: register.DE = NextShort(); return 12;
                case 0x21: register.HL = NextShort(); return 12;
                case 0x31: register.SP = NextShort(); return 12;

                case 0xF9: register.SP = register.HL; return 8;
                case 0xF8: LD_HL(); return 12;
                case 0x08: nn = NextShort(); memory.Write(nn, register.SP.Low()); memory.Write((ushort)(nn + 1), register.SP.High()); return 20;
                case 0xF5: Push(register.AF); return 16;
                case 0xC5: Push(register.BC); return 16;
                case 0xD5: Push(register.DE); return 16;
                case 0xE5: Push(register.HL); return 16;

                case 0xF1: register.AF = Pop(); return 12;
                case 0xC1: register.BC = Pop(); return 12;
                case 0xD1: register.DE = Pop(); return 12;
                case 0xE1: register.HL = Pop(); return 12;

                case 0x87: ADD(register.A); return 4;
                case 0x80: ADD(register.B); return 4;
                case 0x81: ADD(register.C); return 4;
                case 0x82: ADD(register.D); return 4;
                case 0x83: ADD(register.E); return 4;
                case 0x84: ADD(register.H); return 4;
                case 0x85: ADD(register.L); return 4;
                case 0x86: ADD(memory.Read(register.HL)); return 8;
                case 0xC6: ADD(NextByte()); return 8;

                case 0x8F: ADD(register.A, true); return 4;
                case 0x88: ADD(register.B, true); return 4;
                case 0x89: ADD(register.C, true); return 4;
                case 0x8A: ADD(register.D, true); return 4;
                case 0x8B: ADD(register.E, true); return 4;
                case 0x8C: ADD(register.H, true); return 4;
                case 0x8D: ADD(register.L, true); return 4;
                case 0x8E: ADD(memory.Read(register.HL), true); return 8;
                case 0xCE: ADD(NextByte(), true); return 8;

                case 0x97: SUB(register.A); return 4;
                case 0x90: SUB(register.B); return 4;
                case 0x91: SUB(register.C); return 4;
                case 0x92: SUB(register.D); return 4;
                case 0x93: SUB(register.E); return 4;
                case 0x94: SUB(register.H); return 4;
                case 0x95: SUB(register.L); return 4;
                case 0x96: SUB(memory.Read(register.HL)); return 8;
                case 0xD6: SUB(NextByte()); return 8;

                case 0x9F: SUB(register.A, true); return 4;
                case 0x98: SUB(register.B, true); return 4;
                case 0x99: SUB(register.C, true); return 4;
                case 0x9A: SUB(register.D, true); return 4;
                case 0x9B: SUB(register.E, true); return 4;
                case 0x9C: SUB(register.H, true); return 4;
                case 0x9D: SUB(register.L, true); return 4;
                case 0x9E: SUB(memory.Read(register.HL), true); return 8;
                case 0xDE: SUB(NextByte()); return 8;

                case 0xA7: AND(register.A); return 4;
                case 0xA0: AND(register.B); return 4;
                case 0xA1: AND(register.C); return 4;
                case 0xA2: AND(register.D); return 4;
                case 0xA3: AND(register.E); return 4;
                case 0xA4: AND(register.H); return 4;
                case 0xA5: AND(register.L); return 4;
                case 0xA6: AND(memory.Read(register.HL)); return 8;
                case 0xE6: AND(NextByte()); return 8;

                case 0xB7: OR(register.A); return 4;
                case 0xB0: OR(register.B); return 4;
                case 0xB1: OR(register.C); return 4;
                case 0xB2: OR(register.D); return 4;
                case 0xB3: OR(register.E); return 4;
                case 0xB4: OR(register.H); return 4;
                case 0xB5: OR(register.L); return 4;
                case 0xB6: OR(memory.Read(register.HL)); return 8;
                case 0xF6: OR(NextByte()); return 8;

                case 0xAF: XOR(register.A); return 4;
                case 0xA8: XOR(register.B); return 4;
                case 0xA9: XOR(register.C); return 4;
                case 0xAA: XOR(register.D); return 4;
                case 0xAB: XOR(register.E); return 4;
                case 0xAC: XOR(register.H); return 4;
                case 0xAD: XOR(register.L); return 4;
                case 0xAE: XOR(memory.Read(register.HL)); return 8;
                case 0xEE: XOR(NextByte()); return 8;

                case 0xBF: CP(register.A); return 4;
                case 0xB8: CP(register.B); return 4;
                case 0xB9: CP(register.C); return 4;
                case 0xBA: CP(register.D); return 4;
                case 0xBB: CP(register.E); return 4;
                case 0xBC: CP(register.H); return 4;
                case 0xBD: CP(register.L); return 4;
                case 0xBE: CP(memory.Read(register.HL)); return 8;
                case 0xFE: n = NextByte(); CP(n); return 8;

                case 0x3C: register.A = INC(register.A); return 4;
                case 0x04: register.B = INC(register.B); return 4;
                case 0x0C: register.C = INC(register.C); return 4;
                case 0x14: register.D = INC(register.D); return 4;
                case 0x1C: register.E = INC(register.E); return 4;
                case 0x24: register.H = INC(register.H); return 4;
                case 0x2C: register.L = INC(register.L); return 4;
                case 0x34: memory.Write(register.HL, INC(memory.Read(register.HL))); return 12;

                case 0x3D: DEC(ref register.A); return 4;
                case 0x05: DEC(ref register.B); return 4;
                case 0x0D: DEC(ref register.C); return 4;
                case 0x15: DEC(ref register.D); return 4;
                case 0x1D: DEC(ref register.E); return 4;
                case 0x25: DEC(ref register.H); return 4;
                case 0x2D: DEC(ref register.L); return 4;
                case 0x35: n = memory.Read(register.HL); DEC(ref n); memory.Write(register.HL, n); return 12;

                case 0x09: ADD_HL(register.BC); return 8;
                case 0x19: ADD_HL(register.DE); return 8;
                case 0x29: ADD_HL(register.HL); return 8;
                case 0x39: ADD_HL(register.SP); return 8;

                case 0xE8: ADD_SP(NextByte()); return 16;

                case 0x03: register.BC++; return 8;
                case 0x13: register.DE++; return 8;
                case 0x23: register.HL++; return 8;
                case 0x33: register.SP++; return 8;

                case 0x0B: register.BC--; return 8;
                case 0x1B: register.DE--; return 8;
                case 0x2B: register.HL--; return 8;
                case 0x3B: register.SP--; return 8;

                case 0xCB: return SubOpcodeTable();

                case 0x27: DAA(); return 4;

                case 0x2F: register.A = (byte)~register.A; register.SetFlag(Flag.Negative, true); register.SetFlag(Flag.HalfCarry, true); return 4;
                case 0x3F: register.SetFlag(Flag.Negative, false); register.SetFlag(Flag.HalfCarry, false); register.SetFlag(Flag.FullCarry, !register.GetFlag(Flag.FullCarry)); return 4;
                case 0x37: register.SetFlag(Flag.Negative, false); register.SetFlag(Flag.HalfCarry, false); register.SetFlag(Flag.FullCarry, true); return 4;

                case 0x07: register.A = RotateLeft(register.A, false, false); return 4;
                case 0x17: register.A = RotateLeft(register.A, true, false); return 4;
                case 0x0F: register.A = RotateRight(register.A, false, false); return 4;
                case 0x1F: register.A = RotateRight(register.A, true, false); return 4;

                case 0xC3: JP(NextShort()); return 12;
                case 0xC2: nn = NextShort(); if(register.GetFlag(Flag.Zero) == false) { JP(nn); return 16; } else { return 12; }
                case 0xCA: nn = NextShort(); if(register.GetFlag(Flag.Zero) == true) { JP(nn); return 16; } else { return 12; }
                case 0xD2: nn = NextShort(); if(register.GetFlag(Flag.FullCarry) == false) { JP(nn); return 16; } else { return 12; }
                case 0xDA: nn = NextShort(); if(register.GetFlag(Flag.FullCarry) == true) { JP(nn); return 16; } else { return 12; }
                case 0xE9: JP(memory.Read(register.HL)); return 4;

                case 0x18: n = NextByte(); JR((sbyte)n); return 8;

                case 0x20: n = NextByte(); if(register.GetFlag(Flag.Zero) == false) { JR((sbyte)n); return 16; } else { return 12; }
                case 0x28: n = NextByte(); if(register.GetFlag(Flag.Zero) == true) { JR((sbyte)n); return 16; } else { return 12; }
                case 0x30: n = NextByte(); if(register.GetFlag(Flag.FullCarry) == false) { JR((sbyte)n); return 16; } else { return 12; }
                case 0x38: n = NextByte(); if(register.GetFlag(Flag.FullCarry) == true) { JR((sbyte)n); return 16; } else { return 12; }

                case 0xCD: CALL(NextShort()); return 12;

                case 0xC4: if(register.GetFlag(Flag.Zero) == false) { CALL(NextByte()); return 24; } else { return 12; }
                case 0xCC: if(register.GetFlag(Flag.Zero) == true) { CALL(NextByte()); return 24; } else { return 12; }
                case 0xD4: if(register.GetFlag(Flag.FullCarry) == false) { CALL(NextByte()); return 24; } else { return 12; }
                case 0xDC: if(register.GetFlag(Flag.FullCarry) == true) { CALL(NextByte()); return 24; } else { return 12; }

                case 0xC7: RST(0x00); return 16;
                case 0xCF: RST(0x08); return 16;
                case 0xD7: RST(0x10); return 16;
                case 0xDF: RST(0x18); return 16;
                case 0xE7: RST(0x20); return 16;
                case 0xEF: RST(0x28); return 16;
                case 0xF7: RST(0x30); return 16;
                case 0xFF: RST(0x38); return 16;

                case 0xC9: RET(); return 16;
                case 0xC0: if(register.GetFlag(Flag.Zero) == false) { RET(); return 20; } else { return 8; }
                case 0xC8: if(register.GetFlag(Flag.Zero) == true) { RET(); return 20; } else { return 8; }
                case 0xD0: if(register.GetFlag(Flag.FullCarry) == false) { RET(); return 20; } else { return 8; }
                case 0xD8: if(register.GetFlag(Flag.FullCarry) == true) { RET(); return 20; } else { return 8; }

                case 0x00: return 4;

                case 0x76: return 4;// TODO: Halt
                case 0x10: return 4;// TODO: Stop
                case 0xF3: return 4;// TODO: DI interupt in one instruction
                case 0xFB: return 4;// TODO: EI interupt in one instruction

                case 0xD9: RET(); return 8; // TODO: enable interupts

                case 0xD3: Debug.Log("Illegal Instruction : 0xD3", ConsoleColor.Red); return 0;
                case 0xDB: Debug.Log("Illegal Instruction : 0xDB", ConsoleColor.Red); return 0;
                case 0xDD: Debug.Log("Illegal Instruction : 0xDD", ConsoleColor.Red); return 0;
                case 0xE3: Debug.Log("Illegal Instruction : 0xE3", ConsoleColor.Red); return 0;
                case 0xE4: Debug.Log("Illegal Instruction : 0xE4", ConsoleColor.Red); return 0;
                case 0xEB: Debug.Log("Illegal Instruction : 0xEB", ConsoleColor.Red); return 0;
                case 0xEC: Debug.Log("Illegal Instruction : 0xEC", ConsoleColor.Red); return 0;
                case 0xED: Debug.Log("Illegal Instruction : 0xED", ConsoleColor.Red); return 0;
                case 0xF4: Debug.Log("Illegal Instruction : 0xF4", ConsoleColor.Red); return 0;
                case 0xFC: Debug.Log("Illegal Instruction : 0xFC", ConsoleColor.Red); return 0;
                case 0xFD: Debug.Log("Illegal Instruction : 0xFD", ConsoleColor.Red); return 0;

                default: return 0;
            }
        }

        #region Commands

        #region 8-Bit

        void ADD(byte n, bool addCarry = false)
        {
            byte carry = (byte)(addCarry && register.GetFlag(Flag.FullCarry) ? 1 : 0);
            byte result = (byte)(register.A + n + carry);

            register.SetFlag(Flag.Zero, result == 0);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, (register.A & 0xF) + (n & 0xF) + carry > 0xF);
            register.SetFlag(Flag.FullCarry, (register.A + n + carry) > 0xFF);

            register.A = result;
        }

        void SUB(byte n, bool subCarry = false)
        {
            byte carry = (byte)(subCarry && register.GetFlag(Flag.FullCarry) ? 1 : 0);
            byte result = (byte)(register.A - n - carry);

            register.SetFlag(Flag.Zero, result == 0);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, (register.A & 0xF) - (n & 0xF) - carry < 0);
            register.SetFlag(Flag.FullCarry, (register.A - n - carry) < 0);

            register.A = result;
        }

        void AND(byte n)
        {
            byte result = (byte)(register.A & n);

            register.SetFlag(Flag.Zero, result == 0);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, true);
            register.SetFlag(Flag.FullCarry, false);

            register.A = result;
        }

        void OR(byte n)
        {
            byte result = (byte)(register.A | n);

            register.SetFlag(Flag.Zero, result == 0);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, false);
            register.SetFlag(Flag.FullCarry, false);

            register.A = result;
        }

        void XOR(byte n)
        {
            byte result = (byte)(register.A ^ n);

            register.SetFlag(Flag.Zero, result == 0);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, false);
            register.SetFlag(Flag.FullCarry, false);

            register.A = result;
        }

        void CP(byte n)
        {
            byte result = register.A;
            SUB(n);
            register.A = result;
        }

        byte INC(byte n)
        {
            n++;
            register.SetFlag(Flag.Zero, n == 0);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, (n & 0xF) + 1 > 0xF);

            return n;
        }

        void DEC(ref byte n)
        {
            n--;
            register.SetFlag(Flag.Zero, n == 0);
            register.SetFlag(Flag.Negative, true);
            register.SetFlag(Flag.HalfCarry, (n & 0xF) - 1 < 0);
        }

        #endregion

        #region 16-Bit

        void LD_HL()
        {
            register.HL = (ushort)(register.SP + ((sbyte)NextByte()));

            register.SetFlag(Flag.Zero, false);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, (register.HL & 0xF) < (register.SP & 0xF));
            register.SetFlag(Flag.FullCarry, (register.HL & 0xFF) < (register.SP & 0xFF));
        }

        void ADD_HL(ushort n)
        {
            ushort result = (ushort)(register.HL + n);

            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, (((register.HL & 0xFFF) + (n & 0xFFF)) & 0x1000) != 0);
            register.SetFlag(Flag.FullCarry, register.HL > 0xFFFF - n);

            register.HL = result;
        }

        void ADD_SP(byte n)
        {
            ushort result = (ushort)(register.SP + ((sbyte)n));

            register.SetFlag(Flag.Zero, false);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, (result & 0xF) < (register.SP & 0xF));
            register.SetFlag(Flag.FullCarry, (result & 0xFF) < (register.SP & 0xFF));

            register.SP = result;
        }

        private void Push(ushort word)
        {
            register.SP--;
            memory.Write(register.SP, word.High());
            register.SP--;
            memory.Write(register.SP, word.Low());
        }

        private ushort Pop()
        {
            byte lower = memory.Read(register.SP);
            memory.Write(register.SP, 0);
            register.SP++;
            byte high = memory.Read(register.SP);
            memory.Write(register.SP, 0);
            register.SP++;
            return lower.ToShort(high);
        }

        #endregion

        #region Sub Operation Table

        byte SubOpcodeTable()
        {
            Debug.Disable();
            opcode = NextByte();
            Debug.Restore();

            switch(opcode)
            {
                case 0x37: SWAP(ref register.A); return 8;
                case 0x30: SWAP(ref register.B); return 8;
                case 0x31: SWAP(ref register.C); return 8;
                case 0x32: SWAP(ref register.D); return 8;
                case 0x33: SWAP(ref register.E); return 8;
                case 0x34: SWAP(ref register.H); return 8;
                case 0x35: SWAP(ref register.L); return 8;
                case 0x36: byte n1 = memory.Read(register.HL); SWAP(ref n1); memory.Write(register.HL, n1); ; return 16;

                case 0x07: register.A = RotateLeft(register.A, false, true); return 8;
                case 0x00: register.B = RotateLeft(register.B, false, true); return 8;
                case 0x01: register.C = RotateLeft(register.C, false, true); return 8;
                case 0x02: register.D = RotateLeft(register.D, false, true); return 8;
                case 0x03: register.E = RotateLeft(register.E, false, true); return 8;
                case 0x04: register.H = RotateLeft(register.H, false, true); return 8;
                case 0x05: register.L = RotateLeft(register.L, false, true); return 8;
                case 0x06: memory.Write(register.HL, RotateLeft(memory.Read(register.HL), false, true)); return 16;

                case 0x17: register.A = RotateLeft(register.A, true, true); return 8;
                case 0x10: register.B = RotateLeft(register.B, true, true); return 8;
                case 0x11: register.C = RotateLeft(register.C, true, true); return 8;
                case 0x12: register.D = RotateLeft(register.D, true, true); return 8;
                case 0x13: register.E = RotateLeft(register.E, true, true); return 8;
                case 0x14: register.H = RotateLeft(register.H, true, true); return 8;
                case 0x15: register.L = RotateLeft(register.L, true, true); return 8;
                case 0x16: memory.Write(register.HL, RotateLeft(memory.Read(register.HL), true, true)); return 16;

                case 0x0F: register.A = RotateRight(register.A, false, true); return 8;
                case 0x08: register.B = RotateRight(register.B, false, true); return 8;
                case 0x09: register.C = RotateRight(register.C, false, true); return 8;
                case 0x0A: register.D = RotateRight(register.D, false, true); return 8;
                case 0x0B: register.E = RotateRight(register.E, false, true); return 8;
                case 0x0C: register.H = RotateRight(register.H, false, true); return 8;
                case 0x0D: register.L = RotateRight(register.L, false, true); return 8;
                case 0x0E: memory.Write(register.HL, RotateRight(memory.Read(register.HL), false, true)); return 16;

                case 0x1F: register.A = RotateRight(register.A, true, true); return 8;
                case 0x18: register.B = RotateRight(register.B, true, true); return 8;
                case 0x19: register.C = RotateRight(register.C, true, true); return 8;
                case 0x1A: register.D = RotateRight(register.D, true, true); return 8;
                case 0x1B: register.E = RotateRight(register.E, true, true); return 8;
                case 0x1C: register.H = RotateRight(register.H, true, true); return 8;
                case 0x1D: register.L = RotateRight(register.L, true, true); return 8;
                case 0x1E: memory.Write(register.HL, RotateRight(memory.Read(register.HL), true, true)); return 16;

                case 0x27: register.A = ShiftLeft(register.A); return 8;
                case 0x20: register.B = ShiftLeft(register.B); return 8;
                case 0x21: register.C = ShiftLeft(register.C); return 8;
                case 0x22: register.D = ShiftLeft(register.D); return 8;
                case 0x23: register.E = ShiftLeft(register.E); return 8;
                case 0x24: register.H = ShiftLeft(register.H); return 8;
                case 0x25: register.L = ShiftLeft(register.L); return 8;
                case 0x26: memory.Write(register.HL, ShiftLeft(memory.Read(register.HL))); return 16;

                case 0x2F: register.A = ShiftRight(register.A, true); return 8;
                case 0x28: register.B = ShiftRight(register.B, true); return 8;
                case 0x29: register.C = ShiftRight(register.C, true); return 8;
                case 0x2A: register.D = ShiftRight(register.D, true); return 8;
                case 0x2B: register.E = ShiftRight(register.E, true); return 8;
                case 0x2C: register.H = ShiftRight(register.H, true); return 8;
                case 0x2D: register.L = ShiftRight(register.L, true); return 8;
                case 0x2E: memory.Write(register.HL, ShiftRight(memory.Read(register.HL), true)); return 16;

                case 0x3F: register.A = ShiftRight(register.A, false); return 8;
                case 0x38: register.B = ShiftRight(register.B, false); return 8;
                case 0x39: register.C = ShiftRight(register.C, false); return 8;
                case 0x3A: register.D = ShiftRight(register.D, false); return 8;
                case 0x3B: register.E = ShiftRight(register.E, false); return 8;
                case 0x3C: register.H = ShiftRight(register.H, false); return 8;
                case 0x3D: register.L = ShiftRight(register.L, false); return 8;
                case 0x3E: memory.Write(register.HL, ShiftRight(memory.Read(register.HL), false)); return 16;

                case 0x47: BIT((byte)Bit.Bit0, register.A); return 8;
                case 0x40: BIT((byte)Bit.Bit0, register.B); return 8;
                case 0x41: BIT((byte)Bit.Bit0, register.C); return 8;
                case 0x42: BIT((byte)Bit.Bit0, register.D); return 8;
                case 0x43: BIT((byte)Bit.Bit0, register.E); return 8;
                case 0x44: BIT((byte)Bit.Bit0, register.H); return 8;
                case 0x45: BIT((byte)Bit.Bit0, register.L); return 8;
                case 0x46: BIT((byte)Bit.Bit0, memory.Read(register.HL)); return 12;

                case 0x4F: BIT((byte)Bit.Bit1, register.A); return 8;
                case 0x48: BIT((byte)Bit.Bit1, register.B); return 8;
                case 0x49: BIT((byte)Bit.Bit1, register.C); return 8;
                case 0x4A: BIT((byte)Bit.Bit1, register.D); return 8;
                case 0x4B: BIT((byte)Bit.Bit1, register.E); return 8;
                case 0x4C: BIT((byte)Bit.Bit1, register.H); return 8;
                case 0x4D: BIT((byte)Bit.Bit1, register.L); return 8;
                case 0x4E: BIT((byte)Bit.Bit1, memory.Read(register.HL)); return 12;

                case 0x57: BIT((byte)Bit.Bit2, register.A); return 8;
                case 0x50: BIT((byte)Bit.Bit2, register.B); return 8;
                case 0x51: BIT((byte)Bit.Bit2, register.C); return 8;
                case 0x52: BIT((byte)Bit.Bit2, register.D); return 8;
                case 0x53: BIT((byte)Bit.Bit2, register.E); return 8;
                case 0x54: BIT((byte)Bit.Bit2, register.H); return 8;
                case 0x55: BIT((byte)Bit.Bit2, register.L); return 8;
                case 0x56: BIT((byte)Bit.Bit2, memory.Read(register.HL)); return 12;

                case 0x5F: BIT((byte)Bit.Bit3, register.A); return 8;
                case 0x58: BIT((byte)Bit.Bit3, register.B); return 8;
                case 0x59: BIT((byte)Bit.Bit3, register.C); return 8;
                case 0x5A: BIT((byte)Bit.Bit3, register.D); return 8;
                case 0x5B: BIT((byte)Bit.Bit3, register.E); return 8;
                case 0x5C: BIT((byte)Bit.Bit3, register.H); return 8;
                case 0x5D: BIT((byte)Bit.Bit3, register.L); return 8;
                case 0x5E: BIT((byte)Bit.Bit3, memory.Read(register.HL)); return 12;

                case 0x67: BIT((byte)Bit.Bit4, register.A); return 8;
                case 0x60: BIT((byte)Bit.Bit4, register.B); return 8;
                case 0x61: BIT((byte)Bit.Bit4, register.C); return 8;
                case 0x62: BIT((byte)Bit.Bit4, register.D); return 8;
                case 0x63: BIT((byte)Bit.Bit4, register.E); return 8;
                case 0x64: BIT((byte)Bit.Bit4, register.H); return 8;
                case 0x65: BIT((byte)Bit.Bit4, register.L); return 8;
                case 0x66: BIT((byte)Bit.Bit4, memory.Read(register.HL)); return 12;

                case 0x6F: BIT((byte)Bit.Bit5, register.A); return 8;
                case 0x68: BIT((byte)Bit.Bit5, register.B); return 8;
                case 0x69: BIT((byte)Bit.Bit5, register.C); return 8;
                case 0x6A: BIT((byte)Bit.Bit5, register.D); return 8;
                case 0x6B: BIT((byte)Bit.Bit5, register.E); return 8;
                case 0x6C: BIT((byte)Bit.Bit5, register.H); return 8;
                case 0x6D: BIT((byte)Bit.Bit5, register.L); return 8;
                case 0x6E: BIT((byte)Bit.Bit5, memory.Read(register.HL)); return 12;

                case 0x77: BIT((byte)Bit.Bit6, register.A); return 8;
                case 0x70: BIT((byte)Bit.Bit6, register.B); return 8;
                case 0x71: BIT((byte)Bit.Bit6, register.C); return 8;
                case 0x72: BIT((byte)Bit.Bit6, register.D); return 8;
                case 0x73: BIT((byte)Bit.Bit6, register.E); return 8;
                case 0x74: BIT((byte)Bit.Bit6, register.H); return 8;
                case 0x75: BIT((byte)Bit.Bit6, register.L); return 8;
                case 0x76: BIT((byte)Bit.Bit6, memory.Read(register.HL)); return 12;

                case 0x7F: BIT((byte)Bit.Bit7, register.A); return 8;
                case 0x78: BIT((byte)Bit.Bit7, register.B); return 8;
                case 0x79: BIT((byte)Bit.Bit7, register.C); return 8;
                case 0x7A: BIT((byte)Bit.Bit7, register.D); return 8;
                case 0x7B: BIT((byte)Bit.Bit7, register.E); return 8;
                case 0x7C: BIT((byte)Bit.Bit7, register.H); return 8;
                case 0x7D: BIT((byte)Bit.Bit7, register.L); return 8;
                case 0x7E: BIT((byte)Bit.Bit7, memory.Read(register.HL)); return 12;

                case 0xC7: register.A = SET((byte)Bit.Bit0, register.A); return 8;
                case 0xC0: register.B = SET((byte)Bit.Bit0, register.B); return 8;
                case 0xC1: register.C = SET((byte)Bit.Bit0, register.C); return 8;
                case 0xC2: register.D = SET((byte)Bit.Bit0, register.D); return 8;
                case 0xC3: register.E = SET((byte)Bit.Bit0, register.E); return 8;
                case 0xC4: register.H = SET((byte)Bit.Bit0, register.H); return 8;
                case 0xC5: register.L = SET((byte)Bit.Bit0, register.L); return 8;
                case 0xC6: memory.Write(register.HL, SET((byte)Bit.Bit0, memory.Read(register.HL))); return 16;

                case 0xCF: register.A = SET((byte)Bit.Bit1, register.A); return 8;
                case 0xC8: register.B = SET((byte)Bit.Bit1, register.B); return 8;
                case 0xC9: register.C = SET((byte)Bit.Bit1, register.C); return 8;
                case 0xCA: register.D = SET((byte)Bit.Bit1, register.D); return 8;
                case 0xCB: register.E = SET((byte)Bit.Bit1, register.E); return 8;
                case 0xCC: register.H = SET((byte)Bit.Bit1, register.H); return 8;
                case 0xCD: register.L = SET((byte)Bit.Bit1, register.L); return 8;
                case 0xCE: memory.Write(register.HL, SET((byte)Bit.Bit1, memory.Read(register.HL))); return 16;

                case 0xD7: register.A = SET((byte)Bit.Bit2, register.A); return 8;
                case 0xD0: register.B = SET((byte)Bit.Bit2, register.B); return 8;
                case 0xD1: register.C = SET((byte)Bit.Bit2, register.C); return 8;
                case 0xD2: register.D = SET((byte)Bit.Bit2, register.D); return 8;
                case 0xD3: register.E = SET((byte)Bit.Bit2, register.E); return 8;
                case 0xD4: register.H = SET((byte)Bit.Bit2, register.H); return 8;
                case 0xD5: register.L = SET((byte)Bit.Bit2, register.L); return 8;
                case 0xD6: memory.Write(register.HL, SET((byte)Bit.Bit2, memory.Read(register.HL))); return 16;

                case 0xDF: register.A = SET((byte)Bit.Bit3, register.A); return 8;
                case 0xD8: register.B = SET((byte)Bit.Bit3, register.B); return 8;
                case 0xD9: register.C = SET((byte)Bit.Bit3, register.C); return 8;
                case 0xDA: register.D = SET((byte)Bit.Bit3, register.D); return 8;
                case 0xDB: register.E = SET((byte)Bit.Bit3, register.E); return 8;
                case 0xDC: register.H = SET((byte)Bit.Bit3, register.H); return 8;
                case 0xDD: register.L = SET((byte)Bit.Bit3, register.L); return 8;
                case 0xDE: memory.Write(register.HL, SET((byte)Bit.Bit3, memory.Read(register.HL))); return 16;

                case 0xE7: register.A = SET((byte)Bit.Bit4, register.A); return 8;
                case 0xE0: register.B = SET((byte)Bit.Bit4, register.B); return 8;
                case 0xE1: register.C = SET((byte)Bit.Bit4, register.C); return 8;
                case 0xE2: register.D = SET((byte)Bit.Bit4, register.D); return 8;
                case 0xE3: register.E = SET((byte)Bit.Bit4, register.E); return 8;
                case 0xE4: register.H = SET((byte)Bit.Bit4, register.H); return 8;
                case 0xE5: register.L = SET((byte)Bit.Bit4, register.L); return 8;
                case 0xE6: memory.Write(register.HL, SET((byte)Bit.Bit4, memory.Read(register.HL))); return 16;

                case 0xEF: register.A = SET((byte)Bit.Bit5, register.A); return 8;
                case 0xE8: register.B = SET((byte)Bit.Bit5, register.B); return 8;
                case 0xE9: register.C = SET((byte)Bit.Bit5, register.C); return 8;
                case 0xEA: register.D = SET((byte)Bit.Bit5, register.D); return 8;
                case 0xEB: register.E = SET((byte)Bit.Bit5, register.E); return 8;
                case 0xEC: register.H = SET((byte)Bit.Bit5, register.H); return 8;
                case 0xED: register.L = SET((byte)Bit.Bit5, register.L); return 8;
                case 0xEE: memory.Write(register.HL, SET((byte)Bit.Bit5, memory.Read(register.HL))); return 16;

                case 0xF7: register.A = SET((byte)Bit.Bit6, register.A); return 8;
                case 0xF0: register.B = SET((byte)Bit.Bit6, register.B); return 8;
                case 0xF1: register.C = SET((byte)Bit.Bit6, register.C); return 8;
                case 0xF2: register.D = SET((byte)Bit.Bit6, register.D); return 8;
                case 0xF3: register.E = SET((byte)Bit.Bit6, register.E); return 8;
                case 0xF4: register.H = SET((byte)Bit.Bit6, register.H); return 8;
                case 0xF5: register.L = SET((byte)Bit.Bit6, register.L); return 8;
                case 0xF6: memory.Write(register.HL, SET((byte)Bit.Bit6, memory.Read(register.HL))); return 16;

                case 0xFF: register.A = SET((byte)Bit.Bit7, register.A); return 8;
                case 0xF8: register.B = SET((byte)Bit.Bit7, register.B); return 8;
                case 0xF9: register.C = SET((byte)Bit.Bit7, register.C); return 8;
                case 0xFA: register.D = SET((byte)Bit.Bit7, register.D); return 8;
                case 0xFB: register.E = SET((byte)Bit.Bit7, register.E); return 8;
                case 0xFC: register.H = SET((byte)Bit.Bit7, register.H); return 8;
                case 0xFD: register.L = SET((byte)Bit.Bit7, register.L); return 8;
                case 0xFE: memory.Write(register.HL, SET((byte)Bit.Bit7, memory.Read(register.HL))); return 16;

                case 0x87: register.A = RES((byte)Bit.Bit0, register.A); return 8;
                case 0x80: register.B = RES((byte)Bit.Bit0, register.B); return 8;
                case 0x81: register.C = RES((byte)Bit.Bit0, register.C); return 8;
                case 0x82: register.D = RES((byte)Bit.Bit0, register.D); return 8;
                case 0x83: register.E = RES((byte)Bit.Bit0, register.E); return 8;
                case 0x84: register.H = RES((byte)Bit.Bit0, register.H); return 8;
                case 0x85: register.L = RES((byte)Bit.Bit0, register.L); return 8;
                case 0x8F: register.A = RES((byte)Bit.Bit1, register.A); return 8;
                case 0x88: register.B = RES((byte)Bit.Bit1, register.B); return 8;
                case 0x89: register.C = RES((byte)Bit.Bit1, register.C); return 8;
                case 0x8A: register.D = RES((byte)Bit.Bit1, register.D); return 8;
                case 0x8B: register.E = RES((byte)Bit.Bit1, register.E); return 8;
                case 0x8C: register.H = RES((byte)Bit.Bit1, register.H); return 8;
                case 0x8D: register.L = RES((byte)Bit.Bit1, register.L); return 8;
                case 0x97: register.A = RES((byte)Bit.Bit2, register.A); return 8;
                case 0x90: register.B = RES((byte)Bit.Bit2, register.B); return 8;
                case 0x91: register.C = RES((byte)Bit.Bit2, register.C); return 8;
                case 0x92: register.D = RES((byte)Bit.Bit2, register.D); return 8;
                case 0x93: register.E = RES((byte)Bit.Bit2, register.E); return 8;
                case 0x94: register.H = RES((byte)Bit.Bit2, register.H); return 8;
                case 0x95: register.L = RES((byte)Bit.Bit2, register.L); return 8;
                case 0x9F: register.A = RES((byte)Bit.Bit3, register.A); return 8;
                case 0x98: register.B = RES((byte)Bit.Bit3, register.B); return 8;
                case 0x99: register.C = RES((byte)Bit.Bit3, register.C); return 8;
                case 0x9A: register.D = RES((byte)Bit.Bit3, register.D); return 8;
                case 0x9B: register.E = RES((byte)Bit.Bit3, register.E); return 8;
                case 0x9C: register.H = RES((byte)Bit.Bit3, register.H); return 8;
                case 0x9D: register.L = RES((byte)Bit.Bit3, register.L); return 8;
                case 0xA7: register.A = RES((byte)Bit.Bit4, register.A); return 8;
                case 0xA0: register.B = RES((byte)Bit.Bit4, register.B); return 8;
                case 0xA1: register.C = RES((byte)Bit.Bit4, register.C); return 8;
                case 0xA2: register.D = RES((byte)Bit.Bit4, register.D); return 8;
                case 0xA3: register.E = RES((byte)Bit.Bit4, register.E); return 8;
                case 0xA4: register.H = RES((byte)Bit.Bit4, register.H); return 8;
                case 0xA5: register.L = RES((byte)Bit.Bit4, register.L); return 8;
                case 0xAF: register.A = RES((byte)Bit.Bit5, register.A); return 8;
                case 0xA8: register.B = RES((byte)Bit.Bit5, register.B); return 8;
                case 0xA9: register.C = RES((byte)Bit.Bit5, register.C); return 8;
                case 0xAA: register.D = RES((byte)Bit.Bit5, register.D); return 8;
                case 0xAB: register.E = RES((byte)Bit.Bit5, register.E); return 8;
                case 0xAC: register.H = RES((byte)Bit.Bit5, register.H); return 8;
                case 0xAD: register.L = RES((byte)Bit.Bit5, register.L); return 8;
                case 0xB7: register.A = RES((byte)Bit.Bit6, register.A); return 8;
                case 0xB0: register.B = RES((byte)Bit.Bit6, register.B); return 8;
                case 0xB1: register.C = RES((byte)Bit.Bit6, register.C); return 8;
                case 0xB2: register.D = RES((byte)Bit.Bit6, register.D); return 8;
                case 0xB3: register.E = RES((byte)Bit.Bit6, register.E); return 8;
                case 0xB4: register.H = RES((byte)Bit.Bit6, register.H); return 8;
                case 0xB5: register.L = RES((byte)Bit.Bit6, register.L); return 8;
                case 0xBF: register.A = RES((byte)Bit.Bit7, register.A); return 8;
                case 0xB8: register.B = RES((byte)Bit.Bit7, register.B); return 8;
                case 0xB9: register.C = RES((byte)Bit.Bit7, register.C); return 8;
                case 0xBA: register.D = RES((byte)Bit.Bit7, register.D); return 8;
                case 0xBB: register.E = RES((byte)Bit.Bit7, register.E); return 8;
                case 0xBC: register.H = RES((byte)Bit.Bit7, register.H); return 8;
                case 0xBD: register.L = RES((byte)Bit.Bit7, register.L); return 8;

                case 0x86: memory.Write(register.HL, RES((byte)Bit.Bit0, memory.Read(register.HL))); return 16;
                case 0x8E: memory.Write(register.HL, RES((byte)Bit.Bit1, memory.Read(register.HL))); return 16;
                case 0x96: memory.Write(register.HL, RES((byte)Bit.Bit2, memory.Read(register.HL))); return 16;
                case 0x9E: memory.Write(register.HL, RES((byte)Bit.Bit3, memory.Read(register.HL))); return 16;
                case 0xA6: memory.Write(register.HL, RES((byte)Bit.Bit4, memory.Read(register.HL))); return 16;
                case 0xAE: memory.Write(register.HL, RES((byte)Bit.Bit5, memory.Read(register.HL))); return 16;
                case 0xB6: memory.Write(register.HL, RES((byte)Bit.Bit6, memory.Read(register.HL))); return 16;
                case 0xBE: memory.Write(register.HL, RES((byte)Bit.Bit7, memory.Read(register.HL))); return 16;

                default:
                Debug.Log("Not Implemented!", ConsoleColor.Red);
                return 4;
            }
        }

        #endregion

        #region Bit opcodes

        byte RotateLeft(byte n, bool includeCarry = false, bool updateZero = false)
        {
            byte bit7 = (byte)(n >> 7);
            byte result = (byte)(includeCarry ? (n << 1) | (register.GetFlag(Flag.FullCarry) ? 1 : 0) : n << 1 | n >> 7);
            register.SetFlag(Flag.FullCarry, (bit7 == 1));
            register.SetFlag(Flag.HalfCarry, false);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.Zero, (result == 0 && updateZero));
            return result;
        }

        byte RotateRight(byte n, bool includeCarry = false, bool updateZero = false)
        {
            byte bit7 = (byte)(n & 1);
            byte result = (byte)(includeCarry ? (n >> 1) | (register.GetFlag(Flag.FullCarry) ? 1 : 0) << 7 : n >> 1 | (n << 7));
            register.SetFlag(Flag.FullCarry, (bit7 == 1));
            register.SetFlag(Flag.HalfCarry, false);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.Zero, (result == 0 && updateZero));
            return result;
        }

        byte ShiftLeft(byte n)
        {
            byte result = (byte)(n << 1);
            byte bit7 = (byte)(n >> 7);
            register.SetFlag(Flag.FullCarry, bit7 == 1);
            register.SetFlag(Flag.HalfCarry, false);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.Zero, result == 0);
            return result;
        }

        byte ShiftRight(byte n, bool keepBit7)
        {
            byte result = (byte)(keepBit7 ? (n >> 1) | (n & 0x80) : n >> 1);
            register.SetFlag(Flag.FullCarry, (n & 1) == 1);
            register.SetFlag(Flag.HalfCarry, false);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.Zero, result == 0);
            return result;
        }

        void BIT(byte b, byte r)
        {
            register.SetFlag(Flag.Zero, ((~r) & b) != 0);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, true);
        }

        byte SET(byte b, byte r)
        {
            return (byte)(b | r);
        }

        byte RES(byte b, byte r)
        {
            return (byte)(b | r);
        }

        #endregion

        byte NextByte()
        {
            byte result = memory.Read(register.PC);
            register.PC++;

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
            byte a = register.A;
            byte adjust = (byte)(register.GetFlag(Flag.FullCarry) ? 0x60 : 0x00);

            if(register.GetFlag(Flag.FullCarry))
            {
                adjust |= 0x06;
            }

            if(!register.GetFlag(Flag.Negative))
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

            register.SetFlag(Flag.FullCarry, adjust >= 0x60);
            register.SetFlag(Flag.HalfCarry, false);
            register.SetFlag(Flag.Zero, a == 0);
            register.A = a;
        }

        void SWAP(ref byte n)
        {
            register.SetFlag(Flag.Zero, n.Swap() == 0);
            register.SetFlag(Flag.Negative, false);
            register.SetFlag(Flag.HalfCarry, false);
            register.SetFlag(Flag.FullCarry, false);

            n = n.Swap();
        }

        void JP(ushort nn)
        {
            register.PC = nn;
        }

        void JR(sbyte n)
        {
            register.PC += (ushort)n;
        }

        void CALL(ushort nn)
        {
            Push(register.PC);
            register.PC = nn;
        }

        void RST(byte n)
        {
            CALL(n);
        }

        void RET()
        {
            register.PC = Pop();
        }

        #endregion
    }
}
