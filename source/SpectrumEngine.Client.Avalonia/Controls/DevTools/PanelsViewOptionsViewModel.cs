using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class PanelsViewOptionsViewModel: ViewModelBase
{
    private bool _showMemory;
    private bool _showDisassembly;
    private bool _showWatch;
    private int _selectedIndex;

    public bool ShowMemory
    {
        get => _showMemory;
        set
        {
            SetProperty(ref _showMemory, value);
            if (value)
            {
                SelectedIndex = 0;
            }
            else
            {
                SelectVisibleTab();
            }
        }
    }

    public bool ShowDisassembly
    {
        get => _showDisassembly;
        set
        {
            SetProperty(ref _showDisassembly, value);
            if (value)
            {
                SelectedIndex = 1;
            }
            else
            {
                SelectVisibleTab();
            }
        }
    }

    public bool ShowWatch
    {
        get => _showWatch;
        set
        {
            SetProperty(ref _showWatch, value);
            if (value)
            {
                SelectedIndex = 2;
            }
            else
            {
                SelectVisibleTab();
            }
        }
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    private void SelectVisibleTab()
    {
        // --- Active tab is visible?
        if (ShowMemory && SelectedIndex == 0) return;
        if (ShowDisassembly && SelectedIndex == 1) return;
        if (ShowWatch && SelectedIndex == 2) return;
        
        // --- Select the leftmost active tap
        if (ShowMemory)
        {
            SelectedIndex = 0;
        }
        else if (ShowDisassembly)
        {
            SelectedIndex = 1;
        }
        else if (ShowWatch)
        {
            SelectedIndex = 2;
        }
    }
    
    public void ToggleShowMemory() => ShowMemory = !ShowMemory;
    
    public void ToggleShowDisassembly() => ShowDisassembly = !ShowDisassembly;
    
    public void ToggleShowWatch() => ShowWatch = !ShowWatch;
}