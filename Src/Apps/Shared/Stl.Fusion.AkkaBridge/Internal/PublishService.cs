using System;

namespace Stl.Fusion.AkkaBridge.Internal
{
    public sealed record PublishService(Type ServiceType, Func<object> Resolver);
}