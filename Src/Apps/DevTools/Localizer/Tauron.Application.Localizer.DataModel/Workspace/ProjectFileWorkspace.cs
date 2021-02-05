using System;
using System.Reactive.Subjects;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Workspace.Analyzing;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Localizer.DataModel.Workspace
{
    [PublicAPI]
    public sealed class ProjectFileWorkspace : Workspace<ProjectFileWorkspace, ProjectFile>, IObservable<ProjectFile>,
        IDisposable
    {
        private readonly BehaviorSubject<ProjectFile> _projectFile;

        public ProjectFileWorkspace(IActorRefFactory factory)
            : base(new WorkspaceSuperviser(factory, "Project_File_Workspace"))
        {
            _projectFile = new BehaviorSubject<ProjectFile>(new ProjectFile());

            Projects = new ProjectMutator(Engine, this);
            Source = new SourceMutator(Engine, this);
            Entrys = new EntryMutator(Engine);
            Build = new BuildMutator(Engine);

            Analyzer.RegisterRule(new SourceRule());
        }

        public ProjectFile ProjectFile => _projectFile.Value;

        public SourceMutator Source { get; }

        public ProjectMutator Projects { get; }

        public EntryMutator Entrys { get; }

        public BuildMutator Build { get; }

        public void Dispose()
        {
            _projectFile.Dispose();
        }

        public IDisposable Subscribe(IObserver<ProjectFile> observer) => _projectFile.Subscribe(observer);

        public Project Get(string name)
        {
            return ProjectFile.Projects.Find(p => p.ProjectName == name) ?? new Project();
        }

        protected override MutatingContext<ProjectFile> GetDataInternal()
            => MutatingContext<ProjectFile>.New(_projectFile.Value);

        protected override void SetDataInternal(MutatingContext<ProjectFile> data)
        {
            _projectFile.OnNext(data.Data);
        }
    }
}