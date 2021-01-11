using System;
using MongoDB.Bson;

namespace Tauron.Application.AkkNode.Services.CleanUp
{
    public sealed record CleanUpTime(ObjectId Id, TimeSpan Interval, DateTime Last);
}