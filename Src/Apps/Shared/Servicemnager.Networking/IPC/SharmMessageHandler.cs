using Servicemnager.Networking.Data;

namespace Servicemnager.Networking.IPC;

public delegate void SharmMessageHandler(NetworkMessage message, ulong messageId, in Client processsId);