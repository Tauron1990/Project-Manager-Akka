using System;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Shared;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;

public abstract class FileDetailDisplayViewModelBase : ReactiveObject
{
    protected FileDetailDisplayViewModelBase(GlobalState state, IStateFactory stateFactory)
        // ReSharper disable once VirtualMemberCallInConstructor
        => FileInfo = state.Files.QueryFileInfo(GetId(stateFactory));

    public IObservable<ProjectFileInfo?> FileInfo { get; }

    protected abstract IState<ProjectFileId?> GetId(IStateFactory stateFactory);
}