namespace Tauron.Application.AkkaNode.Bootstrap.Console.IpcMessages
{
    public sealed record RegisterNewClient(string Id, string ServiceName);

    public sealed record KillNode;
}