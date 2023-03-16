using System;

namespace TestApp;

[PublicAPI]
public static class ValueTupleExtensions
{
    public static ref ValueTuple<T1, T2> With<T1, T2>(this ref ValueTuple<T1, T2> value, T1 newValue)
    {
        value.Item1 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2> With<T1, T2>(this ref ValueTuple<T1, T2> value, T2 newValue)
    {
        value.Item2 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3> With<T1, T2, T3>(this ref ValueTuple<T1, T2, T3> value, T2 newValue)
    {
        value.Item2 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3> With<T1, T2, T3>(this ref ValueTuple<T1, T2, T3> value, T1 newValue)
    {
        value.Item1 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3> With<T1, T2, T3>(this ref ValueTuple<T1, T2, T3> value, T3 newValue)
    {
        value.Item3 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3, T4> With<T1, T2, T3, T4>(this ref ValueTuple<T1, T2, T3, T4> value, T1 newValue)
    {
        value.Item1 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3, T4> With<T1, T2, T3, T4>(this ref ValueTuple<T1, T2, T3, T4> value, T2 newValue)
    {
        value.Item2 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3, T4> With<T1, T2, T3, T4>(this ref ValueTuple<T1, T2, T3, T4> value, T3 newValue)
    {
        value.Item3 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3, T4> With<T1, T2, T3, T4>(this ref ValueTuple<T1, T2, T3, T4> value, T4 newValue)
    {
        value.Item4 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3, T4, T5> With<T1, T2, T3, T4, T5>(this ref ValueTuple<T1, T2, T3, T4, T5> value, T1 newValue)
    {
        value.Item1 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3, T4, T5> With<T1, T2, T3, T4, T5>(this ref ValueTuple<T1, T2, T3, T4, T5> value, T2 newValue)
    {
        value.Item2 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3, T4, T5> With<T1, T2, T3, T4, T5>(this ref ValueTuple<T1, T2, T3, T4, T5> value, T3 newValue)
    {
        value.Item3 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3, T4, T5> With<T1, T2, T3, T4, T5>(this ref ValueTuple<T1, T2, T3, T4, T5> value, T4 newValue)
    {
        value.Item4 = newValue;
        return ref value;
    }
    
    public static ref ValueTuple<T1, T2, T3, T4, T5> With<T1, T2, T3, T4, T5>(this ref ValueTuple<T1, T2, T3, T4, T5> value, T5 newValue)
    {
        value.Item5 = newValue;
        return ref value;
    }
}