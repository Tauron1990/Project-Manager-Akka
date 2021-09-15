using Akka.Actor;

namespace Akka.Cluster.Utility
{
    public static class DistributedActorTableMessage<TKey>
    {
        public sealed record Create(TKey? Id, object[] Args)
        {
            public Create(object[] args)
                : this(default, args){}
        }

        public sealed record CreateReply(TKey Id, IActorRef Actor);

        public sealed record GetOrCreate(TKey Id, object[] Args);

        #pragma warning disable AV1564
        public sealed record GetOrCreateReply(TKey Id, IActorRef Actor, bool Created);
        #pragma warning restore AV1564

        public sealed record Get(TKey Id);

        public sealed record GetReply(TKey Id, IActorRef Actor);

        public sealed record GetIds;

        public sealed record GetIdsReply(TKey[] Ids);

        // Request to a table to stop all table & actors contained gracefully
        public sealed record GracefulStop(object StopMessage);

        // Ask for a local container to add actor to table. (not for table directly)
        public sealed record Add(TKey Id, IActorRef Actor);

        #pragma warning disable AV1564
        // ReSharper disable once ClassNeverInstantiated.Global
        public sealed record AddReply(TKey Id, IActorRef Actorm, bool Added);
        #pragma warning restore AV1564

        // Ask for a local container to remove actor to table. (not for table directly)
        public sealed record Remove(TKey Id);

        internal static class Internal
        {
            // ReSharper disable MemberHidesStaticFromOuterClass
            internal sealed record Create(TKey Id, object[] Args);

            internal sealed record CreateReply(TKey Id, IActorRef Actor);

            internal sealed record Add(TKey Id, IActorRef Actor);

            internal class AddReply
            {
                public AddReply(TKey id, IActorRef actor, bool added)
                {
                    Id = id;
                    Actor = actor;
                    Added = added;
                }

                public TKey Id { get; }
                public IActorRef Actor { get; }
                public bool Added { get; }
            }

            internal class Remove
            {
                public Remove(TKey id) => Id = id;

                public TKey Id { get; }
            }

            internal class GracefulStop
            {
                public GracefulStop(object stopMessage) => StopMessage = stopMessage;

                public object StopMessage { get; }
            }
            // ReSharper restore MemberHidesStaticFromOuterClass
        }
    }
}