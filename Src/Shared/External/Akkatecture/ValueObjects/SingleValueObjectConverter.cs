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

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Akkatecture.ValueObjects;

[PublicAPI]
public class SingleValueObjectConverter : JsonConverter
{
    private static readonly ConcurrentDictionary<Type, Type> ConstructorArgumenTypes = new();

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not ISingleValueObject singleValueObject) return;

        serializer.Serialize(writer, singleValueObject.GetValue());
    }

    public override object ReadJson(
        JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var parameterType = ConstructorArgumenTypes.GetOrAdd(
            objectType,
            type =>
            {
                var constructorInfo = type.GetTypeInfo()
                   .GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single();
                var parameterInfo = constructorInfo.GetParameters().Single();

                return parameterInfo.ParameterType;
            });

        var value = serializer.Deserialize(reader, parameterType);

        return Activator.CreateInstance(objectType, value) ?? throw new InvalidOperationException("Could not Create Single Value Object");
    }

    public override bool CanConvert(Type objectType)
        => typeof(ISingleValueObject).GetTypeInfo().IsAssignableFrom(objectType);
}