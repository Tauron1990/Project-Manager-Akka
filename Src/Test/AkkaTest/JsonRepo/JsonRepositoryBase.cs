using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SharpRepository.Repository;
using SharpRepository.Repository.Caching;
using SharpRepository.Repository.FetchStrategies;
using Tauron;

namespace AkkaTest.JsonRepo
{
    public abstract class JsonRepositoryBase<T, TKey> : LinqRepositoryBase<T, TKey> 
        where T : class, new()
        where TKey : notnull
    {
        private string _storagePath = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storagePath">Path to the directory.  The XML filename is determined by the TypeName</param>
        /// <param name="cachingStrategy"></param>
        internal JsonRepositoryBase(string storagePath, ICachingStrategy<T, TKey> cachingStrategy = null) : base(cachingStrategy) 
        {
            Initialize(storagePath);
        }

        private void Initialize(string storagePath)
        {
            _storagePath = Path.Combine(storagePath, TypeName + ".json");

            // load up the items
            LoadItems();
        }

        private void LoadItems()
        {
            if (!File.Exists(_storagePath)) return;

            using var stream = new FileStream(_storagePath, FileMode.Open);
            using var reader = new StreamReader(stream);
            Items = JsonConvert.DeserializeObject<Dictionary<TKey, T>>(reader.ReadToEnd()) ?? new Dictionary<TKey, T>();
        }

        private Dictionary<TKey, T> Items { get; set; } = new();

        protected override IQueryable<T> BaseQuery(IFetchStrategy<T> fetchStrategy = null)
        {
            return Items.Values.AsQueryable();
        }

        protected override T GetQuery(TKey key, IFetchStrategy<T> fetchStrategy)
        {
            return BaseQuery(fetchStrategy).FirstOrDefault(x => MatchOnPrimaryKey(x, key));
        }

        protected override void AddItem(T entity)
        {
            if (GenerateKeyOnAdd && GetPrimaryKey(entity, out TKey id) && Equals(id, default(TKey)))
            {
                id = GeneratePrimaryKey();
                SetPrimaryKey(entity, id);
            }

            Items.Add(entity);
        }

        protected override void DeleteItem(T entity)
        {
            GetPrimaryKey(entity, out TKey pkValue);

            var index = Items.FindIndex(x => MatchOnPrimaryKey(x, pkValue));
            if (index >= 0)
            {
                Items.RemoveAt(index);
            }
        }

        protected override void UpdateItem(T entity)
        {
            GetPrimaryKey(entity, out TKey pkValue);

            var index = Items.FindIndex(x => MatchOnPrimaryKey(x, pkValue));
            if (index >= 0)
            {
                Items[index] = entity;
            }
        }

        // need to match on primary key instead of using Equals() since the objects are not the same
        private bool MatchOnPrimaryKey(T item, TKey keyValue)
        {
            return GetPrimaryKey(item, out TKey value) && keyValue.Equals(value);
        }

        protected override void SaveChanges()
        {
            var writer = new StreamWriter(_storagePath, false);
            var serializer = new XmlSerializer(typeof(List<T>));
            serializer.Serialize(writer, Items);
            writer.Close();
        }

        public override void Dispose()
        {
        }

        protected virtual TKey GeneratePrimaryKey()
        {
            if (typeof(TKey) == typeof(Guid))
            {
                return (TKey)Convert.ChangeType(Guid.NewGuid(), typeof(TKey));
            }

            if (typeof(TKey) == typeof(string))
            {
                return (TKey)Convert.ChangeType(Guid.NewGuid().ToString("N"), typeof(TKey));
            }

            var last = Items.LastOrDefault() ?? new T();

            if (typeof(TKey) == typeof(Int32))
            {
                GetPrimaryKey(last, out TKey pkValue);

                var nextInt = Convert.ToInt32(pkValue) + 1;
                return (TKey)Convert.ChangeType(nextInt, typeof(TKey));
            }

            throw new InvalidOperationException("Primary key could not be generated. This only works for GUID, Int32 and String.");
        }

        public override string ToString()
        {
            return "SharpRepository.XmlRepository";
        }
    }
}
