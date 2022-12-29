using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.Core;

[PublicAPI]
public sealed class RoomCreationException : Exception
{
    public RoomCreationException(string roomName, Exception error)
        : base($"Error on Create {roomName}", error)
        => RoomName = roomName;

    public string RoomName { get; }
}