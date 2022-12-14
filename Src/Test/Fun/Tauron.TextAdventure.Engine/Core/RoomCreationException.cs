using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.Core;

[PublicAPI]
public sealed class RoomCreationException : Exception
{
    public string RoomName { get; }

    public RoomCreationException(string roomName, Exception error)
        : base($"Error on Create {roomName}", error)
        => RoomName = roomName;
}