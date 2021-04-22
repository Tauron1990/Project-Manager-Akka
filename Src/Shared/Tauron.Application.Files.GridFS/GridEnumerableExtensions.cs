using System;
using System.Collections.Generic;

namespace Tauron.Application.Files.GridFS
{
    public static class GridEnumerableExtensions
    {
        public static bool IsSingle<TData>(this IEnumerable<TData> enumerable, Func<TData, bool> evaluator)
        {
            var found = false;

            foreach (var data in enumerable)
            {
                if (evaluator(data))
                {
                    if (found)
                        return false;

                    found = true;
                }
            }

            return found;
        }
    }
}