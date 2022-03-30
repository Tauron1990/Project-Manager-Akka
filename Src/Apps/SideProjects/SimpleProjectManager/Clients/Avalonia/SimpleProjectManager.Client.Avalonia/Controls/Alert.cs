using Avalonia.Controls;
using Avalonia.Media;
using JetBrains.Annotations;
using Material.Icons;

namespace SimpleProjectManager.Client.Avalonia.Controls;

public enum AlertSeverity
{
    Error,
    Warning,
    Information
}

[PublicAPI]
public sealed partial class Alert : UserControl
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
                    _ => Brushes.Transparent
                };

            Icon.Kind =
                value switch {
                    AlertSeverity.Error => MaterialIconKind.Error,
                    AlertSeverity.Warning => MaterialIconKind.Warning,
                    AlertSeverity.Information => MaterialIconKind.Information,
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
}