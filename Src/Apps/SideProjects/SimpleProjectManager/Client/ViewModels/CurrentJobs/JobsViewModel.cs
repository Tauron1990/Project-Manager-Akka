/*using System.Reactive.Linq;
using System.Reactive.Subjects;
using SimpleProjectManager.Client.Data.States;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class JobsViewModel : IDisposable
{
    private readonly BehaviorSubject<JobSortOrderPair?> _jobsSubject = new(null);

    public JobSortOrderPair? Current => _jobsSubject.Value;

    public IObservable<JobSortOrderPair?> CurrentInfo => _jobsSubject.DistinctUntilChanged();

    public IState<JobSortOrderPair?> CurrentJobState { get; }

    public JobsViewModel(IStateFactory stateFactory)
    {
        var state = stateFactory.NewMutable<JobSortOrderPair?>();

        CurrentInfo.Subscribe(p => state.Set(p));
        CurrentJobState = state;
    }

    public void Publish(JobSortOrderPair? info)
        => _jobsSubject.OnNext(info);

    public void Dispose()
        => _jobsSubject.Dispose();
}*/