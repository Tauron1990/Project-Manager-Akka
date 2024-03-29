﻿using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Workshop.Mutating;

[PublicAPI]
public sealed record MutatingContext<TData>(MutatingChange? Change, TData Data)
{
    public TType GetChange<TType>()
        where TType : MutatingChange
        => Change?.Cast<TType>() ?? throw new InvalidCastException("Change has not the Requested Type");

    #pragma warning disable MA0018
    public static MutatingContext<TData> New(TData data) => new(null, data);
    #pragma warning restore MA0018


    public void Deconstruct(out MutatingChange? mutatingChange, out TData data)
    {
        mutatingChange = Change;
        data = Data;
    }

    public MutatingContext<TData> Update(MutatingChange? change, TData newData)
    {
        if(change is not null && change != Change && newData is ICanApplyChange<TData> apply)
            newData = apply.Apply(change);

        if(Change is null || change is null || ReferenceEquals(Change, change))
            return new MutatingContext<TData>(change, newData);

        if(Change is MultiChange multiChange)
            change = new MultiChange(multiChange.Changes.Add(change));
        else
            change = new MultiChange(ImmutableList<MutatingChange>.Empty.Add(change));

        return new MutatingContext<TData>(change, newData);
    }

    public MutatingContext<TData> WithChange(MutatingChange mutatingChange) => Update(mutatingChange, Data);
}