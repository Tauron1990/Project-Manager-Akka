using System.Runtime.CompilerServices;
using LiteDB;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteFileRepository : IInternalFileRepository
{
    private readonly ILiteStorage<string> _databaseAsync;

    public LiteFileRepository(ILiteDatabase databaseAsync)
        => _databaseAsync = databaseAsync.FileStorage;

    public ValueTask<Stream> OpenStream(string id, CancellationToken cancellationToken)
        => To.Task<Stream>(() => _databaseAsync.OpenRead(id));

    public IEnumerable<string> FindIdByFileName(string fileName)
    {
        var files = _databaseAsync.FindAll();
        
        return files.Where(f => f.Filename == fileName).Select(f => f.Id);
    }

    public ValueTask<FileEntry?> FindByIdAsync(string id, CancellationToken token = default)
        => To.Task(
            () =>
            {
                var file = _databaseAsync.FindById(id);

                if(file is null) return null;

                return new FileEntry(
                    file.Id,
                    file.Filename,
                    file.Metadata[nameof(FileEntry.JobName)].AsString,
                    file.Metadata[nameof(FileEntry.FileName)].AsString,
                    file.Length);
            });

    public void Delete(string id)
        => _databaseAsync.Delete(id);

    public async IAsyncEnumerable<FileEntry> FindAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        foreach (var file in _databaseAsync.FindAll())
        {
            yield return new FileEntry(
                file.Id,
                file.Filename,
                file.Metadata[nameof(FileEntry.JobName)].AsString,
                file.Metadata[nameof(FileEntry.FileName)].AsString,
                file.Length);
        }
    }

    public ValueTask DeleteAsync(string id, CancellationToken token)
        => To.TaskV(() => _databaseAsync.Delete(id));

    public ValueTask UploadFromStreamAsync(string id, string fileId, Stream stream, string jobName, string fileName, CancellationToken token)
        => To.TaskV(
            () =>
            {
                var doc = new BsonDocument
                          {
                              { nameof(FileEntry.JobName), new BsonValue(jobName) },
                              { nameof(FileEntry.FileName), new BsonValue(fileName) }
                          };

                _databaseAsync.Upload(id, fileId, stream, doc);
            });
}