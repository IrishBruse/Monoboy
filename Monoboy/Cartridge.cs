using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monoboy.Core
{
    public class Cartridge
    {
        public byte[] cartBank0;        //  0x0000-0x3FFF
        public byte[] cartBankN;        //  0x4000-0x7FFF
        public byte[] cartRAM;          //  0xA000-0xBFFF

        public string Title
        {
            get
            {
                char[] title = new char[16];
                Array.Copy(cartBank0, 0x0134, title, 0, 16);
                title = title.Where(x => x != '\0').ToArray();
                return new string(title);
            }
        }

        public Cartridge()
        {
            cartBank0 = new byte[16384];
            cartBankN = new byte[16384];
            cartRAM = new byte[8192];
        }
    }
}
