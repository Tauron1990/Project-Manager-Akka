using System.Runtime.CompilerServices;
using LiteDB;
using LiteDB.Async;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteFileRepository : IInternalFileRepository
{
    private readonly ILiteStorageAsync<string> _databaseAsync;

    public LiteFileRepository(ILiteDatabaseAsync databaseAsync)
        => _databaseAsync = databaseAsync.FileStorage;

    public async ValueTask<Stream> OpenStream(string id, CancellationToken cancellationToken)
        => await _databaseAsync.OpenReadAsync(id);

    public IEnumerable<string> FindIdByFileName(string fileName)
    {
        var filesTask = _databaseAsync.FindAllAsync();

        if(!filesTask.Wait(TimeSpan.FromMinutes(1)))
            return Array.Empty<string>();
        
        var files = filesTask.Result;

        return files.Where(f => f.Filename == fileName).Select(f => f.Id);
    }

    public async ValueTask<FileEntry?> FindByIdAsync(string id, CancellationToken token = default)
    {
        var file = await _databaseAsync.FindByIdAsync(id);

        if(file is null) return null;

        return new FileEntry(
            file.Id,
            file.Filename,
            file.Metadata[nameof(FileEntry.JobName)].AsString,
            file.Metadata[nameof(FileEntry.FileName)].AsString,
            file.Length);
    }

    public void Delete(string id)
        => _databaseAsync.DeleteAsync(id).Wait(TimeSpan.FromMinutes(1));

    public async IAsyncEnumerable<FileEntry> FindAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var file in await _databaseAsync.FindAllAsync())
        {
            yield return new FileEntry(
                file.Id,
                file.Filename,
                file.Metadata[nameof(FileEntry.JobName)].AsString,
                file.Metadata[nameof(FileEntry.FileName)].AsString,
                file.Length);
        }
    }

    public async ValueTask DeleteAsync(string id, CancellationToken token)
        => await _databaseAsync.DeleteAsync(id);

    public async ValueTask UploadFromStreamAsync(string id, string fileId, Stream stream, string jobName, string fileName, CancellationToken token)
    {
        var doc = new BsonDocument
                  {
                      { nameof(FileEntry.JobName), new BsonValue(jobName) },
                      { nameof(FileEntry.FileName), new BsonValue(fileName) }
                  };

        await _databaseAsync.UploadAsync(id, fileId, stream, doc);
    }
}