namespace Tauron.Application.Akka.Redux.Configuration;

public interface IEffect<TStata>
{
    Effect<TStata> Build();
}