using Akka.Cluster;

namespace Tauron.Application.Master.Commands.ServiceRegistry;

public sealed record RegisterService(ServiceName Name, UniqueAddress Address, ServiceType ServiceType);