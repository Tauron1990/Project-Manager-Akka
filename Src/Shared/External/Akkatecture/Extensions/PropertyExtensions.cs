using System;
using System.Collections.Concurrent;
using Tauron;

namespace Akkatecture.Extensions
{
    public static class PropertyExtensions
    {
        private static readonly ConcurrentDictionary<CacheKey, Func<object?, object[], object?>> _propertys = new();


        public static object? GetPropertyValue(this object data, string name)
        {
            var key = new CacheKey(name, data.GetType());
            return _propertys.GetOrAdd(key, c =>
            {
                var fac = FastReflection.Shared.GetPropertyAccessor(c.Type.GetProperty(c.Name) ?? throw new ArgumentNullException(nameof(name), "Property not Found"), Array.Empty<Type>);

                if (fac == null)
                    throw new InvalidOperationException("no Factory Created");

                return fac;
            })(data, Array.Empty<object>());
        }

        public static TReturn GetPropertyValue<TReturn>(this object data, string name)
        {
            var key = new CacheKey(name, data.GetType());
            return (TReturn) _propertys.GetOrAdd(key, c =>
            {
                var fac = FastReflection.Shared.GetPropertyAccessor(c.Type.GetProperty(c.Name) ?? throw new ArgumentNullException(nameof(name), "Property not Found"), Array.Empty<Type>);

                if (fac == null)
                    throw new InvalidOperationException("no Factory Created");

                return fac;
            })(data, Array.Empty<object>())!;
        }

        private sealed class CacheKey : IEquatable<CacheKey>
        {
            public CacheKey(string name, Type type)
            {
                Name = name;
                Type = type;
            }

            public string Name { get; }

            public Type Type { get; }

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