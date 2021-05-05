using System;
using SharpRepository.Repository;
using SharpRepository.Repository.Caching;

namespace AkkaTest.JsonRepo
{
    /// <summary>
    /// XML Repository layer
    /// </summary>
    /// <typeparam name="T">The object type that is stored as XML.</typeparam>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
    public class JsonRepository<T, TKey> : JsonRepositoryBase<T, TKey> 
        where T : class, new()
        where TKey : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRepository&lt;T, TKey&gt;"/> class.
        /// </summary>
        /// <param name="storagePath">Path to the directory where the XML files are stored.  The XML filename is determined by the TypeName</param>
        /// <param name="cachingStrategy">The caching strategy.  Defaults to <see cref="NoCachingStrategy&lt;T, TKey&gt;" />.</param>
        public JsonRepository(string storagePath, ICachingStrategy<T, TKey>? cachingStrategy = null) : base(storagePath, cachingStrategy) 
        {
            if (string.IsNullOrEmpty(storagePath)) throw new ArgumentNullException(nameof(storagePath));
        }
    }

    /// <summary>
    /// XML Repository layer
    /// </summary>
    /// <typeparam name="T">The object type that is stored as XML.</typeparam>
    public class XmlRepository<T> : JsonRepositoryBase<T, int>, IRepository<T> where T : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRepository&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="storagePath">Path to the directory where the XML files are stored.  The XML filename is determined by the TypeName</param>
        /// <param name="cachingStrategy">The caching strategy.  Defaults to <see cref="NoCachingStrategy&lt;T&gt;" />.</param>
        public XmlRepository(string storagePath, ICachingStrategy<T, int>? cachingStrategy = null) : base(storagePath, cachingStrategy) 
        {
        }
    }
}