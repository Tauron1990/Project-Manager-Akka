using Akka.Actor;

namespace Akka.Cluster.Utility
{
    public static class DistributedActorTableMessage<TKey>
    {
        public class Create
        {
            public Create(object[] args) => Args = args;

            public Create(TKey id, object[] args)
            {
                Id = id;
                Args = args;
            }

            public TKey Id { get; }
            public object[] Args { get; }
        }

        public class CreateReply
        {
            public CreateReply(TKey id, IActorRef actor)
            {
                Id = id;
                Actor = actor;
            }

            public TKey Id { get; }
            public IActorRef Actor { get; }
        }

        public class GetOrCreate
        {
            public GetOrCreate(TKey id, object[] args)
            {
                Id = id;
                Args = args;
            }

            public TKey Id { get; }
            public object[] Args { get; }
        }

        public class GetOrCreateReply
        {
            public GetOrCreateReply(TKey id, IActorRef actor, bool created)
            {
                Id = id;
                Actor = actor;
                Created = created;
            }

            public TKey Id { get; }
            public IActorRef Actor { get; }
            public bool Created { get; }
        }

        public class Get
        {
            public Get(TKey id) => Id = id;

            public TKey Id { get; }
        }

        public class GetReply
        {
            public GetReply(TKey id, IActorRef actor)
            {
                Id = id;
                Actor = actor;
            }

            public TKey Id { get; }
            public IActorRef Actor { get; }
        }

        public class GetIds { }

        public class GetIdsReply
        {
            public TKey[] Ids;

            public GetIdsReply(TKey[] ids) => Ids = ids;
        }

        // Request to a table to stop all table & actors contained gracefully
        public class GracefulStop
        {
            public GracefulStop(object stopMessage) => StopMessage = stopMessage;

            public object StopMessage { get; }
        }

        // Ask for a local container to add actor to table. (not for table directly)
        public class Add
        {
            public Add(TKey id, IActorRef actor)
            {
                Id = id;
                Actor = actor;
            }

            public TKey Id { get; }
            public IActorRef Actor { get; }
        }

        public class AddReply
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

        // Ask for a local container to remove actor to table. (not for table directly)
        public class Remove
        {
            public Remove(TKey id) => Id = id;

            public TKey Id { get; }
        }

        internal static class Internal
        {
            public class Create
            {
                public Create(TKey id, object[] args)
                {
                    Id = id;
                    Args = args;
                }

                public TKey Id { get; }
                public object[] Args { get; }
            }

            public class CreateReply
            {
                public CreateReply(TKey id, IActorRef actor)
                {
                    Id = id;
                    Actor = actor;
                }

                public TKey Id { get; }
                public IActorRef Actor { get; }
            }

            public class Add
            {
                public Add(TKey id, IActorRef actor)
                {
                    Id = id;
                    Actor = actor;
                }

                public TKey Id { get; }
                public IActorRef Actor { get; }
            }

            public class AddReply
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

            public class Remove
            {
                public Remove(TKey id) => Id = id;

                public TKey Id { get; }
            }

            public class GracefulStop
            {
                public GracefulStop(object stopMessage) => StopMessage = stopMessage;

                public object StopMessage { get; }
            }
        }
    }
}