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
using JetBrains.Annotations;

namespace Akkatecture.Core.VersionedTypes;

[PublicAPI]
// ReSharper disable once UnusedTypeParameter
public interface IVersionedTypeDefinitionService<TAttribute, TDefinition>
    where TAttribute : VersionedTypeAttribute
    where TDefinition : VersionedTypeDefinition
{
    void Load(IReadOnlyCollection<Type>? types);
    IEnumerable<TDefinition> GetDefinitions(string name);
    bool TryGetDefinition(string name, int version, [NotNullWhen(true)] out TDefinition? definition);
    IEnumerable<TDefinition> GetAllDefinitions();
    TDefinition GetDefinition(string name, int version);
    TDefinition GetDefinition(Type type);
    IReadOnlyCollection<TDefinition> GetDefinitions(Type type);
    bool TryGetDefinition(Type type, [NotNullWhen(true)] out TDefinition? definition);
    bool TryGetDefinitions(Type type, out IReadOnlyCollection<TDefinition> definitions);
    void Load(params Type[] types);
}