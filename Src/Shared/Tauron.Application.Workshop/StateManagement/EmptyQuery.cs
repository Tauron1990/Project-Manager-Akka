using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    public sealed class EmptyQuery : IQuery
    {
        public static readonly EmptyQuery Instance = new();

        private readonly string? _hash;

        private EmptyQuery() { }

        private EmptyQuery(string? hash) => _hash = hash;

        public string ToHash() => _hash ?? nameof(EmptyQuery);

        public EmptyQuery WithHash(string hash) => new(hash);
    }
}