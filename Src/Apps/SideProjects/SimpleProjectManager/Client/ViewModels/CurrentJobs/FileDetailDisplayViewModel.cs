using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileDetailDisplayViewModel : StatefulViewModel<ProjectFileInfo?>
{
    private readonly IJobFileService _fileService;

    public Interaction<Unit, ProjectFileId?> Id { get; } = new();
    
    public FileDetailDisplayViewModel(IStateFactory stateFactory, IJobFileService fileService) 
        : base(stateFactory)
    {
        _fileService = fileService;
    }

    protected override async Task<ProjectFileInfo?> ComputeState(CancellationToken cancellationToken)
    {
        var id = await Id.Handle(Unit.Default);

        if (id is null) return null;

        return await _fileService.GetJobFileInfo(id, cancellationToken);
    }
}