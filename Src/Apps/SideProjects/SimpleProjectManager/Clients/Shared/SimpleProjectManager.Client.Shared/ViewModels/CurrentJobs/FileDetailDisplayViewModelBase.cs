using System;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Shared;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;

public abstract class FileDetailDisplayViewModelBase : ReactiveObject
{
    public IObservable<ProjectFileInfo?> FileInfo { get; }
    
    protected FileDetailDisplayViewModelBase(GlobalState state)
        // ReSharper disable once VirtualMemberCallInConstructor
        => FileInfo = state.Files.QueryFileInfo(GetId());

    protected abstract IState<ProjectFileId?> GetId();
}