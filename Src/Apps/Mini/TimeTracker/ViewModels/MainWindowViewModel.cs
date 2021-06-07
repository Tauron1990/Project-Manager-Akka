using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Data;
using Autofac;
using DynamicData;
using DynamicData.Alias;
using JetBrains.Annotations;
using MaterialDesignThemes.Wpf;
using Tauron;
using Tauron.Application;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;
using Tauron.ObservableExt;
using TimeTracker.Data;
using TimeTracker.Managers;
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

        public UIProperty<int> HoursAll { get; }

        public UIProperty<MonthState> CurrentState { get; }

        public UIPropertyBase? Come { get; }

        public UIPropertyBase? Go { get; }

        public UIPropertyBase? Correct { get; }

        public UIPropertyBase? AddEntry { get; }

        public UIProperty<int> Remaining { get; }

        public UIProperty<bool> IsProcessable { get; }

        public UIPropertyBase? Configurate { get; }

        public UIProperty<string> ProfileState { get; }

        public MainWindowViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, AppSettings settings, ITauronEnviroment enviroment, 
            ProfileManager profileManager, CalculationManager calculation, SystemClock clock, IEventAggregator aggregator)
            : base(lifetimeScope, dispatcher)
        {
            SnackBarQueue = RegisterProperty<SnackbarMessageQueue?>(nameof(SnackBarQueue));
            dispatcher.InvokeAsync(() => new SnackbarMessageQueue(TimeSpan.FromSeconds(10)))
                      .Subscribe(SnackBarQueue);

            CurrentProfile = RegisterProperty<string>(nameof(CurrentProfile));
            AllProfiles = this.RegisterUiCollection<string>(nameof(AllProfiles))
                              .BindToList(settings.AllProfiles, out var list);

            aggregator.ConsumeErrors().Subscribe(ReportError).DisposeWith(this);

            #region Profile Selection

            IsProcessable = RegisterProperty<bool>(nameof(IsProcessable));
            profileManager.IsProcessable.Subscribe(IsProcessable).DisposeWith(this);

            ProfileState = RegisterProperty<string>(nameof(ProfileState));
            profileManager.IsProcessable
                          .Select(b => b ? "geldaen" : "nicht geladen")
                          .AutoSubscribe(ProfileState, ReportError);

            var loadTrigger = new Subject<string>();

            (from newProfile in CurrentProfile
             where !list.Items.Contains(newProfile)
             select newProfile)
               .Throttle(TimeSpan.FromSeconds(5))
               .AutoSubscribe(s =>
                              {
                                  if (string.IsNullOrWhiteSpace(s)) return;

                                  settings.AllProfiles = settings.AllProfiles.Add(s);
                                  list.Add(s);
                                  loadTrigger.OnNext(s);
                              }, ReportError)
               .DisposeWith(this);

            (from profile in CurrentProfile
             where list.Items.Contains(profile)
             select profile)
               .Subscribe(loadTrigger)
               .DisposeWith(this);

            #endregion

            Configurate = NewCommad.WithCanExecute(profileManager.IsProcessable)
                                   .ThenFlow(obs => (from _ in obs
                                                     from res in this.ShowDialogAsync<ConfigurationDialog, Unit, ConfigurationManager>(() => profileManager.ConfigurationManager)
                                                     select res)
                                                .AutoSubscribe(ReportError))
                                   .ThenRegister(nameof(Configurate));

            profileManager.CreateFileLoadPipeline(loadTrigger).DisposeWith(this);
            
            HoursAll = RegisterProperty<int>(nameof(HoursAll));
            profileManager.ProcessableData.Select(pd => pd.AllHours).DistinctUntilChanged().Subscribe(HoursAll).DisposeWith(this);

            #region Entrys
            
            var isHere = false.ToRx().DisposeWith(this);

            ProfileEntries = this.RegisterUiCollection<UiProfileEntry>(nameof(ProfileEntries))
                                 .BindTo(profileManager.ConnectCache()
                                                       .Select(entry => new UiProfileEntry(entry, calculation, profileManager.ProcessableData, ReportError)));

            dispatcher.InvokeAsync(() =>
                                   {
                                       var view = (ListCollectionView)CollectionViewSource.GetDefaultView(ProfileEntries.Property.Value);
                                       view.CustomSort = Comparer<UiProfileEntry>.Default;
                                   });

            (from data in profileManager.ProcessableData.DistinctUntilChanged(pd => pd.Entries)
             where data.Entries.Count != 0
             select Unit.Default)
               .AutoSubscribe(_ => CheckHere(), ReportError)
               .DisposeWith(this);


            Come = NewCommad
                  .WithCanExecute(profileManager.IsProcessable)
                  .WithCanExecute(from here in isHere 
                                  select !here)
                  .ThenFlow(clock.NowDate,
                       obs => profileManager.Come(obs)
                                            .AutoSubscribe(b =>
                                                           {
                                                               if(b)
                                                                   CheckHere();
                                                               else
                                                                   SnackBarQueue.Value?.Enqueue("Tag Schon eingetragen");
                                                           }, ReportError))
                  .ThenRegister(nameof(Come));

            Go = NewCommad
                .WithCanExecute(profileManager.IsProcessable)
                .WithCanExecute(isHere)
                .ThenFlow(() => clock.NowDate,
                     obs => profileManager.Go(obs)
                                          .AutoSubscribe(b =>
                                                         {
                                                             if(b)
                                                                 CheckHere();
                                                             else
                                                                 SnackBarQueue.Value?.Enqueue("Tag nicht gefunden");
                                                         }, ReportError))
                .ThenRegister(nameof(Go));





            void CheckHere()
            {
                (from item in profileManager.Entries
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
                     .WithCanExecute(profileManager.IsProcessable)
                     .WithCanExecute(from entry in CurrentEntry
                                     select entry != null)
                     .ThenFlow(obs => (from _ in obs
                                       let oldEntry = CurrentEntry.Value
                                       where oldEntry != null
                                       from dialogResult in this.ShowDialogAsync<CorrectionDialog, CorrectionResult, ProfileEntry>(() => oldEntry.Entry)
                                       from result in dialogResult switch
                                       {
                                           UpdateCorrectionResult update => profileManager.UpdateEntry(update.Entry, oldEntry.Entry.Date),
                                           DeleteCorrectionResult delete => profileManager.DeleteEntry(delete.Key).Select(_ => string.Empty),
                                           _ => Observable.Return(string.Empty)
                                       }
                                       select result)
                                  .AutoSubscribe(s =>
                                                 {
                                                     if(string.IsNullOrWhiteSpace(s))
                                                         SnackBarQueue.Value?.Enqueue(s);
                                                     else
                                                        CheckHere();
                                                 }, aggregator.ReportError))
                     .ThenRegister(nameof(Correct));

            AddEntry = NewCommad
                      .WithCanExecute(IsProcessable)
                      .ThenFlow(obs => (from _ in obs 
                                        from data in profileManager.ProcessableData.Take(1)
                                        let parameter = new AddEntryParameter(data.Entries.Select(pe => pe.Value.Date.Day).ToHashSet(), data.CurrentMonth)
                                        from result in this.ShowDialogAsync<AddEntryDialog, AddEntryResult, AddEntryParameter>(() => parameter)
                                        from u in result switch
                                        {
                                            NewAddEntryResult entry => profileManager.AddEntry(entry.Entry),
                                            _ => Observable.Return(Unit.Default)
                                        }
                                        select u)
                                   .AutoSubscribe(_ => CheckHere(), ReportError))
                      .ThenRegister(nameof(AddEntry));

            #endregion

            #region Calculation

            Remaining = RegisterProperty<int>(nameof(Remaining));

            CurrentState = RegisterProperty<MonthState>(nameof(CurrentState))
               .WithDefaultValue(MonthState.Minus);

            calculation.AllHours.Select(ts => ts.Hours).Subscribe(HoursAll)
                       .DisposeWith(this);

            var calc = calculation.CalculationResult;

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

    public sealed class UiProfileEntry : ObservableObject, IComparable<UiProfileEntry>, IDisposable
    {
        private const string TimespanTemplate = @"hh\:mm";

        private string? _hour;
        private readonly IDisposable _subscription;

        public ProfileEntry Entry { get; }

        public string? Date { get; private set; }

        public string? Start { get; private set; }

        public string? Finish { get; private set; }

        public string? Hour
        {
            get => _hour;
            private set => SetProperty(ref _hour, value);
        }

        public bool IsValid { get; private set; }

        public UiProfileEntry(ProfileEntry entry, CalculationManager calculation, IObservable<ProfileData> data, Action<Exception> error)
        {
            Entry = entry;
            UpdateLabels();
            _subscription = calculation.GetEntryHourCalculator(entry, data)
                                       .AutoSubscribe(ts => Hour = ts.ToString(TimespanTemplate), error);
        }

        //public UiProfileEntry()
        //{
        //    Entry = new ProfileEntry(DateTime.Now.Date, TimeSpan.FromHours(8), TimeSpan.FromHours(16));
        //    UpdateLabels();
        //}

        private void UpdateLabels()
        {
            Date = Entry.Date.ToLocalTime().ToString("D");
            Start = Entry.Start?.ToString(TimespanTemplate);
            Finish = Entry.Finish?.ToString(TimespanTemplate);
            //Hour = (Entry.Finish - Entry.Start)?.ToString(TimespanTemplate);

            var start = Entry.Start;
            var finish = Entry.Finish;
            if (Entry.DayType != DayType.Normal)
                IsValid = true;
            else if (start != null && finish == null)
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