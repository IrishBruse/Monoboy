namespace Monoboy.Cartridge;

public class MemoryBankController6 : IMemoryBankController
{
    public byte[] rom;
    public byte[] ram = new byte[32768];

    public byte romBank = 1;
    public byte ramBank;
    public bool ramEnabled;

    public byte ReadBank00(ushort address)
    {
        return rom[address];
    }

    public byte[] GetRam()
    {
        throw new System.NotImplementedException();
    }

    public void Load(string path)
    {
        throw new System.NotImplementedException();
    }

    public byte ReadBankNN(ushort address)
    {
        throw new System.NotImplementedException();
    }

    public byte ReadRam(ushort address)
    {
        throw new System.NotImplementedException();
    }

    public void SetRam(byte[] ram)
    {
        throw new System.NotImplementedException();
    }

    public void WriteBank(ushort address, byte data)
    {
        throw new System.NotImplementedException();
    }

    public void WriteRam(ushort address, byte data)
    {
        throw new System.NotImplementedException();
    }

    enum BankingMode
    {
        Rom,
        Ram,
    }
}
