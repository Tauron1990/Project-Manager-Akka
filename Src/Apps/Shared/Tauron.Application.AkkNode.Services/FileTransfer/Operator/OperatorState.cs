namespace Tauron.Application.AkkaNode.Services.FileTransfer.Operator;

public enum OperatorState
{
    Waiting,
    InitSending,
    InitReciving,
    Sending,
    Reciving,
    Failed,
    Compled,
}