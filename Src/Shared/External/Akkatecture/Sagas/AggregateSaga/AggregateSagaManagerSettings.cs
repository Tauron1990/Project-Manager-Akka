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

using Akka.Configuration;
using Akkatecture.Configuration;

namespace Akkatecture.Sagas.AggregateSaga;

public class AggregateSagaManagerSettings
{
    private const string Section = "akkatecture.aggregate-saga-manager";
    public readonly bool AutoSpawnOnReceive;
    public readonly bool AutoSubscribe;

    public AggregateSagaManagerSettings(Config config)
    {
        Config? aggregateSagaManagerConfig = config.WithFallback(AkkatectureDefaultSettings.DefaultConfig());
        aggregateSagaManagerConfig = aggregateSagaManagerConfig.GetConfig(Section);

        AutoSpawnOnReceive = aggregateSagaManagerConfig.GetBoolean("auto-spawn-on-receive");
        AutoSubscribe = aggregateSagaManagerConfig.GetBoolean("auto-subscribe");
    }
}