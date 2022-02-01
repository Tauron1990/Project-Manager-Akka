namespace Tauron.Applicarion.Redux.Configuration;

public interface IProvideActionDispatcher
{
    void StoreCreated(IActionDispatcher dispatcher);
}

public interface IProvideRootStore
{
    void StoreCreated(IRootStore dispatcher);
}