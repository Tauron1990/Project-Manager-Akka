namespace Stl.Fusion.AkkaBridge.Connector.Data
{
    public interface IRegistryOperation
    {
        ServiceRegistryState Apply(ServiceRegistryState state);
    }
}