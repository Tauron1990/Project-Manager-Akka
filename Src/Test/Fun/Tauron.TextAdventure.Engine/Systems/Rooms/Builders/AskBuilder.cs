using Cottle;
using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.Systems.Rooms.Core;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

[PublicAPI]
public sealed class AskBuilder : RoomBuilderBase
{
    private Func<AssetManager, RenderElement?>? _description;
    private string? _label;
    private Func<string, IEnumerable<IGameCommand>>? _whenEntered;

    public AskBuilder WithDescription(Func<AssetManager, RenderElement?> element)
    {
        _description = element;

        return this;
    }

    public AskBuilder WithLabel(string label)
    {
        _label = label;
        return this;
    }

    public AskBuilder WithCommand(Func<string, IEnumerable<IGameCommand>> command)
    {
        _whenEntered = _whenEntered.Combine(command);
        return this;
    }
    
    protected internal override BaseRoom CreateRoom(AssetManager assetManager)
    {
        if(_whenEntered is null)
            throw new InvalidOperationException("No Command Provider");

        if(_description is null)
            throw new InvalidOperationException("No Description Provided");
        
        return new AskRoom(_description(assetManager), assetManager.GetString(_label ?? string.Empty, Context.Empty), _whenEntered);
    }
}