using Akka.Cluster;

namespace Tauron.Application.Master.Commands.ServiceRegistry;

public sealed record RegisterService(string Name, UniqueAddress Address, ServiceType ServiceType);