namespace Tauron.Applicarion.Redux.Extensions;

public delegate Task<TData> Reqester<TData>(CancellationToken token, TData input);