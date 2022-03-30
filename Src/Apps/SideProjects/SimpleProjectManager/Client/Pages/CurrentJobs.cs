﻿using ReactiveUI;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Pages;

public partial class CurrentJobs
{
    private MudCommandButton? _newJob;

    private MudCommandButton? NewJob
    
    {
        get => _newJob;
        set => this.RaiseAndSetIfChanged(ref _newJob, value);
    }

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        yield return this.BindCommand(ViewModel, m => m.NewJob, v => v.NewJob);
    }
}