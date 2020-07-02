using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monoboy.Utility;

namespace Monoboy.Emulator
{
    public class Memory
    {
        public bool booted = false;

        public string testOutput;

        byte[] boot;            //  0x0000-0x00FF

        byte[] cartBank0;       //  0x0000-0x3FFF
        byte[] cartBankN;       //  0x4000-0x7FFF
        public byte[] videoRAM; //  0x8000-0x9FFF
        byte[] cartRAM;         //  0xA000-0xBFFF
        byte[] workRAM;         //  0xC000-0xFDFF
        byte[] spriteRAM;       //  0xFE00-0xFE9F
                                //  0xFEA0-0xFEFF
        byte[] io;              //  0xFF00-0xFF7F
        byte[] zp;              //  0xFF80-0xFFFF

        byte[] all
        {
            get
            {
                List<byte> bytes = new List<byte>(65536);
                bytes.AddRange(new byte[8192 * 2]);
                bytes.AddRange(new byte[8192 * 2]);
                bytes.AddRange(videoRAM);
                bytes.AddRange(cartRAM);
                bytes.AddRange(workRAM);// 57344
                bytes.AddRange(new byte[7680]);
                bytes.AddRange(spriteRAM);//160
                bytes.AddRange(new byte[96]);//96
                bytes.AddRange(io);
                bytes.AddRange(zp);
                return bytes.ToArray();
            }
        }

        public byte GpuControl { get => io[0x0040]; set => io[0x0040] = value; }
        public byte Stat { get => io[0x0041]; set => io[0x0041] = value; }
        public byte ScrollY { get => io[0x0042]; set => io[0x0042] = value; }
        public byte ScrollX { get => io[0x0043]; set => io[0x0043] = value; }
        public byte Scanline { get => io[0x0044]; set => io[0x0044] = value; }
        public byte BackgroundPallet { get => io[0x0047]; set => io[0x0047] = value; }


        public string location;

        public Memory()
        {
            cartBank0 = new byte[16384];
            cartBankN = new byte[16384];
            videoRAM = new byte[8192];
            cartRAM = new byte[8192];
            workRAM = new byte[8192];
            spriteRAM = new byte[160];
            io = new byte[128];
            zp = new byte[128];

            if(File.Exists("Roms/Boot.gb") == true)
            {
                boot = File.ReadAllBytes("Roms/Boot.gb");
            }
            else
            {
                // TODO: run skip boot
            }
        }

        public void LoadRom(string path)
        {
            using(BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                for(int i = 0; i < 16384; i++)
                {
                    cartBank0[i] = reader.ReadByte();
                }

                for(int i = 0; i < 16384; i++)
                {
                    cartBankN[i] = reader.ReadByte();
                }
            }
        }

        public void Dump()
        {
            List<string> lines = new List<string>();

            byte[] cache = all;

            for(int i = 0; i < 4096; i++)//line
            {
                string offset = (i * 16).ToString("X6") + ":";
                string hexs = " ";
                string texts = " ";
                for(int hex = 0; hex < 16; hex++)
                {
                    byte val = cache[(i * 16) + hex];

                    hexs += val.ToString("X2") + " ";
                    if(val >= 32 && val <= 126)
                    {
                        texts += (char)val;
                    }
                    else
                    {
                        texts += "_";
                    }
                }

                lines.Add(offset + hexs + texts);
            }

            string dump = "Dump.bin";

            if(File.Exists(dump) == true)
                File.Delete(dump);

            File.WriteAllLines(dump, lines.ToArray());
        }

        public void DumpAsImage(GraphicsDevice graphics)
        {
            System.Diagnostics.Debug.WriteLine(all.Length);

            Thread thread = new Thread(() =>
            {
                using(Texture2D dump = new Texture2D(graphics, 256, 98))
                {
                    byte[] cache = all;

                    Color[] pixels = new Color[25088];
                    for(int i = 32768; i < 57856; i++)
                    {
                        if(i < 32768 + 8192 + 8192 + 8192)
                        {
                            if(cache[i] == 0)
                            {
                                if(i < 32768 + 8192)// Vram
                                {
                                    pixels[i - 32768] = new Color(128, 255, 128);
                                }
                                else if(i < 32768 + 8192 + 8192)// Cart
                                {
                                    pixels[i - 32768] = new Color(255, 128, 128);
                                }
                                else if(i < 32768 + 8192 + 8192 + 8192)// Work
                                {
                                    pixels[i - 32768] = new Color(255, 255, 128);
                                }
                            }
                            else
                            {
                                pixels[i - 32768] = new Color(cache[i], cache[i], cache[i]);
                            }
                        }
                        else
                        {
                            if(cache[i + 7680] == 0)
                            {
                                pixels[i - 32768] = new Color(128, 128, 255);
                            }
                            else
                            {
                                pixels[i - 32768] = new Color(cache[i + 7680], cache[i + 7680], cache[i + 7680]);
                            }
                        }
                    }

                    dump.SetData(0, new Rectangle(0, 0, 256, 98), pixels, 0, 25088);

                    using(Stream s = File.Create("Dump.png"))
                    {
                        dump.SaveAsPng(s, dump.Width, dump.Height);
                    }
                }
            });

            thread.Start();
        }

        public byte Read(ushort address)
        {
            byte data;
            if(address >= 0x0000 && address <= 0x3FFF)// 16KB ROM Bank 00     (in cartridge, fixed at bank 00)
            {

                if((address >= 0x0000 && address <= 0x00FF) && booted == false)
                {
                    data = boot[address];
                    location = "boot";
                }
                else
                {
                    data = cartBank0[address];
                    location = "cartBank0";
                }
            }
            else if(address >= 0x4000 && address <= 0x7FFF)// 16KB ROM Bank 01..NN (in cartridge, switchable bank number)
            {
                data = cartBankN[address - 0x4000];
                location = "cartBankN";
            }
            else if(address >= 0x8000 && address <= 0x9FFF)// 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            {
                data = videoRAM[address - 0x8000];
                location = "videoRAM";
            }
            else if(address >= 0xA000 && address <= 0xBFFF)// 8KB External RAM     (in cartridge, switchable bank, if any)
            {
                data = cartRAM[address - 0xA000];
                location = "cartRAM";
            }
            else if(address >= 0xC000 && address <= 0xDFFF)// 4KB Work RAM Bank 0 (WRAM)
            {
                data = workRAM[address - 0xC000];
                location = "workRAM";
            }
            else if(address >= 0xD000 && address <= 0xDFFF)// 4KB Work RAM Bank 1 (WRAM)  (switchable bank 1-7 in CGB Mode)
            {
                data = workRAM[address - 0xC000];
                location = "workRAM";
            }
            else if(address >= 0xE000 && address <= 0xFDFF)// Same as C000-DDFF (ECHO)    (typically not used)
            {
                data = workRAM[address - 0xE000];
                location = "workRAM";
            }
            else if(address >= 0xFE00 && address <= 0xFE9F)// Sprite Attribute Table (OAM)
            {
                data = spriteRAM[address - 0xFE00];
                location = "spriteRAM";
            }
            else if(address >= 0xFEA0 && address <= 0xFEFF)// Not Usable
            {
                data = 0;
                location = "Not Usable";
            }
            else if(address >= 0xFF00 && address <= 0xFF7F)// I/O Ports
            {
                data = io[address - 0xFF00];
                location = "io";
            }
            else if(address >= 0xFF80 && address <= 0xFFFF)// Zero Page RAM
            {
                data = zp[address - 0xFF80];
                location = "zp";
            }
            else
            {
                data = 0;
                location = "Error";
            }

            //Debug.Log("R " + address.ToHex() + "=" + data.ToHex() + " " + location);

            return data;
        }

        public void Write(ushort address, byte data)
        {
            // cant write to rom skipped
            if(address >= 0x8000 && address <= 0x9FFF)// 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
            {
                videoRAM[address - 0x8000] = data;
                location = "videoRAM";
            }
            else if(address >= 0xA000 && address <= 0xBFFF)// 8KB External RAM (in cartridge, switchable bank, if any)
            {
                cartRAM[address - 0xA000] = data;
                location = "cartRAM";
            }
            else if(address >= 0xC000 && address <= 0xCFFF)// 4KB Work RAM Bank 0 (WRAM)
            {
                workRAM[address - 0xC000] = data;
                location = "workRAM";
            }
            else if(address >= 0xD000 && address <= 0xDFFF)// 4KB Work RAM Bank 1 (WRAM) (switchable bank 1-7 in CGB Mode)
            {
                workRAM[address - 0xC000] = data;
                location = "workRAM";
            }
            else if(address >= 0xE000 && address <= 0xFDFF)// Same as C000-DDFF (ECHO) (typically not used)
            {
                workRAM[address - 0xC000 - 0x0200] = data;
                location = "workRAM";
            }
            else if(address >= 0xFE00 && address <= 0xFE9F)// Sprite Attribute Table (OAM)
            {
                spriteRAM[address - 0xFE00] = data;
                location = "spriteRAM";

            }
            else if(address >= 0xFEA0 && address <= 0xFEFF)// Not Usable
            {
                Debug.Log("Not Usable");
            }
            else if(address >= 0xFF00 && address <= 0xFF7F)// I/O Ports
            {
                if(address == 0xFF02 && data == 0x81)
                {
                    testOutput += (char)Read(0xFF01);
                }
                io[address - 0xFF00] = data;
                location = "io";

            }
            else if(address >= 0xFF80 && address <= 0xFFFF)// Zero Page RAM
            {
                zp[address - 0xFF80] = data;
                location = "zp";
            }

            //Debug.Log("W " + address.ToHex() + "=" + data.ToHex() + " " + location);
        }

        public void WriteBit(ushort address, Bit bit)
        {
            Write(address, (byte)(Read(address) | (byte)bit));
        }
    }
}