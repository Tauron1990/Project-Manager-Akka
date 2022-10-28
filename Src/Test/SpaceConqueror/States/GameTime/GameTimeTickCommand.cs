namespace SpaceConqueror.States.GameTime;

public sealed record GameTimeTickCommand : IGameCommand
{
    public static readonly GameTimeTickCommand Inst = new();
}