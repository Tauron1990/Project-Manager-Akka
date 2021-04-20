using System;
using System.Collections.Concurrent;
using SharpRepository.InMemoryRepository;
using SharpRepository.Repository;
using SharpRepository.Repository.Configuration;

namespace AkkaTest.InMemoryStorage
{
    public class PersistentInMemorxConfigRepositoryFactory : ConfigRepositoryFactory
    {
        private static readonly ConcurrentDictionary<Type, object> _repositorys = new();

        public PersistentInMemorxConfigRepositoryFactory(IRepositoryConfiguration config)
            : base(config)
        {
        }

        public override IRepository<T> GetInstance<T>()
            => (IRepository<T>) _repositorys.GetOrAdd(typeof(IRepository<T>), new InMemoryRepository<T>());

        public override IRepository<T, TKey> GetInstance<T, TKey>()
            => (IRepository<T, TKey>) _repositorys.GetOrAdd(typeof(IRepository<T, TKey>), new InMemoryRepository<T, TKey>());

        public override ICompoundKeyRepository<T, TKey, TKey2> GetInstance<T, TKey, TKey2>() 
            => (ICompoundKeyRepository<T, TKey, TKey2>)_repositorys.GetOrAdd(typeof(ICompoundKeyRepository<T, TKey, TKey2>), new InMemoryRepository<T, TKey, TKey2>());

        public override ICompoundKeyRepository<T, TKey, TKey2, TKey3> GetInstance<T, TKey, TKey2, TKey3>() 
            => (ICompoundKeyRepository<T, TKey, TKey2, TKey3>)_repositorys.GetOrAdd(typeof(ICompoundKeyRepository<T, TKey, TKey2, TKey3>), new InMemoryRepository<T, TKey, TKey2, TKey3>());

        public override ICompoundKeyRepository<T> GetCompoundKeyInstance<T>() 
            => (ICompoundKeyRepository<T>) _repositorys.GetOrAdd(typeof(ICompoundKeyRepository<T>), new InMemoryCompoundKeyRepository<T>());
    }
}