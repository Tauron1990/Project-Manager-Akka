using Tauron.Application.Master.Commands.ServiceRegistry;

namespace ServiceHost.ClientApp.Shared;

public static class ServiceTypes
{
    public static readonly ServiceType ServiceManager = new(nameof(ServiceManager), "Service Manager");

    public static readonly ServiceType Infrastructure = new(nameof(Infrastructure), "Cluster infrastruture");

    public static readonly ServiceType SeedNode = new(nameof(SeedNode), "Master Seed Node");

    public static readonly ServiceType ServideHost = new(nameof(ServideHost), "Service Host");
}