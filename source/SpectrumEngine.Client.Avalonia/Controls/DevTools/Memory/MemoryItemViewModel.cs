using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Represents a memory line (8 bytes)
/// </summary>
public class MemoryItemViewModel: ViewModelBase
{
    private ushort _address;
    private byte _b0;
    private byte _b0RegFlags;
    private byte _b1;
    private byte _b1RegFlags;
    private byte _b2;
    private byte _b2RegFlags;
    private byte _b3;
    private byte _b3RegFlags;
    private byte _b4;
    private byte _b4RegFlags;
    private byte _b5;
    private byte _b5RegFlags;
    private byte _b6;
    private byte _b6RegFlags;
    private byte _b7;
    private byte _b7RegFlags;

    public ushort Address
    {
        get => _address; 
        set => SetProperty(ref _address, value);
    }

    public byte B0
    {
        get => _b0;
        set => SetProperty(ref _b0, value);
    }

    public byte B0RegFlags
    {
        get => _b0RegFlags;
        set => SetProperty(ref _b0RegFlags, value);
    } 
    
    public byte B1
    {
        get => _b1;
        set => SetProperty(ref _b1, value);
    }

    public byte B1RegFlags
    {
        get => _b1RegFlags;
        set => SetProperty(ref _b1RegFlags, value);
    } 
    
    public byte B2
    {
        get => _b2;
        set => SetProperty(ref _b2, value);
    }

    public byte B2RegFlags
    {
        get => _b2RegFlags;
        set => SetProperty(ref _b2RegFlags, value);
    } 

    public byte B3
    {
        get => _b3;
        set => SetProperty(ref _b3, value);
    }

    public byte B3RegFlags
    {
        get => _b3RegFlags;
        set => SetProperty(ref _b3RegFlags, value);
    } 

    public byte B4
    {
        get => _b4;
        set => SetProperty(ref _b4, value);
    }

    public byte B4RegFlags
    {
        get => _b4RegFlags;
        set => SetProperty(ref _b4RegFlags, value);
    } 

    public byte B5
    {
        get => _b5;
        set => SetProperty(ref _b5, value);
    }

    public byte B5RegFlags
    {
        get => _b5RegFlags;
        set => SetProperty(ref _b5RegFlags, value);
    } 

    public byte B6
    {
        get => _b6;
        set => SetProperty(ref _b6, value);
    }

    public byte B6RegFlags
    {
        get => _b6RegFlags;
        set => SetProperty(ref _b6RegFlags, value);
    } 

    public byte B7
    {
        get => _b7;
        set => SetProperty(ref _b7, value);
    }

    public byte B7RegFlags
    {
        get => _b7RegFlags;
        set => SetProperty(ref _b7RegFlags, value);
    }

    /// <summary>
    /// Refresh this item's contents from the specified mameory representation
    /// </summary>
    /// <param name="memory">Memory representation</param>
    /// <param name="cpu">CPU information</param>
    public void RefreshFrom(byte[] memory, CpuPanelViewModel cpu)
    {
        var addr = Address;
        B0 = memory[addr];
        B0RegFlags = GetRegFlags(addr++);
        B1 = memory[addr];
        B1RegFlags = GetRegFlags(addr++);
        B2 = memory[addr];
        B2RegFlags = GetRegFlags(addr++);
        B3 = memory[addr];
        B3RegFlags = GetRegFlags(addr++);
        B4 = memory[addr];
        B4RegFlags = GetRegFlags(addr++);
        B5 = memory[addr];
        B5RegFlags = GetRegFlags(addr++);
        B6 = memory[addr];
        B6RegFlags = GetRegFlags(addr++);
        B7 = memory[addr];
        B7RegFlags = GetRegFlags(addr);

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
}