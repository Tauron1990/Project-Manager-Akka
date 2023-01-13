using SimpleProjectManager.Client.Shared.CurrentJobs;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;
using SimpleProjectManager.Shared;
using Stl.Fusion;
using Tauron.Application.Blazor.Parameters;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileDetailDisplayViewModel : FileDetailDisplayViewModelBase, IParameterUpdateable
{
    public FileDetailDisplayViewModel(GlobalState state, IStateFactory stateFactory)
        : base(state, stateFactory) { }

    public ParameterUpdater Updater { get; } = new();

    protected override IState<ProjectFileId?> GetId(IStateFactory stateFactory)
        => Updater.Register<ProjectFileId?>(nameof(FileDetailDisplay.FileId), stateFactory);
}