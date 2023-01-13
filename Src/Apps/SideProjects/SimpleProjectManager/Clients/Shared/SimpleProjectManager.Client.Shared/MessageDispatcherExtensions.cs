using System;
using SimpleProjectManager.Client.Shared.Services;

namespace SimpleProjectManager.Client.Shared;

public static class MessageDispatcherExtensions
{
    public static Func<Exception, bool> IgnoreErrors(this IMessageDispatcher messageDispatcher)
        => ex =>
           {
               messageDispatcher.PublishError(ex);

               return false;
           };

    public static Func<Exception, bool> PropagateErrors(this IMessageDispatcher messageDispatcher)
        => ex =>
           {
               messageDispatcher.PublishError(ex);

               return true;
           };
}