namespace SimpleProjectManager.Server.Data;

public interface IInternalFileRepository
{
    ValueTask<Stream> OpenStream(string id, CancellationToken cancellationToken);

    IFindQuery<string> FindIdByFileName(string fileName);
    void Delete(string id);
}