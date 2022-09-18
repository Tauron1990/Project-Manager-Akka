
using Akkatecture.Core;

namespace Tauron.Application.Akka.Redux.Extensions.Cache;

public sealed class CacheTimeoutId : Identity<CacheTimeoutId>
{
    private static readonly Guid Namespace = new("99956BD0-600D-4413-B7D6-C133B982D95D");
    public CacheTimeoutId(string value) : base(value) { }

    public static CacheTimeoutId FromCacheId(CacheDataId id)
        => NewDeterministic(Namespace, id.Value);
}

public sealed class CacheDataId : Identity<CacheDataId>
{
    private static readonly Guid Namespace = new("C862EEDA-15DB-4FA2-978C-D9C03CFD8194");
    public CacheDataId(string value) : base(value) { }

    public static CacheDataId FromType(Type type)
        => NewDeterministic(Namespace, type.AssemblyQualifiedName ?? throw new InvalidOperationException($"{nameof(type.AssemblyQualifiedName)} was null"));
}