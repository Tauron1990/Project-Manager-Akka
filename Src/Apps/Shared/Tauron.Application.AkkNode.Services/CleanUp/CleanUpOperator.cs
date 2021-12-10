using System;
using System.Linq;
using System.Reactive.Linq;
using SharpRepository.Repository;
using Tauron.Application.VirtualFiles;
using Tauron.Features;

namespace Tauron.Application.AkkaNode.Services.CleanUp;

public sealed class CleanUpOperator : ActorFeatureBase<CleanUpOperator.State>
{
    public static IPreparedFeature New(IRepository<CleanUpTime, string> cleanUp, IRepository<ToDeleteRevision, string> revisions, IDirectory bucket)
        => Feature.Create(() => new CleanUpOperator(), _ => new State(cleanUp, revisions, bucket));

    protected override void ConfigImpl()
        => Receive<StartCleanUp>(
            obs => obs.Take(1)
               .Select(s => new { s.State, Data = s.State.CleanUp.Get(CleanUpManager.TimeKey) })
               .Where(d => d.Data.Last + d.Data.Interval < DateTime.Now)
               .Select(
                    d => (Data: d.State.Revisions.AsQueryable().AsEnumerable()
                             .Select(
                                  revision =>
                                  {
                                      d.State.Bucked.GetFile(revision.FilePath).Delete();

                                      return revision;
                                  }),
                          Repo: d.State.Revisions))
               .ToUnit(
                    enu =>
                    {
                        var (data, repo) = enu;
                        using var batch = repo.BeginBatch();
                        batch.Delete(data);
                        batch.Commit();
                    })
               .Finally(() => Context.Stop(Self))
               .Subscribe(_ => { }, ex => Log.Error(ex, "Error on Clean up Database")));

    public sealed record State(IRepository<CleanUpTime, string> CleanUp, IRepository<ToDeleteRevision, string> Revisions, IDirectory Bucked);
}