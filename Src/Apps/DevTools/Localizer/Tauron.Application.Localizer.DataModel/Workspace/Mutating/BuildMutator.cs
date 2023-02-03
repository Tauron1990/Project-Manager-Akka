using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    [PublicAPI]
    public sealed class BuildMutator
    {
        private readonly MutatingEngine<MutatingContext<ProjectFile>> _engine;

        public BuildMutator(MutatingEngine<MutatingContext<ProjectFile>> engine)
        {
            _engine = engine;
            Intigrate = engine.EventSource(
                mc => mc.GetChange<IntigrateImportChange>().ToEvent(),
                mc => mc.Change is IntigrateImportChange);
            ProjectPath = engine.EventSource(
                mc => mc.GetChange<ProjectPathChange>().ToEventData(),
                context => context.Change is ProjectPathChange);
        }

        public IEventSource<IntigrateImport> Intigrate { get; }

        public IEventSource<ProjectPathChanged> ProjectPath { get; }

        public void SetIntigrate(bool intigrate)
        {
            _engine.Mutate(
                nameof(SetIntigrate),
                obs => obs.Select(
                    mc => mc.Data.BuildInfo.IntigrateProjects == intigrate
                        ? mc
                        : mc.Update(
                            new IntigrateImportChange(intigrate),
                            mc.Data with
                            {
                                BuildInfo = mc.Data.BuildInfo with
                                            {
                                                IntigrateProjects = intigrate
                                            }
                            })));
        }

        public void SetProjectPath(string project, string path)
        {
            _engine.Mutate(
                nameof(SetProjectPath),
                obs => obs.Select(
                    mc =>
                    {
                        if (mc.Data.BuildInfo.ProjectPaths.TryGetValue(project, out var settetPath) && settetPath == path)
                            return mc;

                        return mc.Update(
                            new ProjectPathChange(path, project),
                            mc.Data with
                            {
                                BuildInfo = mc.Data.BuildInfo with
                                            {
                                                ProjectPaths = mc.Data.BuildInfo.ProjectPaths.SetItem(project, path)
                                            }
                            });
                    }));
        }
    }
}