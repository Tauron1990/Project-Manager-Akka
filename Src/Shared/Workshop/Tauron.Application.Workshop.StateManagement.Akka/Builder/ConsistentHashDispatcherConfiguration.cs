using Akka.Actor;
using Akka.Routing;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Akka.Builder;

public sealed class ConsistentHashDispatcherConfiguration : DispatcherPoolConfigurationBase<IConsistentHashDispatcherPoolConfiguration>,
    IConsistentHashDispatcherPoolConfiguration
{
    private int? _vNotes;
    private ConsistentHashMapping _mapping = msg => msg.GetHashCode();
    
    public IConsistentHashDispatcherPoolConfiguration WithVirtualNodesFactor(int vnodes)
    {
        _vNotes = vnodes;

        return this;
    }

    public IConsistentHashDispatcherPoolConfiguration WithHashSelector(Func<object, object> selector)
    {
        _mapping = msg => selector(msg);

        return this;
    }

    public override IStateDispatcherConfigurator Create()
        => new ActualDispatcher(Instances, Resizer, SupervisorStrategy, Dispatcher, _vNotes, Custom, _mapping);

    private sealed class ActualDispatcher : IStateDispatcherConfigurator
    {
        private readonly Func<Props, Props>? _custom;
        private readonly ConsistentHashMapping _consistentHashMapping;
        private readonly string? _dispatcher;
        private readonly int _instances;
        private readonly Resizer? _resizer;
        private readonly SupervisorStrategy _supervisorStrategy;
        private readonly int? _vNotes;

        internal ActualDispatcher(
            int instances, Resizer? resizer, SupervisorStrategy supervisorStrategy,
            string? dispatcher, int? vNotes, Func<Props, Props>? custom, ConsistentHashMapping consistentHashMapping)
        {
            _instances = instances;
            _resizer = resizer;
            _supervisorStrategy = supervisorStrategy;
            _dispatcher = dispatcher;
            _vNotes = vNotes;
            _custom = custom;
            _consistentHashMapping = consistentHashMapping;
        }

        private Props Configurate(Props mutator)
        {
            var router = new ConsistentHashingPool(_instances)
               .WithSupervisorStrategy(_supervisorStrategy)
               .WithHashMapping(_consistentHashMapping);

            if (_resizer != null)
                router = router.WithResizer(_resizer);
            if (!string.IsNullOrWhiteSpace(_dispatcher))
                router = router.WithDispatcher(_dispatcher);
            if (_vNotes != null)
                router = router.WithVirtualNodesFactor(_vNotes.Value);

            mutator = mutator.WithRouter(router);

            return _custom != null ? _custom(mutator) : mutator;
        }
        
        public IDriverFactory Configurate(IDriverFactory factory)
            => AkkaDriverFactory.Get(factory).CustomMutator(Configurate);
    }
}