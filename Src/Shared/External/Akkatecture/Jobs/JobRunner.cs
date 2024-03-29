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
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Akkatecture.Extensions;
using JetBrains.Annotations;
using Tauron;

namespace Akkatecture.Jobs;

[PublicAPI]
public abstract class JobRunner : ReceiveActor
{
    protected JobRunner()
        => InitReceives();

    public void InitReceives()
    {
        Type type = GetType();

        var subscriptionTypes =
            type
               .GetJobRunTypes();

        var methods = type
           .GetTypeInfo()
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "Run", StringComparison.Ordinal))
                        return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1;
                })
           .ToDictionary(
                mi => mi.GetParameters()[0].ParameterType,
                mi => mi);


        MethodInfo method = type
           .GetBaseType("ReceiveActor")
           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
           .Where(
                mi =>
                {
                    if(!string.Equals(mi.Name, "Receive", StringComparison.Ordinal)) return false;

                    var parameters = mi.GetParameters();

                    return
                        parameters.Length == 1
                     && parameters[0].ParameterType.Name.Contains("Func", StringComparison.Ordinal);
                })
           .First();

        foreach (Type subscriptionType in subscriptionTypes)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(subscriptionType, typeof(bool));
            var subscriptionFunction = Delegate.CreateDelegate(funcType, this, methods[subscriptionType]);
            MethodInfo actorReceiveMethod = method.MakeGenericMethod(subscriptionType);

            actorReceiveMethod.InvokeFast(this, subscriptionFunction);
        }
    }
}

// ReSharper disable UnusedTypeParameter
public abstract class JobRunner<TJob, TIdentity> : JobRunner
    where TJob : IJob
    where TIdentity : IJobId { }