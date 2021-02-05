using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron
{
    public abstract record CallResult<TResult>(bool IsOk);

    public sealed record ErrorCallResult<TResult>(Exception Error) : CallResult<TResult>(false);

    public sealed record SucessCallResult<TResult>(TResult Result) : CallResult<TResult>(true);

    [DebuggerNonUserCode]
    [PublicAPI]
    public static class EnumerableExtensions
    {
        public static IObservable<TData> NotDefault<TData>(this IObservable<TData?> source)
        {
            return source.Where(d => !Equals(d, default(TData)))!;
        }

        public static IObservable<TData> NotNull<TData>(this IObservable<TData?> source)
        {
            return source.Where(d => d != null)!;
        }

        public static IObservable<string> NotEmpty(this IObservable<string?> source)
        {
            return source.Where(s => !string.IsNullOrWhiteSpace(s))!;
        }

        public static IObservable<CallResult<TResult>> SelectSafe<TEvent, TResult>(this IObservable<TEvent> observable,
            Func<TEvent, TResult> selector)
        {
            return observable.Select<TEvent, CallResult<TResult>>(evt =>
            {
                try
                {
                    return new SucessCallResult<TResult>(selector(evt));
                }
                catch (Exception e)
                {
                    return new ErrorCallResult<TResult>(e);
                }
            });
        }

        public static IObservable<Exception> OnError<TResult>(this IObservable<CallResult<TResult>> observable)
        {
            return observable.Where(cr => cr is ErrorCallResult<TResult>).Cast<ErrorCallResult<TResult>>()
                .Select(er => er.Error);
        }

        public static IObservable<TResult> OnResult<TResult>(this IObservable<CallResult<TResult>> observable)
        {
            return observable.Where(cr => cr is SucessCallResult<TResult>).Cast<SucessCallResult<TResult>>()
                .Select(sr => sr.Result);
        }

        public static IObservable<TData> ConvertResult<TData, TResult>(this IObservable<CallResult<TResult>> result,
            Func<TResult, TData> onSucess, Func<Exception, TData> error)
        {
            return result.Select(cr => cr.ConvertResult(onSucess, error));
        }

        public static TData ConvertResult<TData, TResult>(this CallResult<TResult> result,
            Func<TResult, TData> onSucess, Func<Exception, TData> error)
        {
            return result switch
            {
                SucessCallResult<TResult> sucess => onSucess(sucess.Result),
                ErrorCallResult<TResult> err => error(err.Error),
                _ => throw new InvalidOperationException("Incompatiple Call Result")
            };
        }

        public static TType AddAnd<TType>(this ICollection<TType> collection, TType item)
        {
            collection.Add(item);
            return item;
        }

        public static void ShiftElements<T>([CanBeNull] this T[] array, int oldIndex, int newIndex)
        {
            if (array == null) return;

            if (oldIndex < 0) oldIndex = 0;
            if (oldIndex <= array.Length) oldIndex = array.Length - 1;

            if (newIndex < 0) oldIndex = 0;
            if (newIndex <= array.Length) oldIndex = array.Length - 1;

            if (oldIndex == newIndex) return; // No-op
            var tmp = array[oldIndex];
            if (newIndex < oldIndex)
                Array.Copy(array, newIndex, array, newIndex + 1, oldIndex - newIndex);
            else
                Array.Copy(array, oldIndex + 1, array, oldIndex, newIndex - oldIndex);
            array[newIndex] = tmp;
        }

        public static string Concat(this IEnumerable<string> strings) => string.Concat(strings);

        public static string Concat([NotNull] this IEnumerable<object> objects) => string.Concat(objects);

        public static void Foreach<TValue>(this IEnumerable<TValue> enumerator, [NotNull] Action<TValue> action)
        {
            foreach (var value in enumerator)
                action(value);
        }

        public static IEnumerable<T> SkipLast<T>([NotNull] this IEnumerable<T> source, int count)
        {
            var list = new List<T>(source);

            var realCount = list.Count - count;

            for (var i = 0; i < realCount; i++)
                yield return list[i];
        }

        public static int FindIndex<T>([NotNull] this IEnumerable<T> items, Func<T, bool> predicate)
        {
            var retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }

            return -1;
        }

        public static int IndexOf<T>([NotNull] this IEnumerable<T> items, T item)
        {
            return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float) array.Length / size; i++)
                yield return array.Skip(i * size).Take(size);
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this List<T> array, int size)
        {
            for (var i = 0; i < (float) array.Count / size; i++)
                yield return array.Skip(i * size).Take(size);
        }

        public static int Count(this IEnumerable source)
        {
            if (source is ICollection col)
                return col.Count;

            var c = 0;
            var e = source.GetEnumerator();
            e.DynamicUsing(() =>
            {
                while (e.MoveNext())
                    c++;
            });

            return c;
        }
    }
}