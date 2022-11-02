namespace SpaceConqueror.States.Actors;

public sealed class PlayerPartyState : IPlayerParty
{
    public string PlayerPosition { get; set; } = "start";
}