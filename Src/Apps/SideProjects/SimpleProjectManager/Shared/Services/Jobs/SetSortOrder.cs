﻿using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record SetSortOrder(
    [property:DataMember, MemoryPackOrder(0)]bool IgnoreIfEmpty, 
    [property:DataMember, MemoryPackOrder(1)]SortOrder? SortOrder);