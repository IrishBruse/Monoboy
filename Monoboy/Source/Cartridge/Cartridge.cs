using System;
using System.IO;

namespace Monoboy
{
    public class Cartridge
    {
        public string Title
        {
            get
            {
                string value = "";

                for(int i = 0x134; i < 0x144; i++)
                {
                    byte val = 0;
                    if(memoryBankController != null)
                    {
                        val = memoryBankController.Read((ushort)i);
                    }

                    if(val == 0)
                    {
                        break;
                    }
                    value += (char)val;
                }

                return value;
            }
        }

        IMemoryBankController memoryBankController;

        public Cartridge()
        {
        }

        public byte Read(ushort address)
        {
            if(memoryBankController != null)
            {
                return memoryBankController.Read(address);
            }

            return 0x00;
        }

        public void Write(ushort address, byte data)
        {
            memoryBankController.Write(address, data);
        }

        public void LoadRom(string path)
        {
            byte cartridgeType;
            using(BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                reader.BaseStream.Seek(0x147, SeekOrigin.Begin);
                cartridgeType = reader.ReadByte();
            }

            if(cartridgeType <= 0x0)
            {
                memoryBankController = new MemoryBankController0();
            }
            else if(cartridgeType <= 0x3)
            {
                memoryBankController = new MemoryBankController1();
            }

            memoryBankController = cartridgeType switch
            {
                0x0 => new MemoryBankController0(),
                0x1 => new MemoryBankController1(),
                0x2 => new MemoryBankController1(),
                0x3 => new MemoryBankController1(),
                _ => throw new NotImplementedException()
            };

            memoryBankController.Load(path);
        }
    }
}
