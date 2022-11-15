using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace SimpleProjectManager.Server.Data.MongoDb;

public sealed class InternalFileRepository : IInternalFileRepository
{
    private readonly GridFSBucket _bucket;

    public InternalFileRepository(GridFSBucket bucket)
        => _bucket = bucket;

    public async ValueTask<Stream> OpenStream(string id, CancellationToken cancellationToken)
        => await _bucket.OpenDownloadStreamAsync(ObjectId.Parse(id), cancellationToken: cancellationToken).ConfigureAwait(false);

    public IEnumerable<string> FindIdByFileName(string fileName)
    {
        var filter = Builders<GridFSFileInfo<ObjectId>>.Filter.Eq(d => d.Filename, fileName);

        return _bucket.Find(filter).ToEnumerable().Select(f => f.Filename);
    }

    public async ValueTask<FileEntry?> FindByIdAsync(string id, CancellationToken token = default)
    {
        var result = await _bucket.FindAsync(Builders<GridFSFileInfo<ObjectId>>.Filter.Eq(f => f.Id, ObjectId.Parse(id)), cancellationToken: token).ConfigureAwait(false);
        var file = await result.FirstOrDefaultAsync(token).ConfigureAwait(false);

        // new DatabaseFile(
        //     new ProjectFileId(d.Filename),
        //     new FileName(d.Metadata.GetValue(FileContentManager.MetaFileNme).AsString),
        //     new FileSize(d.Length),
        //     new JobName(d.Metadata.GetValue(FileContentManager.MetaJobName).AsString))),

        return new FileEntry(
            id,
            file.Filename,
            file.Metadata.GetValue(nameof(FileEntry.JobName)).AsString,
            file.Metadata.GetValue(nameof(FileEntry.FileName)).AsString,
            file.Length);
    }

    public void Delete(string id)
        => _bucket.Delete(ObjectId.Parse(id));

    public async IAsyncEnumerable<FileEntry> FindAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await _bucket.FindAsync(Builders<GridFSFileInfo<ObjectId>>.Filter.Empty, cancellationToken: cancellationToken).ConfigureAwait(false);

        if(!await result.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            yield break;

        foreach (var file in result.Current)
            yield return new FileEntry(
                file.Id.ToString(),
                file.Filename,
                file.Metadata.GetValue(nameof(FileEntry.JobName)).AsString,
                file.Metadata.GetValue(nameof(FileEntry.FileName)).AsString,
                file.Length);
    }

    public async ValueTask DeleteAsync(string id, CancellationToken token)
        => await _bucket.DeleteAsync(ObjectId.Parse(id), token).ConfigureAwait(false);

    public async ValueTask UploadFromStreamAsync(string id, string fileId, Stream stream, string jobName, string fileName, CancellationToken token)
    {
        var meta = new BsonDocument
                   {
                       new BsonElement(nameof(FileEntry.FileName), fileName),
                       new BsonElement(nameof(FileEntry.JobName), jobName)
                   };

        await _bucket.UploadFromStreamAsync(ObjectId.Parse(id), fileId, stream, new GridFSUploadOptions { Metadata = meta }, token).ConfigureAwait(false);
    }
}