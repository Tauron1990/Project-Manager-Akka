﻿using System;
using System.ComponentModel;
using System.Globalization;
using Akka.Util;
using Akka.Util.Extensions;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public enum RoundType : short
    {
        None = 0,

        Hour = 60,

        HalfHour = 30,

        QuaterHour = 15
    }

    [PublicAPI]
    public static class ObjectExtension
    {
        public static void DynamicUsing(this object resource, Action action)
        {
            try
            {
                action();
            }
            finally
            {
                if (resource is IDisposable d)
                    d.Dispose();
            }
        }

        public static Option<TData> OptionNotNull<TData>(this TData? data)
            where TData : class
            => data ?? Option<TData>.None;

        public static Option<TNew> Select<TOld, TNew>(this Option<TOld> old, Func<TOld, TNew> onValue, Func<TNew> defaultValue) 
            => old.HasValue ? old.Select(onValue) : defaultValue();

        public static bool WhenTrue(this bool input, Action run)
        {
            if (input)
                run();

            return input;
        }

        //public static TResult To<TInput, TResult>(this TInput input, Func<TInput, TResult> transformer)
        //    => transformer(input);

        ////public static TResult To<TInput, TResult>(this TInput input, Func<TInput, TResult> transformer)
        ////    where TInput : class where TResult : class
        ////    => transformer(input);

        public static TType DoAnd<TType>(this TType item, params Action<TType>[] todo)
        {
            foreach (var action in todo)
                action(item);
            return item;
        }

        //public static void When<TType>(this TType target, Func<TType, bool> when, Action<TType> then)
        //{
        //    if (when(target))
        //        then(target);
        //}

        //public static void When<TType>(this TType target, Func<TType, bool> when, Action then)
        //{
        //    if (when(target))
        //        then();
        //}

        //public static TResult When<TType, TResult>(this TType target, TResult defaultValue, Func<TType, bool> when, Func<TType, TResult> then) 
        //    => when(target) ? then(target) : defaultValue;

        //public static TResult When<TType, TResult>(this TType target, Func<TType, bool> when, Func<TResult> then, TResult falseValue)
        //    => when(target) ? then() : falseValue;

        //public static void WhenNotEmpty(this string? target, Action<string> then)
        //{
        //    if (string.IsNullOrWhiteSpace(target)) return;
        //    then(target);
        //}

        public static DateTime CutSecond(this DateTime source)
            => new(source.Year, source.Month, source.Day, source.Hour, source.Minute, 0);

        //public static T? GetService<T>(this IServiceProvider provider)
        //    where T : class
        //{
        //    if (provider == null) throw new ArgumentNullException(nameof(provider));
        //    var temp = provider.GetService(typeof(T));

        //    return temp as T;
        //}

        public static bool IsAlive<TType>(this WeakReference<TType> reference) where TType : class
            => reference.TryGetTarget(out _);

        public static DateTime Round(this DateTime source, RoundType type)
        {
            if (!Enum.IsDefined(typeof(RoundType), type))
                throw new InvalidEnumArgumentException(nameof(type), (int) type, typeof(RoundType));
            if (type == RoundType.None)
                throw new ArgumentNullException(nameof(type));

            return Round(source, (double) type);
        }

        public static DateTime Round(this DateTime source, double type)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (type == 0)
                throw new ArgumentNullException(nameof(type));

            var result = source;

            var minutes = type;

            Math.DivRem(source.Minute, (int) minutes, out var modulo);

            if (modulo <= 0) return result;

            result = modulo >= minutes / 2 ? source.AddMinutes(minutes - modulo) : source.AddMinutes(modulo * -1);

            result = result.AddSeconds(source.Second * -1);

            return result;
        }

        [StringFormatMethod("format")]
        public static string SFormat(this string format, params object[] args)
            => string.Format(CultureInfo.InvariantCulture, format, args);

        public static Option<TType> TypedTarget<TType>(this WeakReference<TType> reference) 
            where TType : class
            => reference.TryGetTarget(out var obj) ? obj.AsOption() : default;

        public static TimeSpan LocalTimeSpanToUTC(this TimeSpan ts)
        {
            DateTime dt = DateTime.Now.Date.Add(ts);
            DateTime dtUtc = dt.ToUniversalTime();
            TimeSpan tsUtc = dtUtc.TimeOfDay;
            return tsUtc;
        }

        public static TimeSpan UTCTimeSpanToLocal(this TimeSpan tsUtc)
        {
            DateTime dtUtc = DateTime.UtcNow.Date.Add(tsUtc);
            DateTime dt = dtUtc.ToLocalTime();
            TimeSpan ts = dt.TimeOfDay;
            return ts;
        }
    }
}