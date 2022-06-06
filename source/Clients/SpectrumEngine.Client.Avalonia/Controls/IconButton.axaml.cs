using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class IconButton : TemplatedControl
{
    private ICommand? _command;
    private bool _commandCanExecute = true;

    /// <summary>
    /// Defines the <see cref="Command"/> property.
    /// </summary>
    public static readonly DirectProperty<IconButton, ICommand?> CommandProperty =
        Button.CommandProperty.AddOwner<IconButton>(
            iconButton => iconButton.Command,
            (iconButton, command) => iconButton.Command = command,
            enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object> CommandParameterProperty =
        Button.CommandParameterProperty.AddOwner<IconButton>();

    public static readonly StyledProperty<Brush> FillProperty = 
        AvaloniaProperty.Register<IconButton, Brush>("Fill", new SolidColorBrush(Colors.White));

    public static readonly StyledProperty<Geometry> PathProperty = 
        AvaloniaProperty.Register<IconButton, Geometry>("Path");

    public static readonly StyledProperty<string> HintProperty =
        AvaloniaProperty.Register<IconButton, string>("Hint");
        
    public IconButton()
    {
        Width = 28;
        Height = 28;
        Padding = new Thickness(4);
        Margin = new Thickness(2);
        CommandProperty.Changed.Subscribe(CommandChanged);
        CommandParameterProperty.Changed.Subscribe(CommandParameterChanged);
        s_ClickEvent.AddClassHandler<IconButton>((x, e) => x.OnClick(e));
    }

    /// <summary>
    /// Defines the <see cref="Click"/> event.
    /// </summary>
    private static readonly RoutedEvent<RoutedEventArgs> s_ClickEvent =
        RoutedEvent.Register<IconButton, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

    /// <summary>
    /// Occurs when a <see cref="MenuItem"/> without a submenu is clicked.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Click
    {
        add => AddHandler(s_ClickEvent, value);
        remove => RemoveHandler(s_ClickEvent, value);
    }
    
    /// <summary>
    /// Gets or sets the command associated with the menu item.
    /// </summary>
    public ICommand? Command
    {
        get => _command;
        set => SetAndRaise(CommandProperty, ref _command, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the <see cref="Command"/> property of a
    /// <see cref="MenuItem"/>.
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public Brush Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public Geometry Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public string Hint
    {
        get => GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }
    
    protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;
    
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        if (Command != null)
        {
            Command.CanExecuteChanged += CanExecuteChanged;
        }
    }

    /// <summary>
    /// Called when the <see cref="Command"/> property changes.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void CommandChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is IconButton iconButton)
        {
            if (((ILogical)iconButton).IsAttachedToLogicalTree)
            {
                if (e.OldValue is ICommand oldCommand)
                {
                    oldCommand.CanExecuteChanged -= iconButton.CanExecuteChanged;
                }

                if (e.NewValue is ICommand newCommand)
                {
                    newCommand.CanExecuteChanged += iconButton.CanExecuteChanged;
                }
            }

            iconButton.CanExecuteChanged(iconButton, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Called when the <see cref="CommandParameter"/> property changes.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void CommandParameterChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is IconButton iconButton)
        {
            iconButton.CanExecuteChanged(iconButton, EventArgs.Empty);
        }
    }


    /// <summary>
    /// Called when the <see cref="ICommand.CanExecuteChanged"/> event fires.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void CanExecuteChanged(object sender, EventArgs e)
    {
        var canExecute = Command == null || Command.CanExecute(CommandParameter);

        if (canExecute != _commandCanExecute)
        {
            _commandCanExecute = canExecute;
            UpdateIsEffectivelyEnabled();
        }
    }
    
    /// <summary>
    /// Called when the <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="e">The click event args.</param>
    private void OnClick(RoutedEventArgs e)
    {
        if (e.Handled || Command?.CanExecute(CommandParameter) != true) return;
        Command.Execute(CommandParameter);
        e.Handled = true;
    }
}