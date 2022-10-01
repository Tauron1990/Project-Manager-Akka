using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using DynamicData;
using JetBrains.Annotations;
using ReactiveUI;
using SimpleProjectManager.Client.Avalonia.ViewModels.CriticalErrors;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;

namespace SimpleProjectManager.Client.Avalonia.Views.CriticalErrors;

public partial class CriticalErrorsView : ReactiveUserControl<CriticalErrorsViewModel>
{
    [UsedImplicitly]
    public CriticalErrorsView()
    {
        
    }
    
    public CriticalErrorsView(IMessageDispatcher messageDispatcher)
    {
        InitializeComponent();

        this.WhenActivated(Init);
        
        IEnumerable<IDisposable> Init()
        {
            if(ViewModel is null) yield break;

            var errors = ErrorViewModel.TranslateErrorList(ViewModel, messageDispatcher, out var disposer);

            yield return disposer;
            yield return errors.Bind(out var list).Subscribe();

            Errors.Items = list;

            var hasErrors = ErrorViewModel.TranslateHasError(errors);
        
            yield return hasErrors.Select(p => p.NoConnection || p.NoError).ObserveOn(RxApp.MainThreadScheduler).BindTo(this, v => v.DiplayLabel.IsVisible);
        
            yield return hasErrors.Where(p => p.NoConnection).Select(_ => "Keine Verbindung zum Server").ObserveOn(RxApp.MainThreadScheduler).BindTo(this, v => v.DiplayLabel.Text);
            yield return hasErrors.Where(p => p.NoError).Select(_ => "Keine Fehler").ObserveOn(RxApp.MainThreadScheduler).BindTo(this, v => v.DiplayLabel.Text);

            yield return hasErrors.Select(p => !p.NoConnection && !p.NoError).ObserveOn(RxApp.MainThreadScheduler).BindTo(this, v => v.Errors.IsVisible);
        }
    }
}