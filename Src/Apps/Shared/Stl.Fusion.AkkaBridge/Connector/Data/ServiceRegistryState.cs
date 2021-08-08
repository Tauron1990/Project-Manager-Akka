using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Akka.Cluster;

namespace Stl.Fusion.AkkaBridge.Connector.Data
{
    public sealed record ServiceRegistryState(
        ImmutableDictionary<IActorRef, Type> CurrentlyHosted, ImmutableDictionary<string, ImmutableHashSet<string>> Registry,
        ImmutableList<IRegistryOperation> ToApply, ImmutableList<IActorRef> Others, Cluster Cluster)
    {
        public static ServiceRegistryState Create(Cluster cluster) => new(
            ImmutableDictionary<IActorRef, Type>.Empty, 
            ImmutableDictionary<string, ImmutableHashSet<string>>.Empty, 
            ImmutableList<IRegistryOperation>.Empty,
            ImmutableList<IActorRef>.Empty, cluster);

        public ServiceRegistryState NewService(IActorRef actor, Type serviceType)
        {
            var newList = CurrentlyHosted.Add(actor, serviceType);
            var op = new AddOperation(ExtractServiceKey(serviceType), actor.Path.ToStringWithAddress(Cluster.SelfAddress));
            var newOperations = ToApply.Add(op);
            Others.ForEach(r => r.Tell(op));

            return op.Apply(this) with { CurrentlyHosted = newList, ToApply = newOperations };
        }

        public ServiceRegistryState RemoveService(IActorRef actor)
        {
            if (!CurrentlyHosted.TryGetValue(actor, out var serviceType)) return this;

            var newList = CurrentlyHosted.Remove(actor);
            var op = new RemoveOperation(ExtractServiceKey(serviceType), actor.Path.ToStringWithAddress(Cluster.SelfAddress));
            var newOperations = RemoveAdd(op);
            Others.ForEach(r => r.Tell(op));

            return op.Apply(this) with { CurrentlyHosted = newList, ToApply = newOperations };
        }

        public ServiceRegistryState AddNewRemote(IActorRef other)
        {
            other.Tell(new MultiUpdateOperation(ToApply));

            return this with { Others = Others.Add(other) };
        }

        public ServiceRegistryState RemoveRemote(IActorRef other)
            => this with { Others = Others.Remove(other) };

        public ServiceRegistryState ApplyOperation(IRegistryOperation operation)
            => operation.Apply(this);

        public IImmutableSet<string> GetServices(Type serviceInterface)
            => Registry.TryGetValue(ExtractServiceKey(serviceInterface), out var list) ? list : ImmutableHashSet<string>.Empty;


        private ImmutableList<IRegistryOperation> RemoveAdd(RemoveOperation op)
        {
            var (serviceKey, actorPath) = op;

            return ToApply.Remove(ToApply.OfType<AddOperation>().First(ro => ro.ServiceKey == serviceKey && ro.ActorPath == actorPath));
        }

        private static string ExtractServiceKey(Type keyInterface)
            => keyInterface.AssemblyQualifiedName ?? throw new InvalidOperationException("Service not found (Assembly Qualifind Name)");
    }
}