using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI.Internal;

public sealed class RunningGame
{
    private readonly EventManager _eventManager;
    private readonly IUILayer _uiLayer;
    private readonly IRenderVisitor _visitor;

    private StateList<CommandPairBase>? _currentCommands;
    private Mode _mode;

    public RunningGame(IServiceProvider serviceProvider)
    {
        _uiLayer = serviceProvider.GetRequiredService<IUILayer>();
        _visitor = _uiLayer.CreateForPage();
        _eventManager = serviceProvider.GetRequiredService<EventManager>();
    }

    internal async ValueTask RunGame()
    {
        CommandMenu mainMenu = CreateMainMenu();

        while (true)
        {
            _visitor.Visit(CreateDiplayUI(mainMenu));
            string? result = await _uiLayer.ExecutePage(_visitor).ConfigureAwait(false);

            switch (result)
            {
                case UiKeys.SaveGame:
                    _eventManager.Save();
                    break;
                case UiKeys.CloseRunningGame:
                    _eventManager.Save();
                    return;
                case null:
                    continue;
                default:
                    ProcessCommand(result);
                    break;
            }
        }
    }

    private void ProcessCommand(string command)
    {
        if(command is "null") return;
        if(_currentCommands is null) throw new InvalidOperationException("_currentCommands not Found");

        switch (_mode)
        {

            case Mode.Command:
                IGameCommand[] resultCommand =
                (
                    from pairBase in _currentCommands
                    let commandFunc = pairBase.Find(command)
                    where command is not null
                    from actualCommand in commandFunc()
                    select actualCommand
                ).ToArray();

                if(resultCommand.OfType<EndGameCommand>().Any())
                    return;

                _eventManager.SendCommand(new TickCommand(resultCommand));
                break;
            case Mode.Ask:
                AskCommand ask = _currentCommands.SelectMany(Unfold).OfType<AskCommand>().First();

                _eventManager.SendCommand(new TickCommand(ask.AskCompled(command).ToArray()));
                break;
            default:
                #pragma warning disable EX002
                throw new UnreachableException();
            #pragma warning restore EX002
        }

        IEnumerable<CommandPairBase> Unfold(CommandPairBase toUnfold)
        {
            if(toUnfold is CommandPairMenu menu)
                foreach (CommandPairBase pair in menu.Commands.SelectMany(Unfold))
                    yield return pair;
            else
                yield return toUnfold;
        }
    }

    [MemberNotNull(nameof(_currentCommands))]
    private RenderElement CreateDiplayUI(RenderElement mainMenu)
    {
        var renderState = _eventManager.GameState.Get<RenderState>();
        _currentCommands = renderState.Commands;
        var element = MultiElement.Create(renderState.ToRender);

        element = MultiElement.Add(element, new SpacingElement { Amount = 3 });
        element = MultiElement.AddRange(element, renderState.Commands.Select(c => c.Collect()));
        element = MultiElement.Add(element, mainMenu);

        _mode = _currentCommands.Any(c => c.IsAsk) ? Mode.Ask : Mode.Command;

        return element;
    }

    private CommandMenu CreateMainMenu()
        => new(
            UiKeys.GameCoreMenu,
            new CommandItem(UiKeys.SaveGame),
            new CommandItem(UiKeys.CloseRunningGame)
        );

    private enum Mode
    {
        Command,
        Ask,
    }
}