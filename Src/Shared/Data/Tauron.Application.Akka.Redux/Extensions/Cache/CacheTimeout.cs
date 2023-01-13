﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Tauron.Application.Akka.Redux.Extensions.Cache;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public sealed record CacheTimeout([property: Key] CacheTimeoutId Id, CacheDataId DataKey, DateTime Timeout);