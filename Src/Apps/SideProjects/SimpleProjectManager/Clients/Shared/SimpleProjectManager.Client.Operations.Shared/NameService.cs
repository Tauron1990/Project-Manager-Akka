using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SimpleProjectManager.Client.Operations.Shared;

public sealed class NameService : IObservable<NameState>, IObserver<NameState>
{ 
    private readonly BehaviorSubject<NameState> _cache = new(new NameState(ObjectName.Empty, NameClientState.UnInitialized));
    
    public IDisposable Subscribe(IObserver<NameState> observer)
        => _cache.DistinctUntilChanged().Subscribe(observer);
    
    void IObserver<NameState>.OnCompleted()
        => _cache.OnCompleted();

    void IObserver<NameState>.OnError(Exception error)
        => _cache.OnError(error);

    void IObserver<NameState>.OnNext(NameState value)
        => _cache.OnNext(value);
}