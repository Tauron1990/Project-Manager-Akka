﻿// The MIT License (MIT)
//
// Copyright (c) 2015-2020 Rasmus Mikkelsen
// Copyright (c) 2015-2020 eBay Software Foundation
// Modified from original source https://github.com/eventflow/EventFlow
//
// Copyright (c) 2018 - 2020 Lutando Ngqakaza
// https://github.com/Lutando/Akkatecture 
// 
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

namespace Akkatecture.ValueObjects;

[PublicAPI]
#pragma warning disable GU0025
public abstract class ValueObject
    #pragma warning restore GU0025
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<PropertyInfo>> TypeProperties = new();

    public override bool Equals(object? obj)
    {
        if(ReferenceEquals(this, obj)) return true;
        if(obj is null) return false;
        if(GetType() != obj.GetType()) return false;

        return obj is ValueObject other && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    [DebuggerNonUserCode]
    public override int GetHashCode()
    {
        unchecked
        {
            return GetEqualityComponents()
               .Aggregate(17, (current, obj) => current * 23 + (obj?.GetHashCode() ?? 0));
        }
    }

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);

    public override string ToString()
        => $"{{{string.Join(", ", GetProperties().Select(propertyInfo => $"{propertyInfo.Name}: {propertyInfo.GetValue(this)}"))}}}";

    [DebuggerNonUserCode]
    protected virtual IEnumerable<object?> GetEqualityComponents()
        => GetProperties().Select(propertyInfo => propertyInfo.GetValue(this));

    protected virtual IEnumerable<PropertyInfo> GetProperties()
    {
        return TypeProperties.GetOrAdd(
            GetType(),
            type => type
               .GetTypeInfo()
               .GetProperties(BindingFlags.Instance | BindingFlags.Public)
               .OrderBy(info => info.Name, StringComparer.Ordinal)
               .ToList());
    }
}