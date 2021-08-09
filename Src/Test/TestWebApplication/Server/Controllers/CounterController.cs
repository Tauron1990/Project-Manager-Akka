using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using TestWebApplication.Shared.Counter;

namespace TestWebApplication.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController, JsonifyErrors]
    public class CounterController : Controller, ICounterService
    {
        private readonly ICounterService _counterService;

        public CounterController(ICounterService counterService) 
            => _counterService = counterService;

        [HttpGet, Publish]
        public Task<int> GetCounter(string key, CancellationToken token = default) 
            => _counterService.GetCounter(key, token);

        [HttpPost]
        public Task Increment(IncrementCommand command, CancellationToken token = default) 
            => _counterService.Increment(command, token);

        [HttpPost]
        public Task SetOffset(SetOffsetCommand command, CancellationToken token = default) 
            => _counterService.SetOffset(command, token);
    }
}