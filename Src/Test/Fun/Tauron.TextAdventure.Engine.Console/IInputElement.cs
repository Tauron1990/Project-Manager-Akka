using Tauron.TextAdventure.Engine.Core;

namespace Tauron.TextAdventure.Engine.Console;

public interface IInputElement
{
    ValueTask<string> Execute(AssetManager manager, Action reRender);
}