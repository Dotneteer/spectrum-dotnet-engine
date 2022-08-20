using SpectrumEngine.Client.Avalonia.ViewModels;
// ReSharper disable MemberCanBePrivate.Global

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Represents a memory line (8 bytes)
/// </summary>
public class MemoryItemViewModel: ViewModelBase
{
    public ushort Address { get; set; }
    public ushort Address2 { get; set; }
    public string? B0 { get; set; }
    public ushort B0RegFlags { get; set; }
    public string? B1 { get; set; }
    public ushort B1RegFlags { get; set; }
    public string? B2 { get; set; }
    public ushort B2RegFlags { get; set; }
    public string? B3 { get; set; }
    public ushort B3RegFlags { get; set; }
    public string? B4 { get; set; }
    public ushort B4RegFlags { get; set; }
    public string? B5 { get; set; }
    public ushort B5RegFlags { get; set; }
    public string? B6 { get; set; }
    public ushort B6RegFlags { get; set; }
    public string? B7 { get; set; }
    public ushort B7RegFlags { get; set; }
    public string? B8 { get; set; }
    public ushort B8RegFlags { get; set; }
    public string? B9 { get; set; }
    public ushort B9RegFlags { get; set; }
    public string? Ba { get; set; }
    public ushort BaRegFlags { get; set; }
    public string? Bb { get; set; }
    public ushort BbRegFlags { get; set; }
    public string? Bc { get; set; }
    public ushort BcRegFlags { get; set; }
    public string? Bd { get; set; }
    public ushort BdRegFlags { get; set; }
    public string? Be { get; set; }
    public ushort BeRegFlags { get; set; }
    public string? Bf { get; set; }
    public ushort BfRegFlags { get; set; }
    public string? CharLine { get; set; }
    public string? CharLine2 { get; set; }

    /// <summary>
    /// Refresh this item's contents from the specified mameory representation
    /// </summary>
    /// <param name="memory">Memory representation</param>
    /// <param name="cpu">CPU information</param>
    public void RefreshFrom(byte[] memory, CpuPanelViewModel cpu)
    {
        var addr = Address;
        CharLine = string.Empty;
        CharLine2 = string.Empty;
        B0 = memory[addr].ToString("X2");
        CharLine += ConverChar(memory[addr]);
        B0RegFlags = GetRegFlags(addr++);
        B1 = memory[addr].ToString("X2");
        CharLine += ConverChar(memory[addr]);
        B1RegFlags = GetRegFlags(addr++);
        B2 = memory[addr].ToString("X2");
        CharLine += ConverChar(memory[addr]);
        B2RegFlags = GetRegFlags(addr++);
        B3 = memory[addr].ToString("X2");
        CharLine += ConverChar(memory[addr]);
        B3RegFlags = GetRegFlags(addr++);
        B4 = memory[addr].ToString("X2");
        CharLine += ConverChar(memory[addr]);
        B4RegFlags = GetRegFlags(addr++);
        B5 = memory[addr].ToString("X2");
        CharLine += ConverChar(memory[addr]);
        B5RegFlags = GetRegFlags(addr++);
        B6 = memory[addr].ToString("X2");
        CharLine += ConverChar(memory[addr]);
        B6RegFlags = GetRegFlags(addr++);
        B7 = memory[addr].ToString("X2");
        CharLine += ConverChar(memory[addr]);
        B7RegFlags = GetRegFlags(addr++);
        Address2 = addr;
        B8 = memory[addr].ToString("X2");
        CharLine2 += ConverChar(memory[addr]);
        B8RegFlags = GetRegFlags(addr++);
        B9 = memory[addr].ToString("X2");
        CharLine2 += ConverChar(memory[addr]);
        B9RegFlags = GetRegFlags(addr++);
        Ba = memory[addr].ToString("X2");
        CharLine2 += ConverChar(memory[addr]);
        BaRegFlags = GetRegFlags(addr++);
        Bb = memory[addr].ToString("X2");
        CharLine2 += ConverChar(memory[addr]);
        BbRegFlags = GetRegFlags(addr++);
        Bc = memory[addr].ToString("X2");
        CharLine2 += ConverChar(memory[addr]);
        BcRegFlags = GetRegFlags(addr++);
        Bd = memory[addr].ToString("X2");
        CharLine2 += ConverChar(memory[addr]);
        BdRegFlags = GetRegFlags(addr++);
        Be = memory[addr].ToString("X2");
        CharLine2 += ConverChar(memory[addr]);
        BeRegFlags = GetRegFlags(addr++);
        Bf = memory[addr].ToString("X2");
        CharLine2 += ConverChar(memory[addr]);
        BfRegFlags = GetRegFlags(addr);

        ushort GetRegFlags(ushort address)
        {
            var flags = memory[address] << 8;
            if (address == cpu.BC) flags |= 0x01;
            if (address == cpu.DE) flags |= 0x02;
            if (address == cpu.HL) flags |= 0x04;
            if (address == cpu.SP) flags |= 0x08;
            if (address == cpu.IX) flags |= 0x10;
            if (address == cpu.IY) flags |= 0x20;
            if (address == cpu.PC) flags |= 0x40;
            return (ushort) flags;
        }

        string ConverChar(byte value) => value is < 0x20 or > 0x7f ? "." : ((char) value).ToString();
    }
}