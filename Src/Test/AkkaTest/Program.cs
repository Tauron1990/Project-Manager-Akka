using System;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using tiesky.com;

namespace AkkaTest
{
    public sealed record ToDeleteRevision(ObjectId Id, string BuckedId, [property: BsonIgnore] bool IsChanged = false);

    internal static class Program
    {

        private static void Main()
        {
            var doc = new BsonDocument();

            BsonSerializer.Serialize(new BsonDocumentWriter(doc), new ToDeleteRevision(ObjectId.Empty, "Test", true));
            var test = BsonSerializer.Deserialize<ToDeleteRevision>(doc);

        }
    }
}