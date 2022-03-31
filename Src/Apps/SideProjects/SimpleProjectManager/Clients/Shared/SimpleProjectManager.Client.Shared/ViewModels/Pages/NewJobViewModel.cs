using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Client.Shared.ViewModels.EditJob;

namespace SimpleProjectManager.Client.Shared.ViewModels.Pages;

public sealed class NewJobViewModel : ViewModelBase
{
    public JobEditorViewModelBase EditorModel { get; }

    public ReactiveCommand<JobEditorCommit, bool>? Commit { get; private set; }

    public ReactiveCommand<Unit, Unit>? Cancel { get; private set; }

    private ObservableAsPropertyHelper<bool>? _commiting;

    public bool IsCommiting => _commiting?.Value ?? false;
    
    public NewJobViewModel(PageNavigation pageNavigation, GlobalState globalState, JobEditorViewModelBase editorModel)
    {
        EditorModel = editorModel;

        this.WhenActivated(Init);
        
        IEnumerable<IDisposable> Init()
        {
            yield return Cancel = ReactiveCommand.Create(pageNavigation.ShowStartPage);

            yield return Commit = ReactiveCommand.CreateFromObservable<JobEditorCommit, bool>(
                commit =>
                {
                    var sub = new Subject<bool>();
                    globalState.Dispatch(new CommitJobEditorData(commit, sub.OnNext));

                    return sub.Take(1);
                },
                globalState.IsOnline);

            yield return Commit.Subscribe(
                s =>
                {
                    if (s)
                        pageNavigation.ShowStartPage();
                });

            yield return _commiting = Commit.IsExecuting.ToProperty(this, m => m.IsCommiting);
        }
    }
}