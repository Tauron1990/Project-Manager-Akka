﻿using System;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using JetBrains.Annotations;
using Material.Icons;

namespace SimpleProjectManager.Client.Avalonia.Controls;

public enum AlertSeverity
{
    Error,
    Warning,
    Information, 
    Sucess
}

[PublicAPI]
public sealed partial class Alert : UserControl, ICommandSource
{
    private AlertSeverity _severity;
    private string _message = string.Empty;

    public AlertSeverity Severity
    {
        get => _severity;
        set
        {
            _severity = value;
            
            AlertBorder.Background =
                value switch
                {
                    AlertSeverity.Error => Brushes.DarkRed,
                    AlertSeverity.Warning => Brushes.DarkOrange,
                    AlertSeverity.Information => Brushes.DarkBlue,
                    AlertSeverity.Sucess => Brushes.DarkGreen,
                    _ => Brushes.Transparent
                };

            Icon.Kind =
                value switch {
                    AlertSeverity.Error => MaterialIconKind.Error,
                    AlertSeverity.Warning => MaterialIconKind.Warning,
                    AlertSeverity.Information => MaterialIconKind.Information,
                    AlertSeverity.Sucess => MaterialIconKind.Done,
                    _ => MaterialIconKind.QuestionMark
                };
        }
    }

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            AlertText.Text = value;
        }
    }

    public Alert()
        => InitializeComponent();

    private bool IsPressed { get; set; }
    
    private void AlertBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        IsPressed = true;
        e.Handled = true;
    }

    private void AlertBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!IsPressed || e.InitialPressMouseButton != MouseButton.Left) return;

        IsPressed = false;
        e.Handled = true;

        if (this.GetVisualsAt(e.GetPosition(this)).Any(c => Equals(this, c) || this.IsVisualAncestorOf(c)))
        {
            OnClick();
        }
    }

    private void OnClick()
    {
        if(Command is null) return;
        
        if(Command.CanExecute(CommandParameter))
            Command.Execute(CommandParameter);
    }

    public void CanExecuteChanged(object sender, EventArgs e) { }

    public ICommand? Command { get; }
    
    public object? CommandParameter { get; }
}