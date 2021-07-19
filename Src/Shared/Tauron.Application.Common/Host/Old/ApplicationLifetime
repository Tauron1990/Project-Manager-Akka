using System;
using System.Threading;
using NLog;

namespace Tauron.Host
{
    public class ApplicationLifetime : IActorApplicationLifetime, IApplicationLifetime, IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _startedSource = new();

        private readonly CancellationTokenSource _stoppedSource = new();

        private readonly CancellationTokenSource _stoppingSource = new();

        public void Shutdown(int exitCode)
        {
            Environment.ExitCode = exitCode;
            StopApplication();
        }

        public void Dispose()
        {
            _startedSource.Dispose();
            _stoppingSource.Dispose();
            _stoppedSource.Dispose();

            GC.SuppressFinalize(this);
        }

        public CancellationToken ApplicationStarted => _startedSource.Token;

        public CancellationToken ApplicationStopping => _stoppingSource.Token;

        public CancellationToken ApplicationStopped => _stoppedSource.Token;

        public void StopApplication()
        {
            lock (_stoppingSource)
            {
                try
                {
                    ExecuteHandlers(_stoppingSource);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "An error occurred stopping the application");
                }
            }
        }

        public void NotifyStarted()
        {
            try
            {
                ExecuteHandlers(_startedSource);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "An error occurred starting the application");
            }
        }

        public void NotifyStopped()
        {
            try
            {
                ExecuteHandlers(_stoppedSource);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "An error occurred stopping the application");
            }
        }

        private static void ExecuteHandlers(CancellationTokenSource cancel)
        {
            if (!cancel.IsCancellationRequested)
                cancel.Cancel(false);
        }
    }
}