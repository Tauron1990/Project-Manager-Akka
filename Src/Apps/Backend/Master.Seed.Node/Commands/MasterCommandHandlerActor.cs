using System;
using System.Text;
using Petabridge.Cmd;
using Petabridge.Cmd.Host;

namespace Master.Seed.Node.Commands
{
    public sealed class MasterCommandHandlerActor : CommandHandlerActor
    {
        public MasterCommandHandlerActor()
            : base(MasterCommands.MasterPalette)
        {
            Process(MasterCommands.Kill.Name, _ => KillSwitch.KillCluster());
            Process(MasterCommands.ListServices.Name, ListServices);
        }

        private void ListServices(Command obj)
        {
            var reg = ServiceRegistry.Get(Context.System);

            reg.QueryServices()
               .ContinueWith(
                    r =>
                    {
                        try
                        {
                            var builder = new StringBuilder();

                            foreach (var (name, memberAddress, serviceType) in r.Result.Services)
                                builder.AppendLine($"{name} - -{memberAddress} -- {serviceType.DisplayName}");

                            return new CommandResponse(builder.ToString());
                        }
                        catch (Exception e)
                        {
                            return new ErroredCommandResponse(e.ToString());
                        }
                    }).PipeTo(Sender)
               .Ignore();
        }
    }
}