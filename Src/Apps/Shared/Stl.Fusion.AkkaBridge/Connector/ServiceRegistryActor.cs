using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.DistributedData;
using Tauron.Features;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public sealed class ServiceRegistryActor : ActorFeatureBase<ServiceRegistryActor.State>
    {
        private const string SharedId = "4D114988-9827-40A4-879C-E6C77734BD15";
        
        public sealed record State(ImmutableDictionary<Type, IActorRef> CurrentlyHosted, DistributedData Data, Random Selector, Cluster Cluster);

        public static Func<IPreparedFeature> Factory()
        {
            IPreparedFeature _()
                => Feature.Create(() => new ServiceRegistryActor(), 
                    c => new State(ImmutableDictionary<Type, IActorRef>.Empty, DistributedData.Get(c.System),
                        new Random(), Cluster.Get(c.System)));

            return _;
        }

        protected override void ConfigImpl()
        {
            
        }

        private string ExtractServiceKey(Type keyInterface)
            => keyInterface.AssemblyQualifiedName ?? throw new InvalidOperationException("Service not found (Assembly Qualifind Name)");
        
        private async Task<IEnumerable<string>> GetServices(State currentState, Type keyInterface)
        {
            var key        = new ORMultiValueDictionaryKey<string, string>(SharedId);
            var response   = await currentState.Data.GetAsync(key);
            var serviceKey = ExtractServiceKey(keyInterface);

            return response.TryGetValue(serviceKey, out var set)
                       ? set
                       : Array.Empty<string>();
        }

        private async Task UpdateRegistry(
            State                                                                                         currentState,
            Func<Cluster, ORMultiValueDictionary<string, string>, ORMultiValueDictionary<string, string>> updater)
        {
            var key      = new ORMultiValueDictionaryKey<string, string>(SharedId);
            var response = await currentState.Data.GetAsync(key);
            await currentState.Data.UpdateAsync(key, updater(currentState.Cluster, response));
        }
    }
}