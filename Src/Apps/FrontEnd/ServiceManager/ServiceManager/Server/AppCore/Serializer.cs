using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Serializers;

namespace ServiceManager.Server.AppCore
{

    public class ImmutableListSerializer<TValue> : EnumerableInterfaceImplementerSerializerBase<ImmutableList<TValue>, TValue>
    {

        protected override object CreateAccumulator() =>
            ImmutableList.CreateBuilder<TValue>();

        protected override ImmutableList<TValue> FinalizeResult(object accumulator) =>
            ((ImmutableList<TValue>.Builder)accumulator).ToImmutable();

    }
}
