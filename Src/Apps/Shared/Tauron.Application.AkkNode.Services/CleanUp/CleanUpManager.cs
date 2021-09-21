using System;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using SharpRepository.Repository;
using SharpRepository.Repository.Configuration;
using Tauron.Application.VirtualFiles;
using Tauron.Features;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    [PublicAPI]
    public sealed class CleanUpManager : ActorFeatureBase<CleanUpManager.CleanUpManagerState>
    {
        public const string TimeKey = "Master";
        public const string RepositoryKey = nameof(CleanUpManager);

        public static readonly InitCleanUp Initialization = new();

        [PublicAPI]
        public static IPreparedFeature New(ISharpRepositoryConfiguration repositoryConfiguration, IDirectory storageProvider)
            => Feature.Create(
                () => new CleanUpManager(),
                _ => new CleanUpManagerState(
                    repositoryConfiguration.GetInstance<CleanUpTime, string>(RepositoryKey),
                    repositoryConfiguration.GetInstance<ToDeleteRevision, string>(RepositoryKey),
                    storageProvider,
                    IsRunning: false));

        protected override void ConfigImpl()
        {
            Receive<InitCleanUp>(
                obs => obs
                   .Where(m => !m.State.IsRunning)
                   .Select(data => new { Data = data.State.CleanUpRepository.Get(TimeKey), data.State.CleanUpRepository, data.Timers, data.State })
                   .ApplyWhen(d => d.Data is null, d => d.CleanUpRepository.Add(new CleanUpTime(TimeKey, TimeSpan.FromDays(7), DateTime.Now)))
                   .StartPeriodicTimer(d => d.Timers, Initialization, new StartCleanUp(), TimeSpan.FromSeconds(1), TimeSpan.FromHours(1))
                   .Select(d => d.State with { IsRunning = true }));

            Receive<StartCleanUp>(
                obs => obs.Where(m => m.State.IsRunning)
                   .Select(data => (Props: CleanUpOperator.New(data.State.CleanUpRepository, data.State.Revisions, data.State.Bucket), data.Event))
                   .ForwardToActor(d => Context.ActorOf(d.Props), d => d.Event));

            SupervisorStrategy = SupervisorStrategy.StoppingStrategy;
        }

        public sealed record CleanUpManagerState(
            IRepository<CleanUpTime, string> CleanUpRepository, IRepository<ToDeleteRevision, string> Revisions,
            IDirectory Bucket, bool IsRunning);

        public sealed record InitCleanUp
        {
            internal InitCleanUp() { }
        }
    }
}