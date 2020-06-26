using System;
using Monoboy.Emulator.Utility;

namespace Monoboy.Emulator
{
    public class CPU
    {
        public Register register;
        public Memory memory;

        private int cycles;

        public CPU()
        {
            memory = new Memory();

            // What the boot rom does
            register.AF = 0x01B0;
            register.BC = 0x0013;
            register.DE = 0x00D8;
            register.HL = 0x014D;
            register.SP = 0x0100;
        }

        public void Step()
        {
            cycles += NextInstruction();
        }

        // Returns cycles to jump
        byte NextInstruction()
        {
            Console.WriteLine("");

            // TODO Handle interupts
            byte opcode = NextByte();

            LogHex(opcode, ConsoleColor.Blue);

            switch (opcode)
            {
                #region 8-Bit Loads

                // Load into Register A
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
                case 0xFA: register.A = memory.Read(NextWord()); return 16;
                case 0x3E: register.A = NextByte(); return 8;
                case 0x3A: register.A = memory.Read(register.HL--); return 8;
                case 0x2A: register.A = memory.Read(register.HL++); return 8;
                case 0xF0: register.A = memory.Read((ushort)(0xFF00 + NextWord())); return 12;

                // Load into Register B
                case 0x47: register.B = register.A; return 4;
                case 0x40: return 4;
                case 0x41: register.B = register.C; return 4;
                case 0x42: register.B = register.D; return 4;
                case 0x43: register.B = register.E; return 4;
                case 0x44: register.B = register.H; return 4;
                case 0x45: register.B = register.L; return 4;
                case 0x46: register.B = memory.Read(register.HL); return 8;
                case 0x06: register.B = NextByte(); return 8;

                // Load into Register C
                case 0x4F: register.C = register.A; return 4;
                case 0x48: register.C = register.B; return 4;
                case 0x49: return 4;
                case 0x4A: register.C = register.D; return 4;
                case 0x4B: register.C = register.E; return 4;
                case 0x4C: register.C = register.H; return 4;
                case 0x4D: register.C = register.L; return 4;
                case 0x4E: register.C = memory.Read(register.HL); return 8;
                case 0x0E: register.C = NextByte(); return 8;

                // Load into Register D
                case 0x57: register.D = register.A; return 4;
                case 0x50: register.D = register.B; return 4;
                case 0x51: register.D = register.C; return 4;
                case 0x52: return 4;
                case 0x53: register.D = register.E; return 4;
                case 0x54: register.D = register.H; return 4;
                case 0x55: register.D = register.L; return 4;
                case 0x56: register.D = memory.Read(register.HL); return 8;
                case 0x16: register.D = NextByte(); return 8;

                // Load into Register E
                case 0x5F: register.E = register.A; return 4;
                case 0x58: register.E = register.B; return 4;
                case 0x59: register.E = register.C; return 4;
                case 0x5A: register.E = register.D; return 4;
                case 0x5B: return 4;
                case 0x5C: register.E = register.H; return 4;
                case 0x5D: register.E = register.L; return 4;
                case 0x5E: register.E = memory.Read(register.HL); return 8;
                case 0x1E: register.E = NextByte(); return 8;

                // Load into Register H
                case 0x67: register.H = register.A; return 4;
                case 0x60: register.H = register.B; return 4;
                case 0x61: register.H = register.C; return 4;
                case 0x62: register.H = register.D; return 4;
                case 0x63: register.H = register.E; return 4;
                case 0x64: return 4;
                case 0x65: register.H = register.L; return 4;
                case 0x66: register.H = memory.Read(register.HL); return 8;
                case 0x26: register.H = NextByte(); return 8;

                // Load into Register L
                case 0x6F: register.L = register.A; return 4;
                case 0x68: register.L = register.B; return 4;
                case 0x69: register.L = register.C; return 4;
                case 0x6A: register.L = register.D; return 4;
                case 0x6B: register.L = register.E; return 4;
                case 0x6C: register.L = register.H; return 4;
                case 0x6D: return 4;
                case 0x6E: register.L = memory.Read(register.HL); return 8;
                case 0x2E: register.L = NextByte(); return 8;

                case 0xE2: memory.Write((ushort)(0xFF00 + register.C), NextByte()); return 8;

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
                case 0xEA: memory.Write(NextWord(), register.A); return 16;
                case 0xE0: memory.Write((ushort)(0xFF00 + NextByte()), register.A); return 12;

                #endregion

                #region 16-Bit Loads

                case 0x01: register.BC = NextWord(); return 12;
                case 0x11: register.DE = NextWord(); return 12;
                case 0x21: register.HL = NextWord(); return 12;
                case 0x31: register.SP = NextWord(); return 12;

                case 0xF9: register.SP = register.HL; return 8;
                case 0xF8: LD_HL(); return 12;
                case 0x08: memory.Write(NextWord(), register.SP); return 20;
                case 0xF5: Push(register.AF); return 16;
                case 0xC5: Push(register.BC); return 16;
                case 0xD5: Push(register.DE); return 16;
                case 0xE5: Push(register.HL); return 16;

                case 0xF1: register.AF = Pop(); return 12;
                case 0xC1: register.BC = Pop(); return 12;
                case 0xD1: register.DE = Pop(); return 12;
                case 0xE1: register.HL = Pop(); return 12;

                #endregion

                #region 8-Bit ALU

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
                //case 0x??: SUB(register.A, NextByte()); return ?;

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
                case 0xFE: CP(NextByte()); return 8;

                case 0x3C: INC(ref register.A); return 4;
                case 0x04: INC(ref register.B); return 4;
                case 0x0C: INC(ref register.C); return 4;
                case 0x14: INC(ref register.D); return 4;
                case 0x1C: INC(ref register.E); return 4;
                case 0x24: INC(ref register.H); return 4;
                case 0x2C: INC(ref register.L); return 4;
                case 0x34: byte n1 = memory.Read(register.HL); INC(ref n1); memory.Write(register.HL, n1); return 12;

                case 0x3D: DEC(ref register.A); return 4;
                case 0x05: DEC(ref register.B); return 4;
                case 0x0D: DEC(ref register.C); return 4;
                case 0x15: DEC(ref register.D); return 4;
                case 0x1D: DEC(ref register.E); return 4;
                case 0x25: DEC(ref register.H); return 4;
                case 0x2D: DEC(ref register.L); return 4;
                case 0x35: byte n2 = memory.Read(register.HL); DEC(ref n2); memory.Write(register.HL, n2); return 12;

                #endregion

                #region 16-Bit Arithmetic
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

                case 0x2F: register.A = (byte)~register.A; register.SetFlag(Flag.N, true); register.SetFlag(Flag.H, true); return 4;
                case 0x3F: register.SetFlag(Flag.N, false); register.SetFlag(Flag.H, false); register.SetFlag(Flag.C, !register.GetFlag(Flag.C)); return 4;
                case 0x37: register.SetFlag(Flag.N, false); register.SetFlag(Flag.H, false); register.SetFlag(Flag.C, true); return 4;

                case 0x00: return 4;

                case 0x76: return 4;// TODO: Halt
                case 0x10: return 4;// TODO: Stop
                case 0xF3: return 4;// TODO: DI interupt in one instruction
                case 0xFB: return 4;// TODO: EI interupt in one instruction

                case 0x07: register.A = RotateLeft(register.A, false, false); return 4;
                case 0x17: register.A = RotateLeft(register.A, true, false); return 4;
                case 0x0F: register.A = RotateRight(register.A, false, false); return 4;
                case 0x1F: register.A = RotateRight(register.A, true, false); return 4;



                #endregion

                default:
                    Log(" : Not Implemented! ", ConsoleColor.Red);
                    return 4;
            }
        }

        #region Commands

        #region 8-Bit

        void ADD(byte n, bool addCarry = false)
        {
            byte carry = (byte)(addCarry && register.GetFlag(Flag.C) ? 0xff : 0x00);
            byte result = (byte)(register.A + n + carry);

            register.SetFlag(Flag.Z, result == 0);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, (register.A & 0b1111) + (n & 0b1111) + carry > 0b1111);
            register.SetFlag(Flag.C, (register.A + n + carry) > 0b11111111);

            register.A = result;
        }

        void SUB(byte n, bool subCarry = false)
        {
            byte carry = (byte)(subCarry && register.GetFlag(Flag.C) ? 0xff : 0x00);
            byte result = (byte)(register.A - n - carry);

            register.SetFlag(Flag.Z, result == 0);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, (register.A & 0b1111) - (n & 0b1111) - carry < 0);
            register.SetFlag(Flag.C, (register.A - n - carry) < 0);

            register.A = result;
        }

        void AND(byte n)
        {
            byte result = (byte)(register.A & n);

            register.SetFlag(Flag.Z, result == 0);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, true);
            register.SetFlag(Flag.C, false);

            register.A = result;
        }

        void OR(byte n)
        {
            byte result = (byte)(register.A | n);

            register.SetFlag(Flag.Z, result == 0);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, false);
            register.SetFlag(Flag.C, false);

            register.A = result;
        }

        void XOR(byte n)
        {
            byte result = (byte)(register.A ^ n);

            register.SetFlag(Flag.Z, result == 0);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, false);
            register.SetFlag(Flag.C, false);

            register.A = result;
        }

        void CP(byte n)
        {
            byte result = register.A;
            SUB(n);
            register.A = result;
        }

        void INC(ref byte n)
        {
            n++;
            register.SetFlag(Flag.Z, n == 0);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, (n & 0b1111) + 1 > 0b1111);
        }

        void DEC(ref byte n)
        {
            n--;
            register.SetFlag(Flag.Z, n == 0);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, (n & 0b1111) - 1 < 0);
        }

        #endregion

        #region 16-Bit

        void LD_HL()
        {
            register.HL = (ushort)(register.SP + ((sbyte)NextByte()));

            register.SetFlag(Flag.Z, false);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, (register.HL & 0xF) < (register.SP & 0xF));
            register.SetFlag(Flag.C, (register.HL & 0xFF) < (register.SP & 0xFF));
        }

        void ADD_HL(ushort n)
        {
            ushort result = (ushort)(register.HL + n);

            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, (((register.HL & 0xFFF) + (n & 0xFFF)) & 0x1000) != 0);
            register.SetFlag(Flag.C, register.HL > 0xFFFF - n);

            register.HL = result;
        }

        void ADD_SP(byte n)
        {
            ushort result = (ushort)(register.SP + ((sbyte)n));

            register.SetFlag(Flag.Z, false);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, (result & 0xF) < (register.SP & 0xF));
            register.SetFlag(Flag.C, (result & 0xFF) < (register.SP & 0xFF));

            register.SP = result;
        }

        private void Push(ushort word)
        {
            memory.Write(register.SP, word);
            register.SP -= 2;
        }

        private ushort Pop()
        {
            ushort result = memory.Read(register.SP);
            register.SP += 2;
            return result;
        }

        #endregion

        #region Sub Operation Table

        byte SubOpcodeTable()
        {
            byte opcode = NextByte();
            Log(" : ");
            LogHex(opcode, ConsoleColor.Green);

            switch (opcode)
            {
                case 0x37: SWAP(ref register.A); return 8;
                case 0x30: SWAP(ref register.B); return 8;
                case 0x31: SWAP(ref register.C); return 8;
                case 0x32: SWAP(ref register.D); return 8;
                case 0x33: SWAP(ref register.E); return 8;
                case 0x34: SWAP(ref register.H); return 8;
                case 0x35: SWAP(ref register.L); return 8;

                case 0x36: byte n1 = memory.Read(register.HL); SWAP(ref n1); memory.Write(register.HL, n1); ; return 16;

                case 0x07: register.A = RotateLeft(register.A, false, false); return 8;
                case 0x00: register.B = RotateLeft(register.B, false, false); return 8;
                case 0x01: register.C = RotateLeft(register.C, false, false); return 8;
                case 0x02: register.D = RotateLeft(register.D, false, false); return 8;
                case 0x03: register.E = RotateLeft(register.E, false, false); return 8;
                case 0x04: register.H = RotateLeft(register.H, false, false); return 8;
                case 0x05: register.L = RotateLeft(register.L, false, false); return 8;
                case 0x06: memory.Write(register.HL, RotateLeft(memory.Read(register.HL), false, false)); return 16;

                case 0x17: register.A = RotateLeft(register.A, true, false); return 8;
                case 0x10: register.B = RotateLeft(register.B, true, false); return 8;
                case 0x11: register.C = RotateLeft(register.C, true, false); return 8;
                case 0x12: register.D = RotateLeft(register.D, true, false); return 8;
                case 0x13: register.E = RotateLeft(register.E, true, false); return 8;
                case 0x14: register.H = RotateLeft(register.H, true, false); return 8;
                case 0x15: register.L = RotateLeft(register.L, true, false); return 8;
                case 0x16: memory.Write(register.HL, RotateLeft(memory.Read(register.HL), true, false)); return 16;

                case 0x0F: register.A = RotateRight(register.A, false, false); return 8;
                case 0x08: register.B = RotateRight(register.B, false, false); return 8;
                case 0x09: register.C = RotateRight(register.C, false, false); return 8;
                case 0x0A: register.D = RotateRight(register.D, false, false); return 8;
                case 0x0B: register.E = RotateRight(register.E, false, false); return 8;
                case 0x0C: register.H = RotateRight(register.H, false, false); return 8;
                case 0x0D: register.L = RotateRight(register.L, false, false); return 8;
                case 0x0E: memory.Write(register.HL, RotateRight(memory.Read(register.HL), false, false)); return 16;

                case 0x1F: register.A = RotateRight(register.A, true, false); return 8;
                case 0x18: register.B = RotateRight(register.B, true, false); return 8;
                case 0x19: register.C = RotateRight(register.C, true, false); return 8;
                case 0x1A: register.D = RotateRight(register.D, true, false); return 8;
                case 0x1B: register.E = RotateRight(register.E, true, false); return 8;
                case 0x1C: register.H = RotateRight(register.H, true, false); return 8;
                case 0x1D: register.L = RotateRight(register.L, true, false); return 8;
                case 0x1E: memory.Write(register.HL, RotateRight(memory.Read(register.HL), true, false)); return 16;

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
                case 0x2E: memory.Write(register.HL, ShiftRight(memory.Read(register.HL), false)); return 16;

                default:
                    Log(" : Not Implemented! ", ConsoleColor.Red);
                    return 4;
            }
        }

        void SWAP(ref byte n)
        {
            register.SetFlag(Flag.Z, n.Swap() == 0);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.H, false);
            register.SetFlag(Flag.C, false);

            n = n.Swap();
        }

        #endregion


        #region Rotate Shift

        byte RotateLeft(byte n, bool includeCarry = false, bool updateZero = false)
        {
            byte bit7 = (byte)(n >> 7);
            byte result = (byte)(includeCarry ? (n << 1) | (register.GetFlag(Flag.C) ? 0xff : 0x00) : (n << 1) | (n >> (32 - 1)));
            register.SetFlag(Flag.C, (bit7 == 1));
            register.SetFlag(Flag.H, false);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.Z, (result == 0 && updateZero));
            return result;
        }

        byte RotateRight(byte n, bool includeCarry = false, bool updateZero = false)
        {
            byte bit7 = (byte)(n >> 7);
            byte result = (byte)(includeCarry ? (n << 1) | (register.GetFlag(Flag.C) ? 0xff : 0x00) : (n >> 1) | (n >> (32 - 1)));
            register.SetFlag(Flag.C, (bit7 == 1));
            register.SetFlag(Flag.H, false);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.Z, (result == 0 && updateZero));
            return result;
        }

        byte ShiftLeft(byte n)
        {
            byte result = (byte)(n << 1);
            byte bit7 = (byte)(n >> 7);
            register.SetFlag(Flag.C, bit7 == 1);
            register.SetFlag(Flag.H, false);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.Z, result == 0);
            return result;
        }

        byte ShiftRight(byte n, bool keepBit7)
        {
            byte result = (byte)(keepBit7 ? (n >> 1) | (n & 0x80) : n >> 1);
            register.SetFlag(Flag.C, (n & 1) == 1);
            register.SetFlag(Flag.H, false);
            register.SetFlag(Flag.N, false);
            register.SetFlag(Flag.Z, result == 0);
            return result;
        }

        #endregion

        byte NextByte()
        {
            byte result = memory.Read(register.PC);
            register.PC++;
            return result;
        }

        ushort NextWord()
        {
            byte low = NextByte();
            byte high = NextByte();
            return low.ToShort(high);
        }

        void DAA()
        {
            byte a = register.A;
            byte adjust = (byte)(register.GetFlag(Flag.C) ? 0x60 : 0x00);

            if (register.GetFlag(Flag.C))
            {
                adjust |= 0x06;
            }

            if (!register.GetFlag(Flag.N))
            {
                if ((a & 0x0F) > 0x09)
                {
                    adjust |= 0x06;
                }

                if (a > 0x99)
                {
                    adjust |= 0x60;
                }

                a += adjust;
            }
            else
            {
                a -= adjust;
            }

            register.SetFlag(Flag.C, adjust >= 0x60);
            register.SetFlag(Flag.H, false);
            register.SetFlag(Flag.Z, a == 0);
            register.A = a;
        }

        #endregion

        #region Debugging
        public void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
        }

        public void LogHex(byte hex, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write("0x" + hex.ToString("X2"));
            Console.ResetColor();
        }
        #endregion
    }
}
