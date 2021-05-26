﻿using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Autofac;
using DynamicData;
using JetBrains.Annotations;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Tauron;
using Tauron.Application;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;
using Tauron.ObservableExt;
using TimeTracker.Data;

namespace TimeTracker.ViewModels
{
    [UsedImplicitly]
    public sealed class MainWindowViewModel : UiActor
    {
        public UIProperty<SnackbarMessageQueue?> SnackBarQueue { get;  }

        public UIProperty<string> CurrentProfile { get; }

        public UICollectionProperty<string> AllProfiles { get; }

        public UIProperty<int> HoursMonth { get; }

        public UIProperty<int> HoursShort { get; }

        public UIProperty<int> HoursAll { get; }

        public MainWindowViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher, AppSettings settings, ITauronEnviroment enviroment) 
            : base(lifetimeScope, dispatcher)
        {
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
            var profileData = new ProfileData(string.Empty, 0, 0, 0).ToRx();

            (from newProfile in CurrentProfile
             where !list.Items.Contains(newProfile)
             select newProfile)
               .Throttle(TimeSpan.FromSeconds(10))
               .AutoSubscribe(s =>
                              {
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
             select new ProfileData(GetFileName(toLoad), 0, 0, 0))
               .Subscribe(profileData)
               .DisposeWith(this);

            (from data in profileData.DistinctUntilChanged(pd => pd.FileName)
             where data.IsProcessable && File.Exists(data.FileName)
             from fileContent in File.ReadAllTextAsync(data.FileName)
             let newData = JsonConvert.DeserializeObject<ProfileData>(fileContent)
             where newData != null
             select newData)
               .AutoSubscribe(profileData!, ReportError);

            (from data in profileData
             where data.IsProcessable
             select data)
               .ToUnit(pd => File.WriteAllTextAsync(pd.FileName, JsonConvert.SerializeObject(pd)))
               .AutoSubscribe(ReportError)
               .DisposeWith(this);

            #endregion

            var processData = profileData.Where(pd => pd.IsProcessable);
            processData.Select(pd => pd.AllHours).Subscribe(HoursAll).DisposeWith(this);
            processData.Select(pd => pd.MinusShortTimeHours).Subscribe(HoursShort).DisposeWith(this);
            processData.Select(pd => pd.MothHours).Subscribe(HoursMonth).DisposeWith(this);


            string GetFileName(string profile)
                => Path.Combine(
                    enviroment.AppData(),
                    GuidFactories.Deterministic.Create(fileNameBase, profile) + "json");

            void ReportError(Exception e) 
                => dispatcher.InvokeAsync(() => SnackBarQueue.Value?.Enqueue($"Fehler: {e.GetType().Name}--{e.Message}"));
        }

        public sealed record ProfileData(string FileName, int MothHours, int MinusShortTimeHours, int AllHours)
        {
            [JsonIgnore] 
            public bool IsProcessable => !string.IsNullOrWhiteSpace(FileName);
        };
    }
}