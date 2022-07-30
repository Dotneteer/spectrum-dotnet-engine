using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Tools.Commands;
using SpectrumEngine.Tools.Output;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// This control implements the interactive command panel
/// </summary>
public partial class CommandsPanel : UserControl
{
    // --- Stores the command output panel
    private readonly OutputBuffer _buffer = new();

    // --- Stores the command history
    private readonly List<string> _commandHistory = new();
    
    // --- Stores the last command index
    private int _lastCommandIndex;
    
    public CommandsPanel()
    {
        InitializeComponent();
        _buffer.ContentsChanged += (_, _) =>
        {
            // --- Refresh the output whenever it changes
            if (Vm != null)
            {
                Vm.Commands.Buffer = _buffer.Clone().Contents;
            }
        };
    }

    /// <summary>
    /// Set the focus to the prompt
    /// </summary>
    public void FocusPrompt()
    {
        Prompt.Focus();
    }
    
    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;
    
    /// <summary>
    /// Respond to special input keys
    /// </summary>
    private async void OnPromptKeyDown(object? _, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                // --- Execute the command
                if (string.IsNullOrWhiteSpace(Prompt.Text))
                {
                    Prompt.Text = "";
                    break;
                }
                e.Handled = true;
                _commandHistory.Add(Prompt.Text);
                var result = await ExecuteComand(Prompt.Text);
                if (!result.Success)
                {
                    _buffer.ResetFormat();
                    _buffer.Color = OutputColors.Red;
                    _buffer.WriteLine(result.FinalMessage ?? "Error");
                    _buffer.ResetFormat();
                }
                await Task.Delay(50);
                Scroller.ScrollToEnd();
                Prompt.Text = "";
                _lastCommandIndex = _commandHistory.Count;
                break;
            
            case Key.Up:
                // --- Navigate to the previous command in history
                e.Handled = true;
                if (_commandHistory.Count == 0)
                {
                    break;
                }
                _lastCommandIndex = _lastCommandIndex <= 0 ? _commandHistory.Count - 1 : _lastCommandIndex - 1;
                Prompt.Text = _commandHistory[_lastCommandIndex];
                Prompt.CaretIndex = Prompt.Text.Length;
                break;
            
            case Key.Down:
                // --- Navigate to the next command in history
                e.Handled = true;
                if (_commandHistory.Count == 0)
                {
                    break;
                }
                _lastCommandIndex = _lastCommandIndex >= _commandHistory.Count - 1 ? 0 : _lastCommandIndex + 1;
                Prompt.Text = _commandHistory[_lastCommandIndex];
                Prompt.CaretIndex = Prompt.Text.Length;
                break;
        }
    }

    private async Task<InteractiveCommandResult> ExecuteComand(string commandText)
    {
        // --- Display the command
        _buffer.ResetFormat();
        _buffer.Color = OutputColors.BrightGreen;
        _buffer.WriteLine($"> {commandText}");
        _buffer.ResetFormat();

        // --- Parse the command
        var tokens = CommandTokenStream.ParseCommand(commandText);
        if (tokens.Count == 0)
        {
            return new InteractiveCommandResult(false, "No command to execute");
        }

        // --- Obtain command from the registry
        var commandId = tokens[0].Text;
        var command = CommandRegistry.GetCommand(commandId);
        if (command == null)
        {
            return new InteractiveCommandResult(false, $"Cannot find command '{commandId}'");
        }
        
        // --- Prepare the execution context
        var context = new InteractiveCommandContext
        {
            CommandText = commandText,
            Output = _buffer
        };
        
        // --- Execute the command
        try
        {
            return await command.Execute(context);
        }
        catch (Exception ex)
        {
            return new InteractiveCommandResult(false, ex.Message);
        }
    }
}