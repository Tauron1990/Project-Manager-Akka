using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using Stl.Fusion.AkkaBridge.Connector.Data;
using Tauron;
using Tauron.Features;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public sealed class ServiceRegistryActor : ActorFeatureBase<ServiceRegistryActor.State>
    {
        private const string SharedId = "4D114988-9827-40A4-879C-E6C77734BD15";
        
        public sealed record State(ClusterActorDiscovery Data, Cluster Cluster, ServiceRegistryState Registry);

        public static Func<IPreparedFeature> Factory()
        {
            IPreparedFeature _()
                => Feature.Create(() => new ServiceRegistryActor(), 
                    c => new State(ClusterActorDiscovery.Get(c.System), Cluster.Get(c.System), ServiceRegistryState.Create(Cluster.Get(c.System))));

            return _;
        }

        protected override void ConfigImpl()
        {
            static (State Updater, T Response) CreateResponse<T>(T response, State updater)
                => (updater, response);

            Receive<RegisterService>(
                obs => from state in obs.CatchSafe(
                                             pair => from p in Observable.Return(pair)
                                                     let state = p.State
                                                     let evt = p.Event
                                                     let registration = p.State.Registry.NewService(Context.Watch(p.Event.Host), p.Event.Interface)
                                                     select CreateResponse(
                                                         p.NewEvent(new RegisterServiceResponse(null)),
                                                         state with { Registry = registration }),

                                             (pair, e) => Observable.Return(CreateResponse(pair.NewEvent(new RegisterServiceResponse(e)), pair.State))
                                                                    .Do(p => Log.Error(p.Response.Event.Error, "Error on Register Service")))
                                        .Do(p => p.Response.Sender.Tell(p.Response.Event))
                                        
                       select state.Updater);

            IObservable<State> CreateUnregister(IObservable<StatePair<IActorRef, State>> obs)
                => from pair in obs
                   where pair.State.Registry.CurrentlyHosted.ContainsKey(pair.Event)
                   select pair.State with { Registry = pair.State.Registry.RemoveService(pair.Event)};

            Receive<Terminated>(obs => CreateUnregister(obs.Select(p => p.NewEvent(p.Event.ActorRef))));
            Receive<UnregisterService>(obs => CreateUnregister(obs.Select(p => p.NewEvent(p.Event.Host))));

            Receive<ResolveService>(
                obs => obs.CatchSafe(
                               p => from pair in Observable.Return(p)
                                    let actors = pair.State.Registry.GetServices(pair.Event.Interface)
                                    from actor in TryResolve(pair.Context, actors)
                                    select pair.NewEvent(new ResolveResponse(actor, null)),
                               (p, err) => Observable.Return(p.NewEvent(new ResolveResponse(Nobody.Instance, err))))
                          .ToUnit(p => p.Sender.Tell(p.Event)));

            ConfigRegistryOperations();
        }

        private void ConfigRegistryOperations()
        {
            Receive<IRegistryOperation>(
                obs => from pair in obs
                       select pair.State with { Registry = pair.State.Registry.ApplyOperation(pair.Event) });

            Receive<ClusterActorDiscoveryMessage.ActorDown>(
                obs => from pair in obs
                       where !pair.Event.Actor.Equals(Self)
                       select pair.State with { Registry = pair.State.Registry.RemoveRemote(pair.Event.Actor) });

            Receive<ClusterActorDiscoveryMessage.ActorUp>(
                obs => from pair in obs
                       where !pair.Event.Actor.Equals(Self)
                       select pair.State with { Registry = pair.State.Registry.AddNewRemote(pair.Event.Actor) });

            CurrentState.Data.MonitorActor(new ClusterActorDiscoveryMessage.MonitorActor(SharedId));
            CurrentState.Data.RegisterActor(new ClusterActorDiscoveryMessage.RegisterActor(Self, SharedId));
        }

        private Task<IActorRef> TryResolve(IActorRefFactory context, IImmutableSet<string> actors)
        {
            TaskCompletionSource<IActorRef> selector = new(TaskCreationOptions.RunContinuationsAsynchronously);

            if (actors.Count == 0)
                selector.SetException(new InvalidOperationException("No Actors Found in Shared Data"));
            else
            {
                Task.WhenAll(
                         actors.Select(context.ActorSelection)
                               .Select(
                                    s => s.ResolveOne(TimeSpan.FromSeconds(5))
                                          .ContinueWith(
                                               t =>
                                               {
                                                   if (t.IsCompletedSuccessfully)
                                                       selector.TrySetResult(t.Result);
                                                   else if (t.IsFaulted)
                                                   {
                                                       var err = t.Exception.Unwrap();

                                                       if (err is ActorNotFoundException) return;

                                                       Log.Error(err, "Error on Resolving Service Host");
                                                   }
                                               })))
                    .ContinueWith(
                         _ =>
                         {
                             if (selector.Task.IsCompleted) return;

                             selector.TrySetException(new InvalidOperationException("No Service Chould be Resolved"));
                         });
            }

            return selector.Task;
        }

        //private static async Task<IImmutableSet<string>> GetServices(State currentState, Type keyInterface)
        //{
        //    var response   = await currentState.Data.GetAsync(currentState.RegistryKey, ReadLocal.Instance);
        //    var serviceKey = ExtractServiceKey(keyInterface);

        //    return response != null
        //               ? response.Where(se => se.ServiceType == serviceKey).Select(d => d.ActorPath).ToImmutableHashSet()
        //               : ImmutableHashSet<string>.Empty;
        //}

        //private async Task<Unit> UpdateRegistry(
        //    State currentState,
        //    Func<Cluster, ORSet<ServiceEntry>, ORSet<ServiceEntry>> updater)
        //{
        //    var response = await currentState.Data.GetAsync(currentState.RegistryKey) ?? ORSet<ServiceEntry>.Empty;

        //    var newData = updater(currentState.Cluster, response);
        //    await currentState.Data.UpdateAsync(currentState.RegistryKey, newData, WriteLocal.Instance);

        //    return Unit.Default;
        //}
    }
}