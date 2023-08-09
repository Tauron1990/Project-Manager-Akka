// The MIT License (MIT)
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

using System.Diagnostics;
using System.Reflection;
using Akkatecture.Extensions;
using MemoryPack;

namespace Akkatecture.ValueObjects;

#pragma warning disable MA0097
public abstract class SingleValueObject<T> : ValueObject, IComparable, ISingleValueObject
    #pragma warning restore MA0097
    where T : IComparable
{
    private static readonly Type Type = typeof(T);
    private static readonly TypeInfo TypeInfo = typeof(T).GetTypeInfo();

    private readonly T _value;

    protected SingleValueObject(T value)
    {
        if(TypeInfo.IsEnum && !Enum.IsDefined(Type, value))
            throw new ArgumentException($"The value '{value}' isn't defined in enum '{Type.PrettyPrint()}'", nameof(value));

        _value = value;
    }

    // ReSharper disable once ConvertToAutoProperty
    public T Value => _value;

    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => throw new ArgumentNullException(nameof(obj)),
            SingleValueObject<T> other => Value.CompareTo(other.Value),
            _ => throw new ArgumentException($"Cannot compare '{GetType().PrettyPrint()}' and '{obj.GetType().PrettyPrint()}'", nameof(obj)),
        };
    }

    public object GetValue() => Value;

    [DebuggerNonUserCode]
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString() ?? string.Empty;
}