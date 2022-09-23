using System;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.Pages;

public sealed class EditJobViewModel : ViewModelBase
{
    public IMutableState<string> JobId { get; }

    public IObservable<JobEditorData?> EditorData { get; }
    
    public Action Cancel { get; }
    
    public ReactiveCommand<JobEditorCommit, Unit> Commit { get; }

    public EditJobViewModel(GlobalState state, PageNavigation pageNavigation, IStateFactory stateFactory)
    {
        JobId = stateFactory.NewMutable<string>();
        Cancel = pageNavigation.ShowStartPage;

        EditorData = state.Jobs.GetJobEditorData(JobId.ToObservable());
        
        Commit = ReactiveCommand.CreateFromObservable<JobEditorCommit, Unit>(
            newData => state.Jobs.CommitJobData(newData, pageNavigation.ShowStartPage).ToObservable(),
            state.IsOnline);
    }
}