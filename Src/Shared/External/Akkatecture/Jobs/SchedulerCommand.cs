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

// ReSharper disable UnusedTypeParameter

namespace Akkatecture.Jobs;

#pragma warning disable AV1507
public abstract class SchedulerCommand<TJob, TIdentity> : SchedulerMessage<TJob, TIdentity>
    #pragma warning restore AV1507
    where TJob : IJob
    where TIdentity : IJobId
{
    protected SchedulerCommand(
        TIdentity jobId,
        object? ack = null,
        object? nack = null)
    {
        if(jobId is null) throw new ArgumentNullException(nameof(jobId));

        JobId = jobId;
        Ack = ack;
        Nack = nack;
    }

    public TIdentity JobId { get; }
    public object? Ack { get; }
    public object? Nack { get; }
}