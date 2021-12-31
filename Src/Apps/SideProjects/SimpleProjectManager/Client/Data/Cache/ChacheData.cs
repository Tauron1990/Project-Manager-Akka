using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SimpleProjectManager.Client.Data.Cache;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public sealed record CacheTimeout([property: Key] CacheTimeoutId Id, CacheDataId DataKey, DateTime Timeout);

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public sealed record CacheData([property: Key] CacheDataId Id, string Data);