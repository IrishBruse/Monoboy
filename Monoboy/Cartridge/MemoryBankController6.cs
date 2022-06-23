namespace Monoboy.Cartridge;

public class MemoryBankController6 : IMemoryBankController
{
    public byte[] Rom { get; set; }

    private byte romBank = 1;
    private byte ramBank;
    private bool ramEnabled;

    public byte ReadBank00(ushort address)
    {
        return Rom[address];
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

    private enum BankingMode
    {
        Rom,
        Ram,
    }
}
