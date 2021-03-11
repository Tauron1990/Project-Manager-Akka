using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    public sealed record CleanUpTime(ObjectId Id, TimeSpan Interval, DateTime Last, [property:BsonIgnore] bool IsChanged = false) : IChangeTrackable;
}