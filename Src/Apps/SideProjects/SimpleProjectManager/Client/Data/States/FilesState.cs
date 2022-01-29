using System.Collections.Immutable;
using Microsoft.AspNetCore.Components.Forms;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Data.States;

public sealed class FilesState : StateBase
{
    private static readonly string[] AllowedContentTypes =
    {
        "application/pdf", "application/x-zip-compressed", "application/zip", "image/tiff", "image/x-tiff"
    };
    
    private const long MaxSize = 524_288_000;
    
    public static string IsFileValid(IBrowserFile file)
    {
        var result = AllowedContentTypes.Any(t => t == file.ContentType);
        return !result ? $"Die Datei {file.Name} kann nicht Hochgeladen werden. Nur Tiff, zip und Pdf sinf erlaubt" : string.Empty;
    }
    
    private readonly IJobFileService _service;
    private readonly Func<UploadTransaction> _transactionFactory;

    public IObservable<DatabaseFile[]> AllFiles { get; }

    public FilesState(IStateFactory stateFactory)
        : base(stateFactory)
    {
        _service = stateFactory.Services.GetRequiredService<IJobFileService>();

        AllFiles = FromServer(_service.GetAllFiles);
        _transactionFactory = stateFactory.Services.GetRequiredService<UploadTransaction>;
    }

    public async Task DeleteFile(DatabaseFile file, CancellationToken token)
        => await _service.DeleteFiles(new FileList(ImmutableList<ProjectFileId>.Empty.Add(file.Id)), token);

    public UploadTransaction CreateUpload()
        => _transactionFactory();
}