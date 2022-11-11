using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Tauron.Application.Akka.Redux.Extensions.Cache;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public sealed record CacheData([property: Key] CacheDataId Id, string Data);