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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using Akkatecture.Extensions;
using Akkatecture.ValueObjects;
using JetBrains.Annotations;
using Tauron;

namespace Akkatecture.Core;

[PublicAPI]
#pragma warning disable MA0097
#pragma warning disable MA0018
public abstract class Identity<T> : SingleValueObject<string>, IIdentity
    where T : Identity<T>
{
    // ReSharper disable StaticMemberInGenericType
    private static readonly Regex NameReplace = new("Id$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private static readonly string Name = NameReplace.Replace(typeof(T).Name, string.Empty).ToLowerInvariant();

    private static readonly Regex ValueValidation = new(
        @"^[a-z0-9]+\-(?<guid>[a-f0-9]{8}\-[a-f0-9]{4}\-[a-f0-9]{4}\-[a-f0-9]{4}\-[a-f0-9]{12})$",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(5));
    // ReSharper enable StaticMemberInGenericType

    public static T New => With(Guid.NewGuid());

    public static T NewDeterministic(Guid namespaceId, string name)
    {
        Guid guid = GuidFactories.Deterministic.Create(namespaceId, name);

        return With(guid);
    }

    public static T NewDeterministic(Guid namespaceId, byte[] nameBytes)
    {
        Guid guid = GuidFactories.Deterministic.Create(namespaceId, nameBytes);

        return With(guid);
    }

    public static T NewComb()
    {
        Guid guid = GuidFactories.Comb.Create();

        return With(guid);
    }

    public static T With(string value)
    {
        try
        {
            return (T)(FastReflection.Shared.FastCreateInstance(typeof(T), value) ??
                       throw new InvalidOperationException($"No Instantance of {typeof(T)} Created"));
        }
        catch (TargetInvocationException exception)
        {
            if(exception.InnerException != null) throw exception.InnerException;

            throw;
        }
    }

    public static T With(Guid guid)
    {
        var value = $"{Name}-{guid:D}";

        return With(value);
    }

    public static bool IsValid(string value) => !Validate(value).Any();

    public static IEnumerable<string> Validate(string value)
    {
        if(string.IsNullOrEmpty(value))
        {
            yield return $"Identity of type '{typeof(T).PrettyPrint()}' is null or empty";

            yield break;
        }

        if(!string.Equals(value.Trim(), value, StringComparison.OrdinalIgnoreCase))
            yield return
                $"Identity '{value}' of type '{typeof(T).PrettyPrint()}' contains leading and/or traling spaces";

        if(!value.StartsWith(Name, StringComparison.Ordinal))
            yield return $"Identity '{value}' of type '{typeof(T).PrettyPrint()}' does not start with '{Name}'";
        if(!ValueValidation.IsMatch(value))
            yield return
                $"Identity '{value}' of type '{typeof(T).PrettyPrint()}' does not follow the syntax '[NAME]-[GUID]' in lower case";
    }

    protected Identity(string value)
        : base(value)
    {
        var validationErrors = Validate(value).ToList();

        if(validationErrors.Any())
            throw new ArgumentException($"Identity is invalid: {string.Join(", ", validationErrors)}", nameof(value));

        _lazyGuid = new Lazy<Guid>(() => Guid.Parse(ValueValidation.Match(Value).Groups["guid"].Value));
    }

    private readonly Lazy<Guid> _lazyGuid;

    public Guid GetGuid() => _lazyGuid.Value;

    [UsedImplicitly]
    #pragma warning disable MA0018
    public static bool TryParse(string? value, [NotNullWhen(true)] out T? errorId)
        #pragma warning restore MA0018
    {
        if(!string.IsNullOrWhiteSpace(value) && IsValid(value))
        {
            errorId = With(value);

            return true;
        }

        errorId = null;

        return false;
    }
}