using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using JetBrains.Annotations;

namespace Tauron.Application.Avalonia.Dialogs;

[PublicAPI]
public class DialogBase : TemplatedControl
{
    public static readonly StyledProperty<int> DialogTitleFontSizeProperty =
        AvaloniaProperty.Register<DialogBase, int>("DialogTitleFontSize", 25);

    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<DialogBase, object>("Content");

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<DialogBase, string>("Title");

    public static readonly StyledProperty<object>
        TopProperty = AvaloniaProperty.Register<DialogBase, object>("Top");

    public static readonly StyledProperty<object> BottomProperty =
        AvaloniaProperty.Register<DialogBase, object>("Bottom");

    public static readonly StyledProperty<DataTemplate> ContentTemplateProperty =
        AvaloniaProperty.Register<DialogBase, DataTemplate>("ContentTemplate");

    public DialogBase()
    {
        InitializeComponent();
    }

    public int DialogTitleFontSize
    {
        get => GetValue(DialogTitleFontSizeProperty);
        set => SetValue(DialogTitleFontSizeProperty, value);
    }

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object Top
    {
        get => GetValue(TopProperty);
        set => SetValue(TopProperty, value);
    }

    public object Bottom
    {
        get => GetValue(BottomProperty);
        set => SetValue(BottomProperty, value);
    }

    public DataTemplate ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public Task<TResult> MakeTask<TResult>(Func<TaskCompletionSource<TResult>, object> factory)
    {
        var source = new TaskCompletionSource<TResult>();

        DataContext = factory(source);

        return source.Task;
    }
}