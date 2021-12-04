using MongoDB.Bson;
using MongoDB.Driver;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Application.MongoExtensions;

namespace SimpleProjectManager.Server.Core.Services;

public sealed record CriticalErrorEntry(ObjectId Id, CriticalError Error, bool IsDisabled);

public class CriticalErrorService : ICriticalErrorService
{
    private readonly ILogger<CriticalErrorService> _logger;
    private readonly IMongoCollection<CriticalErrorEntry> _errorEntrys;

    public CriticalErrorService(InternalDataRepository repository, ILogger<CriticalErrorService> logger)
    {
        ImmutableListSerializer<ErrorProperty>.Register();

        _logger = logger;
        _errorEntrys = repository.Collection<CriticalErrorEntry>();
        //#if DEBUG
        //if (_errorEntrys.CountDocuments(Builders<CriticalErrorEntry>.Filter.Empty) == 0)
        //{
        //    var id = ObjectId.GenerateNewId();
        //    _errorEntrys.InsertOne(new CriticalErrorEntry(id, new CriticalError(id.ToString(), DateTime.Now, "Test Part", "Test Nachricht", EnhancedStackTrace.Current().ToString(), ImmutableList<ErrorProperty>.Empty.Add(new ErrorProperty("Test Property", "Test Info"))), false));
        //}
        //#endif
    }

    public virtual async Task<long> CountErrors(CancellationToken token)
    {
        if (Computed.IsInvalidating()) return 0;

        var filter = Builders<CriticalErrorEntry>.Filter.Eq(m => m.IsDisabled, false);

        return await _errorEntrys.CountDocumentsAsync(filter, cancellationToken:token);
    }

    public virtual async Task<CriticalError[]> GetErrors(CancellationToken token)
    {
        if (Computed.IsInvalidating()) return Array.Empty<CriticalError>();
        
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

            Invalidate();
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
            Invalidate();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on Write Error to Database");
        }
    }

    private void Invalidate()
    {
        using (Computed.Invalidate())
        {
            GetErrors(default).Ignore();
            CountErrors(default).Ignore();
        }
    }
}