using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akkatecture.Core;
using JetBrains.Annotations;
using LiquidProjections;

namespace Tauron.Akkatecture.Projections;

[PublicAPI]
public class RepositoryProjectorMap<TProjection, TIdentity>
    where TProjection : class, IProjectorData<TIdentity>
    where TIdentity : IIdentity
{
    private readonly IProjectionRepository _repository;

    protected internal readonly ProjectorMap<TProjection, TIdentity, ProjectionContext> ProjectorMap;

    public RepositoryProjectorMap(IProjectionRepository repository)
    {
        _repository = repository;
        ProjectorMap = new ProjectorMap<TProjection, TIdentity, ProjectionContext>
                       {
                           Create = Create,
                           Delete = Delete,
                           Update = Update,
                           Custom = Custom
                       };
    }

    protected virtual Task Custom(ProjectionContext context, Func<Task> projector) => projector();

    protected virtual async Task Update(
        TIdentity key, ProjectionContext context, Func<TProjection, Task> projector,
        Func<bool> createifmissing)
    {
        try
        {
            TProjection? data = await _repository.Get<TProjection, TIdentity>(context, key);
            if(data is null)
            {
                if(createifmissing())
                    data = await _repository.Create<TProjection, TIdentity>(context, key, _ => true);
                else
                    throw new KeyNotFoundException($"The key {key} is not in The Repository");
            }

            await projector(data);
            await _repository.Commit(context, data, key);
        }
        finally
        {
            await _repository.Completed(key);
        }
    }

    protected virtual async Task<bool> Delete(TIdentity key, ProjectionContext context)
        => await _repository.Delete<TProjection, TIdentity>(context, key);

    protected virtual async Task Create(
        TIdentity key, ProjectionContext context, Func<TProjection, Task> projector,
        Func<TProjection, bool> shouldoverwite)
    {
        try
        {
            TProjection data = await _repository.Create(context, key, shouldoverwite);
            await projector(data);
            await _repository.Commit(context, data, key);
        }
        finally
        {
            await _repository.Completed(key);
        }
    }
}

//Update = async (key, context, pro, missing) =>
//{
//if (!store.TryGetValue(key, out var projection))
//{
//    if (!missing())
//        return;

//    projection = store.GetOrAdd(key, id => new TProjector { Id = id });
//}

//await pro(projection);
//},

//Custom = (context, pro) => pro()