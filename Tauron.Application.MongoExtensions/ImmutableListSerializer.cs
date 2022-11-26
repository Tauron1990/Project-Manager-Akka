using System.Collections.Immutable;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Tauron.Application.MongoExtensions;

public sealed class ImmutableListSerializer<TValue> : EnumerableInterfaceImplementerSerializerBase<ImmutableList<TValue>, TValue>
{
    #pragma warning disable MA0018
    public static void Register()
        #pragma warning restore MA0018
        => BsonSerializer.RegisterSerializer(new ImmutableListSerializer<TValue>());

    protected override object CreateAccumulator() =>
        ImmutableList.CreateBuilder<TValue>();

    protected override ImmutableList<TValue> FinalizeResult(object accumulator) =>
        ((ImmutableList<TValue>.Builder)accumulator).ToImmutable();
}