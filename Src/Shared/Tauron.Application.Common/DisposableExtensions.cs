using System;
using System.Reactive.Disposables;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron
{
    [PublicAPI]
    public static class DisposableExtensions
    {
        public static TValue DisposeWith<TValue>(this TValue value, CompositeDisposable cd)
            where TValue : IDisposable
        {
            cd.Add(value);
            return value;
        }

        public static TValue DisposeWith<TValue>(this TValue value, IResourceHolder cd)
            where TValue : IDisposable
        {
            cd.AddResource(value);
            return value;
        }
    }
}