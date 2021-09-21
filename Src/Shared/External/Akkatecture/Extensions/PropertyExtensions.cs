using System;
using System.Collections.Concurrent;
using Tauron;

namespace Akkatecture.Extensions
{
    public static class PropertyExtensions
    {
        private static readonly ConcurrentDictionary<CacheKey, Func<object?, object[], object?>> Propertys = new();


        public static object? GetPropertyValue(this object data, string name)
        {
            var key = new CacheKey(name, data.GetType());

            return Propertys.GetOrAdd(
                key,
                cacheKey =>
                {
                    var fac = FastReflection.Shared.GetPropertyAccessor(
                        cacheKey.Type.GetProperty(cacheKey.Name)
                     ?? throw new ArgumentNullException(nameof(name), "Property not Found"),
                        Array.Empty<Type>);

                    if (fac is null)
                        throw new InvalidOperationException("no Factory Created");

                    return fac;
                })(data, Array.Empty<object>());
        }

        public static TReturn GetPropertyValue<TReturn>(this object data, string name)
        {
            var key = new CacheKey(name, data.GetType());

            return (TReturn)Propertys.GetOrAdd(
                key,
                cacheKey =>
                {
                    var fac = FastReflection.Shared.GetPropertyAccessor(
                        cacheKey.Type.GetProperty(cacheKey.Name)
                     ?? throw new ArgumentNullException(nameof(name), "Property not Found"),
                        Array.Empty<Type>);

                    if (fac is null)
                        throw new InvalidOperationException("no Factory Created");

                    return fac;
                })(data, Array.Empty<object>())!;
        }

        private sealed class CacheKey : IEquatable<CacheKey>
        {
            internal CacheKey(string name, Type type)
            {
                Name = name;
                Type = type;
            }

            internal string Name { get; }

            internal Type Type { get; }

            public bool Equals(CacheKey? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;

                return Name == other.Name && Type == other.Type;
            }

            public override bool Equals(object? obj)
                => ReferenceEquals(this, obj) || obj is CacheKey other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Name, Type);

            public static bool operator ==(CacheKey? left, CacheKey? right) => Equals(left, right);

            public static bool operator !=(CacheKey? left, CacheKey? right) => !Equals(left, right);
        }
    }
}