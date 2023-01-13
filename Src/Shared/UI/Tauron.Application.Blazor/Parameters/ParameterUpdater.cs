using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Stl;
using Stl.Fusion;

namespace Tauron.Application.Blazor.Parameters;

public sealed class ParameterUpdater
{
    private readonly GroupDictionary<string, IMutableState> _registrations = new();
    private IReadOnlyDictionary<string, object> _parameterView = ImmutableDictionary<string, object>.Empty;

    public IState<TValue> Register<TValue>(string name, IStateFactory stateFactory)
    {
        if(stateFactory is null)
            throw new ArgumentNullException(nameof(stateFactory));
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

        if(_registrations.TryGetValue(name, out var states))
        {
            IMutableState? oldState = states.FirstOrDefault(s => s.Computed.OutputType == typeof(TValue));

            if(oldState != null)
                return (IMutableState<TValue>)oldState;
        }

        var state = stateFactory.NewMutable(new MutableState<TValue>.Options());
        _registrations.Add(name, state);

        if(_parameterView.TryGetValue(name, out object? obj) && obj is TValue value)
            state.Set(value);

        return state;
    }

    public void UpdateParameters(ParameterView parameterView)
    {
        _parameterView = parameterView.ToDictionary();

        foreach ((string key, var states) in _registrations)
        {
            #if DEBUG
            Console.WriteLine($"ParameterUpdater: Name: {key} -- States: {states.Count}");
            #endif

            if(parameterView.TryGetValue(key, out object? value))
                states.Foreach(s => s.Set(Result.Value(value)));
            else
                states.Foreach(
                    s =>
                    {
                        object? defaultValue = s.Computed.OutputType.IsValueType ? Activator.CreateInstance(s.Computed.OutputType) : null;
                        s.Set(Result.Value(defaultValue));
                    });

        }
    }
}