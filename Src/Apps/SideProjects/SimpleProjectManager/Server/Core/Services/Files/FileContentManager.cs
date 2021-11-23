using System.Collections.Immutable;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Application;

namespace SimpleProjectManager.Server.Core.Services;

public sealed class FileContentManager
{
    private readonly GridFSBucket _bucked;
    private readonly TaskManagerCore _taskManager;
    private readonly ILogger<FileContentManager> _logger;
    private readonly IEventAggregator _aggregator;
    private readonly CriticalErrorHelper _criticalErrorHelper;
    
    public FileContentManager(GridFSBucket bucked, ICriticalErrorService criticalErrorService, TaskManagerCore taskManager, 
        ILogger<FileContentManager> logger, IEventAggregator aggregator)
    {
        _bucked = bucked;
        _taskManager = taskManager;
        _logger = logger;
        _aggregator = aggregator;
        _criticalErrorHelper = new CriticalErrorHelper(nameof(FileContentManager), criticalErrorService, logger);
    }

    public async Task<string?> PreRegisterFile(Func<Stream> toRegister, ProjectFileId id, string fileName, CancellationToken token)
    {
        var transaction = new PreRegisterTransaction(_logger);
        var context = new PreRegistrationContext(toRegister, id, _bucked, _taskManager, token);

        return await _criticalErrorHelper.ProcessTransaction(
            await transaction.Execute(context),
            nameof(PreRegisterFile),
            () => ImmutableList<ErrorProperty>.Empty
               .Add(new ErrorProperty("File Id", id.Value))
               .Add(new ErrorProperty("File Name", fileName)));
    }

    public async Task<string?> CommitFile(ProjectFileId id, CancellationToken token)
    {
        var trans = new CommitRegistrationTransaction(_logger);
        var context = new CommitRegistrationContext(_taskManager, id, token);

        return await _criticalErrorHelper.ProcessTransaction(
            await trans.Execute(context),
            nameof(CommitFile),
            () => ImmutableList<ErrorProperty>.Empty
               .Add(new ErrorProperty("File Id", id.Value)));
    }

    public async Task DeleteFile(ProjectFileId id, CancellationToken token)
    {
        var filter = Builders<GridFSFileInfo>.Filter.Eq(m => m.Filename, id.Value);
        var search = await (await _bucked.FindAsync(filter, cancellationToken: token)).FirstOrDefaultAsync(token);
        if(search == null) return;

        await _bucked.DeleteAsync(search.Id, token);
        await _taskManager.Delete(FilePurgeId.For(id).Value, token);
        
        _aggregator.Publish(new FileDeleted(id));
    }
}