using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.CommonUI.Dialogs;

namespace Tauron.Application.Avalonia.Dialogs;

[PublicAPI]
public class DialogHost : TemplatedControl
{
    public static readonly StyledProperty<object> DialogProperty =
        AvaloniaProperty.Register<DialogHost, object>("Dialog", defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object> MainProperty =
        AvaloniaProperty.Register<DialogHost, object>("Main", defaultBindingMode: BindingMode.TwoWay);

    public DialogHost()
    {
        InitializeComponent();

        var coordinator = (IDialogCoordinatorUIEvents)TauronEnviroment.ServiceProvider.GetRequiredService<IDialogCoordinator>();

        coordinator.HideDialogEvent += () =>
                                       {
                                           MainContent.IsEnabled = true;
                                           MainContent.IsVisible = true;

                                           DialogContent.Content = null;
                                           DialogContent.IsEnabled = false;
                                           DialogContent.IsVisible = false;
                                       };

        coordinator.ShowDialogEvent += o =>
                                       {
                                           MainContent.IsEnabled = false;
                                           MainContent.IsVisible = false;

                                           DialogContent.Content = o;
                                           DialogContent.IsEnabled = true;
                                           DialogContent.IsVisible = true;
                                       };
    }


    private ContentControl MainContent => this.Get<ContentControl>("MainContent");
    private ContentControl DialogContent => this.Get<ContentControl>("DialogContent");

    public object Dialog
    {
        get => GetValue(DialogProperty);
        set => SetValue(DialogProperty, value);
    }

    public object Main
    {
        get => GetValue(MainProperty);
        set => SetValue(MainProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}