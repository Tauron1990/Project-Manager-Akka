using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Features;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public interface IServiceRegistryActor : IFeatureActorRef<IServiceRegistryActor>
    {
        Task<RegisterServiceResponse> RegisterService(RegisterService service, TimeSpan timeout);
        Task<ResolveResponse> ResolveService(ResolveService service, TimeSpan timeout);

        void UnRegisterService(UnregisterService service);
    }
    
    public sealed class ServiceRegisterActorRef : FeatureActorRefBase<IServiceRegistryActor>, IServiceRegistryActor
    {
        public ServiceRegisterActorRef() : base("AkkaBridge-SerrviceRegistry") { }
        
        public Task<RegisterServiceResponse> RegisterService(RegisterService service, TimeSpan timeout)
            => Ask<RegisterServiceResponse>(service, timeout);

        public Task<ResolveResponse> ResolveService(ResolveService service, TimeSpan timeout)
            => Ask<ResolveResponse>(service, timeout);

        public void UnRegisterService(UnregisterService service)
            => Tell(service);
    }
}