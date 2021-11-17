using System.Reactive.Subjects;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.EditJob;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class JobEditorModel : BlazorViewModel
{
    public IState<EventCallback<JobEditorCommit>> Commit { get; }

    public IState<EventCallback> Cancel { get; }

    public IState<bool> CanCancel { get; }

    private readonly BehaviorSubject<bool> _isValid;

    public Action<bool> IsValid { get; }
    
    public JobEditorModel(IStateFactory stateFactory)
        : base(stateFactory)
    {
        _isValid = new BehaviorSubject<bool>(false).DisposeWith(this);
        
        Commit = GetParameter<EventCallback<JobEditorCommit>>(nameof(JobEditor.Commit));
        Cancel = GetParameter<EventCallback>(nameof(JobEditor.Cancel));
        CanCancel = GetParameter<bool>(nameof(JobEditor.CanCancel));
    }
}