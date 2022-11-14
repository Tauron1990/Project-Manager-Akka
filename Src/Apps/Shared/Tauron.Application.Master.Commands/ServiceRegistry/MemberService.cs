namespace Tauron.Application.Master.Commands.ServiceRegistry;

public sealed record MemberService(ServiceName Name, MemberAddress Address, ServiceType ServiceType);