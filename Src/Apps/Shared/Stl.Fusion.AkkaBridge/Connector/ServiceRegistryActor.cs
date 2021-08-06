using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.DistributedData;
using Tauron;
using Tauron.Features;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public sealed class ServiceRegistryActor : ActorFeatureBase<ServiceRegistryActor.State>
    {
        private const string SharedId = "4D114988-9827-40A4-879C-E6C77734BD15";
        
        public sealed record State(ImmutableDictionary<IActorRef, Type> CurrentlyHosted, DistributedData Data, Random Selector, Cluster Cluster, ORMultiValueDictionaryKey<string, string> RegistryKey);

        public static Func<IPreparedFeature> Factory()
        {
            IPreparedFeature _()
                => Feature.Create(() => new ServiceRegistryActor(), 
                    c => new State(ImmutableDictionary<IActorRef, Type>.Empty, DistributedData.Get(c.System),
                        new Random(), Cluster.Get(c.System), new ORMultiValueDictionaryKey<string, string>(SharedId)));

            return _;
        }

        protected override void ConfigImpl()
        {
            (Func<State, State> Updater, T Response) CreateResponse<T>(T response, Func<State, State> updater)
                => (updater, response);

            Receive<RegisterService>(
                obs => from state in obs.CatchSafe(
                                             pair => from p in Observable.Return(pair)
                                                     let state = p.State
                                                     let evt = p.Event
                                                     from registration in UpdateRegistry(
                                                         state,
                                                         (cluster, dictionary) => dictionary.AddItem(cluster, ExtractServiceKey(evt.Interface), evt.Host.Path.ToStringWithAddress(cluster.SelfAddress)))
                                                     select CreateResponse(
                                                         p.NewEvent(new RegisterServiceResponse(null)),
                                                         s => s with { CurrentlyHosted = s.CurrentlyHosted.Add(Context.Watch(evt.Host), evt.Interface) }),

                                             (pair, e) => Observable.Return(CreateResponse(pair.NewEvent(new RegisterServiceResponse(e)), s => s))
                                                                    .Do(p => Log.Error(p.Response.Event.Error, "Error on Register Service")))
                                        .Do(p => p.Response.Sender.Tell(p.Response.Event))

                       from toUpdate in UpdateAndSyncActor(state)
                       select state.Updater(toUpdate.State));

            IObservable<State> CreateUnregister(IObservable<StatePair<IActorRef, State>> obs)
                => from pair in obs
                   where pair.State.CurrentlyHosted.ContainsKey(pair.Event)
                   let path = pair.Event.Path
                   let entry = pair.State.CurrentlyHosted[pair.Event]
                   from update in UpdateRegistry(pair.State, (cluster, dictionary) => dictionary.RemoveItem(cluster, ExtractServiceKey(entry), path.ToStringWithAddress(cluster.SelfAddress)))
                   from newPair in UpdateAndSyncActor(pair)
                   select newPair.State with { CurrentlyHosted = newPair.State.CurrentlyHosted.Remove(pair.Event) };

            Receive<Terminated>(obs => CreateUnregister(obs.Select(p => p.NewEvent(p.Event.ActorRef))));
            Receive<UnregisterService>(obs => CreateUnregister(obs.Select(p => p.NewEvent(p.Event.Host))));

            Receive<ResolveService>(
                obs => obs.CatchSafe(
                               p => from pair in Observable.Return(p)
                                    from actors in GetServices(pair.State, pair.Event.Interface)
                                    from actor in TryResolve(pair.Context, actors)
                                    select pair.NewEvent(new ResolveResponse(actor, null)),
                               (p, err) => Observable.Return(p.NewEvent(new ResolveResponse(Nobody.Instance, err))))
                          .ToUnit(p => p.Sender.Tell(p.Event)));
        }

        private static string ExtractServiceKey(Type keyInterface)
            => keyInterface.AssemblyQualifiedName ?? throw new InvalidOperationException("Service not found (Assembly Qualifind Name)");

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
                         t =>
                         {
                             if (selector.Task.IsCompleted) return;

                             selector.TrySetException(new InvalidOperationException("No Service Chould be Resolved"));
                         });
            }

            return selector.Task;
        }

        private async Task<IImmutableSet<string>> GetServices(State currentState, Type keyInterface)
        {
            var response   = await currentState.Data.GetAsync(currentState.RegistryKey);
            var serviceKey = ExtractServiceKey(keyInterface);

            return response != null && response.TryGetValue(serviceKey, out var set)
                       ? set
                       : ImmutableHashSet<string>.Empty;
        }

        private async Task<Unit> UpdateRegistry(
            State                                                                                         currentState,
            Func<Cluster, ORMultiValueDictionary<string, string>, ORMultiValueDictionary<string, string>> updater)
        {
            var response = await currentState.Data.GetAsync(currentState.RegistryKey) ?? ORMultiValueDictionary<string, string>.Empty;
            await currentState.Data.UpdateAsync(currentState.RegistryKey, updater(currentState.Cluster, response));
            
            return Unit.Default;
        }
    }
}