using System;
using System.Reactive.Disposables;
using JetBrains.Annotations;

namespace Tauron;

[PublicAPI]
public static class DisposableExtensions
{
    public static TValue DisposeWith<TValue>(this TValue value, IResourceHolder cd)
        where TValue : IDisposable
    {
        cd.AddResource(value);

        return value;
    }

    public static TValue DisposeWith<TValue>(this TValue value, SerialDisposable cd)
        where TValue : IDisposable
    {
        cd.Disposable = value;

        return value;
    }
}