namespace Tauron.Application.Akka.Redux.Extensions;

public delegate Task<TData> Reqester<TData>(CancellationToken token, TData input);