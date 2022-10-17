using System.Collections.Immutable;
using Akka.Cluster;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.ServiceRegistry;

public sealed record QueryRegistratedServices;

public sealed record QueryRegistratedService(UniqueAddress Address);

[PublicAPI]
public sealed record MemberAddress(int Uid, string Host, int? Port, string System, string Protocol, string UniqeAdress)
{
    public static MemberAddress Empty { get; } =
        new(0, string.Empty, null, string.Empty, string.Empty, string.Empty);

    public static MemberAddress From(UniqueAddress adress) => new(
        adress.Uid,
        adress.Address.Host,
        adress.Address.Port,
        adress.Address.System,
        adress.Address.Protocol,
        adress.ToString());

    public override string ToString() => UniqeAdress;

    public static bool operator ==(UniqueAddress address, MemberAddress memberAddress)
        => address.ToString() == memberAddress.UniqeAdress;

    public static bool operator !=(UniqueAddress address, MemberAddress memberAddress)
        => !(address == memberAddress);
}

public sealed record MemberService(ServiceName Name, MemberAddress Address, ServiceType ServiceType);

public sealed record QueryRegistratedServicesResponse(ImmutableList<MemberService> Services);

public sealed record QueryRegistratedServiceResponse(MemberService? Service);