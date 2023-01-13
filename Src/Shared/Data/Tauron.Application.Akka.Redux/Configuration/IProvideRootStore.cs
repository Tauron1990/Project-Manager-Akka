namespace Tauron.Application.Akka.Redux.Configuration;

public interface IProvideRootStore
{
    void StoreCreated(IRootStore dispatcher);
}