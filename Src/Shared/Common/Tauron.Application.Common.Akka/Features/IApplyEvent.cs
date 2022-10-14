namespace Tauron.Features;

public interface IApplyEvent<out TState, in TEvent>
{
    TState Apply(TEvent @event);
}