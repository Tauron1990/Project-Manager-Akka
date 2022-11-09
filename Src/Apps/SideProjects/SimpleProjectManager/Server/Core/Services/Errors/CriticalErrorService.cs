﻿using MongoDB.Bson;
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
            var id = ObjectId.GenerateNewId().ToString();
            CriticalErrorEntry entry = new(id, error with { Id = id }, false);

            await _errorEntrys.InsertOneAsync(entry, token);
            Invalidate();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on Write Error to Database");
        }
    }

    public virtual async Task<long> CountErrors(CancellationToken token)
    {
        if(Computed.IsInvalidating()) return 0;

        var filter = _errorEntrys.Operations.Eq(m => m.IsDisabled, false);

        return await _errorEntrys.CountEntrys(filter, token);
    }

    public virtual async Task<CriticalError[]> GetErrors(CancellationToken token)
    {
        if(Computed.IsInvalidating()) return Array.Empty<CriticalError>();

        var result = await _errorEntrys.ExecuteArray<DbCriticalError, CriticalError>(
            _errorEntrys
               .Find(_errorEntrys.Operations.Eq(e => e.IsDisabled, false))
               .Select(d => d.Error),
            token);

        return result;
    }

    public virtual async Task<string> DisableError(string id, CancellationToken token)
    {
        var filter = _errorEntrys.Operations.Eq(e => e.Id, id);
        var updater = _errorEntrys.Operations.Set(e => e.IsDisabled, true);

        try
        {
            DbOperationResult result = await _errorEntrys.UpdateOneAsync(filter, updater, token);

            if(!result.IsAcknowledged || result.ModifiedCount != 1)
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

    private void Invalidate()
    {
        using (Computed.Invalidate())
        {
            GetErrors(default).Ignore();
            CountErrors(default).Ignore();
        }
    }
}