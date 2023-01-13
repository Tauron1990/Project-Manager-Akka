namespace Tauron.Application.Akka.Redux.Extensions;

public delegate TSelect Selector<in TState, out TSelect>(TState toSelect);