using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class NewJobViewModel : BlazorViewModel
{
    public JobEditorViewModel EditorModel { get; }

    public Action<JobEditorCommit> Commit { get; }

    public Action Cancel { get; }

    private readonly ObservableAsPropertyHelper<bool> _commiting;

    public bool IsCommiting => _commiting.Value;
    
    public NewJobViewModel(IStateFactory stateFactory, PageNavigation pageNavigation, IEventAggregator eventAggregator, 
        IJobDatabaseService databaseService, JobEditorViewModel editorModel)
        : base(stateFactory)
    {
        EditorModel = editorModel;
        var cancel = ReactiveCommand.Create(pageNavigation.ShowStartPage).DisposeWith(this);
        Cancel = () => ((ICommand)cancel).Execute(Unit.Default);

        var commit = ReactiveCommand.CreateFromObservable<JobEditorCommit, bool>(
            commit =>
            {
                var dataObs = Observable.Return(commit.JobData.NewData);

                return
                    from data in dataObs
                    from sucess in eventAggregator.IsSuccess(
                            () => TimeoutToken.WithDefault(
                                default,
                                token => databaseService.CreateJob(new CreateProjectCommand(data.JobName, data.ProjectFiles, data.Status, data.Deadline), token)))
                       .AsTask()
                    from sucess2 in sucess
                        ? eventAggregator.IsSuccess(
                            () => TimeoutToken.WithDefault(
                                default,
                                token => databaseService.ChangeOrder(new SetSortOrder(true, data.Ordering), token))).AsTask()
                        : Task.FromResult(false)
                    from sucess3 in sucess2
                        ? eventAggregator.IsSuccess(async () => await commit.Upload()).AsTask()
                        : Task.FromResult(false)
                    select sucess3;
            }).DisposeWith(this);

        commit.Subscribe(
            s =>
            {
                if (s)
                    pageNavigation.ShowStartPage();
            }).DisposeWith(this);

        Commit = d => ((ICommand)commit).Execute(d);

        _commiting = commit.IsExecuting.ToProperty(this, m => m.IsCommiting).DisposeWith(this);
    }
}