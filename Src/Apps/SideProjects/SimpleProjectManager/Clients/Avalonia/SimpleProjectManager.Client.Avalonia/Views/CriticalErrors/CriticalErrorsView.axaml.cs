using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SimpleProjectManager.Client.Avalonia.Views.CriticalErrors;

public partial class CriticalErrorsView : UserControl
{
    public CriticalErrorsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}