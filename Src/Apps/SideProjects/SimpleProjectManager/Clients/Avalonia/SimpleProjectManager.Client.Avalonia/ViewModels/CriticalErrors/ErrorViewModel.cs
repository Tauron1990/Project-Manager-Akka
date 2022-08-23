using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Alias; 
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Avalonia.ViewModels.CriticalErrors;

public sealed class ErrorViewModel : CriticalErrorViewModelBase
{
    private readonly CriticalError _error;
    private readonly GlobalState _globalState;
    
    public ErrorViewModel(CriticalError error, GlobalState globalState) 
        : base(globalState)
    {
        _error = error;
        _globalState = globalState;
    }
    
    protected override IState<CriticalError?> GetErrorState()
    {
        var state =  _globalState.StateFactory.NewMutable<CriticalError?>();
        state.Set(_error);

        return state;
    }


    public static IObservable<(bool NoConnection, bool NoError)> TranslateHasError(IObservable<IChangeSet<ErrorViewModel, string>> errors)
        => errors.Scan(
                new ChangeAwareCache<ErrorViewModel, string>(),
                (cache, set) =>
                {
                    cache.Clone(set);
                    cache.CaptureChanges();
                    
                    return cache;
                })
           .Select(c => (c.Count == 1 && c.Keys.ElementAt(0) == "None", c.Count == 0))
           .Replay(1).RefCount();

    public static IObservable<IChangeSet<ErrorViewModel, string>> TranslateErrorList(CriticalErrorsViewModel errors, out IDisposable disposer)
    {
        var cache = new SourceCache<CriticalError, string>(ce => ce.Id);
        var dispoable = new CompositeDisposable();
        disposer = dispoable;
        
        errors.Errors.Aggregate(
            cache,
            (sourceCache, data) =>
            {
                if(data.IsOnline)
                {
                    sourceCache.Edit(
                        e =>
                        {
                            e.Clear();
                            e.AddOrUpdate(data.Errors);
                        });
                }
                else
                {
                    sourceCache.Edit(
                        c =>
                        {
                            c.Clear();
                            c.AddOrUpdate(new CriticalError("None", DateTime.Now, "Verbindung", "Keine Verbindung", null, ImmutableList<ErrorProperty>.Empty));
                        });
                }

                return sourceCache;
            })
           .Subscribe()
           .DisposeWith(dispoable);

        return cache.Connect().Select(c => new ErrorViewModel(c, errors.GlobalState))
           .Replay().RefCount();
    }
}