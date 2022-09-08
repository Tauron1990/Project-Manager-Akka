namespace Tauron.Applicarion.Redux.Configuration;

public interface IEffect<TStata>
{
    Effect<TStata> Build();
}