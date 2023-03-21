using System;
using System.ComponentModel;
using System.Globalization;

namespace Tauron;

#pragma warning disable EPS02

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
            if(resource is IDisposable d)
                d.Dispose();
        }
    }

    public static Option<TData> AsOption<TData>(this TData data) => new(hasValue: true, data);
    
    public static Option<TData> AsOption<TData>(this Stl.Result<TData> data) => 
        data.HasValue ? Option<TData>.Some(data.Value) : Option<TData>.None;

    public static Option<TData> OptionNotNull<TData>(this TData? data)
        where TData : class
        => data ?? Option<TData>.None;

    public static void OnSuccess<TData>(this in Option<TData> data, Action<TData> action)
    {
        if(data.HasValue)
            action(data.Value);
    }

    public static TData GetOrElse<TData>(this in Option<TData> option, TData els)
         => option.HasValue ? option.Value : els;

    public static TData GetOrElse<TData>(this in Option<TData> option, Func<TData> els)
        => option.HasValue ? option.Value : els();
    
    public static Option<TNew> Select<TOld, TNew>(this in Option<TOld> old, Func<TOld, TNew> onValue)
        => old.HasValue ? Option.Some(onValue(old.Value)) : Option.None<TNew>();

    public static Option<TNew> Select<TOld, TNew>(this in Option<TOld> old, Func<TOld, TNew> onValue, Func<TNew> defaultValue)
        => old.HasValue ? old.Select(onValue) : defaultValue();

    public static Option<TNew> FlatSelect<TOld, TNew>(this in Option<TOld> old, Func<TOld, Option<TNew>> onValue)
        => old.HasValue ? onValue(old.Value) : Option.None<TNew>();

    #pragma warning disable EPS06
    public static Result<TNew> Select<TOld, TNew>(this in Result<TOld> old, Func<TOld, TNew> onValue)
    {
        try
        {
            return old.HasValue ? Result.Value(onValue(old.Value)) : ConvertError<TOld, TNew>(old);
        }
        catch (Exception ex)
        {
            return Result.Error<TNew>(ex);
        }
    }

    public static Result<TNew> Select<TOld, TNew>(this in Result<TOld> old, Func<TOld, TNew> onValue, Func<TNew> defaultValue)
    {
        try
        {
            return old.HasValue ? old.Select(onValue) : defaultValue();
        }
        catch (Exception ex)
        {
            return Result.Error<TNew>(ex);
        }
    }

    public static Result<TNew> FlatSelect<TOld, TNew>(this in Result<TOld> old, Func<TOld, Result<TNew>> onValue)
    {
        try
        {
            
            return old.HasValue ? onValue(old.Value) : ConvertError<TOld, TNew>(old);
        }
        catch (Exception e)
        {
            return Result.Error<TNew>(e);
        }
    }

    public static Result<TNew> Collect<TData1, TData2, TNew>(
        this in Result<TData1> result1, 
        in Result<TData2> result2, 
        Func<TData1, TData2, TNew> selector)
    {
        try
        {
            TData1 data1;
            TData2 data2;

            if(result1.HasValue)
                data1 = result1.Value;
            else
                return ConvertError<TData1, TNew>(result1);
            
            if(result2.HasValue)
                data2 = result2.Value;
            else
                return ConvertError<TData2, TNew>(result2);
            
            return Result.Value(selector(data1, data2));
        }
        catch (Exception e)
        {
            return Result.Error<TNew>(e);
        }
    }
    
    public static Result<TNew> Collect<TData1, TData2, TData3, TNew>(
        this in Result<TData1> result1, 
        in Result<TData2> result2,
        in Result<TData3> result3,
        Func<TData1, TData2, TData3,  TNew> selector)
    {
        try
        {
            TData1 data1;
            TData2 data2;
            TData3 data3;

            if(result1.HasValue)
                data1 = result1.Value;
            else
                return ConvertError<TData1, TNew>(result1);
            
            if(result2.HasValue)
                data2 = result2.Value;
            else
                return ConvertError<TData2, TNew>(result2);
            
            if(result3.HasValue)
                data3 = result3.Value;
            else
                return ConvertError<TData3, TNew>(result3);
            
            return Result.Value(selector(data1, data2, data3));
        }
        catch (Exception e)
        {
            return Result.Error<TNew>(e);
        }
    }
    
    public static Result<TNew> Collect<TData1, TData2, TData3, TData4, TNew>(
        this in Result<TData1> result1, 
        in Result<TData2> result2,
        in Result<TData3> result3,
        in Result<TData4> result4,
        Func<TData1, TData2, TData3, TData4,  TNew> selector)
    {
        try
        {
            TData1 data1;
            TData2 data2;
            TData3 data3;
            TData4 data4;
            
            if(result1.HasValue)
                data1 = result1.Value;
            else
                return ConvertError<TData1, TNew>(result1);
            
            if(result2.HasValue)
                data2 = result2.Value;
            else
                return ConvertError<TData2, TNew>(result2);
            
            if(result3.HasValue)
                data3 = result3.Value;
            else
                return ConvertError<TData3, TNew>(result3);
            
            if(result4.HasValue)
                data4 = result4.Value;
            else
                return ConvertError<TData4, TNew>(result4);
            
            return Result.Value(selector(data1, data2, data3, data4));
        }
        catch (Exception e)
        {
            return Result.Error<TNew>(e);
        }
    }

    private static Result<TData> ConvertError<TOld, TData>(Result<TOld> input) => 
        Result.Error<TData>(input.Error ?? new InvalidOperationException($"No Result Error not found ({typeof(TOld)}). Propetly Default Value"));

    public static void Run<TData>(this in Result<TData> result, Action<TData> onValue, Action<Exception> onError)
    {
        if(result.HasValue)
            onValue(result.Value);
        else
            onError(result.Error!);
    }

    public static bool WhenTrue(this bool input, Action run)
    {
        if(input)
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
        if(!Enum.IsDefined(typeof(RoundType), type))
            throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(RoundType));
        if(type == RoundType.None)
            throw new ArgumentNullException(nameof(type));

        return Round(source, (double)type);
    }

    public static DateTime Round(this DateTime source, double type)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(type == 0)
            throw new ArgumentNullException(nameof(type));

        DateTime result = source;

        double minutes = type;

        Math.DivRem(source.Minute, (int)minutes, out int modulo);

        if(modulo <= 0) return result;

        result = modulo >= minutes / 2 ? source.AddMinutes(minutes - modulo) : source.AddMinutes(modulo * -1);

        result = result.AddSeconds(source.Second * -1);

        return result;
    }

    [StringFormatMethod("format")]
    public static string SFormat(this string format, params object[] args)
        => string.Format(CultureInfo.InvariantCulture, format, args);

    public static Option<TType> TypedTarget<TType>(this WeakReference<TType> reference)
        where TType : class
        => reference.TryGetTarget(out TType? obj) ? obj.AsOption() : default;

    public static TimeSpan LocalTimeSpanToUtc(this TimeSpan ts)
    {
        DateTime dt = DateTime.Now.Date.Add(ts);
        DateTime dtUtc = dt.ToUniversalTime();
        TimeSpan tsUtc = dtUtc.TimeOfDay;

        return tsUtc;
    }

    public static TimeSpan UtcTimeSpanToLocal(this TimeSpan tsUtc)
    {
        DateTime dtUtc = DateTime.UtcNow.Date.Add(tsUtc);
        DateTime dt = dtUtc.ToLocalTime();
        TimeSpan ts = dt.TimeOfDay;

        return ts;
    }
}