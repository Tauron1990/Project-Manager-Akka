using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Client.Shared.Data.Files;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.Data.States;

public sealed class FilesState : StateBase
{
    public static readonly FileMime[] AllowedContentTypes =
    {
        new("application/pdf"), new("application/x-zip-compressed"), new("application/zip"), new("image/tiff"), new("image/x-tiff")
    };

    private readonly IMessageDispatcher _aggregator;

    private readonly IJobFileService _service;
    private readonly Func<UploadTransaction> _transactionFactory;

    public FilesState(IStateFactory stateFactory, IJobFileService jobFileService, Func<UploadTransaction> uploadTransaction, IMessageDispatcher aggregator)
        : base(stateFactory)
    {
        _service = jobFileService;

        AllFiles = FromServer(jobFileService.GetAllFiles, Array.Empty<DatabaseFile>());
        _transactionFactory = uploadTransaction;
        _aggregator = aggregator;
    }

    public IObservable<DatabaseFile[]> AllFiles { get; }

    public static SimpleResult IsFileValid(IFileReference file)
    {
        bool result = AllowedContentTypes.Any(t => t == file.ContentType);

        return !result 
            ? SimpleResult.Failure($"Die Datei {file.Name} kann nicht Hochgeladen werden. Nur Tiff, zip und Pdf sinf erlaubt") 
            : SimpleResult.Success();
    }

    public async Task DeleteFile(DatabaseFile file, CancellationToken token)
        => await _service.DeleteFiles(new FileList(ImmutableList<ProjectFileId>.Empty.Add(file.Id)), token).ConfigureAwait(false);

    public UploadTransaction CreateUpload()
        => _transactionFactory();

    public IObservable<ProjectFileInfo?> QueryFileInfo(IState<ProjectFileId?> id)
    {
        async Task<ProjectFileInfo?> ComputeState(IComputedState<ProjectFileInfo?> unused, CancellationToken cancellationToken)
        {
            ProjectFileId? actualId = await id.Use(cancellationToken).ConfigureAwait(false);

            if(actualId is null) return null;

            return await _service.GetJobFileInfo(actualId, cancellationToken).ConfigureAwait(false);
        }

        return Observable.Defer(
            () =>
            {
                var computer = StateFactory.NewComputed<ProjectFileInfo?>(ComputeState);

                return computer.ToObservable(
                    ex =>
                    {
                        _aggregator.PublishError(ex);

                        return true;
                    }).Finally(computer.Dispose);
            });
    }
}