using Tauron.TextAdventure.Engine.Console;

namespace SpaceConqueror.UI;

public sealed class ConsoleUI : SpectreConsoleLayerBase<ConsoleUIVisitor>
{
    protected override ConsoleUIVisitor CreateForConsole() 
        => new();

    protected override string ExecutePage(ConsoleUIVisitor visitor)
    {
        return string.Empty;
    }
}