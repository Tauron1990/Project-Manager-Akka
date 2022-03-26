using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Client.Shared.ViewModels.EditJob;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.Pages;

public sealed class NewJobViewModel : ViewModelBase
{
    public JobEditorViewModelBase EditorModel { get; }

    public ReactiveCommand<JobEditorCommit, bool> Commit { get; }

    public ReactiveCommand<Unit, Unit> Cancel { get; }

    private readonly ObservableAsPropertyHelper<bool> _commiting;

    public bool IsCommiting => _commiting.Value;
    
    public NewJobViewModel(PageNavigation pageNavigation, GlobalState globalState, JobEditorViewModelBase editorModel)
    {
        EditorModel = editorModel;
        Cancel = ReactiveCommand.Create(pageNavigation.ShowStartPage).DisposeWith(this);

        Commit = ReactiveCommand.CreateFromObservable<JobEditorCommit, bool>(
            commit =>
            {
                var sub = new Subject<bool>();
                globalState.Dispatch(new CommitJobEditorData(commit, sub.OnNext));
                
                return sub.Take(1);
            }, globalState.IsOnline).DisposeWith(this);

        Commit.Subscribe(
            s =>
            {
                if (s)
                    pageNavigation.ShowStartPage();
            }).DisposeWith(this);

        _commiting = Commit.IsExecuting.ToProperty(this, m => m.IsCommiting).DisposeWith(this);
    }
}