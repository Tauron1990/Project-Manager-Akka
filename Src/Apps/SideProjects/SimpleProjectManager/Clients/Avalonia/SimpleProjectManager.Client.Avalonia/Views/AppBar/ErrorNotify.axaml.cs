using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SimpleProjectManager.Client.Avalonia.ViewModels.AppBar;

namespace SimpleProjectManager.Client.Avalonia.Views.AppBar;

public partial class ErrorNotify : ReactiveUserControl<NotifyErrorModel>
{
    public ErrorNotify()
    {
        InitializeComponent();
        
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, m => m.HasErrors, v => v.ErrorCounter.IsVisible).DisposeWith(d);
            this.OneWayBind(ViewModel, m => m.ErrorCount, v => v.ErrorCounter.Message).DisposeWith(d);
            this.BindCommand(ViewModel, m => m.NavigateError, v => v.ErrorCounter.Command).DisposeWith(d);
            
            this.WhenAnyValue(v => v.ViewModel!.HasErrors)
               .Select(b => !b)
               .BindTo(this, v => v.OkControl.IsVisible)
               .DisposeWith(d);
        });
    }
}