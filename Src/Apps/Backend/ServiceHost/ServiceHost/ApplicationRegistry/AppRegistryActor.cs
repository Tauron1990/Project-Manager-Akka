using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServiceHost.Services;
using Tauron;
using Tauron.Application.Master.Commands.Administration.Configuration;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Features;
using Tauron.ObservableExt;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class AppRegistryActor : ActorFeatureBase<AppRegistryActor.RegistryState>
    {
        private const string BaseFileName = "apps.dat";
        private const string AppFileExt = ".app";

        public sealed record RegistryState(ImmutableDictionary<string, string> Apps, string AppsDirectory, ImmutableDictionary<Guid, IActorRef> OnGoingQuerys, IAppManager Manager);

        public static Func<IConfiguration, IAppManager, IEnumerable<IPreparedFeature>> New()
        {
            static IEnumerable<IPreparedFeature> _(IConfiguration configuration, IAppManager manager)
            {
                yield return SubscribeFeature.New();
                yield return Feature.Create(
                    () => new AppRegistryActor(),
                    new RegistryState(
                        ImmutableDictionary<string, string>.Empty,
                        Path.GetFullPath(configuration["AppsLocation"]),
                        ImmutableDictionary<Guid, IActorRef>.Empty,
                        manager));
            }
            
            return _;
        }



        protected override void ConfigImpl()
        {
            Receive<AllAppsQuery>(obs => 
                                  (
                                      from input in obs
                                      select new AllAppsResponse(input.State.Apps.Keys.ToArray())
                                  ).ToSender());

            Receive<AppStatusResponse>(
                obs =>
                (
                    from i in obs
                    let sender = i.State.OnGoingQuerys[i.Event.OpId]
                    let ongoing = i.State.OnGoingQuerys.Remove(i.Event.OpId)
                    let apps = i.State.Apps.Keys
                                .Aggregate(
                                     ImmutableList<HostApp>.Empty,
                                     (apps, appKey) =>
                                     {
                                         try
                                         {
                                             var app = LoadApp(appKey, i.State);
                                             if (app == null) return apps;

                                             return apps.Add(new HostApp(
                                                 app.SoftwareName, app.Name, app.Path, app.Version, app.AppType, app.Exe,
                                                 i.Event.Apps.GetValueOrDefault(appKey, false)));
                                         }
                                         catch (Exception e)
                                         {
                                             Log.Error(e, "Error on Load Installed Apps");
                                             return apps;
                                         }
                                     })
                    let newState = i.State with {OnGoingQuerys = ongoing}
                    select (Event:new HostAppsResponse(apps, true), State:newState, Sender:sender)
                ).Do(i => i.Sender.Tell(i.Event))
                 .Select(e => e.State));

            Receive<QueryHostApps>(
                obs => from i in obs
                       let id = Guid.NewGuid()
                       let ongoing = i.State.OnGoingQuerys.Add(id, Sender)
                       let manager = i.State.Manager.Tell(new QueryAppStaus(id))
                       select i.State with {Manager = manager, OnGoingQuerys = ongoing});

            Receive<UpdateRegistrationRequest>(
                obs => obs.Do(m => Log.Info("Update Registraion for {Apps}", m.Event.Name))
                          .SelectMany(m => from request in Observable.Return(m)
                                           from app in m.State.Apps.Lookup(request.Event.Name)
                                           select m.NewEvent((Request: request.Event, App: app)))
                          .ConditionalSelect()
                          .ToResult<StatePair<RegistrationResponse, RegistryState>>(
                               b =>
                               {
                                   b.When(m => m.Event.App.IsEmpty, o => o.Do(m => Log.Warning("No Registration Found {Apps}", m.Event.Request.Name))
                                                                          .Select(m => m.NewEvent(new RegistrationResponse(true, null))));

                                   b.When(m => m.Event.App.HasValue,
                                       o =>
                                       (
                                           from i in o
                                           let newData = JsonConvert.DeserializeObject<InstalledApp>(File.ReadAllText(i.Event.App.Value))?.NewVersion()
                                           let serializedData = JsonConvert.SerializeObject(newData)
                                           select (Data: serializedData, File: i.Event.App, State: i)
                                       ).SelectMany(d => Observable.Return(d)
                                                                   .SelectSafe(s =>
                                                                               {
                                                                                   if (string.IsNullOrWhiteSpace(s.Data))
                                                                                       return new RegistrationResponse(false, new InvalidOperationException("No App Data Found"));
                                                                                   File.WriteAllText(s.File.Value, s.Data);
                                                                                   return new RegistrationResponse(true, null);
                                                                               })
                                                                   .ConvertResult(r => d.State.NewEvent(r), e => d.State.NewEvent(new RegistrationResponse(false, e)))));
                               })
                          .Do(m =>
                              {
                                  var (evt, _) = m;
                                  Sender.Tell(evt);
                                  TellSelf(SendEvent.Create(evt));
                              })
                          .Select(m => m.State));

            Receive<NewRegistrationRequest>(
                obs => obs.Do(m => Log.Info("Register new Application {Apps}", m.Event.Name))
                          .SelectMany(
                               i => Observable.Return(i)
                                              .ConditionalSelect()
                                              .ToResult<StatePair<RegistrationResponse, RegistryState>>(
                                                   b =>
                                                   {
                                                       b.When(m => m.State.Apps.ContainsKey(m.Event.Name),
                                                           o => o.Do(m => Log.Warning("Attempt to Register Duplicate Application {Apps}", m.Event.Name))
                                                                 .Select(m => m.NewEvent(new RegistrationResponse(false, new InvalidOperationException("Duplicate")))));

                                                       b.When(m => !m.State.Apps.ContainsKey(m.Event.Name),
                                                           o =>
                                                               (
                                                                   from request in o.Select(e => e.Event)
                                                                   let fullPath = Path.GetFullPath(request.Path + AppFileExt)
                                                                   let data = JsonConvert.SerializeObject(
                                                                       new InstalledApp(request.SoftwareName, request.Name, request.Path, request.Version, request.AppType, request.ExeFile))
                                                                   let newApps = i.State.Apps.Add(request.Name, fullPath)
                                                                   select (fullPath, data, newApps, request)
                                                               ).Do(m => File.WriteAllText(m.fullPath, m.data))
                                                                .Do(_ => Self.Tell(new SaveData()))
                                                                .Do(m => Log.Info("Registration Compled for {Apps}", m.request.Name))
                                                                .Select(m => i.NewEvent(new RegistrationResponse(true, null), i.State with {Apps = m.newApps}))
                                                                .Catch<StatePair<RegistrationResponse, RegistryState>, Exception>(
                                                                     e => Observable.Return(i.NewEvent(new RegistrationResponse(false, e)))
                                                                                    .Do(_ => Log.Error(e, "Error while registration new Application {Apps}", i.Event.Name))));
                                                   }))
                          .Do(m => Sender.Tell(m.Event))
                          .Do(m => Self.Tell(SendEvent.Create(m.Event)))
                          .Select(m => m.State));

            Receive<InstalledAppQuery>(
                obs => obs.Do(m => Log.Info("Query Apps {Apps}", m.Event.Name))
                          .SelectSafe(m => new InstalledAppRespond(LoadApp(m.Event.Name, m.State) ?? InstalledApp.Empty, false))
                          .ConvertResult(r => r, e =>
                                                 {
                                                     Log.Error(e, "Error While Querining Apps Data");
                                                     return new InstalledAppRespond(InstalledApp.Empty, true);
                                                 })
                          .ToSender());

            Receive<SaveData>(
                obs => obs.SubscribeWithStatus(i =>
                                               {
                                                   try
                                                   {
                                                       string file = Path.Combine(i.State.AppsDirectory, BaseFileName);
                                                       using (var fileStream = new StreamWriter(File.Open(file, FileMode.Create)))
                                                       {
                                                           foreach (var (name, path) in i.State.Apps)
                                                               fileStream.WriteLine($"{name}:{path}");
                                                       }

                                                       File.Copy(file, file + ".bak", true);
                                                   }
                                                   catch (Exception e)
                                                   {
                                                       Log.Error(e, "Error while writing Apps Info");
                                                   }
                                               }));

            Receive<LoadData>(
                obs => obs
                      .Select(i =>
                                  {
                                      var (_, state) = i;
                                      string file = Path.Combine(state.AppsDirectory, BaseFileName);
                                      if (!File.Exists(file))
                                          return state with{Apps = ImmutableDictionary<string, string>.Empty};

                                      try
                                      {
                                          return state with{ Apps = TryRead(file)};
                                      }
                                      catch (Exception e)
                                      {
                                          Log.Warning(e, "Error while Loading AppInfos");

                                          try
                                          {
                                              return state with{Apps = TryRead(file + ".bak")};
                                          }
                                          catch (Exception exception)
                                          {
                                              Log.Error(exception, "Error while Loading AppInfos backup");
                                              return state with {Apps = ImmutableDictionary<string, string>.Empty};
                                          }
                                      }

                                      ImmutableDictionary<string, string> TryRead(string actualFile)
                                      {
                                          return File.ReadAllLines(actualFile).Aggregate(ImmutableDictionary<string, string>.Empty,
                                              (apps, line) =>
                                              {
                                                  var split = line.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

                                                  return apps.Add(split[0].Trim(), split[1].Trim());
                                              });
                                      }
                                  }));

            static async Task<Unit> SaveConfig(string data, string path)
            {
                await File.WriteAllTextAsync(path, data);

                return Unit.Default;
            }

            bool CheckFileExis(string file)
            {
                var result = File.Exists(file);

                if(!result)
                    Log.Warning("File Not Found {File}", file);

                return result;
            }

            IObservable<Unit> PatchApps<TEvent>(StatePair<TEvent, RegistryState> input, string fileName, Func<string, TEvent, string> patcher, bool read = true)
                => from request in Observable.Return(input)
                   from appKey in CurrentState.Apps.Keys
                   let appData = LoadApp(appKey, request.State)
                   let file = Path.Combine(appData.Path, fileName)
                   where CheckFileExis(file)
                   from configContent in File.ReadAllTextAsync(file)
                   let newConfig = patcher(configContent, request.Event)
                   from u in SaveConfig(newConfig, file)
                   select u;

            IObservable<Unit> PatchSelf<TEvent>(StatePair<TEvent, RegistryState> input, string fileName, Func<string, TEvent, string> patcher, bool read = true)
                => from request in Observable.Return(input)
                   let file = Path.GetFullPath(fileName)
                   where CheckFileExis(file)
                   from configContent in File.ReadAllTextAsync(file)
                   let newConfig = patcher(configContent, request.Event)
                   from u in SaveConfig(newConfig, file)
                   select u;

            SharedApiCall<UpdateSeeds, UpdateSeedsResponse>(e => Log.Error(e, "Error on Update Seeds"),
                obs => from request in obs.ObserveOn(Scheduler.Default)
                       from u1 in PatchSelf(request.NewEvent(request.Event.Urls), AkkaConfigurationBuilder.Seed, AkkaConfigurationBuilder.PatchSeedUrls)
                       from u2 in PatchApps(request.NewEvent(request.Event.Urls), AkkaConfigurationBuilder.Seed, AkkaConfigurationBuilder.PatchSeedUrls)
                       select request.NewEvent(new UpdateSeedsResponse(true)));

            //SharedApiCall<UpdateEveryConfiguration, UpdateEveryConfigurationRespond>(e => Log.Error(e, "Error on Update Every Configuration"),
            //    obs => from request in obs 
            //           );

            Self.Tell(new LoadData());
        }

        private void SharedApiCall<TEvent, TResponse>(Action<Exception> error, Func<IObservable<StatePair<TEvent, RegistryState>>, IObservable<StatePair<TResponse, RegistryState>>> processor)
            where TEvent : InternalHostMessages.CommandBase<TResponse>, InternalHostMessages.IHostApiCommand
            where TResponse : OperationResponse, new()
            => Receive<TEvent>(
                obs => obs.CatchSafe(
                               i => processor(Observable.Return(i)),
                               (r, e) =>
                               {
                                   error(e);
                                   return Observable.Return(r.NewEvent((TResponse) r.Event.CreateDefaultFailed()));
                               })
                          .ToUnit(r => r.Sender.Tell(r.Event)));

        private InstalledApp? LoadApp(string name, RegistryState state)
        {
            if (state.Apps.TryGetValue(name, out var path) && File.Exists(path))
            {
                var data = JsonConvert.DeserializeObject<InstalledApp>(path.ReadTextIfExis());
                Log.Info("Load Apps Data Compled {Apps}", name);
                return data;
            }

            Log.Info("No Apps Found {Apps}", name);
            return null;
        }

        private sealed record LoadData;

        private sealed record SaveData;
    }
}