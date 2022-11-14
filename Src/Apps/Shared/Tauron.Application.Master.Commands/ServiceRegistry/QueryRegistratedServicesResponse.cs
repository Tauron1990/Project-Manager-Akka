using System.Collections.Immutable;

namespace Tauron.Application.Master.Commands.ServiceRegistry;

public sealed record QueryRegistratedServicesResponse(ImmutableList<MemberService> Services);