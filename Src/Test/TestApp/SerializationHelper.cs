using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompiler;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tauron;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public static class SerializationHelper<TData>
{
    private static bool _registrated;

    public static void Register()
    {
        if(_registrated) return;

        var accessor = GetAcessor();
        BsonMapper.Global.RegisterType<TData>(
            t =>
            {
                if(t is null)
                    return new BsonDocument();
                
                var token = JToken.FromObject(t);

                var doc = new BsonDocument();

                foreach (var value in token)
                {
                    
                }

                return doc;
            },
            doc => JsonConvert.DeserializeObject<TData>(doc["Data"].AsString) ?? throw new InvalidOperationException("Deserialization of Entity Failed"));
        
        _registrated = true;
    }

    private static Func<TData, string> GetAcessor()
    {
        var convertMetod = typeof(Convert).GetMethod(nameof(Convert.ToString), new[] { typeof(object) });

        if(convertMetod is null)
            throw new InvalidOperationException("Convert Method not found");
        
        var parameter = Expression.Parameter(typeof(TData));
        var exp = Expression.Block(
            new[] { parameter },
            Expression.Call(convertMetod, Expression.Convert(Expression.PropertyOrField(parameter, "Id"), typeof(object)))
        );

        return Expression.Lambda<Func<TData, string>>(exp, parameter)
           .CompileFast();
    }
}