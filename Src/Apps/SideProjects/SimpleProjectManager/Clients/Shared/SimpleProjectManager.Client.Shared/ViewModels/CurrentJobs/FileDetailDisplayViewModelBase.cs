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
        #pragma warning disable MA0056
        => FileInfo = state.Files.QueryFileInfo(GetId(stateFactory));
    #pragma warning restore MA0056

    public IObservable<ProjectFileInfo?> FileInfo { get; }

    protected abstract IState<ProjectFileId?> GetId(IStateFactory stateFactory);
}