using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI.Internal;

public sealed class RunningGame
{
    private readonly IServiceProvider _serviceProvider;
    
    private readonly IUILayer _uiLayer;
    private readonly IRenderVisitor _visitor;
    private readonly EventManager _eventManager;
    
    public RunningGame(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _uiLayer = serviceProvider.GetRequiredService<IUILayer>();
        _visitor = _uiLayer.CreateForPage();
        _eventManager = serviceProvider.GetRequiredService<EventManager>();
    }

    internal ValueTask RunGame()
    {
        
    }

    private CommandMenu CreateMainMenu()
    {
        
    }
}