using System;

using Utility;

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

            Log(opcode.ToHex(), ConsoleColor.Blue);

            switch (opcode)
            {
                #region 8-Bit Loads

                // Load into Register A
                case 0x7F: register.A = register.A; return 4;
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
                case 0xF0: register.A = memory.Read((ushort)(0xFF00 + NextShort())); return 12;

                // Load into Register B
                case 0x47: register.B = register.A; return 4;
                case 0x40: register.B = register.B; return 4;
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
                case 0x49: register.C = register.C; return 4;
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
                case 0x52: register.D = register.D; return 4;
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
                case 0x5B: register.E = register.E; return 4;
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
                case 0x64: register.H = register.H; return 4;
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
                case 0x6D: register.L = register.L; return 4;
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
                case 0xEA: memory.Write(NextShort(), register.A); return 16;
                case 0xE0: memory.Write((ushort)(0xFF00 + NextByte()), register.A); return 12;

                #endregion

                #region 16-Bit Loads

                case 0x01: register.BC = NextShort(); return 12;
                case 0x11: register.DE = NextShort(); return 12;
                case 0x21: register.HL = NextShort(); return 12;
                case 0x31: register.SP = NextShort(); return 12;

                case 0xF9: register.SP = register.HL; return 8;
                case 0xF8: LDHL(); return 12;
                case 0x08: memory.Write(NextShort(), register.SP); return 20;

                #endregion

                default:
                    Log(" : Not Implemented! ", ConsoleColor.Red);
                    return 4;
            }
        }

        #region Commands

        void LDHL()
        {
            register.HL = (ushort)(register.SP + ((sbyte)NextByte()));

            register.SetFlag(Flag.Zero, false);
            register.SetFlag(Flag.Subt, false);
            register.SetFlag(Flag.Half, (register.HL & 0xF) < (register.SP & 0xF));
            register.SetFlag(Flag.Full, (register.HL & 0xFF) < (register.SP & 0xFF));
        }

        #endregion

        #region Helper

        /* SetFlags
        
        register.SetFlag(Flag.Zero, false);
        register.SetFlag(Flag.Subt, false);
        register.SetFlag(Flag.Half, false);
        register.SetFlag(Flag.Full, false);

        */

        byte NextByte()
        {
            byte result = memory.Read(register.PC);
            register.PC++;
            return result;
        }

        ushort NextShort()
        {
            byte lower = NextByte();
            byte higher = NextByte();

            return (ushort)((higher << 8) + lower);
        }

        void JumpTo(ushort address)
        {
            register.PC = address;
        }

        void XorByte(byte n)
        {

        }

        #endregion

        #region Debugging
        public void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
        }

        public void LogHex(byte hex)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("0x" + hex.ToString("X2"));
            Console.ResetColor();
        }
        #endregion
    }
}
