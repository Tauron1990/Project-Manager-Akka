namespace Stl.Fusion.AkkaBridge.Connector
{
    public sealed record TryMethodCall(string Name, object[]? Aguments);

    public sealed record MethodResponse(object? Response, bool Error);
}