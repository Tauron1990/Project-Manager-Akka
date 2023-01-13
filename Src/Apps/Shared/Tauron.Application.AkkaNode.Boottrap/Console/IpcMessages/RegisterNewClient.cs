using Tauron.Servicemnager.Networking.IPC;

namespace Tauron.Application.AkkaNode.Bootstrap.IpcMessages;

public sealed record RegisterNewClient(SharmProcessId Id, string ServiceName);