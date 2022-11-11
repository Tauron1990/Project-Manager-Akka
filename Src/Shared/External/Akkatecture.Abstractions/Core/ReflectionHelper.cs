// The MIT License (MIT)
//
// Copyright (c) 2015-2020 Rasmus Mikkelsen
// Copyright (c) 2015-2020 eBay Software Foundation
// Modified from original source https://github.com/eventflow/EventFlow
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

using System.Linq.Expressions;
using System.Reflection;
using Akkatecture.Extensions;
using FastExpressionCompiler;
using JetBrains.Annotations;

namespace Akkatecture.Core;

[PublicAPI]
#pragma warning disable AV1708
public static class ReflectionHelper
    #pragma warning restore AV1708
{
    public static TResult CompileMethodInvocation<TResult>(
        Type type, string methodName,
        params Type[] methodSignature) where TResult : class
    {
        TypeInfo typeInfo = type.GetTypeInfo();
        var methods = typeInfo
           .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
           .Where(info => string.Equals(info.Name, methodName, StringComparison.Ordinal));

        MethodInfo? methodInfo = !methodSignature.Any()
            ? methods.SingleOrDefault()
            : methods.SingleOrDefault(
                info
                    => info.GetParameters().Select(mp => mp.ParameterType).SequenceEqual(methodSignature));

        if(methodInfo == null)
            throw new ArgumentException($"Type '{type.PrettyPrint()}' doesn't have a method called '{methodName}'", nameof(type));

        return CompileMethodInvocation<TResult>(methodInfo);
    }

    #pragma warning disable AV1551
    #pragma warning disable MA0051
    public static TResult CompileMethodInvocation<TResult>(MethodInfo methodInfo) where TResult : class
    #pragma warning restore MA0051
    #pragma warning restore AV1551
    {
        var genericArguments = typeof(TResult).GetTypeInfo().GetGenericArguments();
        var methodArgumentList = methodInfo.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToList();
        var funcArgumentList = genericArguments.Skip(1).Take(methodArgumentList.Count).ToList();

        if(funcArgumentList.Count != methodArgumentList.Count)
            throw new ArgumentException("Incorrect number of arguments", nameof(methodInfo));

        ParameterExpression instanceArgument = Expression.Parameter(genericArguments[0]);

        var argumentPairs = funcArgumentList.Zip(methodArgumentList, (source, destination) => (Source: source, Destination: destination))
           .ToList();
        if(argumentPairs.All(pair => pair.Source == pair.Destination))
        {
            // No need to do anything fancy, the types are the same
            var parameters = funcArgumentList.Select(Expression.Parameter).ToList();

            return Expression.Lambda<TResult>(
                Expression.Call(instanceArgument, methodInfo, parameters),
                new[] { instanceArgument }.Concat(parameters)).Compile();
        }

        var lambdaArgument = new List<ParameterExpression>
                             {
                                 instanceArgument
                             };

        Type type = methodInfo.DeclaringType ?? typeof(object);
        ParameterExpression instanceVariable = Expression.Variable(type);
        var blockVariables = new List<ParameterExpression>
                             {
                                 instanceVariable,
                             };
        var blockExpressions = new List<Expression>
                               {
                                   Expression.Assign(instanceVariable, Expression.ConvertChecked(instanceArgument, type)),
                               };
        var callArguments = new List<ParameterExpression>();

        foreach ((Type source, Type destination) in argumentPairs)
            if(source == destination)
            {
                ParameterExpression sourceParameter = Expression.Parameter(source);
                lambdaArgument.Add(sourceParameter);
                callArguments.Add(sourceParameter);
            }
            else
            {
                ParameterExpression sourceParameter = Expression.Parameter(source);
                ParameterExpression destinationVariable = Expression.Variable(destination);
                BinaryExpression assignToDestination = Expression.Assign(
                    destinationVariable,
                    Expression.Convert(sourceParameter, destination));

                lambdaArgument.Add(sourceParameter);
                callArguments.Add(destinationVariable);
                blockVariables.Add(destinationVariable);
                blockExpressions.Add(assignToDestination);
            }

        MethodCallExpression callExpression = Expression.Call(instanceVariable, methodInfo, callArguments);
        blockExpressions.Add(callExpression);

        BlockExpression block = Expression.Block(blockVariables, blockExpressions);

        var lambdaExpression = Expression.Lambda<TResult>(block, lambdaArgument);

        return lambdaExpression.CompileFast<TResult>();
    }
}