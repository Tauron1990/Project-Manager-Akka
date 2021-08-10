using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stl;
using Stl.Fusion;
using Stl.Fusion.Bridge;
using TestWebApplication.Shared.Counter;

namespace TestWebApplication.Server.Test
{
    public sealed class TestSeriveExecutor : BackgroundService
    {
        private readonly IPublisher _publisher;
        private readonly ICounterService _service;

        public TestSeriveExecutor(IPublisher publisher, ICounterService service)
        {
            _publisher = publisher;
            _service = service;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => Task.Run(
                async () =>
                {
                    var comuptent = await Computed.Capture(c => _service.GetCounter("Main", c), stoppingToken);
                    var pub = _publisher.Publish(comuptent);

                    using (pub.Use())
                    {
                        var temp = comuptent.Value;
                    }

                    var info = new PublicationStateInfo(pub.Ref, new LTag(1), comuptent.IsConsistent());
                });
    }
}