using Akkatecture.Core;

namespace Tauron.Application.Akka.Redux.Extensions.Cache;

#pragma warning disable MA0097
public sealed class CacheDataId : Identity<CacheDataId>
    #pragma warning restore MA0097
{
    private static readonly Guid Namespace = new("C862EEDA-15DB-4FA2-978C-D9C03CFD8194");
    public CacheDataId(string value) : base(value) { }

    public static CacheDataId FromType(Type type)
        => NewDeterministic(Namespace, type.AssemblyQualifiedName ?? throw new InvalidOperationException($"{nameof(type.AssemblyQualifiedName)} was null"));
}