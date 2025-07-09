namespace Monoboy;

// https://gbdev.io/pandocs/The_Cartridge_Header.html
public class CartridgeHeader
{
    /// <summary>
    /// Entry Instruction defaults to `NOP JP $0150` <br/>
    /// 0x100-0x103 <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#0100-0103--entry-point
    /// </summary>
    public byte[] EntryPoint { get; set; }

    /// <summary>
    /// Nintendo scrolling logo <br/>
    /// 0x104-0x133 <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#0104-0133--nintendo-logo
    /// </summary>
    public byte[] NintendoLogo { get; set; }

    /// <summary>
    /// Title <br/>
    /// 0x134-0x13E <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#0134-0143--title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Manufacturer code <br/>
    /// 0x13F-0x142 <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#013f-0142--manufacturer-code
    /// </summary>
    public string ManufacturerCode { get; set; }

    /// <summary>
    /// CGB flag <br/>
    /// 0x143 <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#0143--cgb-flag
    /// </summary>
    public byte CGB { get; set; }

    /// <summary>
    /// New licensee code <br/>
    /// 0x144–0x145 <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#01440145--new-licensee-code
    /// </summary>
    public string LicenseCode { get; set; }

    /// <summary>
    /// SGB flag <br/>
    /// 0x146 <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#0146--sgb-flag
    /// </summary>
    public byte SGB { get; set; }

    /// <summary>
    /// Cartridge type <br/>
    /// 0x147 <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#0147--cartridge-type
    /// </summary>
    public byte CartridgeType { get; set; }

    /// <summary>
    /// ROM size <br/>
    /// 0x148 <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#0148--rom-size
    /// </summary>
    public byte RomSize { get; set; }

    /// <summary>
    /// RAM size <br/>
    /// 0x149 <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#0149--ram-size
    /// </summary>
    public byte RamSize { get; set; }

    /// <summary>
    /// Destination code <br/>
    /// 0x14A <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#014a--destination-code
    /// </summary>
    public byte DestinationCode { get; set; }

    /// <summary>
    /// Old licensee code <br/>
    /// 0x14B <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#014b--old-licensee-code
    /// </summary>
    public byte OldLicenseeCode { get; set; }

    /// <summary>
    /// Mask ROM version number <br/>
    /// 0x14C <br/>
    /// https://gbdev.io/pandocs/The_Cartridge_Header.html#014c--mask-rom-version-number
    /// </summary>
    public byte Version { get; set; }

    public override string ToString()
    {
        // EntryPoint: {EntryPoint}
        // NintendoLogo: {NintendoLogo}
        return $"""
        Title: {Title}
        ManufacturerCode: {ManufacturerCode}
        CGB: {CGB}
        LicenseCode: {GetLicenseeName(LicenseCode)}
        SGB: {SGB}
        CartridgeType: {GetNameMBC(CartridgeType)}
        RomSize: {RomSize}
        RamSize: {RamSize}
        DestinationCode: {DestinationCode}
        OldLicenseeCode: {OldLicenseeCode}
        Version: {Version}
        """;
    }

    static string GetNameMBC(byte type) => type switch
    {
        0x00 => "ROM ONLY",
        0x01 => "MBC1",
        0x02 => "MBC1+RAM",
        0x03 => "MBC1+RAM+BATTERY",
        0x05 => "MBC2",
        0x06 => "MBC2+BATTERY",
        0x08 => "ROM+RAM",
        0x09 => "ROM+RAM+BATTERY",
        0x0B => "MMM01",
        0x0C => "MMM01+RAM",
        0x0D => "MMM01+RAM+BATTERY",
        0x0F => "MBC3+TIMER+BATTERY",
        0x10 => "MBC3+TIMER+RAM+BATTERY 2",
        0x11 => "MBC3",
        0x12 => "MBC3+RAM 2",
        0x13 => "MBC3+RAM+BATTERY 2",
        0x19 => "MBC5",
        0x1A => "MBC5+RAMkl,",
        0x1B => "MBC5+RAM+BATTERY",
        0x1C => "MBC5+RUMBLE",
        0x1D => "MBC5+RUMBLE+RAM",
        0x1E => "MBC5+RUMBLE+RAM+BATTERY",
        0x20 => "MBC6",
        0x22 => "MBC7+SENSOR+RUMBLE+RAM+BATTERY",
        0xFC => "POCKET CAMERA",
        0xFD => "BANDAI TAMA5",
        0xFE => "HuC3",
        0xFF => "HuC1+RAM+BATTERY",
        _ => "Unknown",
    };

    static string GetLicenseeName(string code) => code switch
    {
        "00" => "None",
        "01" => "Nintendo Research & Development",
        "08" => "Capcom",
        "13" => "EA (Electronic Arts)",
        "18" => "Hudson Soft",
        "19" => "B-AI",
        "20" => "KSS",
        "22" => "Planning Office WADA",
        "24" => "PCM Complete",
        "25" => "San-X",
        "28" => "Kemco",
        "29" => "SETA Corporation",
        "30" => "Viacom",
        "31" => "Nintendo",
        "32" => "Bandai",
        "33" => "Ocean Software / Acclaim Entertainment",
        "34" => "Konami",
        "35" => "HectorSoft",
        "37" => "Taito",
        "38" => "Hudson Soft",
        "39" => "Banpresto",
        "41" => "Ubi Soft",
        "42" => "Atlus",
        "44" => "Malibu Interactive",
        "46" => "Angel",
        "47" => "Bullet-Proof Software",
        "49" => "Irem",
        "50" => "Absolute",
        "51" => "Acclaim Entertainment",
        "52" => "Activision",
        "53" => "Sammy USA Corporation",
        "54" => "Konami",
        "55" => "Hi Tech Expressions",
        "56" => "LJN",
        "57" => "Matchbox",
        "58" => "Mattel",
        "59" => "Milton Bradley Company",
        "60" => "Titus Interactive",
        "61" => "Virgin Games Ltd.",
        "64" => "Lucasfilm Games",
        "67" => "Ocean Software",
        "69" => "EA (Electronic Arts)",
        "70" => "Infogrames 5",
        "71" => "Interplay Entertainment",
        "72" => "Broderbund",
        "73" => "Sculptured Software",
        "75" => "The Sales Curve Limited",
        "78" => "THQ",
        "79" => "Accolade",
        "80" => "Misawa Entertainment",
        "83" => "lozc",
        "86" => "Tokuma Shoten",
        "87" => "Tsukuda Original",
        "91" => "Chunsoft Co.",
        "92" => "Video System",
        "93" => "Ocean Software / Acclaim Entertainment",
        "95" => "Varie",
        "96" => "Yonezawa/s’pal",
        "97" => "Kaneko",
        "99" => "Pack-In-Video",
        "9H" => "Bottom Up",
        "A4" => "Konami (Yu-Gi-Oh!)",
        "BL" => "MTO",
        "DK" => "Kodansha",

        _ => "Unknown",
    };
}
