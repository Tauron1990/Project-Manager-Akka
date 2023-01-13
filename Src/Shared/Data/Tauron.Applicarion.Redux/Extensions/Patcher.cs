namespace Tauron.Applicarion.Redux.Extensions;

public delegate TState Patcher<in TData, TState>(TData data, TState state);