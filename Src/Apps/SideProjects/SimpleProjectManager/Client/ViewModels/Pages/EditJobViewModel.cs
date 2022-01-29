using System.Reactive;
using System.Reactive.Threading.Tasks;
using ReactiveUI;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Pages;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class EditJobViewModel : BlazorViewModel
{
    private IState<string> JobId { get; }

    public IObservable<JobEditorData?> EditorData { get; }
    
    public Action Cancel { get; }
    
    public Action<JobEditorCommit> Commit { get; }

    public EditJobViewModel(IStateFactory stateFactory, GlobalState state, PageNavigation pageNavigation)
        : base(stateFactory)
    {
        JobId = GetParameter<string>(nameof(EditJob.ProjectId));
        Cancel = pageNavigation.ShowStartPage;

        EditorData = state.JobsState.GetJobEditorData(JobId.ToObservable());
        
        var commit = ReactiveCommand.CreateFromObservable<JobEditorCommit, Unit>(
            newData => state.JobsState.CommitJobData(newData, pageNavigation.ShowStartPage).ToObservable());
        
        Commit = commit.ToAction();
    }
}