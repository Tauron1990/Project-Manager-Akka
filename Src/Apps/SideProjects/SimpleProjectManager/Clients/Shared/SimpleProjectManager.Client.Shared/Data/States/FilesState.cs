using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Client.Shared.Data.Files;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Client.Shared.Data.States;

public sealed class FilesState : StateBase
{
    public static readonly string[] AllowedContentTypes =
    {
        "application/pdf", "application/x-zip-compressed", "application/zip", "image/tiff", "image/x-tiff"
    };
    
    public const long MaxSize = 524_288_000;
    
    public static string IsFileValid(IFileReference file)
    {
        var result = AllowedContentTypes.Any(t => t == file.ContentType);
        return !result ? $"Die Datei {file.Name} kann nicht Hochgeladen werden. Nur Tiff, zip und Pdf sinf erlaubt" : string.Empty;
    }
    
    private readonly IJobFileService _service;
    private readonly Func<UploadTransaction> _transactionFactory;

    public IObservable<DatabaseFile[]> AllFiles { get; }

    public FilesState(IStateFactory stateFactory, IJobFileService jobFileService, Func<UploadTransaction> uploadTransaction)
        : base(stateFactory)
    {
        _service = jobFileService;

        AllFiles = FromServer(_service.GetAllFiles);
        _transactionFactory = uploadTransaction;
    }

    public async Task DeleteFile(DatabaseFile file, CancellationToken token)
        => await _service.DeleteFiles(new FileList(ImmutableList<ProjectFileId>.Empty.Add(file.Id)), token);

    public UploadTransaction CreateUpload()
        => _transactionFactory();

    public IObservable<ProjectFileInfo?> QueryFileInfo(IState<ProjectFileId?> id)
    {
        async Task<ProjectFileInfo?> ComputeState(IComputedState<ProjectFileInfo?> unused, CancellationToken cancellationToken)
        {
            var actualId = await id.Use(cancellationToken);

            if (actualId is null) return null;

            return await _service.GetJobFileInfo(actualId, cancellationToken);
        }

        return Observable.Defer(
            () =>
            {
                var computer = StateFactory.NewComputed<ProjectFileInfo?>(ComputeState);

                return computer.ToObservable().Finally(computer.Dispose);
            });
    }
}