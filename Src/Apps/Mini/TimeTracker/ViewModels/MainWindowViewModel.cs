using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Event;
using Autofac;
using DynamicData;
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

        public UIPropertyBase? Come { get; }

        public UIPropertyBase? Go { get; }

        public UIPropertyBase? Correct { get; }

        public MainWindowViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, AppSettings settings, ITauronEnviroment enviroment)
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

            #region Profile Selection

            var trigger = new Subject<string>();
            var profileData = new ProfileData(string.Empty, 0, 0, 0, ImmutableList<ProfileEntry>.Empty, DateTime.MinValue).ToRx().DisposeWith(this);

            (from newProfile in CurrentProfile
             where !list.Items.Contains(newProfile)
             select newProfile)
               .Throttle(TimeSpan.FromSeconds(10))
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

            #endregion

            #region FileHandling

            var fileNameBase = Guid.Parse("42CB06B0-B6F0-4F50-A1D9-294F47AA2AF6");
            (from toLoad in trigger
             select new ProfileData(GetFileName(toLoad), 0, 0, 0, ImmutableList<ProfileEntry>.Empty, DateTime.MinValue))
               .Subscribe(profileData)
               .DisposeWith(this);

            (from data in profileData.DistinctUntilChanged(pd => pd.FileName)
             where data.IsProcessable && File.Exists(data.FileName)
             from fileContent in File.ReadAllTextAsync(data.FileName)
             let newData = JsonConvert.DeserializeObject<ProfileData>(fileContent, serializationSettings)
             where newData != null
             select newData)
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

            (from data in profileData.DistinctUntilChanged(d => d.FileName).ObserveOn(Scheduler.Default)
             where data.IsProcessable
             select data.Entries)
               .AutoSubscribe(e =>
                              {
                                  cache.Clear();
                                  e.ForEach(cache.AddOrUpdate);

                                  (from item in cache.Items
                                   where item.Start != null && item.Finish == null
                                   orderby item.Date
                                   select item).FirstOrDefault()
                                               .OptionNotNull()
                                               .OnEmpty(() => isHere.Value = true);
                              })
               .DisposeWith(this);

            ProfileEntries = this.RegisterUiCollection<UiProfileEntry>(nameof(ProfileEntries))
                                 .BindTo(cache.Connect()
                                              .Sort(Comparer<ProfileEntry>.Create((entry1, entry2) => entry1.Date.CompareTo(entry2.Date)))
                                              .Select(entry => new UiProfileEntry(entry)));

            Come = NewCommad
                  .WithCanExecute(from data in profileData
                                  select data.IsProcessable)
                  .WithCanExecute(from here in isHere 
                                  select !here)
                  .WithExecute(() =>
                               {
                                   var date = SystemClock.NowDate;
                                   var entry = cache.Lookup(date).ValueOr(() => new ProfileEntry(date, null, null));
                                   
                                   if(entry.Start == null)
                                       cache.AddOrUpdate(entry with{Start = SystemClock.NowTime});
                                   isHere.Value = true;
                               })
                  .ThenFlow(CheckNewMonth)
                  .ThenRegister(nameof(Come));

            Go = NewCommad
                .WithCanExecute(from data in profileData 
                                select data.IsProcessable)
                .WithCanExecute(isHere)
                .WithExecute(() =>
                             {
                                 var date = SystemClock.NowDate;
                                 var entry = cache.Lookup(date).ValueOr(() => new ProfileEntry(date, null, null));

                                 if(entry.Start != null)
                                    cache.AddOrUpdate(entry with { Finish = SystemClock.NowTime });
                                 isHere.Value = false;
                             })
               .ThenRegister(nameof(Go));

            IDisposable CheckNewMonth(IObservable<Unit> obs)
                => (from _ in obs.ObserveOn(Scheduler.Default)
                    from data in profileData.Take(1)
                    let month = SystemClock.NowDate
                    where data.IsProcessable
                    let current = data.CurrentMonth
                    where current.Month != month.Month
                    select (New:data with {CurrentMonth = month}, Old:data))
                       .AutoSubscribe(MakeBackup, ReportError);

            void MakeBackup((ProfileData New, ProfileData Old) obj)
            {
                var (newData, oldData) = obj;

                var oldEntrys = oldData.Entries.FindAll(pe => pe.Date == oldData.CurrentMonth).ToImmutableList();
                cache.RemoveKeys(oldEntrys.Select(oe => oe.Date));

                profileData.Value = newData;

                var originalFile = newData.FileName;
                var originalName = Path.GetFileName(newData.FileName);

                File.WriteAllText(
                    originalFile.Replace(originalName, $"{originalName}-{oldData.CurrentMonth:d}"), 
                    JsonConvert.SerializeObject(oldEntrys));
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
                                       from entry in this.ShowDialogAsync<CorrectionDialog, ProfileEntry?, ProfileEntry>(() => oldEntry.Entry)
                                       where entry != null
                                       select (New:entry, Old:oldEntry.Entry))
                                  .AutoSubscribe(e =>
                                                 {
                                                     cache!.AddOrUpdate(e.New);
                                                     var data = profileData.Value;
                                                     profileData.Value = data with {Entries = data.Entries.Replace(e.Old, e.New!)};

                                                 },ReportError))
                     .ThenRegister(nameof(Correct));

            #endregion

            #region Calculation

            

            #endregion

            void ReportError(Exception e)
                => dispatcher.InvokeAsync(() => SnackBarQueue.Value?.Enqueue($"Fehler: {e.GetType().Name}--{e.Message}"));
        }
    }

    public sealed class UiProfileEntry : ObservableObject
    {
        public ProfileEntry Entry { get; }


        public string? Date { get; set; }

        public string? Start { get; set; }

        public string? Finish { get; set; }

        public string? Hour { get; set; }

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
        }
    }
}