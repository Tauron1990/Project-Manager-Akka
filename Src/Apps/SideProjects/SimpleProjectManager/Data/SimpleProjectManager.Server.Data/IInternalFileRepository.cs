namespace SimpleProjectManager.Server.Data;

public interface IInternalFileRepository
{
    IOperationFactory<FileEntry> Operations { get; }

    ValueTask<Stream> OpenStream(string id, CancellationToken cancellationToken);
    
    IFindQuery<string> FindIdByFileName(string fileName);
    
    void Delete(string id);
    
    ValueTask<IAsyncEnumerable<FileEntry>> FindAsync(IFilter<FileEntry> filterEmpty, CancellationToken cancellationToken);
    ValueTask DeleteAsync(string id, CancellationToken token);
    ValueTask UploadFromStreamAsync(string id, string fileId, Stream stream, string jobName, string fileName, CancellationToken token);
}