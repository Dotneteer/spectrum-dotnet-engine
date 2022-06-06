﻿namespace SpectrumEngine.Client.Avalonia.ViewModels;

public class ViewOptionsViewModel : ViewModelBase
{
    private bool _showToolbar;
    private bool _showStatusBar;
    private bool _showMenu;
    private bool _isMuted;

    public bool ShowToolbar
    {
        get => _showToolbar;
        set => SetProperty(ref _showToolbar, value);
    }

    public bool ShowStatusBar
    {
        get => _showStatusBar;
        set => SetProperty(ref _showStatusBar, value);
    }

    public bool ShowMenu
    {
        get => _showMenu;
        set => SetProperty(ref _showMenu, value);
    }
    
    public bool IsMuted
    {
        get => _isMuted;
        set => SetProperty(ref _isMuted, value);
    }
}