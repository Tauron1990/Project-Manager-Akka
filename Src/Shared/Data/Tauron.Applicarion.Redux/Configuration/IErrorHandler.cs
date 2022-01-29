namespace Tauron.Applicarion.Redux.Configuration;

public interface IErrorHandler
{
    void RequestError(string error);

    void RequestError(Exception error);

    void StateDbError(Exception error);

    void TimeoutError(Exception error);
}