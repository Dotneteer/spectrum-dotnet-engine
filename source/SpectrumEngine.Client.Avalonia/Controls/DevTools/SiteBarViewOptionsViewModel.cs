using System;
using Avalonia.Controls;
using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class SiteBarViewOptionsViewModel: ViewModelBase
{
    private bool _showCpu;
    private bool _showUla;
    private bool _showBreakpoints;
    private bool _cpuExpanded;
    private bool _ulaExpanded;
    private bool _breakpointsExpanded;

    public bool ShowCpu
    {
        get => _showCpu;
        set
        {
            SetProperty(ref _showCpu, value);
            RaisePropertyChanged(nameof(CpuHeight));
            if (value) OnPanelGotVisible?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool ShowUla
    {
        get => _showUla;
        set
        {
            SetProperty(ref _showUla, value);
            RaisePropertyChanged(nameof(UlaHeight));
            if (value) OnPanelGotVisible?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool ShowBreakpoints
    {
        get => _showBreakpoints;
        set
        {
            SetProperty(ref _showBreakpoints, value);
            RaisePropertyChanged(nameof(BreakpointsHeight));
            if (value) OnPanelGotVisible?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool CpuExpanded
    {
        get => _cpuExpanded;
        set
        {
            SetProperty(ref _cpuExpanded, value);
            RaisePropertyChanged(nameof(CpuHeight));
        }
    }

    public bool UlaExpanded
    {
        get => _ulaExpanded;
        set
        {
            SetProperty(ref _ulaExpanded, value);
            RaisePropertyChanged(nameof(UlaHeight));
        }
    }

    public bool BreakpointsExpanded
    {
        get => _breakpointsExpanded;
        set
        {
            SetProperty(ref _breakpointsExpanded, value);
            RaisePropertyChanged(nameof(BreakpointsHeight));
        }
    }

    public GridLength CpuHeight => ShowCpu
        ? CpuExpanded ? new GridLength(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Auto)
        : new GridLength(0, GridUnitType.Pixel);

    public GridLength UlaHeight => ShowUla
        ? UlaExpanded ? new GridLength(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Auto)
        : new GridLength(0, GridUnitType.Pixel);

    public GridLength BreakpointsHeight => ShowBreakpoints
        ? BreakpointsExpanded ? new GridLength(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Auto)
        : new GridLength(0, GridUnitType.Pixel);

    public void ToggleShowCpu() => ShowCpu = !ShowCpu;

    public void ToggleShowUla() => ShowUla = !ShowUla;
    
    public void ToggleShowBreakpoints() => ShowBreakpoints = !ShowBreakpoints;

    public event EventHandler? OnPanelGotVisible;
}