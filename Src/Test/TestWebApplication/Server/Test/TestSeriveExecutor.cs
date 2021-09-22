using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TestWebApplication.Server.Test
{
    public sealed class TestSeriveExecutor : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => Task.CompletedTask;
    }
}