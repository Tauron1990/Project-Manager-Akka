using System.Threading.Tasks;
using JetBrains.Annotations;
using MongoDB.Driver.GridFS;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.DataFactorys;

namespace Tauron.Application.AkkaNode.Services.MongoDb
{
    public sealed record GridFSBucketEntity(GridFSBucket Bucket) : IStateEntity;

    [PublicAPI]
    public sealed class BuckedSourceFactory : SingleValueDataFactory<GridFSBucketEntity>
    {
        private readonly GridFSBucket _bucket;

        public BuckedSourceFactory(GridFSBucket bucket) { }
        protected override Task<GridFSBucketEntity> CreateValue(CreationMetadata? metadata) => Task.FromResult(new GridFSBucketEntity(_bucket));
    }
}