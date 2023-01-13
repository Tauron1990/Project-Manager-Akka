using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Tauron;

// ReSharper disable once UnusedTypeParameter
[DebuggerNonUserCode]
[PublicAPI]
public static class EnumerableExtensions
{
    public static TData[] AsOrToArray<TData>(this IEnumerable<TData> input)
    {
        if(input is TData[] arr)
            return arr;

        return input.ToArray();
    }

    public static TType AddAnd<TType>(this ICollection<TType> collection, TType item)
    {
        collection.Add(item);

        return item;
    }

    public static void ShiftElements<T>(this T[]? array, int oldIndex, int newIndex)
    {
        if(array == null) return;

        if(oldIndex < 0) oldIndex = 0;
        if(oldIndex <= array.Length) oldIndex = array.Length - 1;

        if(newIndex < 0) oldIndex = 0;
        if(newIndex <= array.Length) oldIndex = array.Length - 1;

        if(oldIndex == newIndex) return; // No-op

        T tmp = array[oldIndex];
        if(newIndex < oldIndex)
            Array.Copy(array, newIndex, array, newIndex + 1, oldIndex - newIndex);
        else
            Array.Copy(array, oldIndex + 1, array, oldIndex, newIndex - oldIndex);
        array[newIndex] = tmp;
    }

    public static string Concat(this IEnumerable<string> strings) => string.Concat(strings);

    public static string Concat(this IEnumerable<object> objects) => string.Concat(objects);

    public static void Foreach<TValue>(this IEnumerable<TValue> enumerator, Action<TValue> action)
    {
        foreach (TValue value in enumerator)
            action(value);
    }

    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int count)
    {
        var list = new List<T>(source);

        int realCount = list.Count - count;

        for (var i = 0; i < realCount; i++)
            yield return list[i];
    }

    public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
        var retVal = 0;
        foreach (T item in items)
        {
            if(predicate(item)) return retVal;

            retVal++;
        }

        return -1;
    }

    public static int IndexOf<T>(this IEnumerable<T> items, T item)
    {
        return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));
    }

    public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
    {
        for (var i = 0; i < (float)array.Length / size; i++)
            yield return array.Skip(i * size).Take(size);
    }

    public static IEnumerable<IEnumerable<T>> Split<T>(this ICollection<T> array, int size)
    {
        for (var i = 0; i < (float)array.Count / size; i++)
            yield return array.Skip(i * size).Take(size);
    }

    public static int Count(this IEnumerable source)
    {
        if(source is ICollection col)
            return col.Count;

        var c = 0;
        IEnumerator e = source.GetEnumerator();
        e.DynamicUsing(
            () =>
            {
                while (e.MoveNext())
                    c++;
            });

        return c;
    }
}