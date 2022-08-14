using SpectrumEngine.Client.Avalonia.ViewModels;
// ReSharper disable MemberCanBePrivate.Global

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Represents a memory line (8 bytes)
/// </summary>
public class MemoryItemViewModel: ViewModelBase
{
    private ushort _address;
    private ushort _address2;
    private string? _b0;
    private byte _b0RegFlags;
    private string? _b1;
    private byte _b1RegFlags;
    private string? _b2;
    private byte _b2RegFlags;
    private string? _b3;
    private byte _b3RegFlags;
    private string? _b4;
    private byte _b4RegFlags;
    private string? _b5;
    private byte _b5RegFlags;
    private string? _b6;
    private byte _b6RegFlags;
    private string? _b7;
    private byte _b7RegFlags;
    private string? _b8;
    private byte _b8RegFlags;
    private string? _b9;
    private byte _b9RegFlags;
    private string? _ba;
    private byte _baRegFlags;
    private string? _bb;
    private byte _bbRegFlags;
    private string? _bc;
    private byte _bcRegFlags;
    private string? _bd;
    private byte _bdRegFlags;
    private string? _be;
    private byte _beRegFlags;
    private string? _bf;
    private byte _bfRegFlags;

    public ushort Address
    {
        get => _address;
        set
        {
            SetProperty(ref _address, value);
            Address2 = (ushort) (value + 8);
        }
    }

    public ushort Address2
    {
        get => _address2; 
        set => SetProperty(ref _address2, value);
    }

    public string? B0
    {
        get => _b0;
        set => SetProperty(ref _b0, value);
    }

    public byte B0RegFlags
    {
        get => _b0RegFlags;
        set => SetProperty(ref _b0RegFlags, value);
    } 
    
    public string? B1
    {
        get => _b1;
        set => SetProperty(ref _b1, value);
    }

    public byte B1RegFlags
    {
        get => _b1RegFlags;
        set => SetProperty(ref _b1RegFlags, value);
    } 
    
    public string? B2
    {
        get => _b2;
        set => SetProperty(ref _b2, value);
    }

    public byte B2RegFlags
    {
        get => _b2RegFlags;
        set => SetProperty(ref _b2RegFlags, value);
    } 

    public string? B3
    {
        get => _b3;
        set => SetProperty(ref _b3, value);
    }

    public byte B3RegFlags
    {
        get => _b3RegFlags;
        set => SetProperty(ref _b3RegFlags, value);
    } 

    public string? B4
    {
        get => _b4;
        set => SetProperty(ref _b4, value);
    }

    public byte B4RegFlags
    {
        get => _b4RegFlags;
        set => SetProperty(ref _b4RegFlags, value);
    } 

    public string? B5
    {
        get => _b5;
        set => SetProperty(ref _b5, value);
    }

    public byte B5RegFlags
    {
        get => _b5RegFlags;
        set => SetProperty(ref _b5RegFlags, value);
    } 

    public string? B6
    {
        get => _b6;
        set => SetProperty(ref _b6, value);
    }

    public byte B6RegFlags
    {
        get => _b6RegFlags;
        set => SetProperty(ref _b6RegFlags, value);
    } 

    public string? B7
    {
        get => _b7;
        set => SetProperty(ref _b7, value);
    }

    public byte B7RegFlags
    {
        get => _b7RegFlags;
        set => SetProperty(ref _b7RegFlags, value);
    }

    public string? B8
    {
        get => _b8;
        set => SetProperty(ref _b8, value);
    }

    public byte B8RegFlags
    {
        get => _b8RegFlags;
        set => SetProperty(ref _b8RegFlags, value);
    }

    public string? B9
    {
        get => _b9;
        set => SetProperty(ref _b9, value);
    }

    public byte B9RegFlags
    {
        get => _b9RegFlags;
        set => SetProperty(ref _b9RegFlags, value);
    }

    public string? Ba
    {
        get => _ba;
        set => SetProperty(ref _ba, value);
    }

    public byte BaRegFlags
    {
        get => _baRegFlags;
        set => SetProperty(ref _baRegFlags, value);
    }

    public string? Bb
    {
        get => _bb;
        set => SetProperty(ref _bb, value);
    }

    public byte BbRegFlags
    {
        get => _bbRegFlags;
        set => SetProperty(ref _bbRegFlags, value);
    }

    public string? Bc
    {
        get => _bc;
        set => SetProperty(ref _bc, value);
    }

    public byte BcRegFlags
    {
        get => _bcRegFlags;
        set => SetProperty(ref _bcRegFlags, value);
    }

    public string? Bd
    {
        get => _bd;
        set => SetProperty(ref _bd, value);
    }

    public byte BdRegFlags
    {
        get => _bdRegFlags;
        set => SetProperty(ref _bdRegFlags, value);
    }

    public string? Be
    {
        get => _be;
        set => SetProperty(ref _be, value);
    }

    public byte BeRegFlags
    {
        get => _beRegFlags;
        set => SetProperty(ref _beRegFlags, value);
    }

    public string? Bf
    {
        get => _bf;
        set => SetProperty(ref _bf, value);
    }

    public byte BfRegFlags
    {
        get => _bfRegFlags;
        set => SetProperty(ref _bfRegFlags, value);
    }

    /// <summary>
    /// Refresh this item's contents from the specified mameory representation
    /// </summary>
    /// <param name="memory">Memory representation</param>
    /// <param name="cpu">CPU information</param>
    public void RefreshFrom(byte[] memory, CpuPanelViewModel cpu)
    {
        var addr = Address;
        B0 = memory[addr].ToString("X2");
        B0RegFlags = GetRegFlags(addr++);
        B1 = memory[addr].ToString("X2");
        B1RegFlags = GetRegFlags(addr++);
        B2 = memory[addr].ToString("X2");
        B2RegFlags = GetRegFlags(addr++);
        B3 = memory[addr].ToString("X2");
        B3RegFlags = GetRegFlags(addr++);
        B4 = memory[addr].ToString("X2");
        B4RegFlags = GetRegFlags(addr++);
        B5 = memory[addr].ToString("X2");
        B5RegFlags = GetRegFlags(addr++);
        B6 = memory[addr].ToString("X2");
        B6RegFlags = GetRegFlags(addr++);
        B7 = memory[addr].ToString("X2");
        B7RegFlags = GetRegFlags(addr++);
        B8 = memory[addr].ToString("X2");
        B8RegFlags = GetRegFlags(addr++);
        B9 = memory[addr].ToString("X2");
        B9RegFlags = GetRegFlags(addr++);
        Ba = memory[addr].ToString("X2");
        BaRegFlags = GetRegFlags(addr++);
        Bb = memory[addr].ToString("X2");
        BbRegFlags = GetRegFlags(addr++);
        Bc = memory[addr].ToString("X2");
        BcRegFlags = GetRegFlags(addr++);
        Bd = memory[addr].ToString("X2");
        BdRegFlags = GetRegFlags(addr++);
        Be = memory[addr].ToString("X2");
        BeRegFlags = GetRegFlags(addr++);
        Bf = memory[addr].ToString("X2");
        BfRegFlags = GetRegFlags(addr);

        byte GetRegFlags(ushort address)
        {
            var flags = 0;
            if (address == cpu.BC) flags |= 0x01;
            if (address == cpu.DE) flags |= 0x02;
            if (address == cpu.HL) flags |= 0x04;
            if (address == cpu.SP) flags |= 0x08;
            if (address == cpu.IX) flags |= 0x10;
            if (address == cpu.IY) flags |= 0x20;
            if (address == cpu.PC) flags |= 0x40;
            return (byte) flags;
        }
    }

    public void CopyInto(MemoryItemViewModel other)
    {
        other._address = _address;
        other._address2 = _address2;
        other._b0 = _b0;
        other._b0RegFlags = _b0RegFlags;
        other._b1 = _b1;
        other._b1RegFlags = _b1RegFlags;
        other._b2 = _b2;
        other._b2RegFlags = _b2RegFlags;
        other._b3 = _b3;
        other._b3RegFlags = _b3RegFlags;
        other._b4 = _b4;
        other._b4RegFlags = _b4RegFlags;
        other._b5 = _b5;
        other._b5RegFlags = _b5RegFlags;
        other._b6 = _b6;
        other._b6RegFlags = _b6RegFlags;
        other._b7 = _b7;
        other._b7RegFlags = _b7RegFlags;
        other._b8 = _b8;
        other._b8RegFlags = _b8RegFlags;
        other._b9 = _b9;
        other._b9RegFlags = _b9RegFlags;
        other._ba = _ba;
        other._baRegFlags = _baRegFlags;
        other._bb = _bb;
        other._bbRegFlags = _bbRegFlags;
        other._bc = _bc;
        other._bcRegFlags = _bcRegFlags;
        other._bd = _bd;
        other._bdRegFlags = _bdRegFlags;
        other._be = _be;
        other._beRegFlags = _beRegFlags;
        other._bf = _bf;
        other._bfRegFlags = _bfRegFlags;
    }
}