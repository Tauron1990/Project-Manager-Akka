using System;
using MongoDB.Bson;

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    public sealed record CleanUpTime(ObjectId Id, TimeSpan Interval, DateTime Last);
}