using System.Reactive.Linq;
using System.Reactive.Subjects;
using SimpleProjectManager.Client.Shared.CurrentJobs;
using Stl;
using Stl.Fusion;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class JobsViewModel : IDisposable
{
    private readonly BehaviorSubject<JobSortOrderPair?> _jobsSubject = new(null);

    public JobSortOrderPair? Current => _jobsSubject.Value;

    public IObservable<JobSortOrderPair?> CurrentInfo => _jobsSubject.DistinctUntilChanged();

    public IState<JobSortOrderPair?> CurrentJobState { get; }

    public JobsViewModel(IStateFactory stateFactory)
    {
        var state = stateFactory.NewMutable(Result.Value<JobSortOrderPair?>(null));

        CurrentInfo.Subscribe(p => state.Set(p));

        CurrentJobState = state;
    }

    public void Publish(JobSortOrderPair? info)
        => _jobsSubject.OnNext(info);

    public void Dispose()
        => _jobsSubject.Dispose();
}