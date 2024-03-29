using System;
using System.Reactive;
using System.Threading.Tasks;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.Services;

public interface IMessageDispatcher
{
    Func<TInput, bool> IsSuccess<TInput>(Func<TInput, SimpleResult> runner);

    ValueTask<bool> IsSuccess(Func<ValueTask<SimpleResult>> runner);

    ValueTask<bool> IsSuccess(Func<ValueTask<Unit>> runner);

    ValueTask<bool> IsSuccess(Func<ValueTask> runner);

    void PublishError(Exception exception);

    void PublishError(string error);

    void PublishWarnig(string warning);

    void PublishMessage(string message);
}