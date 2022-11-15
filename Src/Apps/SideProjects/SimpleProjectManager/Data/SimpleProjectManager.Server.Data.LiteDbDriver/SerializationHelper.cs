using System.Linq.Expressions;
using System.Reflection;
using Akkatecture.ValueObjects;
using FastExpressionCompiler;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NewtonSerializer = Newtonsoft.Json.JsonSerializer;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

internal static class SerializationHelper
{
    internal static readonly NewtonSerializer PrivateSerializer = CreateSerializer();

    private static NewtonSerializer CreateSerializer()
    {


        var settings = new JsonSerializerSettings();

        return NewtonSerializer.Create(settings);
    }

    internal static void Init()
    {
        BsonMapper.Global.EnumAsInteger = true;
        BsonMapper.Global.ResolveMember +=
            (_, info, mapper) =>
            {
                Type? memberType = info switch
                {
                    PropertyInfo propertyInfo => propertyInfo.PropertyType,
                    FieldInfo fieldInfo => fieldInfo.FieldType,
                    _ => null
                };

                if(memberType is null || !memberType.IsAssignableTo(typeof(ISingleValueObject))) return;

                Type? targetType = memberType;

                while (targetType is not null && !string.Equals(targetType.Name, "SingleValueObject`1", StringComparison.Ordinal))
                    targetType = targetType.BaseType;

                if(targetType is null) return;

                targetType = targetType.GetGenericArguments()[0];

                mapper.Serialize = (target, bsonMapper) => bsonMapper.Serialize(((ISingleValueObject)target).GetValue());
                mapper.Deserialize = (value, bsonMapper) => Activator.CreateInstance(memberType, bsonMapper.Deserialize(targetType, value));
            };
    }
}

/*public sealed class TestValue : SingleValueObject<bool>
{
    public TestValue(bool value)
        : base(value)
    {
        
    }
}

public sealed class TestId : Identity<TestId>
{
    public TestId(string value)
        : base(value)
    {
        
    }
}

public sealed record TestClass
{
    public TestId Id { get; init; }

    public TestValue TestValue { get; init; }
}*/

internal static class SerializationHelper<TData>
{
    // ReSharper disable once StaticMemberInGenericType
    private static bool _registrated = true;

    // ReSharper disable once CognitiveComplexity
    internal static void Register()
    {
        if(_registrated) return;

        if(IsPrimitive())
        {
            _registrated = true;

            return;
        }

        var accessor = GetAcessor();


        BsonMapper.Global.RegisterType(
            t =>
            {
                if(t is null)
                    return new BsonDocument();

                JToken token = JToken.FromObject(t, SerializationHelper.PrivateSerializer);
                BsonValue boson = MapBson(token);

                if(boson is BsonDocument doc)
                    doc["_id"] = new BsonValue(accessor(t));

                return boson;
            },
            doc =>
            {
                if(doc is BsonDocument bsonDocument && bsonDocument.ContainsKey("_id"))
                    bsonDocument.Remove("_id");

                JToken? token = MapJToken(doc);

                if(token is null) return default!;

                var result = token.ToObject<TData>(SerializationHelper.PrivateSerializer);

                return result;

            });

        _registrated = true;
    }

    private static bool IsPrimitive()
    {
        Type dataType = typeof(TData);

        return dataType.IsPrimitive || dataType.IsEnum || dataType.IsArray || dataType == typeof(string) || dataType == typeof(Guid)
            || dataType == typeof(DateTime);
    }

    private static Func<TData, string> GetAcessor()
    {
        MethodInfo? convertMetod = typeof(Convert).GetMethod(nameof(Convert.ToString), new[] { typeof(object) });

        if(convertMetod is null)
            throw new InvalidOperationException("Convert Method not found");

        ParameterExpression parameter = Expression.Parameter(typeof(TData));
        BlockExpression exp = Expression.Block(
            new[] { parameter },
            Expression.Call(convertMetod, Expression.Convert(Expression.PropertyOrField(parameter, "Id"), typeof(object)))
        );

        return Expression.Lambda<Func<TData, string>>(exp, parameter)
           .CompileFast();
    }

    private static BsonValue MapBson(JToken? token)
        => token switch
        {
            null => BsonValue.Null,
            JArray array => new BsonArray(array.Select(MapBson)),
            JObject jObject => new BsonDocument(jObject.ToDictionary<KeyValuePair<string, JToken?>, string, BsonValue>(p => p.Key, p => MapBson(p.Value), StringComparer.Ordinal)),
            JValue jValue => new BsonValue(jValue.Value),
            _ => throw new InvalidOperationException($"Token tyoe not Supported: {token.GetType()}")
        };

    private static JToken? MapJToken(BsonValue value)
        => value switch
        {
            BsonArray array => new JArray(array.Select(MapJToken)),
            BsonDocument document => new JObject(document.Select(p => new JProperty(p.Key, MapJToken(p.Value)))),
            _ => value.Type switch
            {
                BsonType.Null => null,
                BsonType.Int32 => new JValue(value.AsInt32),
                BsonType.Int64 => new JValue(value.AsInt64),
                BsonType.Double => new JValue(value.AsDouble),
                BsonType.Decimal => new JValue(value.AsDecimal),
                BsonType.String => new JValue(value.AsString),
                BsonType.Binary => new JValue(value.AsBinary),
                BsonType.ObjectId => new JValue(value.AsObjectId.ToString()),
                BsonType.Guid => new JValue(value.AsGuid),
                BsonType.Boolean => new JValue(value.AsBoolean),
                BsonType.DateTime => new JValue(value.AsDateTime),
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"BsonValue type not Supported: {value.Type}")
            }
        };
}