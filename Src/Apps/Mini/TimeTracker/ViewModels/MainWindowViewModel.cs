﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Data;
using Autofac;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Alias;
using DynamicData.Kernel;
using JetBrains.Annotations;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Tauron;
using Tauron.Application;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;
using Tauron.ObservableExt;
using TimeTracker.Data;
using TimeTracker.Views;

namespace TimeTracker.ViewModels
{
    [UsedImplicitly]
    public sealed class MainWindowViewModel : UiActor
    {
        public UIProperty<SnackbarMessageQueue?> SnackBarQueue { get;  }

        public UIProperty<string> CurrentProfile { get; }

        public UICollectionProperty<string> AllProfiles { get; }

        public UICollectionProperty<UiProfileEntry> ProfileEntries { get; }

        public UIProperty<UiProfileEntry?> CurrentEntry { get; }

        public UIProperty<int> HoursMonth { get; }

        public UIProperty<int> HoursShort { get; }

        public UIProperty<int> HoursAll { get; }

        public UIProperty<MonthState> CurrentState { get; }

        public UIPropertyBase? Come { get; }

        public UIPropertyBase? Go { get; }

        public UIPropertyBase? Correct { get; }

        public UIPropertyBase? AddEntry { get; }

        public UIProperty<int> Remaining { get; }

        public UIProperty<bool> IsProcessable { get; }

        public UIProperty<double> WeekendMultiplicator { get; }

        public UIProperty<double> HolidayMultiplicator { get; }

        public MainWindowViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, AppSettings settings, ITauronEnviroment enviroment, HolidayManager holidayManager)
            : base(lifetimeScope, dispatcher)
        {
            var serializationSettings = new JsonSerializerSettings {Formatting = Formatting.Indented};
            //serializationSettings.Converters.Add(new noda);

            SnackBarQueue = RegisterProperty<SnackbarMessageQueue?>(nameof(SnackBarQueue));
            dispatcher.InvokeAsync(() => new SnackbarMessageQueue(TimeSpan.FromSeconds(10)))
                      .Subscribe(SnackBarQueue);

            CurrentProfile = RegisterProperty<string>(nameof(CurrentProfile));
            AllProfiles = this.RegisterUiCollection<string>(nameof(AllProfiles))
                              .BindToList(settings.AllProfiles, out var list);

            HoursMonth = RegisterProperty<int>(nameof(HoursMonth));
            HoursShort = RegisterProperty<int>(nameof(HoursShort));
            HoursAll = RegisterProperty<int>(nameof(HoursAll));
            WeekendMultiplicator = RegisterProperty<double>(nameof(WeekendMultiplicator));
            HolidayMultiplicator = RegisterProperty<double>(nameof(HolidayMultiplicator));

            #region Profile Selection

            IsProcessable = RegisterProperty<bool>(nameof(IsProcessable));

            var trigger = new Subject<string>();
            var profileData = new ProfileData(string.Empty, 0, 0, 0, ImmutableDictionary<DateTime, ProfileEntry>.Empty, DateTime.MinValue).ToRx().DisposeWith(this);

            (from newProfile in CurrentProfile
             where !list.Items.Contains(newProfile)
             select newProfile)
               .Throttle(TimeSpan.FromSeconds(5))
               .AutoSubscribe(s =>
                              {
                                  if (string.IsNullOrWhiteSpace(s)) return;

                                  settings.AllProfiles = settings.AllProfiles.Add(s);
                                  list.Add(s);
                                  trigger.OnNext(s);
                              }, ReportError)
               .DisposeWith(this);

            (from profile in CurrentProfile
             where list.Items.Contains(profile)
             select profile)
               .Subscribe(trigger)
               .DisposeWith(this);

            profileData.Select(d => d.IsProcessable).Subscribe(IsProcessable).DisposeWith(this);

            #endregion

            #region FileHandling

            var fileNameBase = Guid.Parse("42CB06B0-B6F0-4F50-A1D9-294F47AA2AF6");
            //(from toLoad in trigger
            // select new ProfileData(GetFileName(toLoad), 0, 0, 0, ImmutableList<ProfileEntry>.Empty, DateTime.MinValue))
            //   .Subscribe(profileData)
            //   .DisposeWith(this);

            (from toLoad in trigger
             let fileName = GetFileName(toLoad)
             where File.Exists(fileName)
             from fileContent in File.ReadAllTextAsync(fileName)
             let newData = JsonConvert.DeserializeObject<ProfileData>(fileContent, serializationSettings)
             where newData != null
             select newData)
               .AutoSubscribe(profileData!, ReportError);

            (from toLoad in trigger
             let fileName = GetFileName(toLoad)
             where !File.Exists(fileName)
             select new ProfileData(fileName, 0,0,0, ImmutableDictionary<DateTime, ProfileEntry>.Empty, DateTime.MinValue))
               .AutoSubscribe(profileData!, ReportError);

            (from data in profileData
             where data.IsProcessable
             select data)
               .ToUnit(pd => File.WriteAllTextAsync(pd.FileName, JsonConvert.SerializeObject(pd, serializationSettings)))
               .AutoSubscribe(ReportError)
               .DisposeWith(this);

            string GetFileName(string profile)
                => Path.Combine(
                    enviroment.AppData(),
                    GuidFactories.Deterministic.Create(fileNameBase, profile) + "json");

            #endregion

            #region Hours

            var processData = profileData.Where(pd => pd.IsProcessable);
            processData.Select(pd => pd.AllHours).DistinctUntilChanged().Subscribe(HoursAll).DisposeWith(this);
            processData.Select(pd => pd.MinusShortTimeHours).DistinctUntilChanged().Subscribe(HoursShort).DisposeWith(this);
            processData.Select(pd => pd.MonthHours).DistinctUntilChanged().Subscribe(HoursMonth).DisposeWith(this);

            (from hour in HoursMonth.DistinctUntilChanged()
             from data in profileData.Take(1)
             where data.IsProcessable
             select data with {MonthHours = hour})
               .AutoSubscribe(profileData, ReportError)
               .DisposeWith(this);

            (from hour in HoursShort.DistinctUntilChanged()
             from data in profileData.Take(1)
             where data.IsProcessable
             select data with {MinusShortTimeHours = hour})
               .AutoSubscribe(profileData, ReportError)
               .DisposeWith(this);

            (from data in processData
             where data.MonthHours > 10
             where data.MinusShortTimeHours == 0
             select data with {MinusShortTimeHours = CalculateShortTimeHours(data.MonthHours)})
               .AutoSubscribe(profileData, ReportError)
               .DisposeWith(this);

            static int CalculateShortTimeHours(int mouthHours)
            {
                var multi = mouthHours / 100d;
                var minus = 10 * multi;

                return (int) Math.Round(minus, 0, MidpointRounding.ToPositiveInfinity);
            }

            #endregion

            #region Entrys

            var cache = new SourceCache<ProfileEntry, DateTime>(e => e.Date).DisposeWith(this);
            var isHere = false.ToRx().DisposeWith(this);

            ProfileEntries = this.RegisterUiCollection<UiProfileEntry>(nameof(ProfileEntries))
                                 .BindTo(cache.Connect()
                                              .Select(entry => new UiProfileEntry(entry)));

            dispatcher.InvokeAsync(() =>
                                   {
                                       var view = (ListCollectionView)CollectionViewSource.GetDefaultView(ProfileEntries.Property.Value);
                                       view.CustomSort = Comparer<UiProfileEntry>.Default;
                                   });

            (from data in profileData.DistinctUntilChanged(d => d.FileName).ObserveOn(Scheduler.Default)
             where data.IsProcessable
             select data.Entries)
               .AutoSubscribe(e =>
                              {
                                  cache.Clear();
                                  e.Select(pair => pair.Value).OrderByDescending(v => v.Date).Foreach(cache.AddOrUpdate);
                                  if(cache.Count == 0) return;

                                  CheckHere();
                              })
               .DisposeWith(this);

            (from ent in profileData.DistinctUntilChanged(pd => pd.HolidaysSet)
             where ent.IsProcessable && !ent.HolidaysSet
             from holidays in holidayManager.RequestFor(ent.CurrentMonth)
             select holidays.Select(day => new DateTime(ent.CurrentMonth.Year, ent.CurrentMonth.Month, day))
                            .ToArray())
               .AutoSubscribe(d =>
                              {
                                  var dic = profileData.Value.Entries;
                                  foreach (var free in d)
                                  {
                                      if (dic.TryGetValue(free, out var entry) && !entry.IsHoliday)
                                          entry = entry with {IsHoliday = true};
                                      else
                                          entry = new ProfileEntry(free, null, null, true);
                                      dic = dic.SetItem(free, entry);
                                      cache.AddOrUpdate(entry);
                                  }

                              },ReportError)
               .DisposeWith(this);

            Come = NewCommad
                  .WithCanExecute(from data in profileData
                                  select data.IsProcessable)
                  .WithCanExecute(from here in isHere 
                                  select !here)
                  .ThenFlow(() => SystemClock.NowDate,
                       obs => (from date in obs
                               from isHoliday in holidayManager.IsHoliday(date, date.Day)
                               select new ProfileEntry(date, SystemClock.NowTime, null, isHoliday))
                          .AutoSubscribe(ent =>
                                         {
                                             if(cache.Lookup(ent.Date).HasValue)
                                                 SnackBarQueue.Value?.Enqueue("Tag Schon eingetragen");
                                             else
                                             {
                                                 profileData.Value = profileData.Value with {Entries = profileData.Value.Entries.Add(ent.Date, ent)};
                                                 cache.AddOrUpdate(ent);
                                                 CheckHere();
                                             }

                                             CheckNewMonth(Observable.Return(Unit.Default));
                                         }, ReportError))
                  .ThenRegister(nameof(Come));

            Go = NewCommad
                .WithCanExecute(from data in profileData 
                                select data.IsProcessable)
                .WithCanExecute(isHere)
                .WithExecute(() =>
                             {
                                 var date = SystemClock.NowDate;
                                 var entry = cache.Lookup(date).ValueOrDefault();

                                 if (entry != null && entry.Start != null)
                                 {
                                     cache.AddOrUpdate(entry with {Finish = SystemClock.NowTime});
                                     isHere.Value = false;
                                 }
                                 else
                                    SnackBarQueue.Value?.Enqueue("Tag nicht gefunden");

                                 CheckHere();
                             })
               .ThenRegister(nameof(Go));

            void CheckNewMonth(IObservable<Unit> obs)
                => (from _ in obs.ObserveOn(Scheduler.Default)
                    from data in profileData.Take(1)
                    let month = SystemClock.NowDate
                    let current = data.CurrentMonth
                    where current.Month != month.Month || current.Year != month.Year
                    select (New:data with
                                {
                                    CurrentMonth = month, 
                                    HolidaysSet = false,
                                    Entries = data.Entries.RemoveRange(from entry in data.Entries.Values 
                                                                       where entry.Date.Month == data.CurrentMonth.Month
                                                                       select entry.Date)
                                }, Old:data))
                       .Subscribe(MakeBackup, ReportError);

            void MakeBackup((ProfileData New, ProfileData Old) obj)
            {
                var (newData, oldData) = obj;

                var oldEntrys = oldData.Entries.Values.Where(pe => pe.Date == oldData.CurrentMonth).ToImmutableList();
                cache!.RemoveKeys(oldEntrys.Select(oe => oe.Date));

                profileData!.Value = newData;

                var originalFile = newData.FileName;
                var originalName = Path.GetFileName(newData.FileName);

                if(oldEntrys.Count == 0) return;
                File.WriteAllText(
                    originalFile.Replace(originalName, $"{originalName}-{oldData.CurrentMonth:d}"), 
                    JsonConvert.SerializeObject(oldEntrys));
            }

            void CheckHere()
            {
                (from item in cache!.Items
                 where item.Start != null && item.Finish == null
                 orderby item.Date
                 select item).FirstOrDefault()
                             .OptionNotNull()
                             .Run(_ => isHere!.Value = true, () => isHere!.Value = false);
            }

            #endregion

            #region Correction

            CurrentEntry = RegisterProperty<UiProfileEntry?>(nameof(CurrentEntry));

            Correct = NewCommad
                     .WithCanExecute(from data in profileData
                                     select data.IsProcessable)
                     .WithCanExecute(from entry in CurrentEntry
                                     select entry != null)
                     .ThenFlow(obs => (from _ in obs
                                       let oldEntry = CurrentEntry.Value
                                       where oldEntry != null
                                       from result in this.ShowDialogAsync<CorrectionDialog, CorrectionResult, ProfileEntry>(() => oldEntry.Entry)
                                       select (New:result, Old:oldEntry.Entry))
                                  .AutoSubscribe(pair =>
                                                 {
                                                     var (correctionResult, old) = pair;
                                                     switch (correctionResult)
                                                     {
                                                         case UpdateCorrectionResult update:
                                                             UpdateProfileEntry(update.Entry, old);
                                                             break;
                                                         case DeleteCorrectionResult delete:
                                                             profileData.Value = profileData.Value with {Entries = profileData.Value.Entries.Remove(delete.Key)};
                                                             cache.RemoveKey(delete.Key);
                                                             CheckHere();
                                                             break;
                                                         default:
                                                             return;
                                                     }
                                                 },ReportError))
                     .ThenRegister(nameof(Correct));

            AddEntry = NewCommad
                      .WithCanExecute(IsProcessable)
                      .ThenFlow(obs => (from _ in obs 
                                        from data in profileData.Take(1)
                                        let parameter = new AddEntryParameter(data.Entries.Select(pe => pe.Value.Date.Day).ToHashSet(), data.CurrentMonth)
                                        from result in this.ShowDialogAsync<AddEntryDialog, AddEntryResult, AddEntryParameter>(() => parameter)
                                        select result)
                                   .AutoSubscribe(res =>
                                                  {
                                                      if (res is not NewAddEntryResult result) return;
                                                      
                                                      var data = profileData.Value;
                                                      profileData.Value = data with {Entries = data.Entries.Add(result.Entry.Date, result.Entry)};
                                                      cache.AddOrUpdate(result.Entry);
                                                      CheckHere();

                                                  }, ReportError))
                      .ThenRegister(nameof(AddEntry));

            void UpdateProfileEntry(ProfileEntry newEntry, ProfileEntry oldEntry)
            {
                if (newEntry.Date.Month != profileData!.Value.CurrentMonth.Month)
                {
                    SnackBarQueue?.Value?.Enqueue($"Nicht der selbe Monat wurde Gewält: {profileData.Value.CurrentMonth.Date.Month}");
                    return;
                }

                if (newEntry.Date.Year != profileData.Value.CurrentMonth.Year)
                {
                    SnackBarQueue?.Value?.Enqueue($"Nicht das selbe Jahr wurde Gewält: {profileData.Value.CurrentMonth.Date.Year}");
                    return;
                }

                var (oldDate, _, _, _) = oldEntry;
                var dic = profileData.Value.Entries;

                if (newEntry.Date != oldDate)
                {
                    cache?.RemoveKey(oldDate);
                    dic = dic.Remove(oldDate);
                }
                cache?.AddOrUpdate(newEntry!);
                var data = profileData.Value;
                profileData.Value = data with { Entries = dic.SetItem(newEntry.Date, newEntry) };

                CheckHere();
            }

            #endregion

            #region Calculation

            Remaining = RegisterProperty<int>(nameof(Remaining));

            CurrentState = RegisterProperty<MonthState>(nameof(CurrentState))
               .WithDefaultValue(MonthState.Minus);

            cache.Connect().ForAggregation().Sum(e =>
                                                 {
                                                     var (_, start, finish, _) = e;
                                                     if (start == null && finish == null)
                                                         return 8d;
                                                     if (start == null || finish == null) return 0;

                                                     var erg = finish - start;
                                                     if (erg.Value.TotalHours > 0)
                                                         return erg.Value.TotalHours;

                                                     return 0;
                                                 })
                 .Select(TimeSpan.FromHours)
                 .Select(ts => ts.Hours).Subscribe(HoursAll)
                 .DisposeWith(this);

            var calc = (from hours in HoursAll
                        from hoursMonth in HoursMonth.Take(1)
                        from data in profileData.Take(1)
                        where data.IsProcessable && hours > 0 && hoursMonth > 0
                        select CalculationResult.Calc(data.CurrentMonth, hours, hoursMonth, data.MinusShortTimeHours))
                      .Publish().RefCount();

            (from result in calc
             select result.MonthState)
               .Subscribe(CurrentState)
               .DisposeWith(this);

            (from result in calc
             select result.Remaining)
               .Subscribe(Remaining)
               .DisposeWith(this);

            #endregion

            void ReportError(Exception e)
                => dispatcher.InvokeAsync(() => SnackBarQueue!.Value?.Enqueue($"Fehler: {e.GetType().Name}--{e.Message}"));
        }
    }

    public sealed record CalculationResult(MonthState MonthState, int Remaining)
    {
        public static CalculationResult Calc(DateTime currentMonth, int allHouers, int targetHours, int maxShort)
        {
            var targetDays = SystemClock.DaysInCurrentMonth(currentMonth);
            var targetCurrent = targetHours / (targetDays - SystemClock.NowDay);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (targetCurrent == double.PositiveInfinity)
                targetCurrent = targetHours;

            var remaining = allHouers - targetCurrent;
            if (remaining > 0)
                return new CalculationResult(MonthState.Ok, (int)remaining);
            return maxShort > 0 && remaining > maxShort * -1
                ? new CalculationResult(MonthState.Short, (int)remaining)
                : new CalculationResult(MonthState.Minus, (int)remaining);
        }
    }

    public sealed class UiProfileEntry : ObservableObject, IComparable<UiProfileEntry>, IDisposable
    {
        private string? _date;
        private string? _start;
        private string? _finish;
        private string? _hour;
        private bool _isValid;
        private readonly IDisposable _subscription;

        public ProfileEntry Entry { get; }

        public string? Date
        {
            get => _date;
            private set => _date = value;
        }

        public string? Start
        {
            get => _start;
            private set => _start = value;
        }

        public string? Finish
        {
            get => _finish;
            private set => _finish = value;
        }

        public string? Hour
        {
            get => _hour;
            private set => _hour = value;
        }

        public bool IsValid
        {
            get => _isValid;
            private set => _isValid = value;
        }

        public UiProfileEntry(ProfileEntry entry)
        {
            Entry = entry;
            UpdateLabels();
        }

        //public UiProfileEntry()
        //{
        //    Entry = new ProfileEntry(DateTime.Now.Date, TimeSpan.FromHours(8), TimeSpan.FromHours(16));
        //    UpdateLabels();
        //}

        private void UpdateLabels()
        {
            Date = Entry.Date.ToLocalTime().ToString("D");
            Start = Entry.Start?.ToString(@"hh\:mm");
            Finish = Entry.Finish?.ToString(@"hh\:mm");
            Hour = (Entry.Finish - Entry.Start)?.ToString(@"hh\:mm");

            var start = Entry.Start;
            var finish = Entry.Finish;
            if (start != null && finish == null)
                IsValid = false;
            else if (start > finish)
                IsValid = false;
            else
                IsValid = true;
        }

        public int CompareTo(UiProfileEntry? other) 
            => other == null ? 1 : Entry.Date.CompareTo(other.Entry.Date) * -1;

        public void Dispose() => _subscription.Dispose();
    }
}