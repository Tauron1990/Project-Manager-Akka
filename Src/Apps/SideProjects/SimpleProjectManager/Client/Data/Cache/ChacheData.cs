using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SimpleProjectManager.Client.Data.Cache;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public sealed record CacheTimeout([property: Key] int Id, string DataKey, DateTime Timeout);

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public sealed record CacheData([property: Key] string Id, string Data);