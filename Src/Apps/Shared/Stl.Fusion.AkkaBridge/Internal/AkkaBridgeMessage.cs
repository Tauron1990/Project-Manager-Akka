using Stl.Fusion.Bridge.Messages;

namespace Stl.Fusion.AkkaBridge.Internal
{
    public sealed record AkkaBridgeMessage(bool KillClient, BridgeMessage? Message);
}