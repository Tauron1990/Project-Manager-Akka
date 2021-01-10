using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace Tauron.Application.AkkNode.Services
{
    public static class MongoExtensions
    {
        public static bool Contains<TSource>(this IAsyncCursor<TSource> cursor, Func<TSource, bool> predicate)
        {
            while (cursor.MoveNext())
            {
                if (cursor.Current.Any(predicate))
                    return true;
            }

            return false;
        }

        public static IEnumerable<TSource> ToEnumerable<TSource>(this IAsyncCursor<TSource> cursor)
        {
            while (cursor.MoveNext())
            {
                foreach (var data in cursor.Current)
                    yield return data;
            }
        }
    }
}