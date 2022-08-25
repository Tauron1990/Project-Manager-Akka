namespace SimpleProjectManager.Server.Data;

public interface IInternalFileRepository
{
    ValueTask<Stream> OpenStream(string id, CancellationToken cancellationToken);
    
    IEnumerable<string> FindIdByFileName(string fileName);

    ValueTask<FileEntry?> FindByIdAsync(string id, CancellationToken token = default);

    void Delete(string id);
    
    IAsyncEnumerable<FileEntry> FindAllAsync(CancellationToken cancellationToken);
    ValueTask DeleteAsync(string id, CancellationToken token);
    ValueTask UploadFromStreamAsync(string id, string fileId, Stream stream, string jobName, string fileName, CancellationToken token);
}