using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Tauron.Application;
using TimeTracker.Data;

namespace TimeTracker.Managers
{
    public sealed class DataManager : IDisposable
    {
        private readonly ConcurancyManager _concurancyManager;
        private readonly BehaviorSubject<ProfileData> _currentData = new(
            new ProfileData(string.Empty, 0, 0, 0, ImmutableDictionary<DateTime, ProfileEntry>.Empty, DateTime.MinValue, ImmutableList<HourMultiplicator>.Empty, false, 0));

        private readonly IObserver<ProfileData> _update;

        public DataManager(ConcurancyManager concurancyManager, IEventAggregator aggregator)
        {
            _concurancyManager = concurancyManager;
            _update = Observer.Create<ProfileData>(_currentData.OnNext, aggregator.ReportError);
        }

        public IObservable<ProfileData> Mutate(Func<IObservable<ProfileData>, IObservable<ProfileData>> change)
            => _concurancyManager.SyncCall(
                                      _currentData.AsObservable().Take(1),
                                      observable => change(observable).Do(_update))
                                 .ObserveOn(Scheduler.Default);

        public IObservable<ProfileData> Stream => _currentData.ObserveOn(Scheduler.Default);

        public void Dispose()
        {
            _currentData.OnCompleted();
            _currentData.Dispose();
        }
    }
}