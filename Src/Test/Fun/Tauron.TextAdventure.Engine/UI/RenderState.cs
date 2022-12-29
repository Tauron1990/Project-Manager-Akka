using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

[PublicAPI]
public sealed class RenderState : IState
{
    public StateList<RenderElement> ToRender { get; } = new();

    public StateList<CommandPairBase> Commands { get; } = new();

    public void Write(BinaryWriter writer) { }

    public void Read(BinaryReader reader) { }
}