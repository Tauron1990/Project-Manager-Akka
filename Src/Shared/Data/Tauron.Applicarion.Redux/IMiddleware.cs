namespace Tauron.Applicarion.Redux;

public interface IMiddleware
{
    void Init(IRootStore rootStore);

    IObservable<object> Connect(IRootStore actionObservable);
}