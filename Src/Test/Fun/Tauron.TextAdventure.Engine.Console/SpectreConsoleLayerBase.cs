using Spectre.Console;
using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Console;

public abstract class SpectreConsoleLayerBase<TVisitor> : IUILayer
    where TVisitor : SpectreVisitor
{
    protected SpectreConsoleLayerBase(GameBase game)
        => System.Console.Title = game.AppName;

    public IRenderVisitor CreateForPage()
        => CreateForConsole();

    protected abstract TVisitor CreateForConsole();

    public ValueTask<string?> ExecutePage(IRenderVisitor visitor)
    {
        if(visitor is TVisitor spectreVisitor)
            return ExecutePage(spectreVisitor);

        throw new InvalidOperationException("The render Visitor is not Compatible with this UI Layer");
    }

    public abstract RenderElement CreateTitle();
    public void CriticalError(Exception exception)
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine("Schwerwigender Fehler");
        AnsiConsole.WriteLine();
        AnsiConsole.WriteException(exception);
        System.Console.ReadLine();
    }

    protected abstract ValueTask<string?> ExecutePage(TVisitor visitor);
}