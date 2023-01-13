using Akka.Cluster;

namespace Tauron.Application.Master.Commands.ServiceRegistry;

public sealed record QueryRegistratedService(UniqueAddress Address);