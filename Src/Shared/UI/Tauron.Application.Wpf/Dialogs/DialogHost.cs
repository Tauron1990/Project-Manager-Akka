using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.AkkaHost;
using Tauron.Application.CommonUI.Dialogs;

namespace Tauron.Application.Wpf.Dialogs;

[PublicAPI]
[ContentProperty("Main")]
[DefaultProperty("Main")]
[TemplatePart(Name = "DialogContent", Type = typeof(ContentPresenter))]
[TemplatePart(Name = "MainContent", Type = typeof(ContentPresenter))]
public class DialogHost : Control
{
    public static readonly DependencyProperty DialogProperty = DependencyProperty.Register(
        "Dialog",
        typeof(object),
        typeof(DialogHost),
        new PropertyMetadata(
            default,
            (o, args) => ((DialogHost)o).DialogContent?.SetValue(ContentControl.ContentProperty, args.NewValue)));

    public static readonly DependencyProperty MainProperty = DependencyProperty.Register(
        "Main",
        typeof(object),
        typeof(DialogHost),
        new PropertyMetadata(
            default,
            (o, args) => ((DialogHost)o).MainContent?.SetValue(ContentControl.ContentProperty, args.NewValue)));

    static DialogHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DialogHost),
            new FrameworkPropertyMetadata(typeof(DialogHost)));
    }

    public DialogHost()
    {
        if(!ActorApplication.IsStarted)
            return;

        var coordinator =
            (IDialogCoordinatorUIEvents)TauronEnviroment.ServiceProvider.GetRequiredService<IDialogCoordinator>();

        coordinator.HideDialogEvent += () =>
                                       {
                                           if(MainContent != null)
                                           {
                                               MainContent.IsEnabled = true;
                                               MainContent.Visibility = Visibility.Visible;
                                           }

                                           if(DialogContent == null) return;

                                           DialogContent.Content = null;
                                           DialogContent.IsEnabled = false;
                                           DialogContent.Visibility = Visibility.Collapsed;
                                       };

        coordinator.ShowDialogEvent += o =>
                                       {
                                           if(MainContent != null)
                                           {
                                               MainContent.IsEnabled = false;
                                               MainContent.Visibility = Visibility.Collapsed;
                                           }

                                           if(DialogContent == null) return;

                                           DialogContent.Content = o;
                                           DialogContent.IsEnabled = true;
                                           DialogContent.Visibility = Visibility.Visible;
                                       };
    }

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

    private ContentPresenter? DialogContent { get; set; }

    private ContentPresenter? MainContent { get; set; }

    public override void OnApplyTemplate()
    {
        MainContent = GetTemplateChild("MainContent") as ContentPresenter;
        DialogContent = GetTemplateChild("DialogContent") as ContentPresenter;

        if(MainContent != null)
            MainContent.Content = Main;
        if(DialogContent != null)
            DialogContent.Content = Dialog;

        base.OnApplyTemplate();
    }
}