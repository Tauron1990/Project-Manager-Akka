using System.Reactive.Linq;
using System.Reactive.Subjects;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public sealed class JobsViewModel : IDisposable
{
    private BehaviorSubject<JobInfo?> _jobsSubject = new(null);

    public IObservable<JobInfo?> CurrentInfo => _jobsSubject.AsObservable();

    public void Publ

    public void Dispose()
        => _jobsSubject.Dispose();
}