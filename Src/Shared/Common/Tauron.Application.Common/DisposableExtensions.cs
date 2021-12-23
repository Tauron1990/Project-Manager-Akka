using System;
using JetBrains.Annotations;
using Tauron.TAkka;

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
}