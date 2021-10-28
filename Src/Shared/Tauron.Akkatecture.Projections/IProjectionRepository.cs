using System;
using System.Threading.Tasks;
using Akkatecture.Core;
using JetBrains.Annotations;
using LiquidProjections;

namespace Tauron.Akkatecture.Projections
{
    [PublicAPI]
    public interface IProjectionRepository
    {
        Task<TProjection?> Get<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
            where TProjection : class, IProjectorData<TIdentity>
            where TIdentity : IIdentity;

        Task<TProjection> Create<TProjection, TIdentity>(
            ProjectionContext context, TIdentity identity,
            Func<TProjection, bool> shouldoverwite)
            where TProjection : class, IProjectorData<TIdentity>
            where TIdentity : IIdentity;

        Task<bool> Delete<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
            where TProjection : class, IProjectorData<TIdentity>
            where TIdentity : IIdentity;

        Task Commit<TProjection, TIdentity>(ProjectionContext context, TProjection projection, TIdentity identity)
            where TIdentity : IIdentity
            where TProjection : class, IProjectorData<TIdentity>;
    

        Task Completed<TIdentity>(TIdentity identity)
                where TIdentity : IIdentity;

        long GetLastCheckpoint<TProjection, TIdentity>()
            where TProjection : class, IProjectorData<TIdentity>
            where TIdentity : IIdentity;
    }
}