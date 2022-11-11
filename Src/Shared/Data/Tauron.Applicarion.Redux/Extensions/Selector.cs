namespace Tauron.Applicarion.Redux.Extensions;

public delegate TSelect Selector<in TState, out TSelect>(TState toSelect);