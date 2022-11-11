namespace Tauron.Applicarion.Redux;

public interface IActionDispatcher
{
    bool CanProcess<TAction>();

    bool CanProcess(Type type);

    IObservable<TAction> ObservAction<TAction>()
        where TAction : class;

    void Dispatch(object action);
}