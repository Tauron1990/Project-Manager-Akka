using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    [PublicAPI]
    public sealed class SourceMutator
    {
        private readonly MutatingEngine<MutatingContext<ProjectFile>> _engine;
        private readonly ProjectFileWorkspace _workspace;

        public SourceMutator(MutatingEngine<MutatingContext<ProjectFile>> engine, ProjectFileWorkspace workspace)
        {
            _engine = engine;
            _workspace = workspace;
            SaveRequest = engine.EventSource(
                _ => new SaveRequest(workspace.ProjectFile),
                context => !(context.Change is ResetChange));
            ProjectReset = engine.EventSource(
                _ => new ProjectRest(workspace.ProjectFile),
                context => context.Change is ResetChange);
            SourceUpdate = engine.EventSource(
                mc => new SourceUpdated(mc.Data.Source),
                context => context.Change is SourceChange);
        }

        public IEventSource<SaveRequest> SaveRequest { get; }

        public IEventSource<ProjectRest> ProjectReset { get; }

        public IEventSource<SourceUpdated> SourceUpdate { get; }

        public void Reset(ProjectFile file)
        {
            _workspace.Reset(file);
        }
        //    => _engine.Mutate(nameof(Reset), context => context.Update(new ResetChange(), file));

        public void UpdateSource(string file)
        {
            _engine.Mutate(
                nameof(UpdateSource),
                context => context.Select(c => c.Update(new SourceChange(), c.Data with { Source = file })));
        }
    }
}