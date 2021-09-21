using System.Collections.Immutable;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Tauron.Application.MongoExtensions
{
    public sealed class ImmutableListSerializer<TValue> : EnumerableInterfaceImplementerSerializerBase<ImmutableList<TValue>, TValue>
    {
        public static void Register()
            => BsonSerializer.RegisterSerializer(new ImmutableListSerializer<TValue>());

        protected override object CreateAccumulator() =>
            ImmutableList.CreateBuilder<TValue>();

        protected override ImmutableList<TValue> FinalizeResult(object accumulator) =>
            ((ImmutableList<TValue>.Builder)accumulator).ToImmutable();
    }
}