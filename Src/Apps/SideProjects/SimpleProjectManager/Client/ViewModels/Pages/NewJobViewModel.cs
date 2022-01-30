using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using ReactiveUI;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Data.States;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class NewJobViewModel : BlazorViewModel
{
    public JobEditorViewModel EditorModel { get; }

    public Action<JobEditorCommit> Commit { get; }

    public Action Cancel { get; }

    private readonly ObservableAsPropertyHelper<bool> _commiting;

    public bool IsCommiting => _commiting.Value;
    
    public NewJobViewModel(IStateFactory stateFactory, PageNavigation pageNavigation, GlobalState globalState, JobEditorViewModel editorModel)
        : base(stateFactory)
    {
        EditorModel = editorModel;
        var cancel = ReactiveCommand.Create(pageNavigation.ShowStartPage).DisposeWith(this);
        Cancel = () => ((ICommand)cancel).Execute(Unit.Default);

        var commit = ReactiveCommand.CreateFromObservable<JobEditorCommit, bool>(
            commit =>
            {
                var sub = new Subject<bool>();
                globalState.Dispatch(new CommitJobEditorData(commit, sub.OnNext));
                
                return sub.Take(1);
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