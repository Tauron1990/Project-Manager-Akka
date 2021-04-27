using Tauron.Application.AkkaNode.Services.Commands;

namespace AkkaTest.CommandTest
{
    public sealed record TestCommand : FileTransferCommand<TestApi, TestCommand>
    {
        protected override string Info => "Test File Transfer Commands";
    }
}