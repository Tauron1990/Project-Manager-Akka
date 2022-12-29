using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Server.Data.Data;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron.Application.MongoExtensions;

namespace SimpleProjectManager.Server.Core.Services;

public class CriticalErrorService : ICriticalErrorService
{
    private readonly MappingDatabase<DbCriticalErrorEntry, CriticalErrorEntry> _errorEntrys;
    private readonly ILogger<CriticalErrorService> _logger;

    public CriticalErrorService(IInternalDataRepository dataRepository, ILogger<CriticalErrorService> logger)
    {
        ImmutableListSerializer<ErrorProperty>.Register();

        _logger = logger;
        _errorEntrys = new MappingDatabase<DbCriticalErrorEntry, CriticalErrorEntry>(
            dataRepository.Databases.CriticalErrors,
            dataRepository.Databases.Mapper);
        //#if DEBUG
        //if (_errorEntrys.CountDocuments(Builders<CriticalErrorEntry>.Filter.Empty) == 0)
        //{
        //    var id = ObjectId.GenerateNewId();
        //    _errorEntrys.InsertOne(new CriticalErrorEntry(id, new CriticalError(id.ToString(), DateTime.Now, "Test Part", "Test Nachricht", EnhancedStackTrace.Current().ToString(), ImmutableList<ErrorProperty>.Empty.Add(new ErrorProperty("Test Property", "Test Info"))), false));
        //}
        //#endif
    }

    public virtual async Task WriteError(CriticalError error, CancellationToken token)
    {
        try
        {
            CriticalErrorEntry entry = new(error.Id, error, false);

            await _errorEntrys.InsertOneAsync(entry, token).ConfigureAwait(false);
            Invalidate();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on Write Error to Database");
        }
    }

    public virtual async Task<ErrorCount> CountErrors(CancellationToken token)
    {
        if(Computed.IsInvalidating()) return ErrorCount.From(0);

        var filter = _errorEntrys.Operations.Eq(m => m.IsDisabled, false);

        return ErrorCount.From(await _errorEntrys.CountEntrys(filter, token).ConfigureAwait(false));
    }

    public virtual async Task<CriticalErrorList> GetErrors(CancellationToken token)
    {
        if(Computed.IsInvalidating()) return CriticalErrorList.Empty;

        var result = _errorEntrys.ExecuteAsyncEnumerable<DbCriticalError, CriticalError>(
            _errorEntrys
               .Find(_errorEntrys.Operations.Eq(e => e.IsDisabled, false))
               .Select(d => d.Error),
            token);

        return new CriticalErrorList(await result.ToImmutableList(token).ConfigureAwait(false));
    }

    public virtual async Task<SimpleResult> DisableError(ErrorId id, CancellationToken token)
    {
        var filter = _errorEntrys.Operations.Eq(e => e.Id, id.Value);
        var updater = _errorEntrys.Operations.Set(e => e.IsDisabled, true);

        try
        {
            DbOperationResult result = await _errorEntrys.UpdateOneAsync(filter, updater, token).ConfigureAwait(false);

            if(!result.IsAcknowledged || result.ModifiedCount != 1)
                return SimpleResult.Failure("Unbekannter fehler beim Aktualisieren der Datenbank");

            Invalidate();

            return SimpleResult.Success();

        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error on Disabling ErrorEntry");

            return SimpleResult.Failure(e);
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