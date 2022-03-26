using System;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Shared;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;

public sealed class FileDetailDisplayViewModelBase : ReactiveObject
{
    public IObservable<ProjectFileInfo?> FileInfo { get; }
    
    public FileDetailDisplayViewModelBase(GlobalState state, IState<ProjectFileId?> id)
        => FileInfo = state.Files.QueryFileInfo(id);
}