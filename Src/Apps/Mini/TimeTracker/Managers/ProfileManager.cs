using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using DynamicData;
using Newtonsoft.Json;
using Tauron;
using Tauron.Application;
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
        private readonly HolidayManager _holidayManager;
        private readonly BehaviorSubject<ProfileData> _currentData;
        private readonly Subject<Exception> _errors = new();
        private readonly SourceCache<ProfileEntry, DateTime> _entryCache = new(pe => pe.Date);

        public IObservable<bool> IsProcessable => from data in _currentData
                                                 select data.IsProcessable;

        public IObservable<ProfileData> ProcessableData => from data in _currentData
                                                           where data.IsProcessable
                                                           select data;

        public IObservable<Exception> Errors => _errors.AsObservable();

        public IEnumerable<ProfileEntry> Entries => _entryCache.Items

        public ConfigurationManager ConfigurationManager { get; }

        public ProfileManager(SystemClock clock, ITauronEnviroment enviroment, ConcurancyManager concurancy, HolidayManager holidayManager)
        {
            _clock = clock;
            _enviroment = enviroment;
            _concurancy = concurancy;
            _holidayManager = holidayManager;
            _currentData = new BehaviorSubject<ProfileData>(
                new ProfileData(string.Empty, 0, 0, 0, ImmutableDictionary<DateTime, ProfileEntry>.Empty, DateTime.MinValue, ImmutableList<HourMultiplicator>.Empty, false));
            ConfigurationManager = new ConfigurationManager(_currentData, _concurancy, _errors.OnNext);

            _cleanUp = new CompositeDisposable(
                Disposable.Create(() =>
                                  {
                                      _currentData.OnCompleted();
                                      _errors.Dispose();
                                  }),
                CreateFileSafePipeLine(), _currentData, _entryCache, _errors, AutoUpdateShirtTimeHours(), CreateCacheUpdater(),
                ConfigurationManager, AutoUpdateHoliDays());
        }

        public IObservable<IChangeSet<ProfileEntry, DateTime>> ConnectCache()
            => _entryCache.Connect();

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

        private IDisposable CreateCacheUpdater()
            => (from data in ProcessableData.DistinctUntilChanged(d => d.FileName).ObserveOn(Scheduler.Default)
                select data.Entries)
               .AutoSubscribe(e =>
                              {
                                  _entryCache.Clear();
                                  e.Select(pair => pair.Value).OrderByDescending(v => v.Date).Foreach(_entryCache.AddOrUpdate);
                              }, _errors.OnNext);

        private IDisposable AutoUpdateShirtTimeHours()
            => (from data in ProcessableData.SyncCall(_concurancy)
                where data.MonthHours > 10
                where data.MinusShortTimeHours == 0
                select data with {MinusShortTimeHours = CalculationManager.CalculateShortTimeHours(data.MonthHours)})
               .AutoSubscribe(_currentData, _errors.OnNext);

        private IDisposable AutoUpdateHoliDays()
        {
            return (from ent in ProcessableData.DistinctUntilChanged(pd => pd.HolidaysSet)
                    where !ent.HolidaysSet
                    from holidays in _holidayManager.RequestFor(ent.CurrentMonth)
                    let days = holidays.Select(day => new DateTime(ent.CurrentMonth.Year, ent.CurrentMonth.Month, day))
                    from data in _currentData.SyncCall(_concurancy).Take(1)
                    select data with {Entries = UpdateEntries(data.Entries, days)})
               .AutoSubscribe(_currentData, _errors.OnNext);


            ImmutableDictionary<DateTime, ProfileEntry> UpdateEntries(ImmutableDictionary<DateTime, ProfileEntry> original, IEnumerable<DateTime> days)
                => days.Aggregate(original, (dic, free) =>
                                            {
                                                if (dic.TryGetValue(free, out var entry) && entry.DayType == DayType.Normal)
                                                    entry = entry with {DayType = DayType.Holiday};
                                                else
                                                    entry = new ProfileEntry(free, null, null, DayType.Holiday);
                                                dic = dic.SetItem(free, entry);
                                                _entryCache.AddOrUpdate(entry);
                                                return dic;
                                            });
        }

        public IObservable<bool> Come(IObservable<DateTime> dateObs)
        {
            return (from date in dateObs
                    from isHoliday in _holidayManager.IsHoliday(date, date.Day)
                    from result in _entryCache.Lookup(date).HasValue
                        ? Observable.Return(false)
                        : CreateEntry(Observable.Return(date), isHoliday ? DayType.Holiday : DayType.Normal)
                    select result);

            IObservable<bool> CreateEntry(IObservable<DateTime> returnDate, DayType type)
                => from date in returnDate
                   let entry = new ProfileEntry(date, _clock.NowTime, null, type)
                   from _ in SetEntry(entry)
                   from result in CheckNewMonth(date)
                   select result;

            IObservable<bool> CheckNewMonth(DateTime month)
                => from data in _currentData.Take(1).SyncCall(_concurancy)
                   let current = data.CurrentMonth
                   let doBackup = current.Month != month.Month || current.Year != month.Year
                   from result in doBackup
                       ? StartBackUp(data, month)
                       : Observable.Return(true)
                   select result;


            IObservable<bool> StartBackUp(ProfileData startData, DateTime month)
                => (from data in Observable.Return(startData)
                    select (New: data with
                                 {
                                     CurrentMonth = month,
                                     HolidaysSet = false,
                                     Entries = data.Entries.RemoveRange(from entry in data.Entries.Values
                                                                        where entry.Date.Month == data.CurrentMonth.Month
                                                                        select entry.Date)
                                 }, Old: data))
                  .Do(MakeBackup)
                  .Select(_ => true);

            void MakeBackup((ProfileData New, ProfileData Old) obj)
            {
                try
                {
                    var (newData, oldData) = obj;

                    var oldEntrys = oldData.Entries.Values.Where(pe => pe.Date == oldData.CurrentMonth).ToImmutableList();
                    _entryCache.RemoveKeys(oldEntrys.Select(oe => oe.Date));

                    _currentData.OnNext(newData!);

                    var originalFile = newData.FileName;
                    var originalName = Path.GetFileName(newData.FileName);

                    if (oldEntrys.Count == 0) return;
                    File.WriteAllTextAsync(
                             originalFile.Replace(originalName, $"{originalName}-{oldData.CurrentMonth:d}"),
                             JsonConvert.SerializeObject(oldEntrys)).ToObservable()
                        .Subscribe(_ => { }, _errors.OnNext);
                }
                catch (Exception e)
                {
                    _errors.OnNext(e);
                }
            }
        }

        private IObservable<Unit> SetEntry(ProfileEntry entry)
            => (from data in _currentData.SyncCall(_concurancy).Take(1)
                select data with {Entries = data.Entries.SetItem(entry.Date, entry)})
              .Do(pd =>
                  {
                      _currentData.OnNext(pd);
                      _entryCache.AddOrUpdate(entry);
                  }).ToUnit();

        public IObservable<bool> Go(IObservable<DateTime> timeobs)
        {
            return from date in timeobs
                   let data = _entryCache.Lookup(date)
                   from result in data.HasValue
                       ? MakeUpdate(data.Value)
                       : Observable.Return(false)
                   select result;

            IObservable<bool> MakeUpdate(ProfileEntry entry)
                => from data in Observable.Return(entry)
                   let newEntry = data with {Finish = _clock.NowTime}
                   from _ in SetEntry(newEntry)
                   select true;
        }

        public IObservable<Unit> AddEntry(ProfileEntry entry) => SetEntry(entry);

        public IObservable<string> UpdateEntry(ProfileEntry entry, ProfileEntry oldEntry)
        {
            IObservable<string> 

            //if (newEntry.Date.Month != profileData!.Value.CurrentMonth.Month)
            //{
            //    SnackBarQueue?.Value?.Enqueue($"Nicht der selbe Monat wurde Gewält: {profileData.Value.CurrentMonth.Date.Month}");
            //    return;
            //}

            //if (newEntry.Date.Year != profileData.Value.CurrentMonth.Year)
            //{
            //    SnackBarQueue?.Value?.Enqueue($"Nicht das selbe Jahr wurde Gewält: {profileData.Value.CurrentMonth.Date.Year}");
            //    return;
            //}

            //var (oldDate, _, _, _) = oldEntry;
            //var dic = profileData.Value.Entries;

            //if (newEntry.Date != oldDate)
            //{
            //    cache?.RemoveKey(oldDate);
            //    dic = dic.Remove(oldDate);
            //}
            //cache?.AddOrUpdate(newEntry!);
            //var data = profileData.Value;
            //profileData.Value = data with { Entries = dic.SetItem(newEntry.Date, newEntry) };

            //CheckHere();
        }

        public IObservable<string> DeleteEntry(ProfileEntry entry)
        {

        }

        void IDisposable.Dispose() => _cleanUp.Dispose();
    }
}