using SimpleProjectManager.Client.Shared.CurrentJobs;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;
using SimpleProjectManager.Shared;
using Stl.Fusion;
using Tauron.Application.Blazor.Parameters;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileDetailDisplayViewModel : FileDetailDisplayViewModelBase, IParameterUpdateable
{
    private readonly IStateFactory _stateFactory;
    
    public FileDetailDisplayViewModel(GlobalState state, IStateFactory stateFactory) : base(state)
        => _stateFactory = stateFactory;

    protected override IState<ProjectFileId?> GetId()
        => Updater.Register<ProjectFileId?>(nameof(FileDetailDisplay.FileId), _stateFactory);

    public ParameterUpdater Updater { get; } = new();
}