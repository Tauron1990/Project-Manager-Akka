using MongoDB.Bson;
using MongoDB.Driver;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Server.Core.Services;

public sealed record CriticalErrorEntry(ObjectId Id, CriticalError Error, bool IsDisabled);

public class CriticalErrorService : ICriticalErrorService
{
    private readonly ILogger<CriticalErrorService> _logger;
    private readonly IMongoCollection<CriticalErrorEntry> _errorEntrys;

    public CriticalErrorService(InternalDataRepository repository, ILogger<CriticalErrorService> logger)
    {
        _logger = logger;
        _errorEntrys = repository.Collection<CriticalErrorEntry>();
    }

    public virtual async Task<CriticalError[]> GetErrors(CancellationToken token)
    {
        var list = await _errorEntrys.Find(Builders<CriticalErrorEntry>.Filter.Eq(e => e.IsDisabled, false)).Project(d => d.Error).ToListAsync(token);

        return list.ToArray();
    }

    public virtual async Task<string> DisableError(string id, CancellationToken token)
    {
        var filter = Builders<CriticalErrorEntry>.Filter.Eq(e => e.Id, new ObjectId(id));
        var updater = Builders<CriticalErrorEntry>.Update.Set(e => e.IsDisabled, true);

        try
        {
            var result = await _errorEntrys.UpdateOneAsync(filter, updater, cancellationToken: token);

            if (!result.IsAcknowledged || result.ModifiedCount != 1) 
                return "Unbekannter fehler beim Aktualisieren der Datenbank";

            using (Computed.Invalidate())
                GetErrors(token).Ignore();
            return string.Empty;

        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error on Disabling ErrorEntry");

            return e.Message;
        }
    }

    public virtual async Task WriteError(CriticalError error, CancellationToken token)
    {
        try
        {
            var id = ObjectId.GenerateNewId();
            await _errorEntrys.InsertOneAsync(new CriticalErrorEntry(id, error with { Id = id.ToString() }, false), cancellationToken:token);
            using(Computed.Invalidate())
                GetErrors(token).Ignore();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on Write Error to Database");
        }
    }
}