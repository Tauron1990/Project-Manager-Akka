using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Shared.CurrentJobs;
using SimpleProjectManager.Shared;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileDetailDisplayViewModel : BlazorViewModel
{
    public IObservable<ProjectFileInfo?> FileInfo { get; }
    
    public FileDetailDisplayViewModel(IStateFactory stateFactory, GlobalState state) 
        : base(stateFactory)
    {
        var id = GetParameter<ProjectFileId?>(nameof(FileDetailDisplay.FileId));
        FileInfo = state.Files.QueryFileInfo(id);
    }
}