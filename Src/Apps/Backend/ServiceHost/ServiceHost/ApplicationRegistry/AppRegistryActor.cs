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
using Akka.Cluster;
using Newtonsoft.Json;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceHost.Services;
using Tauron;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Master.Commands.Administration.Configuration;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Features;
using Tauron.ObservableExt;
using Tauron.Operations;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class AppRegistryActor : ActorFeatureBase<AppRegistryActor.RegistryState>
    {
        private const string BaseFileName = "apps.dat";
        private const string AppFileExt = ".app";

        public static Func<AppNodeInfo, IAppManager, AppNodeInfo, IEnumerable<IPreparedFeature>> New()
        {
            static IEnumerable<IPreparedFeature> _(AppNodeInfo configuration, IAppManager manager, AppNodeInfo info)
            {
                yield return SubscribeFeature.New();
                yield return Feature.Create(
                    () => new AppRegistryActor(),
                    new RegistryState(
                        ImmutableDictionary<string, string>.Empty,
                        Path.GetFullPath(configuration.AppsLocation),
                        ImmutableDictionary<Guid, IActorRef>.Empty,
                        manager,
                        info));
            }

            return _;
        }


        protected override void ConfigImpl()
        {
            Receive<CompledRestart>(obs => obs.ToUnit(Cluster.Get(Context.System).LeaveAsync));

            Receive<AllAppsQuery>(
                obs =>
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

                                        return apps.Add(
                                            new HostApp(
                                                app.SoftwareName,
                                                app.Name,
                                                app.Path,
                                                app.Version,
                                                app.AppType,
                                                app.Exe,
                                                i.Event.Apps.GetValueOrDefault(appKey, false)));
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Error(e, "Error on Load Installed Apps");

                                        return apps;
                                    }
                                })
                        let newState = i.State with { OnGoingQuerys = ongoing }
                        select (Event: new HostAppsResponse(apps, Success: true), State: newState, Sender: sender)
                    ).Do(i => i.Sender.Tell(i.Event))
                   .Select(e => e.State));

            Receive<QueryHostApps>(
                obs => from i in obs
                       let id = Guid.NewGuid()
                       let ongoing = i.State.OnGoingQuerys.Add(id, Sender)
                       let manager = i.State.Manager.Tell(new QueryAppStaus(id))
                       select i.State with { Manager = manager, OnGoingQuerys = ongoing });

            Receive<UpdateRegistrationRequest>(
                obs => obs.Do(m => Log.Info("Update Registraion for {Apps}", m.Event.Name))
                   .SelectMany(
                        m => from request in Observable.Return(m)
                             from app in m.State.Apps.Lookup(request.Event.Name)
                             select m.NewEvent((Request: request.Event, App: app)))
                   .ConditionalSelect()
                   .ToResult<StatePair<RegistrationResponse, RegistryState>>(
                        b =>
                        {
                            b.When(
                                m => m.Event.App.IsEmpty,
                                o => o.Do(m => Log.Warning("No Registration Found {Apps}", m.Event.Request.Name))
                                   .Select(m => m.NewEvent(new RegistrationResponse(Scceeded: true, Error: null))));

                            b.When(
                                m => m.Event.App.HasValue,
                                o =>
                                (
                                    from i in o
                                    let newData = JsonConvert.DeserializeObject<InstalledApp>(File.ReadAllText(i.Event.App.Value))?.NewVersion()
                                    let serializedData = JsonConvert.SerializeObject(newData)
                                    select (Data: serializedData, File: i.Event.App, State: i)
                                ).SelectMany(
                                    d => Observable.Return(d)
                                       .SelectSafe(
                                            s =>
                                            {
                                                if (string.IsNullOrWhiteSpace(s.Data))
                                                    return new RegistrationResponse(Scceeded: false, new InvalidOperationException("No App Data Found"));

                                                File.WriteAllText(s.File.Value, s.Data);

                                                return new RegistrationResponse(Scceeded: true, Error: null);
                                            })
                                       .ConvertResult(r => d.State.NewEvent(r), e => d.State.NewEvent(new RegistrationResponse(Scceeded: false, e)))));
                        })
                   .Do(
                        m =>
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
                                    b.When(
                                        m => m.State.Apps.ContainsKey(m.Event.Name),
                                        o => o.Do(m => Log.Warning("Attempt to Register Duplicate Application {Apps}", m.Event.Name))
                                           .Select(m => m.NewEvent(new RegistrationResponse(Scceeded: false, new InvalidOperationException("Duplicate")))));

                                    b.When(
                                        m => !m.State.Apps.ContainsKey(m.Event.Name),
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
                                           .Select(m => i.NewEvent(new RegistrationResponse(Scceeded: true, Error: null), i.State with { Apps = m.newApps }))
                                           .Catch<StatePair<RegistrationResponse, RegistryState>, Exception>(
                                                e => Observable.Return(i.NewEvent(new RegistrationResponse(Scceeded: false, e)))
                                                   .Do(_ => Log.Error(e, "Error while registration new Application {Apps}", i.Event.Name))));
                                }))
                   .Do(m => Sender.Tell(m.Event))
                   .Do(m => Self.Tell(SendEvent.Create(m.Event)))
                   .Select(m => m.State));

            Receive<InstalledAppQuery>(
                obs => obs.Do(m => Log.Info("Query Apps {Apps}", m.Event.Name))
                   .SelectSafe(m => new InstalledAppRespond(LoadApp(m.Event.Name, m.State) ?? InstalledApp.Empty, Fault: false))
                   .ConvertResult(
                        r => r,
                        e =>
                        {
                            Log.Error(e, "Error While Querining Apps Data");

                            return new InstalledAppRespond(InstalledApp.Empty, Fault: true);
                        })
                   .ToSender());

            Receive<SaveData>(
                obs => obs.SubscribeWithStatus(
                    i =>
                    {
                        try
                        {
                            string file = Path.Combine(i.State.AppsDirectory, BaseFileName);
                            using (var fileStream = new StreamWriter(File.Open(file, FileMode.Create)))
                            {
                                foreach (var (name, path) in i.State.Apps)
                                    fileStream.WriteLine($"{name}:{path}");
                            }

                            File.Copy(file, file + ".bak", overwrite: true);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Error while writing Apps Info");
                        }
                    }));

            Receive<LoadData>(
                obs => obs
                   .Select(
                        i =>
                        {
                            var (_, state) = i;
                            string file = Path.Combine(state.AppsDirectory, BaseFileName);

                            if (!File.Exists(file))
                                return state with { Apps = ImmutableDictionary<string, string>.Empty };

                            try
                            {
                                return state with { Apps = TryRead(file) };
                            }
                            catch (Exception e)
                            {
                                Log.Warning(e, "Error while Loading AppInfos");

                                try
                                {
                                    return state with { Apps = TryRead(file + ".bak") };
                                }
                                catch (Exception exception)
                                {
                                    Log.Error(exception, "Error while Loading AppInfos backup");

                                    return state with { Apps = ImmutableDictionary<string, string>.Empty };
                                }
                            }

                            ImmutableDictionary<string, string> TryRead(string actualFile)
                            {
                                return File.ReadAllLines(actualFile).Aggregate(
                                    ImmutableDictionary<string, string>.Empty,
                                    (apps, line) =>
                                    {
                                        var split = line.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

                                        return apps.Add(split[0].Trim(), split[1].Trim());
                                    });
                            }
                        }));

            InitConfigApi();

            Self.Tell(new LoadData());
        }

        private void InitConfigApi()
        {
            static async Task<Unit> SaveConfig(string data, string path)
            {
                await File.WriteAllTextAsync(path, data);

                return Unit.Default;
            }

            bool CheckFileExis(string file)
            {
                var result = File.Exists(file);

                if (!result)
                    Log.Warning("File Not Found {File}", file);

                return result;
            }

            IObservable<string> TryReadFile(string? fileName)
                => string.IsNullOrWhiteSpace(fileName)
                    ? Observable.Return(string.Empty)
                    : from name in Observable.Return(fileName)
                      where CheckFileExis(name)
                      from content in File.ReadAllTextAsync(name)
                      select content;

            IObservable<Unit> PatchApps<TEvent>(StatePair<TEvent, RegistryState> input, string fileName, Func<string, TEvent, IObservable<string>> patcher, bool read = true)
                => (from request in Observable.Return(input)
                    from appKey in CurrentState.Apps.Keys
                    let appData = LoadApp(appKey, request.State)
                    let file = read ? Path.Combine(appData.Path, fileName) : null
                    from configContent in TryReadFile(file)
                    from newConfig in patcher(configContent, request.Event)
                    from u in SaveConfig(newConfig, file)
                    select u)
                   .LastAsync();

            IObservable<Unit> PatchSelf<TEvent>(StatePair<TEvent, RegistryState> input, string fileName, Func<string, TEvent, IObservable<string>> patcher, bool read = true)
                => from request in Observable.Return(input)
                   let file = read ? Path.GetFullPath(fileName) : null
                   from configContent in TryReadFile(file)
                   from newConfig in patcher(configContent, request.Event)
                   from u in SaveConfig(newConfig, file)
                   select u;

            SharedApiCall<UpdateSeeds, UpdateSeedsResponse>(
                e => Log.Error(e, "Error on Update Seeds"),
                obs => from request in obs.ObserveOn(Scheduler.Default)
                       from u1 in PatchSelf(request.NewEvent(request.Event.Urls), AkkaConfigurationBuilder.Seed, AkkaConfigurationBuilder.PatchSeedUrls)
                       from u2 in PatchApps(request.NewEvent(request.Event.Urls), AkkaConfigurationBuilder.Seed, AkkaConfigurationBuilder.PatchSeedUrls)
                       select request.NewEvent(new UpdateSeedsResponse(Success: true)));

            var api = ConfigurationApi.CreateProxy(Context.System);

            IObservable<Unit> DownloadConfigForApps(IEnumerable<string> apps)
                => (from _ in Observable.Return(Unit.Default)
                    from appsKey in apps
                    let appData = LoadApp(appsKey, CurrentState)
                    let file = Path.Combine(appData.Path, AkkaConfigurationBuilder.Main)
                    from queryResult in api.Query<QueryFinalConfigData, FinalAppConfig>(new QueryFinalConfigData(appData.Name, appData.SoftwareName), new ApiParameter(TimeSpan.FromMinutes(1)))
                    let result = queryResult.GetOrThrow()
                    from u in string.IsNullOrWhiteSpace(result.Data)
                        ? Task.FromResult(Unit.Default)
                        : SaveConfig(result.Data, file)
                    select u
                    ).LastAsync();

            IObservable<Unit> DownloadConfigForSelf()
                => from _ in Observable.Return(Unit.Default)
                   from queryResult in api.Query<QueryFinalConfigData, FinalAppConfig>(new QueryFinalConfigData(CurrentState.Info.ApplicationName, "ServiceHost"), new ApiParameter(TimeSpan.FromMinutes(1)))
                   let result = queryResult.GetOrThrow()
                   let file = Path.GetFullPath(AkkaConfigurationBuilder.Main)
                   from u in string.IsNullOrWhiteSpace(result.Data)
                       ? Task.FromResult(Unit.Default)
                       : SaveConfig(result.Data, file)
                   select u;

            Unit FinalizeUpdate(UpdateEveryConfiguration request)
            {
                if (request.Restart)
                    Task.Delay(5000).PipeTo(Self, success: () => new CompledRestart());

                return Unit.Default;
            }

            SharedApiCall<UpdateEveryConfiguration, UpdateEveryConfigurationRespond>(
                e => Log.Error(e, "Error on Update Every Configuration"),
                obs => from request in obs
                       from s in DownloadConfigForSelf()
                       from a in DownloadConfigForApps(request.State.Apps.Keys)
                       let f = FinalizeUpdate(request.Event)
                       select request.NewEvent(new UpdateEveryConfigurationRespond(Success: true)));

            SharedApiCall<UpdateHostConfigCommand, UpdateHostConfigResponse>(
                e => Log.Error(e, "Error on Update Host Configuration"),
                obs => from request in obs
                       from _ in DownloadConfigForSelf()
                       select request.NewEvent(new UpdateHostConfigResponse(Success: true)));

            SharedApiCall<UpdateAppConfigCommand, UpdateAppConfigResponse>(
                e => Log.Error(e, "Error on Update App Configuration"),
                obs => from request in obs
                       from _ in DownloadConfigForApps(new[] { request.Event.App })
                       from u in Observable.Return(Unit.Default)
                          .ToUnit(
                               () =>
                               {
                                   if (request.Event.Restart)
                                       CurrentState.Manager.Tell(new RestartApp(request.Event.App));
                               })
                       select request.NewEvent(new UpdateAppConfigResponse(Success: true, request.Event.App)));
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

                            return Observable.Return(r.NewEvent((TResponse)r.Event.CreateDefaultFailed()));
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

        public sealed record RegistryState(ImmutableDictionary<string, string> Apps, string AppsDirectory, ImmutableDictionary<Guid, IActorRef> OnGoingQuerys, IAppManager Manager, AppNodeInfo Info);

        private sealed record LoadData;

        private sealed record SaveData;

        private sealed record CompledRestart;
    }
}