namespace Tauron.Application.Blazor.Parameters;

public interface IParameterUpdateable
{
    ParameterUpdater Updater { get; }
}