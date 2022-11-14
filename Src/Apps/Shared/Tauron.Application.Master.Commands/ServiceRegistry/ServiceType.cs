using System;

namespace Tauron.Application.Master.Commands.ServiceRegistry;

public sealed class ServiceType : IEquatable<ServiceType>
{
    public static readonly ServiceType Empty = new(string.Empty, string.Empty);

    public ServiceType(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public string Id { get; }
    public string DisplayName { get; }

    public bool Equals(ServiceType? other)
    {
        if(ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || string.Equals(Id, other.Id, StringComparison.Ordinal);

    }

    public void Deconstruct(out string id, out string displayName)
    {
        id = Id;
        displayName = DisplayName;
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is ServiceType other && Equals(other));

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Id);

    public static bool operator ==(ServiceType? left, ServiceType? right) => Equals(left, right);

    public static bool operator !=(ServiceType? left, ServiceType? right) => !Equals(left, right);
}