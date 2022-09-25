using System;
using System.Reactive;
using System.Threading.Tasks;
using SimpleProjectManager.Client.Shared.Services;
using Tauron.Application;

namespace SimpleProjectManager.Client.Avalonia.Models.Services;

public sealed class MessageDispatcher : IMessageDispatcher
{
    private readonly IEventAggregator _aggregator;

    public MessageDispatcher(IEventAggregator aggregator)
        => _aggregator = aggregator;

    public Func<TInput, bool> IsSuccess<TInput>(Func<TInput, string> runner)
    {
        return input =>
               {
                   try
                   {
                       var result = runner(input);

                       if (string.IsNullOrWhiteSpace(result))
                           return true;

                       _aggregator.PublishSharedMessage(SharedMessage.CreateWarning(result));

                       return false;
                   }
                   catch (Exception e)
                   {
                       _aggregator.PublishSharedMessage(SharedMessage.CreateError(e));

                       return false;
                   }
               };
    }
    
    public async ValueTask<bool> IsSuccess(Func<ValueTask<string>> runner)
    {
        try
        {
            var result = await runner();

            if (string.IsNullOrWhiteSpace(result))
                return true;

            _aggregator.PublishSharedMessage(SharedMessage.CreateWarning(result));

            return false;
        }
        catch (Exception e)
        {
            _aggregator.PublishSharedMessage(SharedMessage.CreateError(e));

            return false;
        }
    }
    
    public async ValueTask<bool> IsSuccess(Func<ValueTask<Unit>> runner)
    {
        try
        {
            await runner();
            return true;
        }
        catch (Exception e)
        {
            _aggregator.PublishSharedMessage(SharedMessage.CreateError(e));

            return false;
        }
    }
    
    public async ValueTask<bool> IsSuccess(Func<ValueTask> runner)
    {
        try
        {
            await runner();
            return true;
        }
        catch (Exception e)
        {
            _aggregator.PublishSharedMessage(SharedMessage.CreateError(e));

            return false;
        }
    }

    public void PublishError(Exception exception)
        => _aggregator.PublishSharedMessage(SharedMessage.CreateError(exception));

    public void PublishError(string error)
        => _aggregator.PublishSharedMessage(SharedMessage.CreateError(error));

    public void PublishWarnig(string warning)
        => _aggregator.PublishSharedMessage(SharedMessage.CreateWarning(warning));

    public void PublishMessage(string message)
        => _aggregator.PublishSharedMessage(SharedMessage.CreateInfo(message));
}