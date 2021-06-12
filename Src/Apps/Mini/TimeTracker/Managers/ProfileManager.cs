using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
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
        private readonly HolidayManager _holidayManager;
        private readonly DataManager _dataManager;
        private readonly IEventAggregator _aggregator;
        private readonly SourceCache<ProfileEntry, DateTime> _entryCache = new(pe => pe.Date);

        public IObservable<bool> IsProcessable
            => (from data in _dataManager.Stream
                select data.IsProcessable)
               .ObserveOn(Scheduler.Default);

        public IObservable<ProfileData> ProcessableData => from data in _dataManager.Stream
                                                           where data.IsProcessable
                                                           select data;

        public IEnumerable<ProfileEntry> Entries => _entryCache.Items;

        public ConfigurationManager ConfigurationManager { get; }

        public ProfileManager(SystemClock clock, ITauronEnviroment enviroment, HolidayManager holidayManager, DataManager dataManager, 
            IEventAggregator aggregator)
        {
            _clock = clock;
            _enviroment = enviroment;
            _holidayManager = holidayManager;
            _dataManager = dataManager;
            _aggregator = aggregator;
            ConfigurationManager = new ConfigurationManager(dataManager, _aggregator.ReportError);

            _cleanUp = new CompositeDisposable(
                CreateFileSafePipeLine(), _entryCache, CreateCacheUpdater(), ConfigurationManager, AutoUpdateHoliDays());
        }
        

        //public ProfileManager(DataManager dataManager)
        //{
        //    _dataManager = dataManager;
        //    //ConfigurationManager = new ConfigurationManager(dataManager, _aggregator.ReportError);

        //    //_cleanUp = new CompositeDisposable(
        //    //    CreateFileSafePipeLine(), _entryCache, CreateCacheUpdater(), ConfigurationManager, AutoUpdateHoliDays());
        //}

        public IObservable<IChangeSet<ProfileEntry, DateTime>> ConnectCache()
            => _entryCache.Connect().ObserveOn(Scheduler.Default);

        public IDisposable CreateFileLoadPipeline(IObservable<string> toLoadObservable)
        {
            return (from name in toLoadObservable
                    let fileName = GetFileName(name)
                    from data in _dataManager.Mutate(_ => from data in File.Exists(fileName)
                                                              ? FromFile(fileName)
                                                              : NewFile(fileName)
                                                          select data)
                    select data)
               .AutoSubscribe(_aggregator.ReportError);
        }

        private IObservable<ProfileData> FromFile(string input)
            =>  Observable.Return(input)
                          .CatchSafe(file => from targetFile in Observable.Return(file)
                                             from text in File.ReadAllTextAsync(targetFile)
                                             let data = JsonConvert.DeserializeObject<ProfileData>(text, SerializationSettings)
                                             select data,
                               (s, e) =>
                               {
                                   _aggregator.ReportError(e);
                                   return NewFile(s);
                               })
                          .SelectMany(pd => pd == null ? NewFile(input) : Observable.Return(pd));

        private IObservable<ProfileData> NewFile(string file)
            => Observable.Return(ProfileData.New(file, _clock));

        private IDisposable CreateFileSafePipeLine()
            => ProcessableData
              .ToUnit(pd => File.WriteAllTextAsync(pd.FileName, JsonConvert.SerializeObject(pd, SerializationSettings)))
              .AutoSubscribe(_aggregator.ReportError);

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
                                  Task.Delay(1000).ContinueWith(_ => e.Select(pair => pair.Value).OrderByDescending(v => v.Date).Foreach(_entryCache.AddOrUpdate));
                              }, _aggregator.ReportError);

        //private IDisposable AutoUpdateShortTimeHours()
        //    => (from data in ProcessableData.SyncCall(_concurancy)
        //        where data.MonthHours > 10
        //        where data.MinusShortTimeHours == 0
        //        select data with {MinusShortTimeHours = CalculationManager.CalculateShortTimeHours(data.MonthHours)})
        //       .AutoSubscribe(_dataManager.Update, _aggregator.ReportError);

        private IDisposable AutoUpdateHoliDays()
        {
            return (from ent in ProcessableData.DistinctUntilChanged(pd => pd.HolidaysSet)
                    where !ent.HolidaysSet
                    from holidays in _holidayManager.RequestFor(ent.CurrentMonth)
                    let days = holidays.Select(day => new DateTime(ent.CurrentMonth.Year, ent.CurrentMonth.Month, day))
                    from data in _dataManager.Mutate(o => from data in o
                                                          select data with {Entries = UpdateEntries(data.Entries, days), HolidaysSet = true})
                    select data)
               .AutoSubscribe(_aggregator.ReportError);


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

        public IObservable<bool> Come(IObservable<ComeParameter> dateObs)
        {
            return from parameter in dateObs
                   let date = parameter.Time
                   from isHoliday in _holidayManager.IsHoliday(date, date.Day)
                   from result in !parameter.Override || _entryCache.Lookup(date).HasValue
                       ? Observable.Return(false)
                       : CreateEntry(Observable.Return(date), isHoliday ? DayType.Holiday : DayType.Normal)
                   select result;

            IObservable<bool> CreateEntry(IObservable<DateTime> returnDate, DayType type)
                => from date in returnDate
                   let entry = new ProfileEntry(date, _clock.NowTime, null, type)
                   from _ in SetEntry(entry)
                   from result in CheckNewMonth(date)
                   select result;

            IObservable<bool> CheckNewMonth(DateTime month)
                => from data in _dataManager.Stream.Take(1)
                   let current = data.CurrentMonth
                   let doBackup = current.Month != month.Month || current.Year != month.Year
                   from result in doBackup
                       ? StartBackUp(month)
                       : Observable.Return(true)
                   select result;


            IObservable<bool> StartBackUp(DateTime month)
                => _dataManager.Mutate(o => (from data in o
                                             select (New: data with
                                                          {
                                                              CurrentMonth = month,
                                                              HolidaysSet = false,
                                                              Entries = data.Entries.RemoveRange(from entry in data.Entries.Values
                                                                                                 where entry.Date.Month == data.CurrentMonth.Month
                                                                                                 select entry.Date)
                                                          }, Old: data))
                                          .SelectMany(MakeBackup))
                               .Select(_ => true);

            IObservable<ProfileData> MakeBackup((ProfileData New, ProfileData Old) obj)
            {
                try
                {
                    var (newData, oldData) = obj;

                    var oldEntrys = oldData.Entries.Values.Where(pe => pe.Date == oldData.CurrentMonth).ToImmutableList();
                    _entryCache.RemoveKeys(oldEntrys.Select(oe => oe.Date));

                    var originalFile = newData.FileName;
                    var originalName = Path.GetFileName(newData.FileName);

                    if (oldEntrys.Count != 0)
                    {
                        File.WriteAllTextAsync(
                                 originalFile.Replace(originalName, $"{originalName}-{oldData.CurrentMonth:d}"),
                                 JsonConvert.SerializeObject(oldEntrys)).ToObservable()
                            .Subscribe(_ => { }, _aggregator.ReportError);
                    }
                }
                catch (Exception e)
                {
                    _aggregator.ReportError(e);
                }

                return Observable.Return(obj.New);
            }
        }

        private IObservable<Unit> SetEntry(params ProfileEntry[] entrys)
            => _dataManager.Mutate(o => from data in o
                                        select data with {Entries = entrys.Aggregate(data.Entries, (entries, entry) => entries.SetItem(entry.Date, entry))})
                           .ToUnit(() => _entryCache.AddOrUpdate(entrys));

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

        public IObservable<string> UpdateEntry(ProfileEntry entry, DateTime oldDate)
        {
            return from mounthResult in MonthValidation()
                   from yearResult in YearValidation()
                   let error = !string.IsNullOrWhiteSpace(mounthResult)
                       ? mounthResult
                       : !string.IsNullOrWhiteSpace(yearResult)
                           ? yearResult
                           : string.Empty
                   from result in string.IsNullOrWhiteSpace(error)
                       ? entry.Date != oldDate
                           ? RemoveOldEntry()
                           : SetEntry(entry).Select(_ => string.Empty)
                       : Observable.Return(error)
                   select result;

            IObservable<string> MonthValidation()
                => from data in _dataManager.Stream.Take(1)
                   let monthOk = entry.Date.Month == data.CurrentMonth.Month
                   select monthOk
                       ? string.Empty
                       : $"Nicht der selbe Monat wurde Gewält: {data.CurrentMonth.Date.Month}";

            IObservable<string> YearValidation()
                => from data in _dataManager.Stream.Take(1)
                   let yearOk = entry.Date.Year == data.CurrentMonth.Year
                   select yearOk
                       ? string.Empty
                       : $"Nicht das selbe Jahr wurde Gewält: {data.CurrentMonth.Date.Year}";

            IObservable<string> RemoveOldEntry()
            {
                _entryCache.RemoveKey(oldDate);
                _entryCache.AddOrUpdate(entry);

                return _dataManager.Mutate(o => from data in o
                                                select data with{Entries = data.Entries.Remove(oldDate).SetItem(entry.Date, entry)})
                                   .Select(_ => string.Empty);
            }
        }

        public IObservable<Unit> DeleteEntry(DateTime entry)
            => _dataManager.Mutate(o => from data in o
                                        select data with {Entries = data.Entries.Remove(entry)})
                           .ToUnit(() => _entryCache.RemoveKey(entry));

        public IObservable<Unit> AddVacation(DateTime[] dates)
        {
            return from result in FilterOut().ToObservable()
                   from res in SetEntry(result
                                       .Select(dateTime => new ProfileEntry(dateTime, null, null, DayType.Vacation))
                                       .ToArray())
                   select res;

            async Task<ImmutableArray<DateTime>> FilterOut()
            {
                var result = ImmutableArray<DateTime>.Empty;

                foreach (var date in dates)
                {
                    if(!SystemClock.IsWeekDay(date) || await  _holidayManager.IsHoliday(date, date.Day))
                        continue;
                    result = result.Add(date);
                }

                return result;
            }
        }

        void IDisposable.Dispose() => _cleanUp.Dispose();
    }
}