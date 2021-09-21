using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using Tauron;
using TestWebApplication.Shared.Counter;

namespace TestWebApplication.Server.Services
{
    public class CounterService : ICounterService
    {
        private readonly ConcurrentDictionary<string, int> _data = new();
        private readonly IMutableState<int> _offset;

        public CounterService(IStateFactory factory) => _offset = factory.NewMutable<int>(0);

        public virtual async Task<int> GetCounter(string key, CancellationToken token = default)
        {
            if (Computed.IsInvalidating()) return 0;

            var offset = await _offset.Use(token);
            var counter = _data.GetValueOrDefault(key);

            return counter + offset;
        }

        public virtual Task Increment(IncrementCommand command, CancellationToken token = default)
        {
            _data.AddOrUpdate(command.Key, _ => 1, (_, c) => c + 1);

            using (Computed.Invalidate())
            {
                GetCounter(command.Key, token).Ignore();
            }

            return Task.CompletedTask;
        }

        public virtual Task SetOffset(SetOffsetCommand command, CancellationToken token = default)
        {
            _offset.Set(command.Offset);

            return Task.CompletedTask;
        }
    }
}