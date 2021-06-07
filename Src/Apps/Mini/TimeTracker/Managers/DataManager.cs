using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Tauron.Application;
using TimeTracker.Data;

namespace TimeTracker.Managers
{
    public sealed class DataManager : IDisposable
    {
        private readonly IEventAggregator _aggregator;
        private readonly ConcurancyManager _concurancyManager;
        private readonly BehaviorSubject<ProfileData> _currentData = new(
            new ProfileData(string.Empty, 0, 0, 0, ImmutableDictionary<DateTime, ProfileEntry>.Empty, DateTime.MinValue, ImmutableList<HourMultiplicator>.Empty, false, 0));

        public DataManager(IEventAggregator aggregator, ConcurancyManager concurancyManager)
        {
            _aggregator = aggregator;
            _concurancyManager = concurancyManager;
            Update = Observer.Create<ProfileData>(_currentData.OnNext, _aggregator.ReportError);
        }

        public IObserver<ProfileData> Update { get; } 

        public IObservable<ProfileData> Mutate()
            => _currentData.Take(1).SyncCall(_concurancyManager);

        public IObservable<ProfileData> Stream => _currentData.AsObservable();

        public void Dispose()
        {
            _currentData.OnCompleted();
            _currentData.Dispose();
        }
    }
}