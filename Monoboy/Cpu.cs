namespace Monoboy;

using System;

using Monoboy.Constants;
using Monoboy.Utility;

public class Cpu
{
    public bool Halted { get; set; }
    public bool HaltBug { get; set; }
    public byte Opcode { get; set; }

    private readonly Emulator emulator;

    private Register Reg => emulator.register;

    private readonly byte[] mainTimes = {
        1, 3, 2, 2, 1, 1, 2, 1, 5, 2, 2, 2, 1, 1, 2, 1,
        0, 3, 2, 2, 1, 1, 2, 1, 3, 2, 2, 2, 1, 1, 2, 1,
        2, 3, 2, 2, 1, 1, 2, 1, 2, 2, 2, 2, 1, 1, 2, 1,
        2, 3, 2, 2, 3, 3, 3, 1, 2, 2, 2, 2, 1, 1, 2, 1,
        1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
        1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
        1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
        2, 2, 2, 2, 2, 2, 0, 2, 1, 1, 1, 1, 1, 1, 2, 1,
        1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
        1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
        1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
        1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
        2, 3, 3, 4, 3, 4, 2, 4, 2, 4, 3, 0, 3, 6, 2, 4,
        2, 3, 3, 0, 3, 4, 2, 4, 2, 4, 3, 0, 3, 0, 2, 4,
        3, 3, 2, 0, 0, 4, 2, 4, 4, 1, 4, 0, 0, 0, 2, 4,
        3, 3, 2, 1, 0, 4, 2, 4, 3, 2, 4, 1, 0, 0, 2, 4,
    };

    private readonly byte[] cbTimes = {
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 3, 2,
        2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 3, 2,
        2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 3, 2,
        2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 3, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
    };

    public Cpu(Emulator emulator)
    {
        this.emulator = emulator;
    }

    public byte Step()
    {
        Opcode = NextByte();

        if (HaltBug)
        {
            HaltBug = false;
            Reg.PC--;
        }

        byte conditionalCycles = 0;

        switch (Opcode)
        {
            #region 8-Bit Loads

            // LD nn,n
            case 0x06:
            Reg.B = NextByte();
            break;
            case 0x0E:
            Reg.C = NextByte();
            break;
            case 0x16:
            Reg.D = NextByte();
            break;
            case 0x1E:
            Reg.E = NextByte();
            break;
            case 0x26:
            Reg.H = NextByte();
            break;
            case 0x2E:
            Reg.L = NextByte();
            break;

            // LD A,r2
            case 0x7F:
            Reg.A = Reg.A;
            break;
            case 0x78:
            Reg.A = Reg.B;
            break;
            case 0x79:
            Reg.A = Reg.C;
            break;
            case 0x7A:
            Reg.A = Reg.D;
            break;
            case 0x7B:
            Reg.A = Reg.E;
            break;
            case 0x7C:
            Reg.A = Reg.H;
            break;
            case 0x7D:
            Reg.A = Reg.L;
            break;
            case 0xFA:
            Reg.A = emulator.Read(NextShort());
            break;

            // LD B,r2
            case 0x47:
            Reg.B = Reg.A;
            break;
            case 0x40:
            Reg.B = Reg.B;
            break;
            case 0x41:
            Reg.B = Reg.C;
            break;
            case 0x42:
            Reg.B = Reg.D;
            break;
            case 0x43:
            Reg.B = Reg.E;
            break;
            case 0x44:
            Reg.B = Reg.H;
            break;
            case 0x45:
            Reg.B = Reg.L;
            break;
            case 0x46:
            Reg.B = emulator.Read(Reg.HL);
            break;

            // LD C,r2
            case 0x4F:
            Reg.C = Reg.A;
            break;
            case 0x48:
            Reg.C = Reg.B;
            break;
            case 0x49:
            Reg.C = Reg.C;
            break;
            case 0x4A:
            Reg.C = Reg.D;
            break;
            case 0x4B:
            Reg.C = Reg.E;
            break;
            case 0x4C:
            Reg.C = Reg.H;
            break;
            case 0x4D:
            Reg.C = Reg.L;
            break;
            case 0x4E:
            Reg.C = emulator.Read(Reg.HL);
            break;

            // LD D,r2
            case 0x57:
            Reg.D = Reg.A;
            break;
            case 0x50:
            Reg.D = Reg.B;
            break;
            case 0x51:
            Reg.D = Reg.C;
            break;
            case 0x52:
            Reg.D = Reg.D;
            break;
            case 0x53:
            Reg.D = Reg.E;
            break;
            case 0x54:
            Reg.D = Reg.H;
            break;
            case 0x55:
            Reg.D = Reg.L;
            break;
            case 0x56:
            Reg.D = emulator.Read(Reg.HL);
            break;

            // LD E,r2
            case 0x5F:
            Reg.E = Reg.A;
            break;
            case 0x58:
            Reg.E = Reg.B;
            break;
            case 0x59:
            Reg.E = Reg.C;
            break;
            case 0x5A:
            Reg.E = Reg.D;
            break;
            case 0x5B:
            Reg.E = Reg.E;
            break;
            case 0x5C:
            Reg.E = Reg.H;
            break;
            case 0x5D:
            Reg.E = Reg.L;
            break;
            case 0x5E:
            Reg.E = emulator.Read(Reg.HL);
            break;

            // LD H,r2
            case 0x67:
            Reg.H = Reg.A;
            break;
            case 0x60:
            Reg.H = Reg.B;
            break;
            case 0x61:
            Reg.H = Reg.C;
            break;
            case 0x62:
            Reg.H = Reg.D;
            break;
            case 0x63:
            Reg.H = Reg.E;
            break;
            case 0x64:
            Reg.H = Reg.H;
            break;
            case 0x65:
            Reg.H = Reg.L;
            break;
            case 0x66:
            Reg.H = emulator.Read(Reg.HL);
            break;

            // LD L,r2
            case 0x6F:
            Reg.L = Reg.A;
            break;
            case 0x68:
            Reg.L = Reg.B;
            break;
            case 0x69:
            Reg.L = Reg.C;
            break;
            case 0x6A:
            Reg.L = Reg.D;
            break;
            case 0x6B:
            Reg.L = Reg.E;
            break;
            case 0x6C:
            Reg.L = Reg.H;
            break;
            case 0x6D:
            Reg.L = Reg.L;
            break;
            case 0x6E:
            Reg.L = emulator.Read(Reg.HL);
            break;

            // LD (HL),r2
            case 0x77:
            emulator.Write(Reg.HL, Reg.A);
            break;
            case 0x70:
            emulator.Write(Reg.HL, Reg.B);
            break;
            case 0x71:
            emulator.Write(Reg.HL, Reg.C);
            break;
            case 0x72:
            emulator.Write(Reg.HL, Reg.D);
            break;
            case 0x73:
            emulator.Write(Reg.HL, Reg.E);
            break;
            case 0x74:
            emulator.Write(Reg.HL, Reg.H);
            break;
            case 0x75:
            emulator.Write(Reg.HL, Reg.L);
            break;
            case 0x36:
            emulator.Write(Reg.HL, NextByte());
            break;

            // LD A,n
            case 0x0A:
            Reg.A = emulator.Read(Reg.BC);
            break;
            case 0x1A:
            Reg.A = emulator.Read(Reg.DE);
            break;
            case 0x7E:
            Reg.A = emulator.Read(Reg.HL);
            break;
            case 0x3E:
            Reg.A = NextByte();
            break;

            // LD n,A
            case 0x02:
            emulator.Write(Reg.BC, Reg.A);
            break;
            case 0x12:
            emulator.Write(Reg.DE, Reg.A);
            break;
            case 0xEA:
            emulator.Write(NextShort(), Reg.A);
            break;

            // LD A,(C)
            case 0xF2:
            Reg.A = emulator.Read((ushort)(0xFF00 + Reg.C));
            break;

            // LD (C),A
            case 0xE2:
            emulator.Write((ushort)(0xFF00 + Reg.C), Reg.A);
            break;

            // LD A,(HL-)
            case 0x3A:
            Reg.A = emulator.Read(Reg.HL--);
            break;

            // LD (HL-),A
            case 0x32:
            emulator.Write(Reg.HL--, Reg.A);
            break;

            // LD A,(HL+)
            case 0x2A:
            Reg.A = emulator.Read(Reg.HL++);
            break;

            // LD (HL+),A
            case 0x22:
            emulator.Write(Reg.HL++, Reg.A);
            break;

            // LDH (n),A
            case 0xE0:
            emulator.Write((ushort)(0xFF00 + NextByte()), Reg.A);
            break;

            // LDH A,(n)
            case 0xF0:
            Reg.A = emulator.Read((ushort)(0xFF00 + NextByte()));
            break;

            #endregion

            #region 16-Bit Loads

            // LD n,nn
            case 0x01:
            Reg.BC = NextShort();
            break;
            case 0x11:
            Reg.DE = NextShort();
            break;
            case 0x21:
            Reg.HL = NextShort();
            break;
            case 0x31:
            Reg.SP = NextShort();
            break;

            // LD SP,HL
            case 0xF9:
            Reg.SP = Reg.HL;
            break;

            // LD HL,SP+n
            case 0xF8:
            Reg.HL = ADDS(Reg.SP);
            break;

            // LD (nn),SP
            case 0x08:
            emulator.WriteShort(NextShort(), Reg.SP);
            break;

            // Push nn
            case 0xF5:
            Push(Reg.AF);
            break;
            case 0xC5:
            Push(Reg.BC);
            break;
            case 0xD5:
            Push(Reg.DE);
            break;
            case 0xE5:
            Push(Reg.HL);
            break;

            // Pop nn
            case 0xF1:
            Reg.AF = Pop();
            break;
            case 0xC1:
            Reg.BC = Pop();
            break;
            case 0xD1:
            Reg.DE = Pop();
            break;
            case 0xE1:
            Reg.HL = Pop();
            break;

            #endregion

            #region 8-Bit ALU

            // ADD A,n
            case 0x87:
            ADD(Reg.A);
            break;
            case 0x80:
            ADD(Reg.B);
            break;
            case 0x81:
            ADD(Reg.C);
            break;
            case 0x82:
            ADD(Reg.D);
            break;
            case 0x83:
            ADD(Reg.E);
            break;
            case 0x84:
            ADD(Reg.H);
            break;
            case 0x85:
            ADD(Reg.L);
            break;
            case 0x86:
            ADD(emulator.Read(Reg.HL));
            break;
            case 0xC6:
            ADD(NextByte());
            break;

            // ADC A,n
            case 0x8F:
            ADC(Reg.A);
            break;
            case 0x88:
            ADC(Reg.B);
            break;
            case 0x89:
            ADC(Reg.C);
            break;
            case 0x8A:
            ADC(Reg.D);
            break;
            case 0x8B:
            ADC(Reg.E);
            break;
            case 0x8C:
            ADC(Reg.H);
            break;
            case 0x8D:
            ADC(Reg.L);
            break;
            case 0x8E:
            ADC(emulator.Read(Reg.HL));
            break;
            case 0xCE:
            ADC(NextByte());
            break;

            // SUB A,n
            case 0x97:
            SUB(Reg.A);
            break;
            case 0x90:
            SUB(Reg.B);
            break;
            case 0x91:
            SUB(Reg.C);
            break;
            case 0x92:
            SUB(Reg.D);
            break;
            case 0x93:
            SUB(Reg.E);
            break;
            case 0x94:
            SUB(Reg.H);
            break;
            case 0x95:
            SUB(Reg.L);
            break;
            case 0x96:
            SUB(emulator.Read(Reg.HL));
            break;
            case 0xD6:
            SUB(NextByte());
            break;

            // SBC A,n
            case 0x9F:
            SBC(Reg.A);
            break;
            case 0x98:
            SBC(Reg.B);
            break;
            case 0x99:
            SBC(Reg.C);
            break;
            case 0x9A:
            SBC(Reg.D);
            break;
            case 0x9B:
            SBC(Reg.E);
            break;
            case 0x9C:
            SBC(Reg.H);
            break;
            case 0x9D:
            SBC(Reg.L);
            break;
            case 0x9E:
            SBC(emulator.Read(Reg.HL));
            break;
            case 0xDE:
            SBC(NextByte());
            break;

            // AND n
            case 0xA7:
            AND(Reg.A);
            break;
            case 0xA0:
            AND(Reg.B);
            break;
            case 0xA1:
            AND(Reg.C);
            break;
            case 0xA2:
            AND(Reg.D);
            break;
            case 0xA3:
            AND(Reg.E);
            break;
            case 0xA4:
            AND(Reg.H);
            break;
            case 0xA5:
            AND(Reg.L);
            break;
            case 0xA6:
            AND(emulator.Read(Reg.HL));
            break;
            case 0xE6:
            AND(NextByte());
            break;

            // OR n
            case 0xB7:
            OR(Reg.A);
            break;
            case 0xB0:
            OR(Reg.B);
            break;
            case 0xB1:
            OR(Reg.C);
            break;
            case 0xB2:
            OR(Reg.D);
            break;
            case 0xB3:
            OR(Reg.E);
            break;
            case 0xB4:
            OR(Reg.H);
            break;
            case 0xB5:
            OR(Reg.L);
            break;
            case 0xB6:
            OR(emulator.Read(Reg.HL));
            break;
            case 0xF6:
            OR(NextByte());
            break;

            // XOR n
            case 0xAF:
            XOR(Reg.A);
            break;
            case 0xA8:
            XOR(Reg.B);
            break;
            case 0xA9:
            XOR(Reg.C);
            break;
            case 0xAA:
            XOR(Reg.D);
            break;
            case 0xAB:
            XOR(Reg.E);
            break;
            case 0xAC:
            XOR(Reg.H);
            break;
            case 0xAD:
            XOR(Reg.L);
            break;
            case 0xAE:
            XOR(emulator.Read(Reg.HL));
            break;
            case 0xEE:
            XOR(NextByte());
            break;

            // CP n
            case 0xBF:
            CP(Reg.A);
            break;
            case 0xB8:
            CP(Reg.B);
            break;
            case 0xB9:
            CP(Reg.C);
            break;
            case 0xBA:
            CP(Reg.D);
            break;
            case 0xBB:
            CP(Reg.E);
            break;
            case 0xBC:
            CP(Reg.H);
            break;
            case 0xBD:
            CP(Reg.L);
            break;
            case 0xBE:
            CP(emulator.Read(Reg.HL));
            break;
            case 0xFE:
            CP(NextByte());
            break;

            // INC n
            case 0x3C:
            Reg.A = INC(Reg.A);
            break;
            case 0x04:
            Reg.B = INC(Reg.B);
            break;
            case 0x0C:
            Reg.C = INC(Reg.C);
            break;
            case 0x14:
            Reg.D = INC(Reg.D);
            break;
            case 0x1C:
            Reg.E = INC(Reg.E);
            break;
            case 0x24:
            Reg.H = INC(Reg.H);
            break;
            case 0x2C:
            Reg.L = INC(Reg.L);
            break;
            case 0x34:
            emulator.Write(Reg.HL, INC(emulator.Read(Reg.HL)));
            break;

            // DEC n
            case 0x3D:
            Reg.A = DEC(Reg.A);
            break;
            case 0x05:
            Reg.B = DEC(Reg.B);
            break;
            case 0x0D:
            Reg.C = DEC(Reg.C);
            break;
            case 0x15:
            Reg.D = DEC(Reg.D);
            break;
            case 0x1D:
            Reg.E = DEC(Reg.E);
            break;
            case 0x25:
            Reg.H = DEC(Reg.H);
            break;
            case 0x2D:
            Reg.L = DEC(Reg.L);
            break;
            case 0x35:
            emulator.Write(Reg.HL, DEC(emulator.Read(Reg.HL)));
            break;

            #endregion

            #region 16-Bit Arithmetic

            // ADD HL,n
            case 0x09:
            ADD(Reg.BC);
            break;
            case 0x19:
            ADD(Reg.DE);
            break;
            case 0x29:
            ADD(Reg.HL);
            break;
            case 0x39:
            ADD(Reg.SP);
            break;


            // ADD SP,n
            case 0xE8:
            Reg.SP = ADDS(Reg.SP);
            break;

            // INC nn
            case 0x03:
            Reg.BC++;
            break;
            case 0x13:
            Reg.DE++;
            break;
            case 0x23:
            Reg.HL++;
            break;
            case 0x33:
            Reg.SP++;
            break;

            // DEC nn
            case 0x0B:
            Reg.BC--;
            break;
            case 0x1B:
            Reg.DE--;
            break;
            case 0x2B:
            Reg.HL--;
            break;
            case 0x3B:
            Reg.SP--;
            break;

            #endregion

            #region Miscellaneous

            // CB Prefixed
            case 0xCB:
            _ = PrefixedTable();
            break;

            // DAA
            case 0x27:
            DAA();
            break;

            // CPL
            case 0x2F:
            CPL();
            break;

            // CCF
            case 0x3F:
            CCF();
            break;

            // SCF
            case 0x37:
            SCF();
            break;

            // NOP
            case 0x00:
            break;

            // HALT
            case 0x76:
            emulator.interrupt.Halt();
            break;

            // STOP
            case 0x10:
            break;

            // DI
            case 0xF3:
            emulator.interrupt.Disable();
            break;

            // EI
            case 0xFB:
            emulator.interrupt.Enable();
            break;

            #endregion

            #region Jumps

            // JP nn
            case 0xC3:
            JP(true);
            break;

            // JP cc,nn
            case 0xC2:
            JP(Reg.GetFlag(Flag.Z) == false);
            break;
            case 0xCA:
            JP(Reg.GetFlag(Flag.Z));
            break;
            case 0xD2:
            JP(Reg.GetFlag(Flag.C) == false);
            break;
            case 0xDA:
            JP(Reg.GetFlag(Flag.C));
            break;

            // JP (HL)
            case 0xE9:
            JP(Reg.HL);
            break;

            // JP n
            case 0x18:
            JR(true);
            break;

            // JR cc,n
            case 0x20:
            JR(Reg.GetFlag(Flag.Z) == false);
            break;
            case 0x28:
            JR(Reg.GetFlag(Flag.Z));
            break;
            case 0x30:
            JR(Reg.GetFlag(Flag.C) == false);
            break;
            case 0x38:
            JR(Reg.GetFlag(Flag.C));
            break;

            #endregion

            #region Calls

            // CALL nn
            case 0xCD:
            CALL(true);
            break;

            // CALL cc,nn
            case 0xC4:
            CALL(Reg.GetFlag(Flag.Z) == false);
            break;
            case 0xCC:
            CALL(Reg.GetFlag(Flag.Z));
            break;
            case 0xD4:
            CALL(Reg.GetFlag(Flag.C) == false);
            break;
            case 0xDC:
            CALL(Reg.GetFlag(Flag.C));
            break;

            #endregion

            #region Restarts

            // RST n
            case 0xC7:
            RST(0x00);
            break;
            case 0xCF:
            RST(0x08);
            break;
            case 0xD7:
            RST(0x10);
            break;
            case 0xDF:
            RST(0x18);
            break;
            case 0xE7:
            RST(0x20);
            break;
            case 0xEF:
            RST(0x28);
            break;
            case 0xF7:
            RST(0x30);
            break;
            case 0xFF:
            RST(0x38);
            break;

            #endregion

            #region Returns

            // RET
            case 0xC9:
            RET(true);
            break;

            // RET cc
            case 0xC0:
            RET(Reg.GetFlag(Flag.Z) == false);

            break;
            case 0xC8:
            RET(Reg.GetFlag(Flag.Z));
            break;

            case 0xD0:
            RET(Reg.GetFlag(Flag.C) ==
              false);
            break;
            case 0xD8:
            RET(Reg.GetFlag(Flag.C));
            break;

            // RETI
            case 0xD9:
            RET(true);
            emulator.interrupt.Enable();
            break;

            #endregion

            #region Rotates

            // RLCA
            case 0x07:
            RLCA();
            break;

            //RLA
            case 0x17:
            RLA();
            break;

            // RRCA
            case 0x0F:
            RRCA();
            break;

            // RRA
            case 0x1F:
            RRA();
            break;

            #endregion

            #region Illegal

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
            default:
            {
                throw new InvalidOperationException("Illegal Instruction : " + Opcode);
            }
            #endregion
        }


        switch (Opcode)
        {
            case 0x20:
            case 0x30:
            if (!Reg.GetFlag(Flag.Z))
            {
                conditionalCycles = 1;
            }
            break;

            case 0x28:
            case 0x38:
            if (Reg.GetFlag(Flag.Z))
            {
                conditionalCycles = 1;
            }
            break;

            case 0xc0:
            case 0xd0:
            if (!Reg.GetFlag(Flag.Z))
            {
                conditionalCycles = 3;
            }
            break;

            case 0xc8:
            case 0xcc:
            case 0xd8:
            case 0xdc:
            if (Reg.GetFlag(Flag.Z))
            {
                conditionalCycles = 3;
            }
            break;

            case 0xc2:
            case 0xd2:
            if (!Reg.GetFlag(Flag.Z))
            {
                conditionalCycles = 3;
            }
            break;

            case 0xca:
            case 0xda:
            if (Reg.GetFlag(Flag.Z))
            {
                conditionalCycles = 1;
            }
            break;

            case 0xc4:
            case 0xd4:
            if (!Reg.GetFlag(Flag.Z))
            {
                conditionalCycles = 3;
            }
            break;
            default:
            break;
        }


        return (byte)(mainTimes[Opcode] + conditionalCycles);
    }

    private byte PrefixedTable()
    {
        Opcode = NextByte();

        switch (Opcode)
        {
            #region Miscellaneous

            // SWAP n
            case 0x37:
            Reg.A = SWAP(Reg.A);
            break;
            case 0x30:
            Reg.B = SWAP(Reg.B);
            break;
            case 0x31:
            Reg.C = SWAP(Reg.C);
            break;
            case 0x32:
            Reg.D = SWAP(Reg.D);
            break;
            case 0x33:
            Reg.E = SWAP(Reg.E);
            break;
            case 0x34:
            Reg.H = SWAP(Reg.H);
            break;
            case 0x35:
            Reg.L = SWAP(Reg.L);
            break;
            case 0x36:
            emulator.Write(Reg.HL, SWAP(emulator.Read(Reg.HL)));
            break;

            #endregion

            #region Rotates & Shifts

            // RLC n
            case 0x07:
            Reg.A = RLC(Reg.A);
            break;
            case 0x00:
            Reg.B = RLC(Reg.B);
            break;
            case 0x01:
            Reg.C = RLC(Reg.C);
            break;
            case 0x02:
            Reg.D = RLC(Reg.D);
            break;
            case 0x03:
            Reg.E = RLC(Reg.E);
            break;
            case 0x04:
            Reg.H = RLC(Reg.H);
            break;
            case 0x05:
            Reg.L = RLC(Reg.L);
            break;
            case 0x06:
            emulator.Write(Reg.HL, RLC(emulator.Read(Reg.HL)));
            break;

            // RL n
            case 0x17:
            Reg.A = RL(Reg.A);
            break;
            case 0x10:
            Reg.B = RL(Reg.B);
            break;
            case 0x11:
            Reg.C = RL(Reg.C);
            break;
            case 0x12:
            Reg.D = RL(Reg.D);
            break;
            case 0x13:
            Reg.E = RL(Reg.E);
            break;
            case 0x14:
            Reg.H = RL(Reg.H);
            break;
            case 0x15:
            Reg.L = RL(Reg.L);
            break;
            case 0x16:
            emulator.Write(Reg.HL, RL(emulator.Read(Reg.HL)));
            break;

            // RRC n
            case 0x0F:
            Reg.A = RRC(Reg.A);
            break;
            case 0x08:
            Reg.B = RRC(Reg.B);
            break;
            case 0x09:
            Reg.C = RRC(Reg.C);
            break;
            case 0x0A:
            Reg.D = RRC(Reg.D);
            break;
            case 0x0B:
            Reg.E = RRC(Reg.E);
            break;
            case 0x0C:
            Reg.H = RRC(Reg.H);
            break;
            case 0x0D:
            Reg.L = RRC(Reg.L);
            break;
            case 0x0E:
            emulator.Write(Reg.HL, RRC(emulator.Read(Reg.HL)));
            break;

            // RR n
            case 0x1F:
            Reg.A = RR(Reg.A);
            break;
            case 0x18:
            Reg.B = RR(Reg.B);
            break;
            case 0x19:
            Reg.C = RR(Reg.C);
            break;
            case 0x1A:
            Reg.D = RR(Reg.D);
            break;
            case 0x1B:
            Reg.E = RR(Reg.E);
            break;
            case 0x1C:
            Reg.H = RR(Reg.H);
            break;
            case 0x1D:
            Reg.L = RR(Reg.L);
            break;
            case 0x1E:
            emulator.Write(Reg.HL, RR(emulator.Read(Reg.HL)));
            break;

            // SLA n
            case 0x27:
            Reg.A = SLA(Reg.A);
            break;
            case 0x20:
            Reg.B = SLA(Reg.B);
            break;
            case 0x21:
            Reg.C = SLA(Reg.C);
            break;
            case 0x22:
            Reg.D = SLA(Reg.D);
            break;
            case 0x23:
            Reg.E = SLA(Reg.E);
            break;
            case 0x24:
            Reg.H = SLA(Reg.H);
            break;
            case 0x25:
            Reg.L = SLA(Reg.L);
            break;
            case 0x26:
            emulator.Write(Reg.HL, SLA(emulator.Read(Reg.HL)));
            break;

            // SRA n
            case 0x2F:
            Reg.A = SRA(Reg.A);
            break;
            case 0x28:
            Reg.B = SRA(Reg.B);
            break;
            case 0x29:
            Reg.C = SRA(Reg.C);
            break;
            case 0x2A:
            Reg.D = SRA(Reg.D);
            break;
            case 0x2B:
            Reg.E = SRA(Reg.E);
            break;
            case 0x2C:
            Reg.H = SRA(Reg.H);
            break;
            case 0x2D:
            Reg.L = SRA(Reg.L);
            break;
            case 0x2E:
            emulator.Write(Reg.HL, SRA(emulator.Read(Reg.HL)));
            break;

            // SRL
            case 0x3F:
            Reg.A = SRL(Reg.A);
            break;
            case 0x38:
            Reg.B = SRL(Reg.B);
            break;
            case 0x39:
            Reg.C = SRL(Reg.C);
            break;
            case 0x3A:
            Reg.D = SRL(Reg.D);
            break;
            case 0x3B:
            Reg.E = SRL(Reg.E);
            break;
            case 0x3C:
            Reg.H = SRL(Reg.H);
            break;
            case 0x3D:
            Reg.L = SRL(Reg.L);
            break;
            case 0x3E:
            emulator.Write(Reg.HL, SRL(emulator.Read(Reg.HL)));
            break;

            #endregion

            #region Bit Opcodes

            // BIT 0,r
            case 0x47:
            BIT(0b00000001, Reg.A);
            break;
            case 0x40:
            BIT(0b00000001, Reg.B);
            break;
            case 0x41:
            BIT(0b00000001, Reg.C);
            break;
            case 0x42:
            BIT(0b00000001, Reg.D);
            break;
            case 0x43:
            BIT(0b00000001, Reg.E);
            break;
            case 0x44:
            BIT(0b00000001, Reg.H);
            break;
            case 0x45:
            BIT(0b00000001, Reg.L);
            break;
            case 0x46:
            BIT(0b00000001, emulator.Read(Reg.HL));
            break;

            // BIT 1,r
            case 0x4F:
            BIT(0b00000010, Reg.A);
            break;
            case 0x48:
            BIT(0b00000010, Reg.B);
            break;
            case 0x49:
            BIT(0b00000010, Reg.C);
            break;
            case 0x4A:
            BIT(0b00000010, Reg.D);
            break;
            case 0x4B:
            BIT(0b00000010, Reg.E);
            break;
            case 0x4C:
            BIT(0b00000010, Reg.H);
            break;
            case 0x4D:
            BIT(0b00000010, Reg.L);
            break;
            case 0x4E:
            BIT(0b00000010, emulator.Read(Reg.HL));
            break;

            // BIT 2,r
            case 0x57:
            BIT(0b00000100, Reg.A);
            break;
            case 0x50:
            BIT(0b00000100, Reg.B);
            break;
            case 0x51:
            BIT(0b00000100, Reg.C);
            break;
            case 0x52:
            BIT(0b00000100, Reg.D);
            break;
            case 0x53:
            BIT(0b00000100, Reg.E);
            break;
            case 0x54:
            BIT(0b00000100, Reg.H);
            break;
            case 0x55:
            BIT(0b00000100, Reg.L);
            break;
            case 0x56:
            BIT(0b00000100, emulator.Read(Reg.HL));
            break;

            // BIT 3,r
            case 0x5F:
            BIT(0b00001000, Reg.A);
            break;
            case 0x58:
            BIT(0b00001000, Reg.B);
            break;
            case 0x59:
            BIT(0b00001000, Reg.C);
            break;
            case 0x5A:
            BIT(0b00001000, Reg.D);
            break;
            case 0x5B:
            BIT(0b00001000, Reg.E);
            break;
            case 0x5C:
            BIT(0b00001000, Reg.H);
            break;
            case 0x5D:
            BIT(0b00001000, Reg.L);
            break;
            case 0x5E:
            BIT(0b00001000, emulator.Read(Reg.HL));
            break;

            // BIT 4,r
            case 0x67:
            BIT(0b00010000, Reg.A);
            break;
            case 0x60:
            BIT(0b00010000, Reg.B);
            break;
            case 0x61:
            BIT(0b00010000, Reg.C);
            break;
            case 0x62:
            BIT(0b00010000, Reg.D);
            break;
            case 0x63:
            BIT(0b00010000, Reg.E);
            break;
            case 0x64:
            BIT(0b00010000, Reg.H);
            break;
            case 0x65:
            BIT(0b00010000, Reg.L);
            break;
            case 0x66:
            BIT(0b00010000, emulator.Read(Reg.HL));
            break;

            // BIT 5,r
            case 0x6F:
            BIT(0b00100000, Reg.A);
            break;
            case 0x68:
            BIT(0b00100000, Reg.B);
            break;
            case 0x69:
            BIT(0b00100000, Reg.C);
            break;
            case 0x6A:
            BIT(0b00100000, Reg.D);
            break;
            case 0x6B:
            BIT(0b00100000, Reg.E);
            break;
            case 0x6C:
            BIT(0b00100000, Reg.H);
            break;
            case 0x6D:
            BIT(0b00100000, Reg.L);
            break;
            case 0x6E:
            BIT(0b00100000, emulator.Read(Reg.HL));
            break;

            // BIT 6,r
            case 0x77:
            BIT(0b01000000, Reg.A);
            break;
            case 0x70:
            BIT(0b01000000, Reg.B);
            break;
            case 0x71:
            BIT(0b01000000, Reg.C);
            break;
            case 0x72:
            BIT(0b01000000, Reg.D);
            break;
            case 0x73:
            BIT(0b01000000, Reg.E);
            break;
            case 0x74:
            BIT(0b01000000, Reg.H);
            break;
            case 0x75:
            BIT(0b01000000, Reg.L);
            break;
            case 0x76:
            BIT(0b01000000, emulator.Read(Reg.HL));
            break;

            // BIT 7,r
            case 0x7F:
            BIT(0b01000000, Reg.A);
            break;
            case 0x78:
            BIT(0b01000000, Reg.B);
            break;
            case 0x79:
            BIT(0b01000000, Reg.C);
            break;
            case 0x7A:
            BIT(0b01000000, Reg.D);
            break;
            case 0x7B:
            BIT(0b01000000, Reg.E);
            break;
            case 0x7C:
            BIT(0b01000000, Reg.H);
            break;
            case 0x7D:
            BIT(0b01000000, Reg.L);
            break;
            case 0x7E:
            BIT(0b01000000, emulator.Read(Reg.HL));
            break;

            // SET 0,r
            case 0xC7:
            Reg.A = SET(0b00000001, Reg.A);
            break;
            case 0xC0:
            Reg.B = SET(0b00000001, Reg.B);
            break;
            case 0xC1:
            Reg.C = SET(0b00000001, Reg.C);
            break;
            case 0xC2:
            Reg.D = SET(0b00000001, Reg.D);
            break;
            case 0xC3:
            Reg.E = SET(0b00000001, Reg.E);
            break;
            case 0xC4:
            Reg.H = SET(0b00000001, Reg.H);
            break;
            case 0xC5:
            Reg.L = SET(0b00000001, Reg.L);
            break;
            case 0xC6:
            emulator.Write(Reg.HL, SET(0b00000001, emulator.Read(Reg.HL)));
            break;

            // SET 1,r
            case 0xCF:
            Reg.A = SET(0b00000010, Reg.A);
            break;
            case 0xC8:
            Reg.B = SET(0b00000010, Reg.B);
            break;
            case 0xC9:
            Reg.C = SET(0b00000010, Reg.C);
            break;
            case 0xCA:
            Reg.D = SET(0b00000010, Reg.D);
            break;
            case 0xCB:
            Reg.E = SET(0b00000010, Reg.E);
            break;
            case 0xCC:
            Reg.H = SET(0b00000010, Reg.H);
            break;
            case 0xCD:
            Reg.L = SET(0b00000010, Reg.L);
            break;
            case 0xCE:
            emulator.Write(Reg.HL, SET(0b00000010, emulator.Read(Reg.HL)));
            break;

            // SET 2,r
            case 0xD7:
            Reg.A = SET(0b00000100, Reg.A);
            break;
            case 0xD0:
            Reg.B = SET(0b00000100, Reg.B);
            break;
            case 0xD1:
            Reg.C = SET(0b00000100, Reg.C);
            break;
            case 0xD2:
            Reg.D = SET(0b00000100, Reg.D);
            break;
            case 0xD3:
            Reg.E = SET(0b00000100, Reg.E);
            break;
            case 0xD4:
            Reg.H = SET(0b00000100, Reg.H);
            break;
            case 0xD5:
            Reg.L = SET(0b00000100, Reg.L);
            break;
            case 0xD6:
            emulator.Write(Reg.HL, SET(0b00000100, emulator.Read(Reg.HL)));
            break;

            // SET 3,r
            case 0xDF:
            Reg.A = SET(0b00001000, Reg.A);
            break;
            case 0xD8:
            Reg.B = SET(0b00001000, Reg.B);
            break;
            case 0xD9:
            Reg.C = SET(0b00001000, Reg.C);
            break;
            case 0xDA:
            Reg.D = SET(0b00001000, Reg.D);
            break;
            case 0xDB:
            Reg.E = SET(0b00001000, Reg.E);
            break;
            case 0xDC:
            Reg.H = SET(0b00001000, Reg.H);
            break;
            case 0xDD:
            Reg.L = SET(0b00001000, Reg.L);
            break;
            case 0xDE:
            emulator.Write(Reg.HL, SET(0b00001000, emulator.Read(Reg.HL)));
            break;

            // SET 4,r
            case 0xE7:
            Reg.A = SET(0b00010000, Reg.A);
            break;
            case 0xE0:
            Reg.B = SET(0b00010000, Reg.B);
            break;
            case 0xE1:
            Reg.C = SET(0b00010000, Reg.C);
            break;
            case 0xE2:
            Reg.D = SET(0b00010000, Reg.D);
            break;
            case 0xE3:
            Reg.E = SET(0b00010000, Reg.E);
            break;
            case 0xE4:
            Reg.H = SET(0b00010000, Reg.H);
            break;
            case 0xE5:
            Reg.L = SET(0b00010000, Reg.L);
            break;
            case 0xE6:
            emulator.Write(Reg.HL, SET(0b00010000, emulator.Read(Reg.HL)));
            break;

            // SET 5,r
            case 0xEF:
            Reg.A = SET(0b00100000, Reg.A);
            break;
            case 0xE8:
            Reg.B = SET(0b00100000, Reg.B);
            break;
            case 0xE9:
            Reg.C = SET(0b00100000, Reg.C);
            break;
            case 0xEA:
            Reg.D = SET(0b00100000, Reg.D);
            break;
            case 0xEB:
            Reg.E = SET(0b00100000, Reg.E);
            break;
            case 0xEC:
            Reg.H = SET(0b00100000, Reg.H);
            break;
            case 0xED:
            Reg.L = SET(0b00100000, Reg.L);
            break;
            case 0xEE:
            emulator.Write(Reg.HL, SET(0b00100000, emulator.Read(Reg.HL)));
            break;

            // SET 6,r
            case 0xF7:
            Reg.A = SET(0b01000000, Reg.A);
            break;
            case 0xF0:
            Reg.B = SET(0b01000000, Reg.B);
            break;
            case 0xF1:
            Reg.C = SET(0b01000000, Reg.C);
            break;
            case 0xF2:
            Reg.D = SET(0b01000000, Reg.D);
            break;
            case 0xF3:
            Reg.E = SET(0b01000000, Reg.E);
            break;
            case 0xF4:
            Reg.H = SET(0b01000000, Reg.H);
            break;
            case 0xF5:
            Reg.L = SET(0b01000000, Reg.L);
            break;
            case 0xF6:
            emulator.Write(Reg.HL, SET(0b01000000, emulator.Read(Reg.HL)));
            break;

            // SET 7,r
            case 0xFF:
            Reg.A = SET(0b01000000, Reg.A);
            break;
            case 0xF8:
            Reg.B = SET(0b01000000, Reg.B);
            break;
            case 0xF9:
            Reg.C = SET(0b01000000, Reg.C);
            break;
            case 0xFA:
            Reg.D = SET(0b01000000, Reg.D);
            break;
            case 0xFB:
            Reg.E = SET(0b01000000, Reg.E);
            break;
            case 0xFC:
            Reg.H = SET(0b01000000, Reg.H);
            break;
            case 0xFD:
            Reg.L = SET(0b01000000, Reg.L);
            break;
            case 0xFE:
            emulator.Write(Reg.HL, SET(0b01000000, emulator.Read(Reg.HL)));
            break;

            // RES 0,r
            case 0x87:
            Reg.A = RES(0b00000001, Reg.A);
            break;
            case 0x80:
            Reg.B = RES(0b00000001, Reg.B);
            break;
            case 0x81:
            Reg.C = RES(0b00000001, Reg.C);
            break;
            case 0x82:
            Reg.D = RES(0b00000001, Reg.D);
            break;
            case 0x83:
            Reg.E = RES(0b00000001, Reg.E);
            break;
            case 0x84:
            Reg.H = RES(0b00000001, Reg.H);
            break;
            case 0x85:
            Reg.L = RES(0b00000001, Reg.L);
            break;
            case 0x86:
            emulator.Write(Reg.HL, RES(0b00000001, emulator.Read(Reg.HL)));
            break;

            // RES 1,r
            case 0x8F:
            Reg.A = RES(0b00000010, Reg.A);
            break;
            case 0x88:
            Reg.B = RES(0b00000010, Reg.B);
            break;
            case 0x89:
            Reg.C = RES(0b00000010, Reg.C);
            break;
            case 0x8A:
            Reg.D = RES(0b00000010, Reg.D);
            break;
            case 0x8B:
            Reg.E = RES(0b00000010, Reg.E);
            break;
            case 0x8C:
            Reg.H = RES(0b00000010, Reg.H);
            break;
            case 0x8D:
            Reg.L = RES(0b00000010, Reg.L);
            break;
            case 0x8E:
            emulator.Write(Reg.HL, RES(0b00000010, emulator.Read(Reg.HL)));
            break;

            // RES 2,r
            case 0x97:
            Reg.A = RES(0b00000100, Reg.A);
            break;
            case 0x90:
            Reg.B = RES(0b00000100, Reg.B);
            break;
            case 0x91:
            Reg.C = RES(0b00000100, Reg.C);
            break;
            case 0x92:
            Reg.D = RES(0b00000100, Reg.D);
            break;
            case 0x93:
            Reg.E = RES(0b00000100, Reg.E);
            break;
            case 0x94:
            Reg.H = RES(0b00000100, Reg.H);
            break;
            case 0x95:
            Reg.L = RES(0b00000100, Reg.L);
            break;
            case 0x96:
            emulator.Write(Reg.HL, RES(0b00000100, emulator.Read(Reg.HL)));
            break;

            // RES 3,r
            case 0x9F:
            Reg.A = RES(0b00001000, Reg.A);
            break;
            case 0x98:
            Reg.B = RES(0b00001000, Reg.B);
            break;
            case 0x99:
            Reg.C = RES(0b00001000, Reg.C);
            break;
            case 0x9A:
            Reg.D = RES(0b00001000, Reg.D);
            break;
            case 0x9B:
            Reg.E = RES(0b00001000, Reg.E);
            break;
            case 0x9C:
            Reg.H = RES(0b00001000, Reg.H);
            break;
            case 0x9D:
            Reg.L = RES(0b00001000, Reg.L);
            break;
            case 0x9E:
            emulator.Write(Reg.HL, RES(0b00001000, emulator.Read(Reg.HL)));
            break;

            // RES 4,r
            case 0xA7:
            Reg.A = RES(0b00010000, Reg.A);
            break;
            case 0xA0:
            Reg.B = RES(0b00010000, Reg.B);
            break;
            case 0xA1:
            Reg.C = RES(0b00010000, Reg.C);
            break;
            case 0xA2:
            Reg.D = RES(0b00010000, Reg.D);
            break;
            case 0xA3:
            Reg.E = RES(0b00010000, Reg.E);
            break;
            case 0xA4:
            Reg.H = RES(0b00010000, Reg.H);
            break;
            case 0xA5:
            Reg.L = RES(0b00010000, Reg.L);
            break;
            case 0xA6:
            emulator.Write(Reg.HL, RES(0b00010000, emulator.Read(Reg.HL)));
            break;

            // RES 5,r
            case 0xAF:
            Reg.A = RES(0b00100000, Reg.A);
            break;
            case 0xA8:
            Reg.B = RES(0b00100000, Reg.B);
            break;
            case 0xA9:
            Reg.C = RES(0b00100000, Reg.C);
            break;
            case 0xAA:
            Reg.D = RES(0b00100000, Reg.D);
            break;
            case 0xAB:
            Reg.E = RES(0b00100000, Reg.E);
            break;
            case 0xAC:
            Reg.H = RES(0b00100000, Reg.H);
            break;
            case 0xAD:
            Reg.L = RES(0b00100000, Reg.L);
            break;
            case 0xAE:
            emulator.Write(Reg.HL, RES(0b00100000, emulator.Read(Reg.HL)));
            break;

            // RES 6,r
            case 0xB7:
            Reg.A = RES(0b01000000, Reg.A);
            break;
            case 0xB0:
            Reg.B = RES(0b01000000, Reg.B);
            break;
            case 0xB1:
            Reg.C = RES(0b01000000, Reg.C);
            break;
            case 0xB2:
            Reg.D = RES(0b01000000, Reg.D);
            break;
            case 0xB3:
            Reg.E = RES(0b01000000, Reg.E);
            break;
            case 0xB4:
            Reg.H = RES(0b01000000, Reg.H);
            break;
            case 0xB5:
            Reg.L = RES(0b01000000, Reg.L);
            break;
            case 0xB6:
            emulator.Write(Reg.HL, RES(0b01000000, emulator.Read(Reg.HL)));
            break;

            // RES 7,r
            case 0xBF:
            Reg.A = RES(0b01000000, Reg.A);
            break;
            case 0xB8:
            Reg.B = RES(0b01000000, Reg.B);
            break;
            case 0xB9:
            Reg.C = RES(0b01000000, Reg.C);
            break;
            case 0xBA:
            Reg.D = RES(0b01000000, Reg.D);
            break;
            case 0xBB:
            Reg.E = RES(0b01000000, Reg.E);
            break;
            case 0xBC:
            Reg.H = RES(0b01000000, Reg.H);
            break;
            case 0xBD:
            Reg.L = RES(0b01000000, Reg.L);
            break;
            case 0xBE:
            emulator.Write(Reg.HL, RES(0b01000000, emulator.Read(Reg.HL)));
            break;
            default:

            #endregion
        }

        return cbTimes[Opcode];
    }

    public void Push(ushort data)
    {
        Reg.SP--;
        emulator.Write(Reg.SP, data.High());
        Reg.SP--;
        emulator.Write(Reg.SP, data.Low());
    }

    public ushort Pop()
    {
        byte low = emulator.Read(Reg.SP++);
        byte high = emulator.Read(Reg.SP++);

        return low.Combine(high);
    }

    private byte NextByte()
    {
        return emulator.Read(Reg.PC++);
    }

    private ushort NextShort()
    {
        byte low = NextByte();
        byte high = NextByte();
        return low.Combine(high);
    }

    #region 8-Bit ALU

    private void ADD(byte n)
    {
        int result = Reg.A + n;

        Reg.SetFlag(Flag.Z, (byte)result == 0);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, ((Reg.A & 0xF) + (n & 0xF)) > 0xF);
        Reg.SetFlag(Flag.C, (result >> 8) != 0);

        Reg.A = (byte)result;
    }

    private void ADC(byte n)
    {
        int carry = Reg.GetFlag(Flag.C) ? 1 : 0;
        int result = Reg.A + n + carry;

        Reg.SetFlag(Flag.Z, (byte)result == 0);
        Reg.SetFlag(Flag.N, false);
        if (Reg.GetFlag(Flag.C))
        {
            Reg.SetFlag(Flag.H, (Reg.A & 0xF) + (n & 0xF) >= 0xF);
        }
        else
        {
            Reg.SetFlag(Flag.H, (Reg.A & 0xF) + (n & 0xF) > 0xF);
        }
        Reg.SetFlag(Flag.C, (result >> 8) != 0);

        Reg.A = (byte)result;
    }

    private void SUB(byte n)
    {
        int result = Reg.A - n;

        Reg.SetFlag(Flag.Z, (byte)result == 0);
        Reg.SetFlag(Flag.N, true);
        Reg.SetFlag(Flag.H, (Reg.A & 0xF) < (n & 0xF));
        Reg.SetFlag(Flag.C, (result >> 8) != 0);

        Reg.A = (byte)result;
    }

    private void SBC(byte n)
    {
        int carry = Reg.GetFlag(Flag.C) ? 1 : 0;
        int result = Reg.A - n - carry;

        Reg.SetFlag(Flag.Z, (byte)result == 0);
        Reg.SetFlag(Flag.N, true);
        Reg.SetFlag(Flag.H, (Reg.A & 0xF) < (n & 0xF) + carry);
        Reg.SetFlag(Flag.C, (result >> 8) != 0);

        Reg.A = (byte)result;
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
        int result = Reg.A - n;

        Reg.SetFlag(Flag.Z, (byte)result == 0);
        Reg.SetFlag(Flag.N, true);
        Reg.SetFlag(Flag.H, (Reg.A & 0xF) < (n & 0xF));
        Reg.SetFlag(Flag.C, (result >> 8) != 0);
    }

    private byte INC(byte n)
    {
        int result = n + 1;

        Reg.SetFlag(Flag.Z, (byte)result == 0);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, ((n & 0xF) + (1 & 0xF)) > 0xF);

        return (byte)result;
    }

    private byte DEC(byte n)
    {
        int result = n - 1;

        Reg.SetFlag(Flag.Z, (byte)result == 0);
        Reg.SetFlag(Flag.N, true);
        Reg.SetFlag(Flag.H, (n & 0xF) < (1 & 0xF));

        return (byte)result;
    }

    #endregion

    #region 16-Bit Arithmetic
    private void ADD(ushort nn)
    {
        int result = Reg.HL + nn;

        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, ((Reg.HL & 0xFFF) + (nn & 0xFFF)) > 0xFFF);
        Reg.SetFlag(Flag.C, (result >> 16) != 0);

        Reg.HL = (ushort)result;
    }

    private ushort ADDS(ushort nn)
    {
        byte n = NextByte();

        Reg.SetFlag(Flag.Z, false);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, (((byte)nn & 0xF) + (n & 0xF)) > 0xF);
        Reg.SetFlag(Flag.C, (((byte)nn + n) >> 8) != 0);

        return (ushort)(nn + (sbyte)n);
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
        byte result = (byte)(((n & 0xF0) >> 4) | ((n & 0x0F) << 4));

        Reg.SetFlag(Flag.Z, result == 0);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, false);

        return result;
    }

    private void DAA()
    {
        if (Reg.GetFlag(Flag.N))
        {
            if (Reg.GetFlag(Flag.C))
            {
                Reg.A -= 0x60;
            }
            if (Reg.GetFlag(Flag.H))
            {
                Reg.A -= 0x6;
            }
        }
        else
        {
            if (Reg.GetFlag(Flag.C) || (Reg.A > 0x99))
            {
                Reg.A += 0x60;
                Reg.SetFlag(Flag.C, true);
            }
            if (Reg.GetFlag(Flag.H) || (Reg.A & 0xF) > 0x9)
            {
                Reg.A += 0x6;
            }
        }

        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.Z, Reg.A == 0);
    }


    #endregion

    #region Rotates & Shifts

    private void RLCA()
    {
        Reg.SetFlag(Flag.Z, false);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (byte)(Reg.A & 0b10000000) != 0);

        Reg.A = (byte)((Reg.A << 1) | (Reg.A >> 7));
    }

    private void RLA()
    {
        int carry = Reg.GetFlag(Flag.C) ? 1 : 0;

        Reg.SetFlag(Flag.Z, false);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (byte)(Reg.A & 0b10000000) != 0);

        Reg.A = (byte)((Reg.A << 1) | carry);
    }

    private void RRCA()
    {
        Reg.SetFlag(Flag.Z, false);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (byte)(Reg.A & 0b00000001) != 0);

        Reg.A = (byte)((Reg.A >> 1) | (Reg.A << 7));
    }

    private void RRA()
    {
        int carry = Reg.GetFlag(Flag.C) ? 0b10000000 : 0;

        Reg.SetFlag(Flag.Z, false);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (byte)(Reg.A & 0b00000001) != 0);

        Reg.A = (byte)((Reg.A >> 1) | carry);
    }

    private byte RLC(byte n)
    {
        byte result = (byte)((n << 1) | (n >> 7));

        Reg.SetFlag(Flag.Z, result == 0);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (n & 0b10000000) != 0);

        return result;
    }

    private byte RL(byte n)
    {
        int carry = Reg.GetFlag(Flag.C) ? 1 : 0;
        byte result = (byte)((n << 1) | carry);

        Reg.SetFlag(Flag.Z, result == 0);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (n & 0b10000000) != 0);

        return result;
    }

    private byte RRC(byte n)
    {
        byte result = (byte)((n >> 1) | (n << 7));

        Reg.SetFlag(Flag.Z, result == 0);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (n & 0b00000001) != 0);

        return result;
    }

    private byte RR(byte n)
    {
        int carry = Reg.GetFlag(Flag.C) ? 0b10000000 : 0;
        byte result = (byte)((n >> 1) | carry);

        Reg.SetFlag(Flag.Z, result == 0);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (n & 0b00000001) != 0);

        return result;
    }

    private byte SLA(byte n)
    {
        byte result = (byte)(n << 1);
        Reg.SetFlag(Flag.Z, result == 0);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (n & 0b10000000) != 0);
        return result;
    }

    private byte SRA(byte n)
    {
        byte result = (byte)((n >> 1) | (n & 0b10000000));

        Reg.SetFlag(Flag.Z, result == 0);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (n & 1) != 0);

        return result;
    }

    private byte SRL(byte n)
    {
        byte result = (byte)(n >> 1);

        Reg.SetFlag(Flag.Z, result == 0);
        Reg.SetFlag(Flag.N, false);
        Reg.SetFlag(Flag.H, false);
        Reg.SetFlag(Flag.C, (n & 1) != 0);

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

    private static byte SET(byte bit, byte r)
    {
        return (byte)(r | bit);
    }

    private static byte RES(byte bit, byte r)
    {
        return (byte)(r & ~bit);
    }

    #endregion

    #region Jumps

    private void JP(ushort nn)
    {
        Reg.PC = nn;
    }

    private void JP(bool condition)
    {
        if (condition)
        {
            Reg.PC = emulator.ReadShort(Reg.PC);
        }
        else
        {
            Reg.PC += 2;
        }
    }

    private void JR(bool condition)
    {
        if (condition)
        {
            JP((ushort)(Reg.PC + (sbyte)emulator.Read(Reg.PC)));
            Reg.PC += 1;
        }
        else
        {
            Reg.PC += 1;
        }
    }

    #endregion

    #region Calls

    public void CALL(bool condition)
    {
        if (condition)
        {
            Push((ushort)(Reg.PC + 2));
            JP(emulator.ReadShort(Reg.PC));
        }
        else
        {
            Reg.PC += 2;
        }
    }

    #endregion

    #region Returns
    private void RET(bool condition)
    {
        if (condition)
        {
            Reg.PC = Pop();
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

    internal void Reset()
    {
        Halted = false;
        HaltBug = false;
    }
}
