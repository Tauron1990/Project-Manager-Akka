using NRules.Extensibility;
using SpaceConqueror.Core;
using SpaceConqueror.Modules;
using SpaceConqueror.States;

namespace SpaceConqueror;

public sealed class GameDependencyRsolver : IDependencyResolver
{
    private readonly GameManager _gameManager;

    public GameDependencyRsolver(GameManager gameManager)
        => _gameManager = gameManager;

    public object Resolve(IResolutionContext context, Type serviceType)
    {
        if(serviceType == typeof(GameManager))
            return _gameManager;

        if(serviceType == typeof(AssetManager))
            return GameManager.AssetManager;

        if(serviceType == typeof(GlobalState))
            return _gameManager.State;

        if(serviceType == typeof(ModuleManager))
            return _gameManager.ModuleManager;

        return null!;
    }
}