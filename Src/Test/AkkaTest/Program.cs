using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace AkkaTest
{
    public record TestData(string Name, int Test1, string Hallo);

    internal static class Program
    {
        private static void Main()
        {
            var first = new TestData();
            var ser = BsonSerializer.SerializerRegistry.GetSerializer(typeof(TestData));
            using var writer = new StringWriter();
            ser.Serialize(BsonSerializationContext.CreateRoot(new JsonWriter(writer)), first);
            var second = ser.Deserialize(BsonDeserializationContext.CreateRoot(new JsonReader(writer.ToString())));

            //var alt = new Subject<string>();
            //var subTest = new Subject<string>();

            //subTest.Subscribe(Console.WriteLine, exception => Console.WriteLine(exception));

            //subTest.OnError(new InvalidOperationException());

            //var test = Result.Create(10); //Maybe.Just(10);

            //var result = from num in test
            //             let num2 = num + 1
            //             where num > 5
            //             select num + num2;
        }
    }
}