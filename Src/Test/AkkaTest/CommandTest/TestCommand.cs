using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace AkkaTest.CommandTest
{
    public sealed record TestCommand : FileTransferCommand<TestApi, TestCommand>
    {
        protected override string Info => "Test File Transfer Commands";
    }
}