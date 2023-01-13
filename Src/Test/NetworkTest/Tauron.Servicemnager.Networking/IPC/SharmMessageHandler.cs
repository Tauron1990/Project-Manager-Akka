using Tauron.Servicemnager.Networking.Data;

namespace Tauron.Servicemnager.Networking.IPC;

public delegate void SharmMessageHandler(NetworkMessage message, ulong messageId, in Client processsId);