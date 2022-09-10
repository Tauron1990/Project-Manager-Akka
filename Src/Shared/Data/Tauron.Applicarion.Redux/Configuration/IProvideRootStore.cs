namespace Tauron.Applicarion.Redux.Configuration;

public interface IProvideRootStore
{
    void StoreCreated(IRootStore dispatcher);
}