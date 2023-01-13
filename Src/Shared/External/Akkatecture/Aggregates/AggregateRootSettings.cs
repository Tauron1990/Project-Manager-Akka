// The MIT License (MIT)
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
using Akka.Configuration;
using Akkatecture.Configuration;

namespace Akkatecture.Aggregates;

public class AggregateRootSettings
{
    private const string Section = "akkatecture.aggregate-root";
    public readonly TimeSpan SetReceiveTimeout;
    public readonly bool UseDefaultEventRecover;
    public readonly bool UseDefaultSnapshotRecover;

    public AggregateRootSettings(Config config)
    {
        Config? aggregateRootConfig = config.WithFallback(AkkatectureDefaultSettings.DefaultConfig());
        aggregateRootConfig = aggregateRootConfig.GetConfig(Section);

        UseDefaultEventRecover = aggregateRootConfig.GetBoolean("use-default-event-recover");
        UseDefaultSnapshotRecover = aggregateRootConfig.GetBoolean("use-default-snapshot-recover");
        SetReceiveTimeout = aggregateRootConfig.GetTimeSpan("set-receive-timeout");
    }

    public AggregateRootSettings(TimeSpan setReceiveTimeout, bool useDefaultEventRecover, bool useDefaultSnapshotRecover)
    {
        SetReceiveTimeout = setReceiveTimeout;
        UseDefaultEventRecover = useDefaultEventRecover;
        UseDefaultSnapshotRecover = useDefaultSnapshotRecover;
    }
}