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


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Akka.Event;
using Akkatecture.Extensions;

namespace Akkatecture.Core.VersionedTypes;

internal static class VersionedTypeDefinitionService
{
    internal static readonly Regex NameRegex = new(
        @"^(Old){0,1}(?<name>[\p{L}\p{Nd}]+?)(V(?<version>[0-9]+)){0,1}$",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture,
        TimeSpan.FromSeconds(5));
}

public abstract class VersionedTypeDefinitionService<TTypeCheck, TAttribute, TDefinition>
    : IVersionedTypeDefinitionService<TAttribute, TDefinition>
    where TAttribute : VersionedTypeAttribute
    where TDefinition : VersionedTypeDefinition
{
    #pragma warning disable AV1115
    private readonly ConcurrentDictionary<string, Dictionary<int, TDefinition>> _definitionByNameAndVersion = new(StringComparer.Ordinal);
    #pragma warning restore AV1115
    private readonly ConcurrentDictionary<Type, List<TDefinition>> _definitionsByType = new();
    private readonly ILoggingAdapter? _logger;

    private readonly object _syncRoot = new();

    protected VersionedTypeDefinitionService(
        ILoggingAdapter? logger)
        => _logger = logger;

    public void Load(params Type[] types)
    {
        Load((IReadOnlyCollection<Type>)types);
    }

    public void Load(IReadOnlyCollection<Type>? types)
    {
        if(types is null) return;

        var invalidTypes = types
           .Where(type => !typeof(TTypeCheck).GetTypeInfo().IsAssignableFrom(type))
           .ToList();

        if(invalidTypes.Any())
            throw new ArgumentException(
                $"The following types are not of type '{typeof(TTypeCheck).PrettyPrint()}': {string.Join(", ", invalidTypes.Select(type => type.PrettyPrint()))}",
                nameof(types));

        lock (_syncRoot)
        {
            var definitions = types
               .Distinct()
               .Where(type => !_definitionsByType.ContainsKey(type))
               .SelectMany(CreateDefinitions)
               .ToList();

            if(!definitions.Any()) return;

            var assemblies = definitions
               .Select(definition => definition.Type.GetTypeInfo().Assembly.GetName().Name)
               .Distinct(StringComparer.Ordinal)
               .OrderBy(name => name)
               .ToList();

            var logs =
                $"Added {definitions.Count} versioned types to '{GetType().PrettyPrint()}' from these assemblies: {string.Join(", ", assemblies)}";

            _logger?.Info(logs);

            foreach (TDefinition definition in definitions)
            {
                var typeDefinitions = _definitionsByType.GetOrAdd(
                    definition.Type,
                    _ => new List<TDefinition>());
                typeDefinitions.Add(definition);

                if(!_definitionByNameAndVersion.TryGetValue(definition.Name, out var versions))
                {
                    versions = new Dictionary<int, TDefinition>();
                    _definitionByNameAndVersion.TryAdd(definition.Name, versions);
                }

                if(versions.ContainsKey(definition.Version))
                {
                    _logger?.Info(
                        "Already loaded versioned type '{0}' v{1}, skipping it",
                        definition.Name,
                        definition.Version);

                    continue;
                }

                versions.Add(definition.Version, definition);
            }
        }
    }

    public IEnumerable<TDefinition> GetDefinitions(string name)
        => _definitionByNameAndVersion.TryGetValue(name, out var versions)
            ? versions.Values.OrderBy(definition => definition.Version)
            : Enumerable.Empty<TDefinition>();

    public IEnumerable<TDefinition> GetAllDefinitions()
        => _definitionByNameAndVersion.SelectMany(kv => kv.Value.Values);

    #pragma warning disable AV1551
    public bool TryGetDefinition(string name, int version, [NotNullWhen(true)] out TDefinition? definition)
        #pragma warning restore AV1551
    {
        if(_definitionByNameAndVersion.TryGetValue(name, out var versions))
            return versions.TryGetValue(version, out definition);

        definition = null!;

        return false;
    }

    #pragma warning disable AV1551
    public TDefinition GetDefinition(string name, int version)
        #pragma warning restore AV1551
    {
        if(!TryGetDefinition(name, version, out TDefinition? definition))
            throw new ArgumentException(
                $"No versioned type definition for '{name}' with version {version} in '{GetType().PrettyPrint()}'",
                nameof(name));

        return definition;
    }

    #pragma warning disable AV1551
    public TDefinition GetDefinition(Type type)
        #pragma warning restore AV1551
    {
        if(!TryGetDefinition(type, out TDefinition? definition))
            throw new ArgumentException(
                $"No definition for type '{type.PrettyPrint()}', have you remembered to load it during Akkatecture initialization",
                nameof(type));

        return definition;
    }

    public IReadOnlyCollection<TDefinition> GetDefinitions(Type type)
    {
        if(!TryGetDefinitions(type, out var definitions))
            throw new ArgumentException(
                $"No definition for type '{type.PrettyPrint()}', have you remembered to load it during Akkatecture initialization",
                nameof(type));

        return definitions;
    }

    #pragma warning disable AV1551
    public bool TryGetDefinition(Type type, [NotNullWhen(true)] out TDefinition? definition)
        #pragma warning restore AV1551
    {
        if(!TryGetDefinitions(type, out var definitions))
        {
            definition = default;

            return false;
        }

        if(definitions.Count > 1)
            throw new InvalidOperationException(
                $"Type '{type.PrettyPrint()}' has multiple definitions: {string.Join(", ", definitions.Select(typeDefinition => typeDefinition.ToString()))}");

        definition = definitions.Single();

        return true;
    }

    public bool TryGetDefinitions(Type type, out IReadOnlyCollection<TDefinition> definitions)
    {
        if(type is null) throw new ArgumentNullException(nameof(type));

        if(!_definitionsByType.TryGetValue(type, out var list))
        {
            definitions = default!;

            return false;
        }

        definitions = list;

        return true;
    }

    protected abstract TDefinition CreateDefinition(int version, Type type, string name);

    private IEnumerable<TDefinition> CreateDefinitions(Type versionedType)
    {
        var hasAttributeDefinition = false;
        foreach (TDefinition definitionFromAttribute in CreateDefinitionFromAttribute(versionedType))
        {
            hasAttributeDefinition = true;

            yield return definitionFromAttribute;
        }

        if(hasAttributeDefinition) yield break;

        yield return CreateDefinitionFromName(versionedType);
    }

    private TDefinition CreateDefinitionFromName(Type versionedType)
    {
        Match match = VersionedTypeDefinitionService.NameRegex.Match(versionedType.Name);

        if(!match.Success)
            throw new ArgumentException($"Versioned type name '{versionedType.Name}' is not a valid name", nameof(versionedType));

        var version = 1;
        Group groups = match.Groups["version"];
        if(groups.Success) version = int.Parse(groups.Value, CultureInfo.InvariantCulture);

        string name = match.Groups["name"].Value;

        return CreateDefinition(
            version,
            versionedType,
            name);
    }

    private IEnumerable<TDefinition> CreateDefinitionFromAttribute(Type versionedType)
    {
        return versionedType
           .GetTypeInfo()
           .GetCustomAttributes()
           .OfType<TAttribute>()
           .Select(attribute => CreateDefinition(attribute.Version, versionedType, attribute.Name));
    }
}