using SimpleProjectManager.Client.Shared;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.EditJob;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Client.Shared.ViewModels.EditJob;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor.Parameters;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileUploaderViewModel : FileUploaderViewModelBase, IParameterUpdateable
{
    private readonly IStateFactory _factory;

    public FileUploaderViewModel(IMessageDispatcher aggregator, GlobalState globalState, IStateFactory factory)
        : base(aggregator, globalState)
        => _factory = factory;

    public ParameterUpdater Updater { get; } = new();

    protected override (IObservable<FileUploadTrigger> triggerUpload, IState<string> nameState) GetModelInformation()
        => (
            Updater.Register<FileUploadTrigger>(nameof(FileUploader.UploadTrigger), _factory).ToObservable(MessageDispatcher.PropagateErrors()),
            Updater.Register<string>(nameof(FileUploader.ProjectName), _factory)
        );
}