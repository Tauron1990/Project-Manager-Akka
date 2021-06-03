using System;
using System.Collections.Immutable;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using Newtonsoft.Json;
using Tauron;
using Tauron.Application;
using Tauron.ObservableExt;
using TimeTracker.Data;

namespace TimeTracker.Managers
{
    public sealed class ProfileManager : IDisposable
    {
        private static readonly JsonSerializerSettings SerializationSettings = new() { Formatting = Formatting.Indented };
        private static readonly Guid FileNamespace = Guid.Parse("42CB06B0-B6F0-4F50-A1D9-294F47AA2AF6");

        private readonly IDisposable _cleanUp;

        private readonly SystemClock _clock;
        private readonly ITauronEnviroment _enviroment;
        private readonly ConcurancyManager _concurancy;
        private readonly BehaviorSubject<ProfileData> _currentData;
        private readonly Subject<Exception> _errors = new();
        private readonly SourceCache<ProfileEntry, DateTime> _entryCache = new(pe => pe.Date);

        public IObservable<bool> IsProcessabe => from data in _currentData
                                                 select data.IsProcessable;

        public IObservable<ProfileData> ProcessableData => from data in _currentData
                                                           where data.IsProcessable
                                                           select data;

        public IObservable<Exception> Errors => _errors.AsObservable();

        public ConfigurationManager ConfigurationManager { get; }

        public ProfileManager(SystemClock clock, ITauronEnviroment enviroment, ConcurancyManager concurancy)
        {
            _clock = clock;
            _enviroment = enviroment;
            _concurancy = concurancy;
            _currentData = new BehaviorSubject<ProfileData>(
                new ProfileData(string.Empty, 0, 0, 0, ImmutableDictionary<DateTime, ProfileEntry>.Empty, DateTime.MinValue, ImmutableList<HourMultiplicator>.Empty, false));
            ConfigurationManager = new ConfigurationManager(_currentData, _concurancy);

            _cleanUp = new CompositeDisposable(
                Disposable.Create(() =>
                                  {
                                      _currentData.OnCompleted();
                                      _currentData.Dispose();

                                      _entryCache.Dispose();

                                      _errors.OnCompleted();
                                      _errors.Dispose();
                                  }),
                CreateFileSafePipeLine());
        }

        public IDisposable CreateFileLoadPipeline(IObservable<string> toLoadObservable)
        {
            return (from name in toLoadObservable
                    let fileName = GetFileName(name)
                    from data in File.Exists(fileName)
                        ? FromFile(fileName).SyncCall(_concurancy)
                        : NewFile(fileName).SyncCall(_concurancy)
                    select data)
               .AutoSubscribe(_currentData, _errors.OnNext);
        }

        private IObservable<ProfileData> FromFile(string input)
            =>  Observable.Return(input)
                          .CatchSafe(file => from targetFile in Observable.Return(file)
                                             from text in File.ReadAllTextAsync(targetFile)
                                             let data = JsonConvert.DeserializeObject<ProfileData>(text, SerializationSettings)
                                             select data,
                               (s, e) =>
                               {
                                   _errors.OnNext(e);
                                   return NewFile(s);
                               })
                          .SelectMany(pd => pd == null ? NewFile(input) : Observable.Return(pd));

        private IObservable<ProfileData> NewFile(string file)
            => Observable.Return(ProfileData.New(file, _clock));

        private IDisposable CreateFileSafePipeLine()
            => ProcessableData
              .ToUnit(pd => File.WriteAllTextAsync(pd.FileName, JsonConvert.SerializeObject(pd, SerializationSettings)))
              .AutoSubscribe(_errors.OnNext);

        private string GetFileName(string profile)
            => Path.Combine(
                _enviroment.AppData(),
                GuidFactories.Deterministic.Create(FileNamespace, profile) + "json");

        void IDisposable.Dispose() => _cleanUp.Dispose();
    }
}