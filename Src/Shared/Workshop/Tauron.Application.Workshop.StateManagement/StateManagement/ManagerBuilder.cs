using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.Dispatcher;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public sealed class ManagerBuilder : IDispatcherConfigurable<ManagerBuilder>
{
    private readonly List<Func<IEffect>> _effects = new();
    private readonly List<Func<IMiddleware>> _middlewares = new();
    private readonly List<StateBuilderBase> _states = new();

    private Func<IStateDispatcherConfigurator> _dispatcherFunc = () => new DefaultStateDispatcher();

    private bool _sendBackSetting;

    internal ManagerBuilder(IDriverFactory driver) => Driver = driver;

    public IServiceProvider? ServiceProvider { get; set; }

    public IDriverFactory Driver { get; }

    public ManagerBuilder WithDispatcher(Func<IStateDispatcherConfigurator>? factory)
    {
        _dispatcherFunc = factory ?? (() => new DefaultStateDispatcher());

        return this;
    }

    public ManagerBuilder WithDispatcher(string name, Func<IStateDispatcherConfigurator>? factory) 
        => throw new NotSupportedException("Polled Dispatcher not Supported");

    public static RootManager CreateManager(IDriverFactory driverFactory, Action<ManagerBuilder> builder)
    {
        var managerBuilder = new ManagerBuilder(driverFactory);
        builder(managerBuilder);

        return managerBuilder.Build(null);
    }

    public IWorkspaceMapBuilder<TData> WithWorkspace<TData>(Func<WorkspaceBase<TData>> source)
        where TData : class
    {
        var builder = new WorkspaceMapBuilder<TData>(source);
        _states.Add(builder);

        return builder;
    }

    public bool StateRegistrated<TData>(Type state) where TData : class, IStateEntity
        => _states.OfType<StateBuilder<TData>>().Any(b => b.State == state);

    public IStateBuilder<TData> WithDataSource<TData>(Func<IExtendedDataSource<TData>> source)
        where TData : class, IStateEntity
    {
        var builder = new StateBuilder<TData>(source);
        _states.Add(builder);

        return builder;
    }

    public ManagerBuilder WithDefaultSendback(bool flag)
    {
        _sendBackSetting = flag;

        return this;
    }

    public ManagerBuilder WithMiddleware(Func<IMiddleware> middleware)
    {
        _middlewares.Add(middleware);

        return this;
    }

    public ManagerBuilder WithEffect(Func<IEffect> effect)
    {
        _effects.Add(effect);

        return this;
    }


    internal RootManager Build(ServiceOptions? serviceOptions)
    {
        List<IEffect> additionalEffects = new();
        List<IMiddleware> additionalMiddlewares = new();
        List<IStateInstanceFactory> stateInstanceFactories = new();

        if (ServiceProvider != null)
        {
            serviceOptions ??= new ServiceOptions();

            if (serviceOptions.ResolveEffects)
                additionalEffects.AddRange(ServiceProvider.GetRequiredService<IEnumerable<IEffect>>());
            if (serviceOptions.ResolveMiddleware)
                additionalMiddlewares.AddRange(ServiceProvider.GetRequiredService<IEnumerable<IMiddleware>>());
            if(serviceOptions.ResolveStateFactorys)
                stateInstanceFactories.AddRange(ServiceProvider.GetRequiredService<IEnumerable<IStateInstanceFactory>>());
        }

        if(stateInstanceFactories.Count == 0)
            stateInstanceFactories.AddRange(new IStateInstanceFactory []{ new ActivatorUtilitiesStateFactory(), new SimpleConstructorStateFactory() });
        
        var man = new RootManager(
            Driver,
            _dispatcherFunc(),
            _states,
            _effects.Select(e => e()).Concat(additionalEffects),
            _middlewares.Select(m => m()).Concat(additionalMiddlewares),
            _sendBackSetting,
            ServiceProvider,
            stateInstanceFactories.ToArray());

        man.PostInit();

        return man;
    }
}