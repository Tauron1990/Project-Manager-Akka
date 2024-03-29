﻿// The MIT License (MIT)
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Akkatecture.Core;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Akkatecture.Aggregates.Snapshot;

[PublicAPI]
public class SnapshotMetadata : MetadataContainer, ISnapshotMetadata
{
    public SnapshotMetadata() { }

    public SnapshotMetadata(IDictionary<string, string> keyValuePairs)
        : base(keyValuePairs) { }

    public SnapshotMetadata(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        : base(keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal)) { }

    public SnapshotMetadata(params KeyValuePair<string, string>[] keyValuePairs)
        : this((IEnumerable<KeyValuePair<string, string>>)keyValuePairs) { }

    [JsonIgnore]
    public string AggregateId
    {
        get => GetMetadataValue(SnapshotMetadataKeys.AggregateId);
        set => Add(SnapshotMetadataKeys.AggregateId, value);
    }

    [JsonIgnore]
    public string AggregateName
    {
        get => GetMetadataValue(SnapshotMetadataKeys.AggregateName);
        set => Add(SnapshotMetadataKeys.AggregateName, value);
    }

    [JsonIgnore]
    public long AggregateSequenceNumber
    {
        get => GetMetadataValue(SnapshotMetadataKeys.AggregateSequenceNumber, long.Parse);
        set => Add(SnapshotMetadataKeys.AggregateSequenceNumber, value.ToString(CultureInfo.InvariantCulture));
    }

    [JsonIgnore]
    public string SnapshotName
    {
        get => GetMetadataValue(SnapshotMetadataKeys.SnapshotName);
        set => Add(SnapshotMetadataKeys.SnapshotName, value);
    }

    [JsonIgnore]
    public ISnapshotId SnapshotId
    {
        get => GetMetadataValue(SnapshotMetadataKeys.SnapshotId, Snapshot.SnapshotId.With);
        set => Add(SnapshotMetadataKeys.SnapshotId, value.Value);
    }

    [JsonIgnore]
    public int SnapshotVersion
    {
        get => GetMetadataValue(SnapshotMetadataKeys.SnapshotVersion, int.Parse);
        set => Add(SnapshotMetadataKeys.SnapshotVersion, value.ToString(CultureInfo.InvariantCulture));
    }
}