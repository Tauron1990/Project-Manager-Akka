using System;
using System.Reactive;
using JetBrains.Annotations;
using OneOf;
using Stl;

namespace Tauron.Operations;

[PublicAPI]
public readonly struct TriOption<TData> : IOneOf
{
    public static TriOption<TData> None => new();
    
    private readonly OneOf<TData, Exception, Unit> _result;

    public TriOption(OneOf<TData, Exception, Unit> result) => _result = result;

    public TriOption() => _result = Unit.Default;

    //Hopefully Fixed Later by OneOf
    
    public Option<TData> ToOption(Func<Exception, Option<TData>> onError)
        // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
#pragma warning disable EPS06
        => _result.Match(Option.Some, onError, _ => Option.None<TData>());


    public bool IsNone => _result.IsT2;
    
    object IOneOf.Value => _result.Value;

    int IOneOf.Index => _result.Index;
    
    public static implicit operator TriOption<TData>(TData data)
            => new (data);
    
    public static implicit operator TriOption<TData>(Exception data)
        => new (data);
}