﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.States;

namespace SimpleProjectManager.Client.Shared.ViewModels.Pages;

public sealed class CurrentJobsViewModel : ReactiveObject
{
    public CurrentJobsViewModel(GlobalState state, PageNavigation pageNavigation)
    {
        Jobs = state.Jobs.CurrentJobs;
        #if DEBUG
        Jobs = Jobs.Do(l => Console.WriteLine($"{nameof(CurrentJobsViewModel)} -- Recived new Data: {l.Length}"));
        #endif

        NewJob = ReactiveCommand.Create(pageNavigation.NewJob, state.IsOnline);
    }

    public IObservable<JobSortOrderPair[]> Jobs { get; }

    public ReactiveCommand<Unit, Unit> NewJob { get; }
}