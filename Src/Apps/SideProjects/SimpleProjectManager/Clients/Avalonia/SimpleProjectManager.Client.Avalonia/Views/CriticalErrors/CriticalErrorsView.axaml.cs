using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SimpleProjectManager.Client.Avalonia.ViewModels.CriticalErrors;
using SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;

namespace SimpleProjectManager.Client.Avalonia.Views.CriticalErrors;

public partial class CriticalErrorsView : ReactiveUserControl<CriticalErrorsViewModel>
{
    public CriticalErrorsView()
    {
        InitializeComponent();

        this.WhenActivated(Init);
    }

    public IEnumerable<IDisposable> Init()
    {
        if(ViewModel is null) yield break;



        yield return ErrorViewModel.TranslateErrorList(ViewModel.Errors, out var disposer)
           .BindTo(this, v => v.Errors.Items);

        yield return disposer;
    }
}