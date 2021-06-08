using System;
using System.Collections.Immutable;
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
        private readonly IEventAggregator _aggregator;

        private readonly BehaviorSubject<ProfileData> _currentData = new(
            new ProfileData(string.Empty, 0, 0, 0, ImmutableDictionary<DateTime, ProfileEntry>.Empty, DateTime.MinValue, ImmutableList<HourMultiplicator>.Empty, false, 0));

        public DataManager(ConcurancyManager concurancyManager, IEventAggregator aggregator)
        {
            _concurancyManager = concurancyManager;
            _aggregator = aggregator;

            Stream = _currentData.ObserveOn(Scheduler.Default);
        }

        public IObservable<ProfileData> Mutate(Func<IObservable<ProfileData>, IObservable<ProfileData>> change)
            => _concurancyManager.SyncCall(
                                      _currentData.AsObservable().Take(1),
                                      observable => change(observable).Do(d => _currentData.OnNext(d), _aggregator.ReportError))
                                 .ObserveOn(Scheduler.Default);

        public IObservable<ProfileData> Stream { get; }

        public void Dispose()
        {
            _currentData.OnCompleted();
            _currentData.Dispose();
        }
    }
}