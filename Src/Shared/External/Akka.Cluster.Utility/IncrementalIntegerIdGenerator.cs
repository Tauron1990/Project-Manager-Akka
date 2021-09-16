namespace Akka.Cluster.Utility
{
    public sealed class IncrementalIntegerIdGenerator : IIdGenerator<long>
    {
        private long _lastId;

        public void Initialize(object[]? args)
        {
            if (args is { Length: 1 })
                _lastId = (long) args[0];
        }

        public long GenerateId()
        {
            _lastId += 1;
            return _lastId;
        }
    }
}