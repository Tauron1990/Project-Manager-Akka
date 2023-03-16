using System;

namespace Tauron;

[PublicAPI]
public static class DelegateExtensions
{
    public static TDel? Combine<TDel>(this TDel? del1, TDel? del2)
        where TDel : Delegate
        => Delegate.Combine(del1, del2) as TDel;

    public static TDel? Remove<TDel>(this TDel? del1, TDel? del2)
        where TDel : Delegate
        => Delegate.Remove(del1, del2) as TDel;

    public static TransformFunc<TSource> Transform<TSource>(this Func<TSource> source) => new(source);

    public readonly struct TransformFunc<TSource>
    {
        private readonly Func<TSource> _source;

        public TransformFunc(Func<TSource> source) => _source = source;

        public Func<TNew> To<TNew>(Func<TSource, TNew> transform)
        {
            var source = _source;
            TNew Func() => transform(source());

            return Func;
        }
    }
}