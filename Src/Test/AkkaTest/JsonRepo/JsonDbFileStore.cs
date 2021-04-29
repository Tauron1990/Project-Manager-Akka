using System;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;

namespace AkkaTest.JsonRepo
{
    public static class JsonDbFileStore
    {
        private static readonly object _saveLock = new();
        private static readonly ConcurrentDictionary<FileKey, object> Store = new();
        private sealed record FileKey(string FileName, Type EntityType, Type KeyType);

        public static ConcurrentDictionary<TKey, TData> Get<TKey, TData>(string file, Func<ConcurrentDictionary<TKey, TData>> load)
            where TKey : notnull
        {
            return (ConcurrentDictionary<TKey, TData>)Store.GetOrAdd(new FileKey(file, typeof(TData), typeof(TKey)), _ => load());
        }

        public static void Save(string path, object obj)
        {
            lock (_saveLock)
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
            }
        }
    }
}