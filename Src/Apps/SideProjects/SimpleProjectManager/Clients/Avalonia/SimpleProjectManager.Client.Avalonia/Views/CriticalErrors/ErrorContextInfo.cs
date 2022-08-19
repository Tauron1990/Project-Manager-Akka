using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Avalonia.Views.CriticalErrors;

public partial class ErrorContextInfo : UserControl
{
    public static DirectProperty<ErrorContextInfo, ImmutableList<ErrorProperty>?> ContextInfoProperty =
        AvaloniaProperty.RegisterDirect<ErrorContextInfo, ImmutableList<ErrorProperty>?>(
            nameof(ContextInfo),
            i => i._contextInfo,
            (i, v) =>
            {
                i._contextInfo = v;
                i.UpdateData();
            },
            ImmutableList<ErrorProperty>.Empty);

    private ImmutableList<ErrorProperty>? _contextInfo;

    public ImmutableList<ErrorProperty>? ContextInfo
    {
        get => GetValue(ContextInfoProperty);
        set => SetValue(ContextInfoProperty, value);
    }

    private void UpdateData()
    {
        if(ContextInfo is null || ContextInfo.IsEmpty)
        {
            NoInfo.IsVisible = true;
            Properys.IsVisible = false;
            Properys.Items = null;
        }
        else
        {
            NoInfo.IsVisible = false;
            Properys.IsVisible = true;
            Properys.Items = ContextInfo;
        }
    }

    public ErrorContextInfo()
    {
        InitializeComponent();
    }
}