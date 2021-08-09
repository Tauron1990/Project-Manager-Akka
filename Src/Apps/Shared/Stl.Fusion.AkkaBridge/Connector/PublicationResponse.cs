using Stl.Fusion.Bridge;

namespace Stl.Fusion.AkkaBridge.Connector
{
    public sealed record PublicationResponse(MethodResponse Response, PublicationStateInfo? StateInfo);
}