using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Monoboy.Core.Utility;

namespace Monoboy.Core
{
    public class Cartridge
    {
        public byte[] cartRom;        //  0x0000-0x7FFF
        public byte[] cartRam;          //  0xA000-0xBFFF

        string title = "";
        public string Title
        {
            get
            {
                if(title == "")
                {
                    char[] title = new char[16];
                    Array.Copy(cartRom, 0x0134, title, 0, 16);
                    title = title.Where(x => x != '\0').ToArray();
                    this.title = new string(title);
                }

                return title;
            }
        }

        public Cartridge()
        {
            cartRam = new byte[8192];
        }

        public byte Read(ushort address)
        {
            return cartRom[address];
        }

        public void Write(ushort address, byte data)
        {
            cartRom[address] = data;
        }

        public void LoadRom(string path)
        {
            cartRom = File.ReadAllBytes(path);

            byte mbc = cartRom[0x0147];

            switch(mbc)
            {
                case 0x00:
                RomOnly(path);
                break;

                case 0x01:
                MBC1(path);
                break;

                default:
                Debug.WriteLine("Unknown mbc: " + mbc.ToHex());
                break;
            }
        }

        private void MBC1(string path)
        {

        }

        private void RomOnly(string path)
        {

        }
    }
}
