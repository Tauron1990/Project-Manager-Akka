using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using JetBrains.Annotations;

namespace Tauron.Application.Dataflow
{
	[PublicAPI]
	public static class SourceBlockExtensions
    {
        public static readonly DataflowLinkOptions DefaultAnonymosLinkOptions = new() {PropagateCompletion = true};

        public static IDisposable LinkTo<T>(this ISourceBlock<T> source)
            => LinkTo_(source, null, null, null, CancellationToken.None);

        public static IDisposable LinkTo<T>(this ISourceBlock<T> source, Action<T> onNext)
            => LinkTo_(source, onNext, null, null, CancellationToken.None);


        public static IDisposable LinkTo<T>(this ISourceBlock<T> source, Action<T> onNext, Action<Exception> onError)
            => LinkTo_(source, onNext, onError, null, CancellationToken.None);

        public static IDisposable LinkTo<T>(this ISourceBlock<T> source, Action<T> onNext, Action onCompleted)
            => LinkTo_(source, onNext, null, onCompleted, CancellationToken.None);

        public static IDisposable LinkTo<T>(this ISourceBlock<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
            => LinkTo_(source, onNext, onError, onCompleted, CancellationToken.None);

        public static Task LinkTo<T>(this ISourceBlock<T> source, Func<T, Task> onNext)
            => LinkToAsync_(source, onNext, null, null, CancellationToken.None);

        public static Task LinkTo<T>(this ISourceBlock<T> source, Func<T, Task> onNext, Func<Exception, Task> onError)
            => LinkToAsync_(source, onNext, onError, null, CancellationToken.None);

        public static Task LinkTo<T>(this ISourceBlock<T> source, Func<T, Task> onNext, Func<Task> onCompleted)
            => LinkToAsync_(source, onNext, null, onCompleted, CancellationToken.None);

        public static Task LinkTo<T>(this ISourceBlock<T> source, Func<T, Task> onNext, Func<Exception, Task> onError, Func<Task> onCompleted)
            => LinkToAsync_(source, onNext, onError, onCompleted, CancellationToken.None);

        public static void LinkTo<T>(this ISourceBlock<T> source, IObserver<T> observer, CancellationToken token)
            => LinkTo_(source, observer.OnNext, observer.OnError, observer.OnCompleted, token);

        public static void LinkTo<T>(this ISourceBlock<T> source, CancellationToken token)
            => LinkTo_(source, null, null, null, token);

        public static void LinkTo<T>(this ISourceBlock<T> source, Action<T> onNext, CancellationToken token)
            => LinkTo_(source, onNext, null, null, token);

        public static void LinkTo<T>(this ISourceBlock<T> source, Action<T> onNext, Action<Exception> onError, CancellationToken token)
            => LinkTo_(source, onNext, onError, null, token);

        public static void LinkTo<T>(this ISourceBlock<T> source, Action<T> onNext, Action onCompleted, CancellationToken token)
            => LinkTo_(source, onNext, null, onCompleted, token);

        public static void LinkTo<T>(this ISourceBlock<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted, CancellationToken token)
            => LinkTo_(source, onNext, onError, onCompleted, token);

        public static Task LinkTo<T>(this ISourceBlock<T> source, Func<T, Task> onNext, CancellationToken token)
            => LinkToAsync_(source, onNext, null, null, token);

        public static Task LinkTo<T>(this ISourceBlock<T> source, Func<T, Task> onNext, Func<Exception, Task> onError, CancellationToken token)
            => LinkToAsync_(source, onNext, onError, null, token);

        public static Task LinkTo<T>(this ISourceBlock<T> source, Func<T, Task> onNext, Func<Task> onCompleted, CancellationToken token)
            => LinkToAsync_(source, onNext, null, onCompleted, token);

        public static Task LinkTo<T>(this ISourceBlock<T> source, Func<T, Task> onNext, Func<Exception, Task> onError, Func<Task> onCompleted, CancellationToken token)
            => LinkToAsync_(source, onNext, onError, onCompleted, token);

        internal static IDisposable LinkTo_<T>(ISourceBlock<T> source, Action<T>? onNext, Action<Exception>? onError, Action? onCompled, CancellationToken token)
        {
            var context = ExecutionContext.Capture();
            ActionBlock<T> handler = onNext != null
                ? ActionBlock.Create(onNext)
                : ActionBlock.Create<T>(_ => { });

            var link = source.LinkTo(handler, DefaultAnonymosLinkOptions);

            if (token.CanBeCanceled)
                token.Register(() =>
                               {
                                   link.Dispose();
                                   handler.Complete();
                               });

            handler.Completion
                   .ContinueWith(t =>
                                 {
                                     try
                                     {
                                         if (t.IsCompletedSuccessfully)
                                             onCompled?.Invoke();
                                         else if (t.IsCanceled)
                                             onCompled?.Invoke();
                                         else if (t.IsFaulted)
                                             onError?.Invoke(t.Exception.Unwrap() ?? new InvalidOperationException());
                                     }
                                     catch (Exception e)
                                     {
                                         if (context != null)
                                             ExecutionContext.Run(context, state => throw ((Exception) state!), e);
                                     }
                                     finally
                                     {
                                         link.Dispose();
                                     }
                                 }, token);
            return link;
        }

        private static async Task LinkToAsync_<T>(ISourceBlock<T> source, Func<T, Task>? onNext, Func<Exception, Task>? onError, Func<Task>? onCompled, CancellationToken token)
        {
			ActionBlock<T> handler = onNext != null 
                ? ActionBlock.Create(onNext)
                : ActionBlock.Create<T>(_ => { });

            var link = source.LinkTo(handler, DefaultAnonymosLinkOptions);

            using (link)
            {
                var failed = false;

                try
                {
                    if (token.CanBeCanceled)
                        token.Register(() =>
                                       {
                                           link.Dispose();
                                           handler.Complete();
                                       });
                    await handler.Completion;
                }
                catch (Exception e)
                {
                    failed = true;
                    if (onError != null)
                        await onError(e);
                }
                finally
                {
                    if (!failed && onCompled != null)
                        await onCompled();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
		public static IDisposable Subscribe<T>(this ISourceBlock<T> source, IObserver<T> observer)
            => source.AsObservable().Subscribe(observer);
    }
}