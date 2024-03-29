﻿using Akka;
using Akka.Streams.Dsl;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data;

public sealed record AppChangedSource(Source<AppInfo, NotUsed> AppSource)
{
    public static readonly AppChangedSource Empty = new(default(Source<AppInfo, NotUsed>)!);
}