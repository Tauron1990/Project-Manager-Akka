using Tauron.Applicarion.Redux.Configuration;

namespace SimpleProjectManager.Client.Shared.Data.States;

public interface IStoreInitializer
{
    void RunConfig(IStoreConfiguration configuration);
}