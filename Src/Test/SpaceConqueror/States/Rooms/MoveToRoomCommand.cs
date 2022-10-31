using NRules.RuleModel;

namespace SpaceConqueror.States.Rooms;

public sealed record MoveToRoomCommand(string Name, object Context) : IGameCommand;