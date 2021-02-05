using Akka.Actor;
using Petabridge.Cmd.Host;

namespace Master.Seed.Node.Commands
{
    public sealed class MasterCommandHandler : CommandPaletteHandler
    {
        private MasterCommandHandler()
            : base(MasterCommands.MasterPalette) =>
            HandlerProps = Props.Create<MasterCommandHandlerActor>();

        public static MasterCommandHandler New => new();

        public override Props HandlerProps { get; }
    }
}