using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Client.Shared.ViewModels.EditJob;
using Stl.Fusion;
using Tauron.Application.Blazor.Parameters;
using SimpleProjectManager.Client.Core;
using SimpleProjectManager.Client.Shared;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using SimpleProjectManager.Client.Shared.EditJob;
using Tauron;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class JobEditorViewModel : JobEditorViewModelBase, IParameterUpdateable
{
    private readonly IStateFactory _factory;

    public JobEditorViewModel(IMessageDispatcher dispatcher, FileUploaderViewModelBase uploaderViewModel, GlobalState globalState, IStateFactory factory) 
        : base(dispatcher, uploaderViewModel, globalState)
    {
        _factory = factory;
    }

    protected override ModelConfig GetModelConfiguration()
        => new
        (
            Updater.Register<EventCallback<JobEditorCommit>>(nameof(JobEditor.Commit), _factory)
               .ToObservable(MessageDispatcher.PropagateErrors())
               .Select(c => (IEventCallback<JobEditorCommit>)new EventCallBackImpl<JobEditorCommit>(c))
               .ToState(_factory),
            Updater.Register<EventCallback>(nameof(JobEditor.Cancel), _factory)
               .ToObservable(MessageDispatcher.PropagateErrors())
               .Select(c => (IEventCallback)new EventCallBackImpl(c))
               .ToState(_factory),
            Updater.Register<bool>(nameof(JobEditor.CanCancel), _factory),
            Updater.Register<JobEditorData?>(nameof(JobEditor.Data), _factory)
               .ToObservable(MessageDispatcher.PropagateErrors())
        );

    public ParameterUpdater Updater { get; } = new();
}