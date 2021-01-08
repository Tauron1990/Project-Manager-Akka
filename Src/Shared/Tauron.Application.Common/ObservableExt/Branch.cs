using System;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron.ObservableExt
{
    [PublicAPI]
    public static class BranchExtensions
    {
        public static IObservable<TSource> Branch<TSource>(this IObservable<TSource> observable, Action<BranchBuilder<TSource>> config)
        {
            var obs = observable.Publish().RefCount();
            config(new BranchBuilder<TSource>(obs));
            return obs;
        }

        public static BranchBuilder<TSource> Branch<TSource>(this IObservable<TSource> observable) => new(observable.Publish().RefCount());
    }

    [PublicAPI]
    public sealed class BranchBuilder<TSource>
    {
        private readonly IObservable<TSource> _target;

        public BranchBuilder(IObservable<TSource> target) => _target = target;
    }
}