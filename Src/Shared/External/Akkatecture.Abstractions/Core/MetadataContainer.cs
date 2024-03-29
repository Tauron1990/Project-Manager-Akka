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

using Akkatecture.Exceptions;
using JetBrains.Annotations;

namespace Akkatecture.Core;

[PublicAPI]
public class MetadataContainer : Dictionary<string, string>
{
    public MetadataContainer() { }

    public MetadataContainer(IDictionary<string, string> keyValuePairs)
        : base(keyValuePairs) { }

    public MetadataContainer(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        : base(keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal)) { }

    public MetadataContainer(params KeyValuePair<string, string>[] keyValuePairs)
        : this((IEnumerable<KeyValuePair<string, string>>)keyValuePairs) { }

    public void AddRange(params KeyValuePair<string, string>[] keyValuePairs)
        => AddRange((IEnumerable<KeyValuePair<string, string>>)keyValuePairs);

    public void AddRange(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
    {
        foreach ((string key, string value) in keyValuePairs) Add(key, value);
    }

    public override string ToString()
        => string.Join(Environment.NewLine, this.Select(kv => $"{kv.Key}: {kv.Value}"));

    public string GetMetadataValue(string key)
        => GetMetadataValue(key, value => value);

    public virtual T GetMetadataValue<T>(string key, Func<string, T> converter)
    {
        #pragma warning disable MA0015
        if(!TryGetValue(key, out string? value)) throw new MetadataKeyNotFoundException(nameof(key), key);
        #pragma warning restore MA0015

        try
        {
            return converter(value);
        }
        catch (Exception exception)
        {
            throw new MetadataParseException(key, value, exception);
        }
    }
}